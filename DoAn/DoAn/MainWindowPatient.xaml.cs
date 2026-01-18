using DoAn.ClassData;
using DoAn.Pages;
using DoAn.Windows;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Data.SqlClient;

namespace DoAn
{
    public partial class MainWindowPatient : Window
    {
        private readonly Database db = new Database();
        private string patientID;
        private string patientName;
        private DispatcherTimer timer;

        public MainWindowPatient(string id, string name)
        {
            InitializeComponent();
            this.patientID = id;
            this.patientName = name;

            txtPatientName.Text = name;

            StartClock();
            this.Loaded += (s, e) => {
                btnHome.IsChecked = true;
                SetMode("Home");
            };
        }

        private void StartClock()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) => txtClock.Text = DateTime.Now.ToString("HH:mm:ss");
            timer.Start();
        }

        private void SetMode(string mode)
        {
            if (mode == "Home")
            {
                brdDashboard.Visibility = Visibility.Visible;
                brdFrameContainer.Visibility = Visibility.Collapsed;
                LoadDashboardStats();
            }
            else
            {
                brdDashboard.Visibility = Visibility.Collapsed;
                brdFrameContainer.Visibility = Visibility.Visible;
            }
        }

        private void LoadDashboardStats()
        {
            try
            {
                // 1. Đếm hóa đơn chưa thanh toán
                string sqlBill = "SELECT COUNT(*) FROM BILL WHERE Patient_ID = @id AND Payment_Status = N'Chưa thanh toán'";
                txtStatBill.Text = db.ExecuteScalar(sqlBill, new SqlParameter("@id", patientID)).ToString();

                // 2. Lấy tình trạng sức khỏe từ bảng Patient
                string sqlInfo = "SELECT Curr_Condition FROM PATIENT WHERE Patient_ID = @id";
                object cond = db.ExecuteScalar(sqlInfo, new SqlParameter("@id", patientID));
                txtStatCondition.Text = cond != null ? cond.ToString() : "Ổn định";
            }
            catch { }
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not ToggleButton btn) return;

            btnHome.IsChecked = (btn == btnHome);
            btnBilling.IsChecked = (btn == btnBilling);
            btnInfo.IsChecked = (btn == btnInfo);

            if (btn == btnHome)
            {
                SetMode("Home");
            }
            else if (btn == btnBilling)
            {
                SetMode("Billing");
                MainFrame.Navigate(new PagePatientBilling(patientID));
            }
            else if (btn == btnInfo)
            {
                SetMode("Info");
                MainFrame.Navigate(new PagePatientProfile(patientID));
            }
        }

        #region Window Controls
        private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        private void Min_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Max_Click(object sender, RoutedEventArgs e) => WindowState = (WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;
        private void Close_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void btnLogOut_Click(object sender, RoutedEventArgs e)
        {
            this.Effect = new System.Windows.Media.Effects.BlurEffect { Radius = 10 };
            if (new LogoutWindow { Owner = this }.ShowDialog() == true)
            {
                new LoginWindow().Show();
                this.Close();
            }
            else this.Effect = null;
        }
        #endregion

        // Nếu bạn chưa có hàm xử lý ảnh đại diện, có thể giữ nguyên hoặc xóa Avatar_Click
        private void Avatar_Click(object sender, MouseButtonEventArgs e) { }
    }
}