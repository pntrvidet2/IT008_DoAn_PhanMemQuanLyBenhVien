using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Net;
using System.Net.Mail;
using DoAn.ClassData;
using System.Text.RegularExpressions;
using DoAn.Pages;

namespace DoAn
{
    public partial class RegisterPatientWindow : Window
    {
        Database db = new Database();
        private string _otpCode = "";
        private bool _isEmailVerified = false;

        public RegisterPatientWindow() { InitializeComponent(); }

        #region 1. CHECK CCCD & CHẶN NẾU ĐÃ CÓ TÀI KHOẢN
        private void txtRegCID_TextChanged(object sender, TextChangedEventArgs e)
        {
            string cid = txtRegCID.Text.Trim();
            if (cid.Length >= 9)
            {
                // Bước A: Kiểm tra xem CCCD này đã được ai đăng ký Account chưa
                string sqlCheck = $"SELECT Acc_ID FROM ACCOUNT WHERE Acc_ID = (SELECT Patient_ID FROM PATIENT WHERE CID = '{cid}')";
                DataTable dtCheck = db.GetData(sqlCheck);

                if (dtCheck != null && dtCheck.Rows.Count > 0)
                {
                    txtCIDStatus.Text = "✕ Hồ sơ này đã có tài khoản rồi!";
                    txtCIDStatus.Foreground = System.Windows.Media.Brushes.Red;
                    ResetProfileUI();
                    return;
                }

                // Bước B: Nếu chưa có thì mới lấy thông tin hồ sơ
                string sqlInfo = $"SELECT Patient_ID, Patient_Name, Phone FROM PATIENT WHERE CID = '{cid}'";
                DataTable dt = db.GetData(sqlInfo);
                if (dt != null && dt.Rows.Count > 0)
                {
                    txtFoundPatientId.Text = dt.Rows[0]["Patient_ID"].ToString();
                    txtFoundName.Text = dt.Rows[0]["Patient_Name"].ToString();
                    txtFoundPhone.Text = dt.Rows[0]["Phone"].ToString();
                    txtFoundDob.Text = "Hồ sơ sẵn sàng";

                    txtGuide.Visibility = Visibility.Collapsed;
                    gridProfile.Opacity = 1;
                    brdProfile.Background = System.Windows.Media.Brushes.White;
                    txtCIDStatus.Text = "✓ Hồ sơ hợp lệ, mời bạn tiếp tục.";
                    txtCIDStatus.Foreground = System.Windows.Media.Brushes.Green;
                }
                else { ResetProfileUI(); txtCIDStatus.Text = "✕ Không tìm thấy hồ sơ bệnh nhân."; }
            }
            else { ResetProfileUI(); }
        }
        #endregion

        #region 2. GỬI OTP & CHẶN NẾU EMAIL ĐÃ DÙNG
        private void btnSendOTP_Click(object sender, RoutedEventArgs e)
        {
            string email = txtRegUser.Text.Trim();
            if (string.IsNullOrEmpty(email) || !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                MessageBox.Show("Email không hợp lệ!"); return;
            }

            // CHẶN NGAY: Kiểm tra Email đã tồn tại trong DB chưa
            string sqlCheckEmail = $"SELECT * FROM ACCOUNT WHERE User_Name = '{email}'";
            DataTable dtEmail = db.GetData(sqlCheckEmail);
            if (dtEmail != null && dtEmail.Rows.Count > 0)
            {
                MessageBox.Show("Email này đã được sử dụng cho một tài khoản khác!", "Lỗi");
                return;
            }

            _otpCode = new Random().Next(100000, 999999).ToString();
            try
            {
                SendOTPMail(email, _otpCode);
                VerificationOTPWindow verifyWin = new VerificationOTPWindow(_otpCode) { Owner = this };
                if (verifyWin.ShowDialog() == true)
                {
                    _isEmailVerified = true;
                    btnSendOTP.Content = "✓ XÁC THỰC XONG";
                    btnSendOTP.Background = System.Windows.Media.Brushes.Green;
                    btnSendOTP.IsEnabled = false;
                    txtRegUser.IsReadOnly = true;
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi gửi mail: " + ex.Message); }
        }
        #endregion

        #region 3. FIX CON MẮT (ẨN/HIỆN MẬT KHẨU)
        private void btnTogglePass_Click(object sender, RoutedEventArgs e)
        {
            if (txtRegPass.Visibility == Visibility.Visible)
            {
                // Hiện mật khẩu
                txtRegPassVisible.Text = txtRegPass.Password;
                txtRegPass.Visibility = Visibility.Collapsed;
                txtRegPassVisible.Visibility = Visibility.Visible;
                (sender as Button).Content = "🙈"; // Đổi icon
            }
            else
            {
                // Ẩn mật khẩu
                txtRegPass.Password = txtRegPassVisible.Text;
                txtRegPassVisible.Visibility = Visibility.Collapsed;
                txtRegPass.Visibility = Visibility.Visible;
                (sender as Button).Content = "👁";
            }
        }
        #endregion

        private void btnDoRegister_Click(object sender, RoutedEventArgs e)
        {
            string pass = txtRegPass.Visibility == Visibility.Visible ? txtRegPass.Password : txtRegPassVisible.Text;
            if (!_isEmailVerified) { MessageBox.Show("Cần xác thực Email trước!"); return; }
            if (pass != txtRegConfirmPass.Password) { MessageBox.Show("Mật khẩu không khớp!"); return; }

            string sql = $"INSERT INTO ACCOUNT (Acc_ID, User_Name, Password, Acc_Type, Acc_Status, IsFirstLogin) " +
                         $"VALUES ('{txtFoundPatientId.Text}', '{txtRegUser.Text}', '{pass}', 'PT003', 'Active', 0)";

            if (db.Execute(sql))
            {
                MessageBox.Show("Đăng ký thành công!");
                new LoginWindow("PT003").Show();
                this.Close();
            }
        }

        private void ResetProfileUI()
        {
            txtFoundName.Text = ""; txtFoundPatientId.Text = ""; txtFoundPhone.Text = "";
            txtGuide.Visibility = Visibility.Visible; gridProfile.Opacity = 0.1;
        }

        private void SendOTPMail(string toEmail, string otp)
        {
            var msg = new MailMessage { From = new MailAddress("24522065@gm.uit.edu.vn"), Subject = "HỆ THỐNG QUẢN LÝ BỆNH VIỆN", Body = "Mã OTP của bạn là: " + otp + "\nVui lòng không cung cấp mã cho người khác."};
            msg.To.Add(toEmail);
            using (var smtp = new SmtpClient("smtp.gmail.com", 587))
            {
                smtp.EnableSsl = true; smtp.Credentials = new NetworkCredential("24522065@gm.uit.edu.vn", "lkjz ekqn qeov nzuo");
                smtp.Send(msg);
            }
        }

        private void btnExit_Click(object sender, RoutedEventArgs e) => this.Close();
        private void btnMinimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();
    }
}