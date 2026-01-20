using System.Windows;

namespace DoAn
{
    public partial class VerificationOTPWindow : Window
    {
      
        private string _correctOTP;

        public bool IsVerified { get; private set; } = false;

        public VerificationOTPWindow(string otpFromServer)
        {
            InitializeComponent();
            _correctOTP = otpFromServer;

           
            txtOTP.Focus();
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            string inputOTP = txtOTP.Text.Trim();

            if (string.IsNullOrEmpty(inputOTP))
            {
                MessageBox.Show("Vui lòng nhập mã OTP đã nhận qua Email!", "Thông báo");
                return;
            }

      
            if (inputOTP == _correctOTP)
            {
                IsVerified = true;
                this.DialogResult = true; 
                this.Close();
            }
            else
            {
                MessageBox.Show("Mã xác thực không chính xác. Vui lòng kiểm tra lại!", "Lỗi xác thực", MessageBoxButton.OK, MessageBoxImage.Error);
                txtOTP.Clear();
                txtOTP.Focus();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            IsVerified = false;
            this.DialogResult = false;
            this.Close();
        }
    }
}