using DoAn.ClassData;
using DoAn.Pages;
using DoAn.Windows;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using System;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace DoAn
{
    public partial class MainWindowHR : Window
    {
        private readonly Database db = new Database();
        private string currentUser;
        private DispatcherTimer timer;

        public MainWindowHR(string userName, string id)
        {
            InitializeComponent();

            this.currentUser = userName;
            txtHRUserName.Text = userName;

            // Khởi tạo đồng hồ
            StartClock();
            LoadUserAvatar();

            this.Loaded += (s, e) => {
                btnHRHome.IsChecked = true;
                SetHRMode("Home");
            };
        }

        private void StartClock()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) => {
                txtClock.Text = DateTime.Now.ToString("HH:mm:ss");
            };
            timer.Start();
        }

        #region ĐIỀU KHIỂN CHẾ ĐỘ (MODE)

        private void SetHRMode(string mode)
        {
            if (mode == "Home")
            {
                brdDashboard.Visibility = Visibility.Visible;
                gridHRData.Visibility = Visibility.Collapsed;
                LoadDashboardStats();
            }
            else
            {
                brdDashboard.Visibility = Visibility.Collapsed;
                gridHRData.Visibility = Visibility.Visible;

                bool isManageable = (mode != "Khoa");
                btnHRAddNew.Visibility = isManageable ? Visibility.Visible : Visibility.Collapsed;
                stkHRActions.Visibility = isManageable ? Visibility.Visible : Visibility.Collapsed;

                LoadHRData(mode);
            }
        }

        private void LoadDashboardStats()
        {
            try
            {
                txtStatTotalEmp.Text = db.ExecuteScalar("SELECT COUNT(*) FROM EMPLOYEE").ToString();
                txtStatDoctors.Text = db.ExecuteScalar("SELECT COUNT(*) FROM EMPLOYEE WHERE Emp_Type = 'DC002'").ToString();
                txtStatDeparts.Text = db.ExecuteScalar("SELECT COUNT(*) FROM DEPARTMENT").ToString();
            }
            catch { }
        }

        private void LoadHRData(string mode)
        {
            string sql = mode switch
            {
                "NhanVien" => "SELECT * FROM EMPLOYEE WHERE Emp_Type != 'DC002'",
                "BacSi" => "SELECT * FROM EMPLOYEE WHERE Emp_Type = 'DC002'",
                "Khoa" => "SELECT * FROM DEPARTMENT",
                _ => ""
            };
            try
            {
                if (!string.IsNullOrEmpty(sql)) dgHRMain.ItemsSource = db.GetData(sql).DefaultView;
            }
            catch (Exception ex) { MessageBox.Show("Lỗi tải dữ liệu: " + ex.Message); }
        }

        #endregion

        #region TƯƠNG TÁC (CLICK)

        private void HRMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not ToggleButton btn) return;

            btnHRHome.IsChecked = (btn == btnHRHome);
            btnHRNhanVien.IsChecked = (btn == btnHRNhanVien);
            btnHRBacSi.IsChecked = (btn == btnHRBacSi);
            btnHRKhoa.IsChecked = (btn == btnHRKhoa);

            if (btn == btnHRHome) SetHRMode("Home");
            else if (btn == btnHRNhanVien) SetHRMode("NhanVien");
            else if (btn == btnHRBacSi) SetHRMode("BacSi");
            else if (btn == btnHRKhoa) SetHRMode("Khoa");
        }

        private void btnHRSearch_Click(object sender, RoutedEventArgs e)
        {
            string key = txtHRSearch.Text.Trim();
            string mode = btnHRNhanVien.IsChecked == true ? "NhanVien" : (btnHRBacSi.IsChecked == true ? "BacSi" : "Khoa");
            if (string.IsNullOrEmpty(key)) { LoadHRData(mode); return; }

            string sql = mode switch
            {
                "NhanVien" => "SELECT * FROM EMPLOYEE WHERE (Emp_ID LIKE @k OR Emp_Name LIKE @k) AND Emp_Type != 'DC002'",
                "BacSi" => "SELECT * FROM EMPLOYEE WHERE (Emp_ID LIKE @k OR Emp_Name LIKE @k) AND Emp_Type = 'DC002'",
                "Khoa" => "SELECT * FROM DEPARTMENT WHERE Depart_Name LIKE @k",
                _ => ""
            };
            dgHRMain.ItemsSource = db.GetData(sql, new SqlParameter("@k", "%" + key + "%")).DefaultView;
        }

        private void btnHRAddNew_Click(object sender, RoutedEventArgs e)
        {
            var w = new AddEmployeeWindow { Owner = this };
            if (w.ShowDialog() == true) LoadHRData("NhanVien");
        }

        private void btnHRUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (dgHRMain.SelectedItem is not DataRowView row) return;
            // Thực hiện gọi Window Update tương tự như bản cũ của bạn
            var updateWin = new Windows.UpdateEmployeeWindow(
                row["Emp_ID"].ToString(),
                row["Emp_Name"].ToString(),
                row["Phone"].ToString(),
                row["Address"].ToString(),
                row["Depart_ID"].ToString())
            { Owner = this };
            if (updateWin.ShowDialog() == true) LoadHRData(btnHRNhanVien.IsChecked == true ? "NhanVien" : "BacSi");
        }

        private void btnHRDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgHRMain.SelectedItem is not DataRowView row) return;
            string id = row["Emp_ID"].ToString();
            if (MessageBox.Show($"Xác nhận xóa {id}?", "Cảnh báo", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    db.Execute("DELETE FROM EMPLOYEE WHERE Emp_ID = @id", new SqlParameter("@id", id));
                    LoadHRData(btnHRNhanVien.IsChecked == true ? "NhanVien" : "BacSi");
                }
                catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
            }
        }

        #endregion

        #region HỆ THỐNG & AVATAR
        private void Avatar_Click(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog { Filter = "Images|*.png;*.jpg;*.jpeg" };
            if (dlg.ShowDialog() == true)
            {
                imgAvatarBrush.ImageSource = new BitmapImage(new Uri(dlg.FileName));
                txtIconUser.Visibility = Visibility.Collapsed;
                db.Execute("UPDATE ACCOUNT SET AvatarPath = @path WHERE User_Name = @u",
                    new SqlParameter("@path", dlg.FileName), new SqlParameter("@u", currentUser));
            }
        }

        private void LoadUserAvatar()
        {
            try
            {
                object pathObj = db.ExecuteScalar("SELECT AvatarPath FROM ACCOUNT WHERE User_Name = @u", new SqlParameter("@u", currentUser));
                if (pathObj != null && File.Exists(pathObj.ToString()))
                {
                    imgAvatarBrush.ImageSource = new BitmapImage(new Uri(pathObj.ToString()));
                    txtIconUser.Visibility = Visibility.Collapsed;
                }
            }
            catch { }
        }

        private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); }
        private void Min_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Max_Click(object sender, RoutedEventArgs e) => WindowState = (WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
        private void Close_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void HRLogOut_Click(object sender, RoutedEventArgs e)
        {
            this.Effect = new System.Windows.Media.Effects.BlurEffect { Radius = 10 };
            if (new LogoutWindow { Owner = this }.ShowDialog() == true) { new LoginWindow().Show(); this.Close(); }
            else this.Effect = null;
        }
        #endregion
    }
}