using DoAn.ClassData;
using DoAn.Pages;
using DoAn.Windows;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace DoAn
{
    public partial class MainWindowDoctor : Window
    {
        private readonly Database db = new Database();
        private string _accId;
        private string _currentMode = "Home";
        private DispatcherTimer _timer;

        public MainWindowDoctor(string doctorId, string doctorName)
        {
            InitializeComponent();
            _accId = doctorId;
            txtUsernameDisplay.Text = doctorName;

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (s, e) => txtClock.Text = DateTime.Now.ToString("HH:mm:ss");
            _timer.Start();

            ShowDashboard();
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not ToggleButton clickedBtn) return;

            btnHome.IsChecked = (clickedBtn == btnHome);
            btnAppointment.IsChecked = (clickedBtn == btnAppointment);
            btnMyPatients.IsChecked = (clickedBtn == btnMyPatients);
            btnMedicalRecord.IsChecked = (clickedBtn == btnMedicalRecord);
            btnExamForm.IsChecked = (clickedBtn == btnExamForm);
            btnMonitoring.IsChecked = (clickedBtn == btnMonitoring);
            btnDrugList.IsChecked = (clickedBtn == btnDrugList);

            if (clickedBtn == btnHome) { _currentMode = "Home"; ShowDashboard(); }
            else
            {
                _currentMode = clickedBtn.Name.Replace("btn", "");
                ShowData(clickedBtn.Content.ToString().Trim());
            }
        }

        private void ShowDashboard()
        {
            brdDashboard.Visibility = Visibility.Visible;
            gridDataDisplay.Visibility = Visibility.Collapsed;
            txtTitleHeader.Text = "DASHBOARD QUẢN LÝ";
            LoadStats();
        }

        private void ShowData(string title)
        {
            brdDashboard.Visibility = Visibility.Collapsed;
            gridDataDisplay.Visibility = Visibility.Visible;
            txtTitleHeader.Text = title.ToUpper();

            bool canModify = (_currentMode != "DrugList" && _currentMode != "MyPatients");
            btnAddRecord.Visibility = canModify ? Visibility.Visible : Visibility.Collapsed;
            stkFooterActions.Visibility = canModify ? Visibility.Visible : Visibility.Collapsed;

            LoadData();
        }

        private string GetEmpId() => $"(SELECT TOP 1 E.Emp_ID FROM EMPLOYEE E JOIN ACCOUNT A ON E.Email = A.User_Name WHERE A.Acc_ID = '{_accId}')";

        private void LoadStats()
        {
            try
            {
                string empId = GetEmpId();
                // Sửa thành Emp_ID vì bảng PATIENT bà đặt là Emp_ID
                txtStatPatient.Text = db.ExecuteScalar($"SELECT COUNT(DISTINCT Patient_ID) FROM PATIENT WHERE Emp_ID = {empId}").ToString();
                // Sửa thành Doctor_ID vì bảng APPOINTMENT bà đặt là Doctor_ID
                txtStatTodayApp.Text = db.ExecuteScalar($"SELECT COUNT(*) FROM APPOINTMENT WHERE Doctor_ID = {empId} AND CAST(App_Date AS DATE) = CAST(GETDATE() AS DATE)").ToString();

                string sql = $@"SELECT A.App_Date, P.Patient_Name, A.Status 
                               FROM APPOINTMENT A 
                               JOIN PATIENT P ON A.Patient_ID = P.Patient_ID 
                               WHERE A.Doctor_ID = {empId} AND CAST(A.App_Date AS DATE) = CAST(GETDATE() AS DATE)
                               ORDER BY A.App_Date ASC";
                dgDashboardApp.ItemsSource = db.GetData(sql)?.DefaultView;
            }
            catch { }
        }

        public void LoadData()
        {
            string sql = "";
            string empId = GetEmpId();
            switch (_currentMode)
            {
                case "Appointment": sql = $"SELECT * FROM APPOINTMENT WHERE Doctor_ID = {empId}"; break;
                // Sửa thành Employ_ID vì bảng MEDICAL_RECORD bà đặt là Employ_ID
                case "MedicalRecord": sql = $"SELECT * FROM MEDICAL_RECORD WHERE Employ_ID = {empId}"; break;
                // Sửa thành Emp_ID cho các bảng còn lại
                case "ExamForm": sql = $"SELECT * FROM MEDICAL_FORM WHERE Emp_ID = {empId}"; break;
                case "Monitoring": sql = $"SELECT * FROM MONITORING_SHEET WHERE Emp_ID = {empId}"; break;
                case "MyPatients": sql = $"SELECT * FROM PATIENT WHERE Emp_ID = {empId}"; break;
                case "DrugList": sql = "SELECT * FROM DRUG"; break;
            }
            if (!string.IsNullOrEmpty(sql)) dgData.ItemsSource = db.GetData(sql)?.DefaultView;
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            if (dgData.ItemsSource == null) return;
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog() { Filter = "CSV file (*.csv)|*.csv", FileName = "BaoCao_" + _currentMode };
            if (sfd.ShowDialog() == true)
            {
                try
                {
                    DataTable dt = ((DataView)dgData.ItemsSource).ToTable();
                    StringBuilder sb = new StringBuilder();
                    sb.Append('\uFEFF');
                    for (int i = 0; i < dt.Columns.Count; i++) sb.Append(dt.Columns[i].ColumnName + (i == dt.Columns.Count - 1 ? "" : ","));
                    sb.AppendLine();
                    foreach (DataRow row in dt.Rows)
                    {
                        for (int i = 0; i < dt.Columns.Count; i++) sb.Append("\"" + row[i].ToString() + "\"" + (i == dt.Columns.Count - 1 ? "" : ","));
                        sb.AppendLine();
                    }
                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Xuất báo cáo thành công!");
                }
                catch { MessageBox.Show("Lỗi xuất file!"); }
            }
        }

        private void btnAddRecord_Click(object sender, RoutedEventArgs e)
        {
            Window win = null;
            if (_currentMode == "Appointment") win = new AddAppointmentWindow(_accId);
            else if (_currentMode == "MedicalRecord") win = new AddMedicalRecordWindow(_accId);
            else if (_currentMode == "ExamForm") win = new AddExamFormWindow(_accId);
            else if (_currentMode == "Monitoring") win = new AddMonitoringWindow(_accId);

            if (win != null)
            {
                // Khi đóng cửa sổ con (bất kể bấm X hay Save)
                win.Closed += (s, ev) =>
                {
                    // 1. Ẩn tab dữ liệu và hiện Dashboard
                    gridDataDisplay.Visibility = Visibility.Collapsed;
                    brdDashboard.Visibility = Visibility.Visible;
                    txtTitleHeader.Text = "DASHBOARD QUẢN LÝ";

                    // 2. Reset trạng thái Menu (Không thêm biến, dùng trực tiếp các nút đã có)
                    btnHome.IsChecked = true;
                    btnAppointment.IsChecked = false;
                    btnMyPatients.IsChecked = false;
                    btnMedicalRecord.IsChecked = false;
                    btnExamForm.IsChecked = false;
                    btnMonitoring.IsChecked = false;
                    btnDrugList.IsChecked = false;

                    _currentMode = "Home";
                };

                // ShowDialog trả về true khi bạn gọi DialogResult = true (thường là sau khi Save thành công)
                if (win.ShowDialog() == true)
                {
                    LoadData();
                    LoadStats();
                }
            }
        }
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (dgData.SelectedItem == null) return;
            DataRowView row = (DataRowView)dgData.SelectedItem;
            Window win = null;
            if (_currentMode == "Appointment") win = new UpdateAppointmentWindow(row, _accId);
            if (win != null && win.ShowDialog() == true) { LoadData(); LoadStats(); }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgData.SelectedItem == null) return;
            DataRowView row = (DataRowView)dgData.SelectedItem;
            if (MessageBox.Show("Xác nhận xóa?", "Thông báo", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                string table = _currentMode == "Appointment" ? "APPOINTMENT" : "MEDICAL_RECORD";
                string idCol = _currentMode == "Appointment" ? "App_ID" : "Med_Record_ID";
                if (db.Execute($"DELETE FROM {table} WHERE {idCol} = '{row[0]}'")) { LoadData(); LoadStats(); }
            }
        }
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

        private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); }
        private void Min_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Close_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        private void btnLogOut_Click(object sender, RoutedEventArgs e) { if (new LogoutWindow { Owner = this }.ShowDialog() == true) { new RoleSelectionWindow().Show(); this.Close(); } }
    }
}