using System.Windows;

namespace DoAn.Windows
{
    public partial class LogoutWindow : Window
    {
        public LogoutWindow()
        {
            InitializeComponent();
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}