using DoAn.ClassData;
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
        private string currentMode = "Home";

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
            currentMode = mode;
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
                // Hiện nút Thêm/Sửa/Xóa khi ở tab Tài khoản (Staff)
                pnlAccountActions.Visibility = (mode == "Staff") ? Visibility.Visible : Visibility.Collapsed;
                LoadDataList(mode, "");
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

                // Lấy tổng tiền từ bảng BILL (Cột Total)
                object revenue = db.ExecuteScalar("SELECT SUM(Total) FROM BILL");
                txtCountRevenue.Text = revenue != DBNull.Value ? string.Format("{0:N0} VNĐ", revenue) : "0 VNĐ";

                // Hiển thị 10 hóa đơn mới nhất (Sửa cột Date)
                DataTable dt = db.GetData("SELECT TOP 10 Bill_ID, Patient_ID, Total, Date FROM BILL ORDER BY Date DESC");
                if (dt != null) dgRecentBills.ItemsSource = dt.DefaultView;
            }
            catch { }
        }

        private void LoadDataList(string mode, string keyword)
        {
            string sql = mode switch
            {
                // ACCOUNT: User_Name, Acc_Type, Acc_Status
                "Staff" => $"SELECT Acc_ID as [Mã TK], User_Name as [Tài khoản], Acc_Type as [Quyền], Acc_Status as [Trạng thái] " +
                           $"FROM ACCOUNT WHERE User_Name LIKE N'%{keyword}%' OR Acc_ID LIKE N'%{keyword}%'",

                // PATIENT: Patient_ID, Patient_Name
                "Patient" => $"SELECT Patient_ID as [Mã BN], Patient_Name as [Họ tên], Gender as [GT], Phone as [SĐT], Address as [Địa chỉ] " +
                             $"FROM PATIENT WHERE Patient_Name LIKE N'%{keyword}%' OR Phone LIKE N'%{keyword}%'",

                // BILL: Khớp hoàn toàn với bảng BILL bạn vừa gửi
                "Bill" => $"SELECT Bill_ID as [Mã HĐ], Patient_ID as [Mã BN], Emp_ID as [Mã NV], Date as [Ngày lập], Total as [Tổng tiền], Payment_Method as [Thanh toán], Payment_Status as [Trạng thái] " +
                          $"FROM BILL WHERE Bill_ID LIKE N'%{keyword}%' OR Patient_ID LIKE N'%{keyword}%' OR Emp_ID LIKE N'%{keyword}%'",

                _ => ""
            };

            if (!string.IsNullOrEmpty(sql))
            {
                DataTable dt = db.GetData(sql);
                if (dt != null) dgAdminMain.ItemsSource = dt.DefaultView;
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e) => LoadDashboardStats();
        #endregion

        #region THÊM - SỬA - XÓA
        private void btnAdminAddNew_Click(object sender, RoutedEventArgs e)
        {
            if (currentMode == "Staff")
            {
                var win = new AddStaffWindow { Owner = this };
                if (win.ShowDialog() == true) LoadDataList("Staff", "");
            }
        }

        private void btnAdminUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (dgAdminMain.SelectedItem is DataRowView row)
            {
                if (currentMode == "Staff")
                {
                    var win = new UpdateStaffWindow(row) { Owner = this };
                    if (win.ShowDialog() == true) LoadDataList("Staff", "");
                }
            }
            else MessageBox.Show("Vui lòng chọn một dòng để thực hiện!");
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgAdminMain.SelectedItem is DataRowView row)
            {
                string id = row[0].ToString();
                if (MessageBox.Show($"Xác nhận xóa {id}?", "Cảnh báo", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    string table = currentMode switch { "Staff" => "ACCOUNT", "Patient" => "PATIENT", "Bill" => "BILL", _ => "" };
                    string col = currentMode switch { "Staff" => "Acc_ID", "Patient" => "Patient_ID", "Bill" => "Bill_ID", _ => "" };

                    if (!string.IsNullOrEmpty(table))
                    {
                        db.Execute($"DELETE FROM {table} WHERE {col} = @id", new SqlParameter("@id", id));
                        LoadDataList(currentMode, "");
                        LoadDashboardStats();
                    }
                }
            }
        }
        #endregion

        #region AVATAR & XUẤT FILE
        private void LoadAdminAvatar()
        {
            try
            {
                object path = db.ExecuteScalar("SELECT AvatarPath FROM ACCOUNT WHERE Acc_ID = @id", new SqlParameter("@id", currentAdminId));
                if (path != null && File.Exists(path.ToString()))
                    imgAdminAvatar.ImageSource = new BitmapImage(new Uri(path.ToString()));
            }
            catch { }
        }

        private void btnExportReport_Click(object sender, RoutedEventArgs e)
        {
            if (dgAdminMain.ItemsSource == null) return;
            try
            {
                DataTable dt = ((DataView)dgAdminMain.ItemsSource).ToTable();
                SaveFileDialog sfd = new SaveFileDialog { Filter = "CSV file (*.csv)|*.csv", FileName = $"BaoCao_{currentMode}.csv" };

                if (sfd.ShowDialog() == true)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append('\uFEFF');
                    for (int i = 0; i < dt.Columns.Count; i++) sb.Append(dt.Columns[i].ColumnName + (i == dt.Columns.Count - 1 ? "" : ","));
                    sb.AppendLine();
                    foreach (DataRow r in dt.Rows)
                    {
                        for (int i = 0; i < dt.Columns.Count; i++) sb.Append("\"" + r[i].ToString() + "\"" + (i == dt.Columns.Count - 1 ? "" : ","));
                        sb.AppendLine();
                    }
                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Xuất file thành công!");
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }
        #endregion

        private void btnAdminSearch_Click(object sender, RoutedEventArgs e)
        {
            string keyword = txtAdminSearch.Text.Trim();
            if (keyword == "Tìm kiếm...") keyword = "";
            LoadDataList(currentMode, keyword);
        }

        private void AdminLogOut_Click(object sender, RoutedEventArgs e)
        {
            this.Effect = new System.Windows.Media.Effects.BlurEffect { Radius = 10 };
            if (new LogoutWindow { Owner = this }.ShowDialog() == true)
            {
                new RoleSelectionWindow().Show();
                this.Close();
            }
            else this.Effect = null;
        }
    }
}