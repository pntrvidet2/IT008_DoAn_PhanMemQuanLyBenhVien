using System;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace DoAn.Pages
{
    public partial class PaymentQRWindow : Window
    {
        private string _billId;
        private decimal _amount;
        private string _description;
        private DispatcherTimer _timer;
        private int _seconds = 0;

        public PaymentQRWindow(string billId, decimal amount)
        {
            InitializeComponent();
            _billId = billId;
            _amount = amount;
            _description = $"Thanh toan hoa don {_billId}";

            txtAmountDisplay.Text = string.Format("{0:N0} VNĐ", _amount);
            txtBillDesc.Text = _description;

            UpdateQR();
            StartAutoCheck();
        }

        private void StartAutoCheck()
        {
            // Giả lập hệ thống kiểm tra ngân hàng tự động mỗi giây
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (s, e) => {
                _seconds++;
                txtStatus.Text = $"Đang xác thực giao dịch... ({_seconds}s)";

                // Sau 8 giây thì tự động báo thành công để demo
                if (_seconds == 8)
                {
                    _timer.Stop();
                    txtStatus.Text = "Thanh toán thành công!";
                    txtStatus.Foreground = System.Windows.Media.Brushes.Green;
                    MessageBox.Show("Hệ thống đã nhận được tiền. Cảm ơn bạn!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                    this.Close();
                }
            };
            _timer.Start();
        }

        private void UpdateQR()
        {
            string url = "";
            string descEncoded = Uri.EscapeDataString(_description);

            if (rbMomo.IsChecked == true)
            {
                // Link QR MoMo cá nhân (Thay 0562318690 bằng SĐT MoMo thật của bà nếu cần)
                url = $"https://api.qrserver.com/v1/create-qr-code/?size=250x250&data=2|99|0562318690|||0|0|{_amount}|{descEncoded}";
            }
            else
            {
                // Link VietQR chuẩn ngân hàng BIDV
                url = $"https://img.vietqr.io/image/BIDV-8863934515-compact2.png?amount={_amount}&addInfo={descEncoded}";
            }

            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(url);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                imgQR.Source = bitmap;
            }
            catch { }
        }

        private void PayMethod_Changed(object sender, RoutedEventArgs e)
        {
            if (this.IsLoaded) UpdateQR();
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (_timer != null) _timer.Stop();
            this.DialogResult = true;
            this.Close();
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (_timer != null) _timer.Stop();
            this.Close();
        }
    }
}