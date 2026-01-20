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

        public void LoadDashboardStats()
        {
            try
            {
                string sqlBill = "SELECT COUNT(*) FROM BILL WHERE Patient_ID = @id AND Payment_Status = 'Unpaid'";
                
                txtStatBill.Text = db.ExecuteScalar(sqlBill, new SqlParameter("@id", patientID)).ToString();

                string sqlInfo = "SELECT Curr_Condition FROM PATIENT WHERE Patient_ID = @id";
                object cond = db.ExecuteScalar(sqlInfo, new SqlParameter("@id", patientID));
                txtStatCondition.Text = cond != null ? cond.ToString() : "Ổn định";

                LoadAppointments();
            }
            catch { }
        }

        private void LoadAppointments()
        {
            try
            {
                // Truy vấn lấy lịch hẹn kèm tên bác sĩ
                string sql = @"SELECT A.App_Date, E.Emp_Name as DoctorName, A.App_Note, A.Status 
                               FROM APPOINTMENT A 
                               JOIN EMPLOYEE E ON A.Doctor_ID = E.Emp_ID 
                               WHERE A.Patient_ID = @id 
                               ORDER BY A.App_Date DESC";

                DataTable dt = db.GetData(sql, new SqlParameter("@id", patientID));
                if (dt != null)
                {
                    dgAppointments.ItemsSource = dt.DefaultView;
                }
            }
            catch { }
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not ToggleButton btn) return;

            btnHome.IsChecked = (btn == btnHome);
            btnBilling.IsChecked = (btn == btnBilling);
            btnInfo.IsChecked = (btn == btnInfo);

            if (btn == btnHome) SetMode("Home");
            else if (btn == btnBilling)
            {
                SetMode("Billing");
                // Đảm bảo bà đã tạo file PagePatientBilling.xaml trong folder Pages
                MainFrame.Navigate(new PagePatientBilling(patientID));
            }
            else if (btn == btnInfo)
            {
                SetMode("Info");
                // Đảm bảo bà đã tạo file PagePatientProfile.xaml trong folder Pages
                MainFrame.Navigate(new PagePatientProfile(patientID));
            }
        }

        #region Window Controls
        private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) DragMove();
        }

        private void Min_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Close_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        private void Max_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
                btnMax.Content = "❐"; // Đổi icon sang 2 ô vuông
            }
            else
            {
                WindowState = WindowState.Normal;
                btnMax.Content = "☐"; // Đổi về 1 ô vuông
            }
        }
        private void btnLogOut_Click(object sender, RoutedEventArgs e)
        {
            this.Effect = new System.Windows.Media.Effects.BlurEffect { Radius = 10 };
            if (new LogoutWindow { Owner = this }.ShowDialog() == true)
            {
                new RoleSelectionWindow().Show();
                this.Close();
            }
            else this.Effect = null;
        }
        #endregion

        private void Avatar_Click(object sender, MouseButtonEventArgs e) { }
    }
}