using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Text.RegularExpressions; // Thư viện để kiểm tra định dạng Email
using Microsoft.Data.SqlClient;
using DoAn.ClassData;

namespace DoAn
{
    public partial class RegisterPatientWindow : Window
    {
        private readonly Database db = new Database();

        // Lưu Patient_ID tìm thấy theo CID (CCCD)
        private string _foundPatientId = null;

        private bool _showPass = false;
        private bool _showConfirm = false;
        private bool _syncPass = false;
        private bool _syncConfirm = false;

        public RegisterPatientWindow()
        {
            InitializeComponent();
        }

        // ===== TitleBar: Điều khiển cửa sổ =====
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2) btnMaximize_Click(sender, e);
            else this.DragMove();
        }
        private void btnMinimize_Click(object sender, RoutedEventArgs e) => this.WindowState = WindowState.Minimized;
        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
                btnMax.Content = "▢";
            }
            else
            {
                this.WindowState = WindowState.Maximized;
                btnMax.Content = "❐";
            }
        }
        private void btnExit_Click(object sender, RoutedEventArgs e) => this.Close();

        // ===== Ẩn/Hiện Mật khẩu =====
        private void btnTogglePass_Click(object sender, RoutedEventArgs e)
        {
            _showPass = !_showPass;
            if (_showPass)
            {
                txtRegPassVisible.Text = txtRegPass.Password;
                txtRegPass.Visibility = Visibility.Collapsed;
                txtRegPassVisible.Visibility = Visibility.Visible;
            }
            else
            {
                txtRegPass.Password = txtRegPassVisible.Text;
                txtRegPassVisible.Visibility = Visibility.Collapsed;
                txtRegPass.Visibility = Visibility.Visible;
            }
        }

        private void btnToggleConfirmPass_Click(object sender, RoutedEventArgs e)
        {
            _showConfirm = !_showConfirm;
            if (_showConfirm)
            {
                txtRegConfirmPassVisible.Text = txtRegConfirmPass.Password;
                txtRegConfirmPass.Visibility = Visibility.Collapsed;
                txtRegConfirmPassVisible.Visibility = Visibility.Visible;
            }
            else
            {
                txtRegConfirmPass.Password = txtRegConfirmPassVisible.Text;
                txtRegConfirmPassVisible.Visibility = Visibility.Collapsed;
                txtRegConfirmPass.Visibility = Visibility.Visible;
            }
        }

        private void txtRegPass_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!_showPass || _syncPass) return;
            _syncPass = true;
            txtRegPassVisible.Text = txtRegPass.Password;
            _syncPass = false;
        }

        private void txtRegPassVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_showPass || _syncPass) return;
            _syncPass = true;
            txtRegPass.Password = txtRegPassVisible.Text;
            _syncPass = false;
        }

        private void txtRegConfirmPass_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (!_showConfirm || _syncConfirm) return;
            _syncConfirm = true;
            txtRegConfirmPassVisible.Text = txtRegConfirmPass.Password;
            _syncConfirm = false;
        }

        private void txtRegConfirmPassVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_showConfirm || _syncConfirm) return;
            _syncConfirm = true;
            txtRegConfirmPass.Password = txtRegConfirmPassVisible.Text;
            _syncConfirm = false;
        }

        // ===== Tự động tìm hồ sơ Bệnh nhân theo CCCD (CID) =====
        private void txtRegCID_TextChanged(object sender, TextChangedEventArgs e)
        {
            string cid = txtRegCID.Text.Trim();

            // Reset UI tìm kiếm
            _foundPatientId = null;
            txtFoundPatientId.Text = "";
            txtFoundName.Text = "";
            txtFoundDob.Text = "";
            txtFoundPhone.Text = "";
            txtCIDStatus.Text = "";
            txtCIDStatus.Foreground = Brushes.Red;

            if (cid.Length < 9) return;

            try
            {
                DataTable dt = db.GetData(@"
                    SELECT Patient_ID, Patient_Name, Day_Of_Birth, Phone
                    FROM PATIENT
                    WHERE CID = @cid;", new SqlParameter("@cid", cid));

                if (dt.Rows.Count == 0)
                {
                    txtCIDStatus.Text = "CCCD chưa có trong hồ sơ. Vui lòng liên hệ nhân viên để tạo hồ sơ.";
                    return;
                }

                DataRow r = dt.Rows[0];
                _foundPatientId = r["Patient_ID"].ToString();

                txtFoundPatientId.Text = _foundPatientId;
                txtFoundName.Text = r["Patient_Name"].ToString();
                txtFoundDob.Text = Convert.ToDateTime(r["Day_Of_Birth"]).ToString("dd/MM/yyyy");
                txtFoundPhone.Text = r["Phone"].ToString();

                txtCIDStatus.Text = "✔ Đã tìm thấy hồ sơ bệnh nhân.";
                txtCIDStatus.Foreground = Brushes.Green;
            }
            catch { }
        }

        // ===== Xử lý ĐĂNG KÝ (User_Name = Email) =====
        private void btnDoRegister_Click(object sender, RoutedEventArgs e)
        {
            string userEmail = txtRegUser.Text.Trim();
            string pass = _showPass ? txtRegPassVisible.Text : txtRegPass.Password;
            string confirm = _showConfirm ? txtRegConfirmPassVisible.Text : txtRegConfirmPass.Password;

            // 1. Kiểm tra rỗng
            if (string.IsNullOrWhiteSpace(userEmail) || string.IsNullOrWhiteSpace(pass) || string.IsNullOrWhiteSpace(confirm))
            {
                MessageBox.Show("Vui lòng điền đầy đủ thông tin đăng ký!", "Thông báo");
                return;
            }

            // 2. Ràng buộc Username phải là định dạng Email
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            if (!Regex.IsMatch(userEmail, emailPattern))
            {
                MessageBox.Show("Tên đăng nhập phải là một địa chỉ Email hợp lệ!", "Lỗi định dạng", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 3. Kiểm tra khớp mật khẩu
            if (pass != confirm)
            {
                MessageBox.Show("Mật khẩu xác nhận không trùng khớp!");
                return;
            }

            // 4. Kiểm tra xem đã tìm thấy hồ sơ bệnh nhân chưa
            if (string.IsNullOrWhiteSpace(_foundPatientId))
            {
                MessageBox.Show("Hệ thống chưa tìm thấy hồ sơ bệnh nhân khớp với CCCD này!");
                return;
            }

            try
            {
                using (SqlConnection conn = Database.CreateConnection())
                {
                    conn.Open();

                    // 5. Kiểm tra Email (Username) đã tồn tại trong hệ thống chưa
                    using (SqlCommand cmdCheckUser = new SqlCommand("SELECT 1 FROM ACCOUNT WHERE User_Name=@u", conn))
                    {
                        cmdCheckUser.Parameters.AddWithValue("@u", userEmail);
                        if (cmdCheckUser.ExecuteScalar() != null)
                        {
                            MessageBox.Show("Email này đã được sử dụng để đăng ký tài khoản khác!");
                            return;
                        }
                    }

                    // 6. Kiểm tra Bệnh nhân này đã có tài khoản chưa (tránh 1 bệnh nhân có 2 tài khoản)
                    using (SqlCommand cmdCheckAcc = new SqlCommand("SELECT 1 FROM ACCOUNT WHERE Acc_ID=@aid", conn))
                    {
                        cmdCheckAcc.Parameters.AddWithValue("@aid", _foundPatientId);
                        if (cmdCheckAcc.ExecuteScalar() != null)
                        {
                            MessageBox.Show("Bệnh nhân này đã được cấp tài khoản trước đó!");
                            return;
                        }
                    }

                    // 7. Thực hiện INSERT vào bảng ACCOUNT
                    string sqlInsert = @"
                        INSERT INTO ACCOUNT (Acc_ID, User_Name, Password, Acc_Type, Date_Create_Acc, Acc_Status)
                        VALUES (@aid, @u, @p, 'PT003', GETDATE(), N'Active');";

                    using (SqlCommand cmdIns = new SqlCommand(sqlInsert, conn))
                    {
                        cmdIns.Parameters.AddWithValue("@aid", _foundPatientId);
                        cmdIns.Parameters.AddWithValue("@u", userEmail);
                        cmdIns.Parameters.AddWithValue("@p", pass);
                        cmdIns.ExecuteNonQuery();
                    }

                    MessageBox.Show("Chúc mừng! Bạn đã tạo tài khoản bệnh nhân thành công.", "Thành công");
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi trong quá trình đăng ký: " + ex.Message);
            }
        }
    }
}