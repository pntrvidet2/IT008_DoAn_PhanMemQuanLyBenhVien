using System;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Windows;
using DoAn.ClassData;
using Microsoft.Data.SqlClient;

namespace DoAn.Pages
{
    public partial class ForgotPassword : Window
    {
        private readonly Database db = new Database();
        private const string FromEmail = "24522065@gm.uit.edu.vn";
        private const string AppPassword = "lkjz ekqn qeov nzuo";

        public ForgotPassword()
        {
            InitializeComponent();
        }

        private void btnVerify_Click(object sender, RoutedEventArgs e)
        {
            string emailUser = txtUsernameEmail.Text.Trim();

            if (string.IsNullOrEmpty(emailUser))
            {
                txtError.Text = "Vui lòng nhập Email tài khoản!";
                return;
            }

            try
            {
                // Vì Username chính là Email, ta chỉ cần tìm theo User_Name
                string sqlCheck = "SELECT Acc_ID FROM ACCOUNT WHERE User_Name = @u";
                DataTable dt = db.GetData(sqlCheck, new SqlParameter("@u", emailUser));

                if (dt != null && dt.Rows.Count > 0)
                {
                    // 1. Tạo mật khẩu tạm 8 ký tự
                    string tempPass = GenerateTempPassword(8);

                    // 2. Cập nhật vào Database
                    bool isUpdated = db.Execute("UPDATE ACCOUNT SET Password = @p WHERE User_Name = @u",
                                    new SqlParameter("@p", tempPass),
                                    new SqlParameter("@u", emailUser));

                    if (isUpdated)
                    {
                        // 3. Gửi Mail về chính Email đó
                        SendResetMail(emailUser, tempPass);

                        MessageBox.Show("Mật khẩu mới đã được gửi về Email của bạn!", "Thành công",
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                        this.Close();
                    }
                }
                else
                {
                    txtError.Text = "Email (Username) này không tồn tại trong hệ thống!";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private static string GenerateTempPassword(int length)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var rnd = new Random();
            char[] buffer = new char[length];
            for (int i = 0; i < length; i++)
                buffer[i] = chars[rnd.Next(chars.Length)];
            return new string(buffer);
        }

        private static void SendResetMail(string toEmail, string tempPass)
        {
            var msg = new MailMessage
            {
                From = new MailAddress(FromEmail, "HOSPITAL MANAGEMENT"),
                Subject = "KHÔI PHỤC MẬT KHẨU TÀI KHOẢN",
                Body = $"Xin chào,\n\nBạn đã yêu cầu cấp lại mật khẩu.\nMật khẩu của bạn là: {tempPass}\n\nVui lòng đăng nhập lại."
            };
            msg.To.Add(toEmail);

            using (var smtp = new SmtpClient("smtp.gmail.com", 587))
            {
                smtp.EnableSsl = true;
                smtp.Credentials = new NetworkCredential(FromEmail, AppPassword);
                smtp.Send(msg);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}