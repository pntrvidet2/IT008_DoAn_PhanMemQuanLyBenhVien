using System;
using System.Windows;
using DoAn.ClassData;
using Microsoft.Data.SqlClient;

namespace DoAn.Pages
{
    public partial class ResetPassMoiWindow : Window
    {
        private readonly Database dbHelper = new Database();
        private readonly string targetUser;

        public ResetPassMoiWindow(string username)
        {
            InitializeComponent();
            targetUser = username;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            txtError.Text = "";
            string p1 = txtNewPass.Password.Trim();
            string p2 = txtConfirmPass.Password.Trim();

            if (p1.Length < 6)
            {
                txtError.Text = "Lỗi: Mật khẩu phải từ 6 ký tự trở lên!";
                return;
            }

            if (p1 != p2)
            {
                txtError.Text = "Lỗi: Xác nhận mật khẩu không khớp!";
                return;
            }

            try
            {
                string sql = "UPDATE ACCOUNT SET Password = @p, IsFirstLogin = 0 WHERE User_Name = @u";
                SqlParameter[] p = {
                    new SqlParameter("@p", p1),
                    new SqlParameter("@u", targetUser)
                };

                if (dbHelper.Execute(sql, p))
                {
                    MessageBox.Show("Đổi mật khẩu thành công! Hãy đăng nhập lại.", "Thông báo");
                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}