using System;
using System.Data;
using System.Windows;
using System.Windows.Input;
using Microsoft.Data.SqlClient;
using DoAn.ClassData;
using DoAn.Windows; // Thêm nếu ResetPassMoiWindow nằm ở đây

namespace DoAn.Pages
{
    public partial class LoginWindow : Window
    {
        Database db = new Database();
        private string _requiredRole;

        public LoginWindow(string roleId)
        {
            InitializeComponent();
            _requiredRole = roleId;
        }

        #region LOGIC CON MẮT HIỆN/ẨN PASSWORD
      
        private void btnShowPassword_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        
            txtPasswordVisible.Text = txtPassword.Password;

        
            txtPassword.Visibility = Visibility.Collapsed;
            txtPasswordVisible.Visibility = Visibility.Visible;

            btnShowPassword.Content = "🔓";
        }

   
        private void btnShowPassword_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
          
            txtPassword.Visibility = Visibility.Visible;
            txtPasswordVisible.Visibility = Visibility.Collapsed;

            btnShowPassword.Content = "👁";
            txtPassword.Focus(); 
        }

      
        private void btnShowPassword_MouseLeave(object sender, MouseEventArgs e)
        {
            txtPassword.Visibility = Visibility.Visible;
            txtPasswordVisible.Visibility = Visibility.Collapsed;
            btnShowPassword.Content = "👁";
        }
        #endregion

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string user = txtUsername.Text.Trim();
            string pass = txtPassword.Password.Trim();

            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ tên đăng nhập và mật khẩu!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string sql = @"SELECT Acc_ID, Acc_Type, IsFirstLogin 
                               FROM ACCOUNT 
                               WHERE User_Name = @user 
                               AND Password = @pass 
                               AND Acc_Type = @role 
                               AND Acc_Status = N'Active'";

                SqlParameter[] p = {
                    new SqlParameter("@user", user),
                    new SqlParameter("@pass", pass),
                    new SqlParameter("@role", _requiredRole)
                };

                DataTable dt = db.GetData(sql, p);

                if (dt == null || dt.Rows.Count == 0)
                {
                    MessageBox.Show("Tài khoản, mật khẩu không đúng hoặc không khớp với vai trò!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string accountID = dt.Rows[0]["Acc_ID"].ToString();
                string roleId = dt.Rows[0]["Acc_Type"].ToString();
                bool isFirstLogin = Convert.ToBoolean(dt.Rows[0]["IsFirstLogin"]);

                if (isFirstLogin)
                {
                    MessageBox.Show("Đây là lần đầu đăng nhập. Vui lòng đổi mật khẩu!", "Thông báo");
                    ResetPassMoiWindow resetWin = new ResetPassMoiWindow(user);
                    if (resetWin.ShowDialog() == true)
                    {
                        txtPassword.Password = "";
                        txtPassword.Focus();
                    }
                    return;
                }

                switch (roleId)
                {
                    case "EM001": new MainWindow(accountID).Show(); break;
                    case "EC003": new MainWindowHR(accountID, user).Show(); break;
                    case "DC002": new MainWindowDoctor(accountID, user).Show(); break;
                    case "PT003": new MainWindowPatient(accountID, user).Show(); break;
                    case "AD004": new MainWindowAdmin(accountID, user).Show(); break;
                    default: MessageBox.Show("Vai trò không hợp lệ!"); break;
                }
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hệ thống: " + ex.Message);
            }
        }

        #region WINDOW CONTROLS
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            new RoleSelectionWindow().Show();
            this.Close();
        }

        private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) this.DragMove();
        }

        private void BtnMin_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void BtnMax_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = (this.WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
        }

        private void txtForgotPassword_Click(object sender, MouseButtonEventArgs e)
        {
            new ForgotPasswordWindow().Show();
        }
        #endregion
    }
}