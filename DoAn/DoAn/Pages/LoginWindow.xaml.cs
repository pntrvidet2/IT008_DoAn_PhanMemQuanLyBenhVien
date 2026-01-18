using System;
using System.Data;
using System.Windows;
using System.Windows.Input;
using Microsoft.Data.SqlClient;
using DoAn.ClassData;
using DoAn.Pages;

namespace DoAn.Pages
{
    public partial class LoginWindow : Window
    {
        // Kết nối DB
        Database db = new Database();

        // Còn giữ để không lỗi nếu nơi khác vẫn truyền role
        private string _roleId;
        private string _roleName;

        // Constructor mặc định
        public LoginWindow()
        {
            InitializeComponent();
        }

        // Constructor 2 tham số (Cách 3 không dùng nhưng giữ để khỏi lỗi compile)
        public LoginWindow(string roleId, string roleName)
        {
            InitializeComponent();
            _roleId = roleId;
            _roleName = roleName;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            RoleSelectionWindow role = new RoleSelectionWindow();
            role.Show();
            this.Close();
        }

        // ====== CÁCH 3: KHÔNG CHỌN VAI TRÒ, KHÔNG LẤY TÊN ======
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string user = txtUsername.Text.Trim();
            string pass = txtPassword.Password.Trim();

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu!",
                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Lấy Acc_ID + Acc_Type để điều hướng
                // (Nếu không muốn check Active thì bỏ AND Acc_Status = N'Active')
                string sql = @"
SELECT Acc_ID, Acc_Type
FROM ACCOUNT
WHERE User_Name = @user AND Password = @pass AND Acc_Status = N'Active';
";

                SqlParameter[] p =
                {
                    new SqlParameter("@user", user),
                    new SqlParameter("@pass", pass)
                };

                DataTable dt = db.GetData(sql, p);

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("Sai tên đăng nhập hoặc mật khẩu (hoặc tài khoản bị khóa)!",
                        "Lỗi đăng nhập", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string accountID = dt.Rows[0]["Acc_ID"].ToString();
                string roleId = dt.Rows[0]["Acc_Type"].ToString();

               

                // Điều hướng theo Acc_Type lấy từ DB
                switch (roleId)
                {
                    case "EM001": // Nhân viên nhập liệu
                        new MainWindow(accountID).Show();
                        break;

                    case "EC003": // Quản lý nhân sự
                        // Nếu MainWindowHR chỉ có 1 tham số thì sửa lại: new MainWindowHR(accountID).Show();
                        new MainWindowHR(accountID, user).Show();
                        break;

                    case "DC002": // Bác sĩ
                        new MainWindowDoctor(accountID, user).Show();
                        break;

                    case "PT003": // Bệnh nhân
                        new MainWindowPatient(accountID, user).Show();
                        break;

                    case "AD004": // ADMIN
                        new MainWindowAdmin(accountID, user).Show();
                        break;

                    default:
                        MessageBox.Show("Vai trò tài khoản chưa hỗ trợ: " + roleId);
                        break;
                }

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống: " + ex.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Kéo thả cửa sổ
        private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) this.DragMove();
        }

        // Nút điều khiển cửa sổ
        private void BtnMin_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;

        private void BtnMax_Click(object sender, RoutedEventArgs e)
            => this.WindowState = (this.WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        // Quên mật khẩu
        private void txtForgotPassword_Click(object sender, MouseButtonEventArgs e)
        {
            ForgotPassword forgotWindow = new ForgotPassword();
            forgotWindow.Show();
        }
    }
}
