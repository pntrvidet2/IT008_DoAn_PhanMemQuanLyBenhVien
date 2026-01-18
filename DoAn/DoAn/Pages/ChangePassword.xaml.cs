using System;
using System.Windows;
using DoAn.ClassData;
using Microsoft.Data.SqlClient;

namespace DoAn.Pages
{
    public partial class ChangePassword : Window
    {
        private readonly Database db = new Database();
        private readonly string _username;

        // Truyền username vào để update đúng tài khoản
        public ChangePassword(string username)
        {
            InitializeComponent();
            _username = username;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            string newPass = txtNewPass.Password.Trim();
            string confirmPass = txtConfirmPass.Password.Trim();

            // Validate
            if (string.IsNullOrWhiteSpace(newPass) || string.IsNullOrWhiteSpace(confirmPass))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ mật khẩu mới và xác nhận.", "Thiếu thông tin",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (newPass.Length < 6)
            {
                MessageBox.Show("Mật khẩu phải có ít nhất 6 ký tự.", "Mật khẩu yếu",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!newPass.Equals(confirmPass))
            {
                MessageBox.Show("Xác nhận mật khẩu không khớp.", "Sai xác nhận",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                string sql = @"
                    UPDATE ACCOUNT
                    SET Password = @newPass,
                        IsFirstLogin = 0
                    WHERE User_Name = @username;
                ";

                bool ok = db.Execute(sql,
                    new SqlParameter("@newPass", newPass),
                    new SqlParameter("@username", _username));

                if (ok)
                {
                    MessageBox.Show("Đổi mật khẩu thành công! Vui lòng đăng nhập lại.", "Thành công",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    Close();
                }
                else
                {
                    MessageBox.Show("Không cập nhật được mật khẩu (không tìm thấy tài khoản).", "Lỗi",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Có lỗi xảy ra: " + ex.Message, "Lỗi",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
