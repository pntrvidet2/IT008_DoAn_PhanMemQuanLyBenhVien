using DoAn.ClassData;
using DoAn.Pages;
using DoAn.Windows;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using System;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace DoAn
{
    public partial class MainWindowAdmin : Window
    {
        private readonly Database db = new Database();
        private string currentAdminId;

        public MainWindowAdmin(string adminName, string id)
        {
            InitializeComponent();
            currentAdminId = id;
            txtAdminUserName.Text = adminName;

            this.Loaded += (s, e) => {
                btnAdminHome.IsChecked = true;
                SetMode("Home");
                LoadAdminAvatar();
            };
        }

        #region HỆ THỐNG
        private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
        private void Min_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Close_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        private void Max_Click(object sender, RoutedEventArgs e) => WindowState = (WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;

        private void SetMode(string mode)
        {
            if (mode == "Home")
            {
                gridAdminDashboard.Visibility = Visibility.Visible;
                gridAdminData.Visibility = Visibility.Collapsed;
                LoadDashboardStats();
            }
            else
            {
                gridAdminDashboard.Visibility = Visibility.Collapsed;
                gridAdminData.Visibility = Visibility.Visible;
                // Chỉ hiện nút Sửa/Xóa/Thêm khi ở tab Tài khoản (Staff)
                pnlAccountActions.Visibility = (mode == "Staff") ? Visibility.Visible : Visibility.Collapsed;
                LoadDataList(mode);
            }
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not ToggleButton clicked) return;
            btnAdminHome.IsChecked = (clicked == btnAdminHome);
            btnAdminNhanVien.IsChecked = (clicked == btnAdminNhanVien);
            btnAdminBenhAn.IsChecked = (clicked == btnAdminBenhAn);
            btnAdminHoaDon.IsChecked = (clicked == btnAdminHoaDon);

            if (clicked == btnAdminHome) SetMode("Home");
            else if (clicked == btnAdminNhanVien) SetMode("Staff");
            else if (clicked == btnAdminBenhAn) SetMode("Patient");
            else if (clicked == btnAdminHoaDon) SetMode("Bill");
        }
        #endregion

        #region DỮ LIỆU & DASHBOARD
        private void LoadDashboardStats()
        {
            try
            {
                txtCountStaff.Text = db.ExecuteScalar("SELECT COUNT(*) FROM ACCOUNT").ToString();
                txtCountBills.Text = db.ExecuteScalar("SELECT COUNT(*) FROM BILL").ToString();
                txtCountRecords.Text = db.ExecuteScalar("SELECT COUNT(*) FROM PATIENT").ToString();
                object revenue = db.ExecuteScalar("SELECT SUM(Total) FROM BILL");
                txtCountRevenue.Text = revenue != DBNull.Value ? string.Format("{0:N0} VNĐ", revenue) : "0 VNĐ";
                dgRecentBills.ItemsSource = db.GetData("SELECT TOP 10 * FROM BILL ORDER BY Date DESC").DefaultView;
            }
            catch { }
        }

        private void LoadDataList(string mode)
        {
            string sql = mode switch
            {
                "Staff" => "SELECT * FROM ACCOUNT",
                "Patient" => "SELECT * FROM PATIENT",
                "Bill" => "SELECT * FROM BILL",
                _ => ""
            };
            if (!string.IsNullOrEmpty(sql)) dgAdminMain.ItemsSource = db.GetData(sql).DefaultView;
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e) => LoadDashboardStats();
        #endregion

        #region THÊM - XÓA - SỬA (CHỈ CHO ACCOUNT)
        private void btnAdminAddNew_Click(object sender, RoutedEventArgs e)
        {
            var win = new Windows.AddStaffWindow { Owner = this };
            if (win.ShowDialog() == true) LoadDataList("Staff");
        }

        private void btnAdminUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (dgAdminMain.SelectedItem is DataRowView row)
            {
                var win = new Windows.UpdateStaffWindow(row) { Owner = this };
                if (win.ShowDialog() == true) LoadDataList("Staff");
            }
            else MessageBox.Show("Chọn một tài khoản!");
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgAdminMain.SelectedItem is DataRowView row)
            {
                string user = row["User_Name"].ToString();
                if (MessageBox.Show($"Xóa tài khoản {user}?", "Cảnh báo", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    db.Execute("DELETE FROM ACCOUNT WHERE User_Name = @u", new SqlParameter("@u", user));
                    LoadDataList("Staff");
                    LoadDashboardStats();
                }
            }
        }
        #endregion

        #region AVATAR & XUẤT BÁO CÁO
        private void Avatar_Click(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog { Filter = "Image files (*.png;*.jpg)|*.png;*.jpg" };
            if (dlg.ShowDialog() == true)
            {
                imgAdminAvatar.ImageSource = new BitmapImage(new Uri(dlg.FileName));
                db.Execute("UPDATE ACCOUNT SET AvatarPath = @path WHERE User_Name = @id",
                    new SqlParameter("@path", dlg.FileName), new SqlParameter("@id", currentAdminId));
            }
        }

        private void LoadAdminAvatar()
        {
            try
            {
                object path = db.ExecuteScalar("SELECT AvatarPath FROM ACCOUNT WHERE User_Name = @id", new SqlParameter("@id", currentAdminId));
                if (path != null && File.Exists(path.ToString()))
                    imgAdminAvatar.ImageSource = new BitmapImage(new Uri(path.ToString()));
            }
            catch { }
        }

        private void btnExportReport_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable dt = db.GetData("SELECT * FROM BILL WHERE MONTH(Date) = MONTH(GETDATE()) AND YEAR(Date) = YEAR(GETDATE())");
                if (dt.Rows.Count == 0) { MessageBox.Show("Tháng này chưa có hóa đơn!"); return; }

                SaveFileDialog sfd = new SaveFileDialog { Filter = "CSV file (*.csv)|*.csv", FileName = $"BaoCao_Thang{DateTime.Now.Month}.csv" };
                if (sfd.ShowDialog() == true)
                {
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < dt.Columns.Count; i++) sb.Append(dt.Columns[i].ColumnName + (i == dt.Columns.Count - 1 ? "" : ","));
                    sb.AppendLine();
                    foreach (DataRow r in dt.Rows)
                    {
                        for (int i = 0; i < dt.Columns.Count; i++) sb.Append(r[i].ToString().Replace(",", " ") + (i == dt.Columns.Count - 1 ? "" : ","));
                        sb.AppendLine();
                    }
                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Xuất báo cáo thành công!");
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }
        #endregion

        private void btnAdminSearch_Click(object sender, RoutedEventArgs e) { /* Logic search */ }
        private void AdminLogOut_Click(object sender, RoutedEventArgs e)
        {
            this.Effect = new System.Windows.Media.Effects.BlurEffect { Radius = 10 };
            var logoutWin = new LogoutWindow { Owner = this };
            if (logoutWin.ShowDialog() == true)
            {
                new LoginWindow().Show();
                this.Close();
            }
            else this.Effect = null;
        }
    }
}