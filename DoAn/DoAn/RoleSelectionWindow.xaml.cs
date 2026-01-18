using DoAn.Pages;
using System.Windows;
using System.Windows.Input;

namespace DoAn
{
    public partial class RoleSelectionWindow : Window
    {
        public RoleSelectionWindow()
        {
            InitializeComponent();
        }

        private void BtnContinue_Click(object sender, RoutedEventArgs e)
        {
            string selectedRoleId = "";
            string selectedRoleName = "";

            // Kiểm tra RadioButton nào được chọn
            if (rbDataEntry.IsChecked == true) { selectedRoleId = "EM001"; selectedRoleName = "Nhân viên nhập liệu"; }
            else if (rbDoctor.IsChecked == true) { selectedRoleId = "DC002"; selectedRoleName = "Bác sĩ"; }
            else if (rbPatient.IsChecked == true) { selectedRoleId = "PT003"; selectedRoleName = "Bệnh nhân"; }
            else if (rbHR.IsChecked == true) { selectedRoleId = "EC003"; selectedRoleName = "Quản lý nhân sự"; }
            else if (rbAdmin.IsChecked == true) { selectedRoleId = "AD004"; selectedRoleName = "ADMIN"; }

            if (string.IsNullOrEmpty(selectedRoleId))
            {
                MessageBox.Show("Vui lòng chọn một vai trò để tiếp tục!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (rbPatient.IsChecked == true)
            {
                CheckAccountWindow check = new CheckAccountWindow();
                check.Show();
                return;
            }

                // Mở LoginWindow và truyền thông tin vai trò sang
            LoginWindow login = new LoginWindow(selectedRoleId, selectedRoleName);
            login.Show();
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        
        private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                // Double click → phóng to / thu về
                if (WindowState == WindowState.Maximized)
                    WindowState = WindowState.Normal;
                else
                    WindowState = WindowState.Maximized;
                return;
            }

            try
            {
                DragMove();
            }
            catch { }
        }
        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        // Nút phóng to / thu về
        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            ToggleMaximize();
        }

        private void ToggleMaximize()
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;

            UpdateMaxButtonGlyph();
        }

        private void UpdateMaxButtonGlyph()
        {
            // Đổi icon chữ cho nút maximize (tuỳ thích)
            if (BtnMax == null) return;

            BtnMax.Content = (WindowState == WindowState.Maximized) ? "❐" : "□";
        }

    }
}
     