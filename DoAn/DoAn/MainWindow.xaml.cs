using DoAn.ClassData;
using DoAn.Pages;
using DoAn.Windows;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using System;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace DoAn
{
    public partial class MainWindow : Window
    {
        private readonly Database db = new Database();
        private string currentUser;
        private DispatcherTimer timer;

        public MainWindow(string username)
        {
            InitializeComponent();
            currentUser = username;
            txtUsernameDisplay.Text = currentUser;

            LoadUserAvatar();
            StartClock();

            this.Loaded += (s, e) => {
                btnHome.IsChecked = true;
                SetMode("Home");
            };
        }

        private void StartClock()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) => txtClock.Text = DateTime.Now.ToString("HH:mm:ss");
            timer.Start();
        }

        #region Window Controls
        private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }
        private void Min_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Close_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        private void Max_Click(object sender, RoutedEventArgs e) => WindowState = (WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
        #endregion

        private void SetMode(string mode)
        {
            if (mode == "Home")
            {
                brdDashboard.Visibility = Visibility.Visible;
                gridDataDisplay.Visibility = Visibility.Collapsed;
                LoadDashboardStats();
            }
            else
            {
                brdDashboard.Visibility = Visibility.Collapsed;
                gridDataDisplay.Visibility = Visibility.Visible;

                btnAddDrug.Visibility = (mode == "Drug") ? Visibility.Visible : Visibility.Collapsed;
                btnAddPatient.Visibility = (mode == "Patient") ? Visibility.Visible : Visibility.Collapsed;
                btnAddBill.Visibility = (mode == "Bill") ? Visibility.Visible : Visibility.Collapsed;

                LoadCurrentList(mode);
            }
        }

        private void LoadDashboardStats()
        {
            try
            {
                txtStatPatient.Text = db.ExecuteScalar("SELECT COUNT(*) FROM PATIENT").ToString();
                txtStatDrug.Text = db.ExecuteScalar("SELECT COUNT(*) FROM DRUG").ToString();
                object revenue = db.ExecuteScalar("SELECT SUM(Total) FROM BILL");
                txtStatRevenue.Text = revenue != DBNull.Value ? string.Format("{0:N0}", revenue) : "0";
            }
            catch { }
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not ToggleButton clicked) return;
            btnHome.IsChecked = (clicked == btnHome);
            btnDrug.IsChecked = (clicked == btnDrug);
            btnPatient.IsChecked = (clicked == btnPatient);
            btnBill.IsChecked = (clicked == btnBill);

            if (clicked == btnHome) SetMode("Home");
            else if (clicked == btnDrug) SetMode("Drug");
            else if (clicked == btnPatient) SetMode("Patient");
            else if (clicked == btnBill) SetMode("Bill");
        }

        private void LoadCurrentList(string mode)
        {
            try
            {
                string sql = mode switch
                {
                    "Drug" => "SELECT * FROM DRUG",
                    "Patient" => "SELECT * FROM PATIENT",
                    "Bill" => "SELECT * FROM BILL",
                    _ => ""
                };
                if (!string.IsNullOrEmpty(sql)) dgData.ItemsSource = db.GetData(sql).DefaultView;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        #region Actions (Add/Update/Delete/Search)
        private void btnAddDrug_Click(object sender, RoutedEventArgs e)
        {
            var w = new AddDrugWindow { Owner = this };
            if (w.ShowDialog() == true) LoadCurrentList("Drug");
        }

        private void btnAddPatient_Click(object sender, RoutedEventArgs e)
        {
            var w = new AddPatientWindow { Owner = this };
            if (w.ShowDialog() == true) LoadCurrentList("Patient");
        }

        private void btnAddBill_Click(object sender, RoutedEventArgs e)
        {
            // Tự động truyền currentUser (Mã nhân viên) vào Window Bill
            var w = new AddBillWindow(currentUser) { Owner = this };
            if (w.ShowDialog() == true) LoadCurrentList("Bill");
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (dgData.SelectedItem is not DataRowView row) return;

            if (btnPatient.IsChecked == true)
            {
                var w = new UpdatePatientWindow(row["Patient_ID"].ToString(), row["Patient_Name"].ToString(), row["Gender"].ToString(), row["Phone"].ToString(), row["CID"].ToString(), row["Address"].ToString(), row["Curr_Condition"].ToString()) { Owner = this };
                if (w.ShowDialog() == true) LoadCurrentList("Patient");
            }
            else if (btnDrug.IsChecked == true)
            {
                var w = new UpdateDrugWindow(row["Drug_ID"].ToString(), Convert.ToDecimal(row["Drug_Price"]), Convert.ToInt32(row["Stock_Quantity"])) { Owner = this };
                if (w.ShowDialog() == true) LoadCurrentList("Drug");
            }
            else if (btnBill.IsChecked == true)
            {
                var w = new UpdateBillWindow(row["Bill_ID"].ToString(), row["Patient_ID"].ToString(), row["Emp_ID"].ToString(), Convert.ToDecimal(row["Total"]), row["Payment_Method"].ToString(), row["Payment_Status"].ToString()) { Owner = this };
                if (w.ShowDialog() == true) LoadCurrentList("Bill");
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgData.SelectedItem is not DataRowView row) return;
            string mode = btnDrug.IsChecked == true ? "Drug" : (btnPatient.IsChecked == true ? "Patient" : "Bill");
            string col = mode switch { "Drug" => "Drug_ID", "Patient" => "Patient_ID", "Bill" => "Bill_ID", _ => "" };
            string id = row[col].ToString();

            if (MessageBox.Show($"Xóa dữ liệu {id}?", "Cảnh báo", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    db.Execute($"DELETE FROM {mode.ToUpper()} WHERE {col}=@id", new SqlParameter("@id", id));
                    LoadCurrentList(mode);
                }
                catch (Exception ex) { MessageBox.Show("Lỗi ràng buộc: " + ex.Message); }
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string key = txtSearch.Text.Trim();
            string mode = btnDrug.IsChecked == true ? "Drug" : (btnPatient.IsChecked == true ? "Patient" : "Bill");
            if (string.IsNullOrEmpty(key)) { LoadCurrentList(mode); return; }
            try
            {
                DataTable dt = mode switch
                {
                    "Drug" => db.GetData("SELECT * FROM DRUG WHERE Drug_ID LIKE @k OR Drug_Name LIKE @k", new SqlParameter("@k", "%" + key + "%")),
                    "Patient" => db.GetData("SELECT * FROM PATIENT WHERE Patient_ID LIKE @k OR Patient_Name LIKE @k", new SqlParameter("@k", "%" + key + "%")),
                    "Bill" => db.GetData("SELECT * FROM BILL WHERE Bill_ID LIKE @k", new SqlParameter("@k", "%" + key + "%")),
                    _ => null
                };
                if (dt != null) dgData.ItemsSource = dt.DefaultView;
            }
            catch { }
        }
        #endregion

        #region Profile & System
        private void LoadUserAvatar()
        {
            try
            {
                object path = db.ExecuteScalar("SELECT AvatarPath FROM ACCOUNT WHERE User_Name = @u", new SqlParameter("@u", currentUser));
                if (path != null && File.Exists(path.ToString()))
                    imgAvatarBrush.ImageSource = new BitmapImage(new Uri(path.ToString()));
            }
            catch { }
        }

        private void Avatar_Click(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog { Filter = "Image Files|*.png;*.jpg;*.jpeg" };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    imgAvatarBrush.ImageSource = new BitmapImage(new Uri(dlg.FileName));
                    db.Execute("UPDATE ACCOUNT SET AvatarPath = @path WHERE User_Name = @u", new SqlParameter("@path", dlg.FileName), new SqlParameter("@u", currentUser));
                }
                catch { }
            }
        }

        private void btnLogOut_Click(object sender, RoutedEventArgs e)
        {
            this.Effect = new System.Windows.Media.Effects.BlurEffect { Radius = 10 };
            if (new LogoutWindow { Owner = this }.ShowDialog() == true)
            {
                new LoginWindow().Show();
                this.Close();
            }
            else this.Effect = null;
        }
        #endregion
    }
}