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

            if (rbDataEntry.IsChecked == true) selectedRoleId = "EM001";
            else if (rbDoctor.IsChecked == true) selectedRoleId = "DC002";
            else if (rbPatient.IsChecked == true) selectedRoleId = "PT003";
            else if (rbHR.IsChecked == true) selectedRoleId = "EC003";
            else if (rbAdmin.IsChecked == true) selectedRoleId = "AD004";

            if (string.IsNullOrEmpty(selectedRoleId))
            {
                MessageBox.Show("Vui lòng chọn một vai trò!");
                return;
            }

            if (selectedRoleId == "PT003")
            {
                new CheckAccountWindow().Show();
            
                return;
            }

            // Truyền cái ID vừa chọn sang LoginWindow
            LoginWindow login = new LoginWindow(selectedRoleId );
            login.Show();
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ToggleMaximize();
                return;
            }
            try { DragMove(); } catch { }
        }

        private void BtnMinimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void BtnMaximize_Click(object sender, RoutedEventArgs e) => ToggleMaximize();

        private void ToggleMaximize()
        {
            WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
            UpdateMaxButtonGlyph();
        }

        private void UpdateMaxButtonGlyph()
        {
            if (BtnMax != null)
                BtnMax.Content = (WindowState == WindowState.Maximized) ? "❐" : "□";
        }
    }
}