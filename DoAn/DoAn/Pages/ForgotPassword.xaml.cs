using System;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Windows;
using System.Windows.Threading;
using DoAn.ClassData;
using Microsoft.Data.SqlClient;

namespace DoAn.Pages
{
    public partial class ForgotPasswordWindow : Window
    {
        private readonly Database db = new Database();
        private const string FromEmail = "24522065@gm.uit.edu.vn";
        private const string AppPassword = "lkjz ekqn qeov nzuo";
        private string _otpCode;
        private DispatcherTimer _timer;
        private int _secondsLeft;

        public ForgotPasswordWindow() { InitializeComponent(); }

        private void btnSendOTP_Click(object sender, RoutedEventArgs e)
        {
            string emailUser = txtUsernameEmail.Text.Trim();
            if (string.IsNullOrEmpty(emailUser)) { txtError.Text = "Vui lòng nhập email!"; return; }

            // Kiểm tra email tồn tại trong DB
            DataTable dt = db.GetData("SELECT Acc_ID FROM ACCOUNT WHERE User_Name = @u", new SqlParameter("@u", emailUser));
            if (dt != null && dt.Rows.Count > 0)
            {
                _otpCode = new Random().Next(100000, 999999).ToString();
                try
                {
                    SendOTPMail(emailUser, _otpCode);
                    MessageBox.Show("Mã OTP đã được gửi đến email của bạn!");

                    _secondsLeft = 120; // 2 phút
                    if (_timer != null) _timer.Stop();

                    _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
                    _timer.Tick += (s, ev) => {
                        _secondsLeft--;
                        txtTimer.Text = $"Mã hiệu lực: {_secondsLeft / 60:D2}:{_secondsLeft % 60:D2}";
                        if (_secondsLeft <= 0)
                        {
                            _timer.Stop();
                            _otpCode = null;
                            txtTimer.Text = "Mã hết hạn!";
                            btnResend.Visibility = Visibility.Visible;
                        }
                    };
                    _timer.Start();
                    btnSendOTP.IsEnabled = false;
                    btnResend.Visibility = Visibility.Collapsed;
                    txtError.Text = "";
                }
                catch (Exception ex) { MessageBox.Show("Lỗi gửi mail: " + ex.Message); }
            }
            else { txtError.Text = "Email này không tồn tại trong hệ thống!"; }
        }

        private void btnVerify_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_otpCode) && txtOTP.Text.Trim() == _otpCode)
            {
                if (_timer != null) _timer.Stop();
               
                ResetPassMoiWindow resetWin = new ResetPassMoiWindow(txtUsernameEmail.Text.Trim());
                resetWin.Show();
                this.Close();
            }
            else { txtError.Text = "Mã OTP sai hoặc đã hết hạn!"; }
        }

        private void SendOTPMail(string toEmail, string otp)
        {
            var msg = new MailMessage
            {
                From = new MailAddress(FromEmail, "HỆ THỐNG QUẢN LÝ BỆNH VIỆN"),
                Subject = "MÃ XÁC THỰC OTP QUÊN MẬT KHẨU",
                Body = $"Chào bạn, mã OTP của bạn là: {otp}. Mã có hiệu lực trong 2 phút."
            };
            msg.To.Add(toEmail);
            using (var smtp = new SmtpClient("smtp.gmail.com", 587))
            {
                smtp.EnableSsl = true;
                smtp.Credentials = new NetworkCredential(FromEmail, AppPassword);
                smtp.Send(msg);
            }
        }

        private void btnResend_Click(object sender, RoutedEventArgs e) => btnSendOTP_Click(null, null);
        private void btnCancel_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}