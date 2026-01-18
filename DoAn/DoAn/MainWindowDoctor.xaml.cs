using DoAn.ClassData;
using DoAn.Pages;
using DoAn.Windows;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace DoAn
{
    public partial class MainWindowDoctor : Window
    {
        private readonly Database db = new Database();
        private string _accId; // Lưu Acc_ID truyền từ Login
        private string _currentMode = "Home";
        private DispatcherTimer _timer;

        public MainWindowDoctor(string doctorId, string doctorName)
        {
            InitializeComponent();
            _accId = doctorId; // Nhận Acc001, Acc002...
            txtUsernameDisplay.Text = doctorName;

            // Khởi chạy đồng hồ hệ thống
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += (s, e) => txtClock.Text = DateTime.Now.ToString("HH:mm:ss");
            _timer.Start();

            ShowDashboard();
        }

        // ===== XỬ LÝ MENU =====
        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton clickedBtn)
            {
                btnHome.IsChecked = (clickedBtn == btnHome);
                btnMyPatients.IsChecked = (clickedBtn == btnMyPatients);
                btnMedicalRecord.IsChecked = (clickedBtn == btnMedicalRecord);
                btnExamForm.IsChecked = (clickedBtn == btnExamForm);
                btnMonitoring.IsChecked = (clickedBtn == btnMonitoring);
                btnDrugList.IsChecked = (clickedBtn == btnDrugList);

                if (clickedBtn == btnHome) { _currentMode = "Home"; ShowDashboard(); }
                else if (clickedBtn == btnMyPatients) { _currentMode = "MyPatients"; ShowData("BỆNH NHÂN ĐANG ĐIỀU TRỊ"); }
                else if (clickedBtn == btnMedicalRecord) { _currentMode = "MedicalRecord"; ShowData("DANH SÁCH BỆNH ÁN"); }
                else if (clickedBtn == btnExamForm) { _currentMode = "ExamForm"; ShowData("PHIẾU KHÁM BỆNH"); }
                else if (clickedBtn == btnMonitoring) { _currentMode = "Monitoring"; ShowData("PHIẾU THEO DÕI"); }
                else if (clickedBtn == btnDrugList) { _currentMode = "Drug"; ShowData("DANH MỤC THUỐC"); }
            }
        }

        private void ShowDashboard()
        {
            brdDashboard.Visibility = Visibility.Visible;
            gridDataDisplay.Visibility = Visibility.Collapsed;
            LoadStats();
        }

        private void ShowData(string title)
        {
            brdDashboard.Visibility = Visibility.Collapsed;
            gridDataDisplay.Visibility = Visibility.Visible;
            txtTitleHeader.Text = title;

            bool canModify = (_currentMode == "MedicalRecord" || _currentMode == "ExamForm" || _currentMode == "Monitoring");
            btnAddRecord.Visibility = canModify ? Visibility.Visible : Visibility.Collapsed;
            stkFooterActions.Visibility = canModify ? Visibility.Visible : Visibility.Collapsed;

            LoadData();
        }

        // ===== TRUY VẤN DỮ LIỆU =====
        private void LoadStats()
        {
            try
            {
                // Subquery để tìm mã nhân viên từ tài khoản đang đăng nhập
                string empSub = $"(SELECT TOP 1 E.Emp_ID FROM EMPLOYEE E JOIN ACCOUNT A ON E.Email = A.User_Name WHERE A.Acc_ID = '{_accId}')";

                txtStatPatient.Text = db.ExecuteScalar($"SELECT COUNT(DISTINCT Patient_ID) FROM MEDICAL_RECORD WHERE Employ_ID = {empSub}").ToString();
                txtStatRecord.Text = db.ExecuteScalar($"SELECT COUNT(*) FROM MEDICAL_RECORD WHERE Employ_ID = {empSub}").ToString();
                txtStatDrug.Text = db.ExecuteScalar("SELECT COUNT(*) FROM DRUG").ToString();
            }
            catch { }
        }

        public void LoadData()
        {
            string sql = "";
            string empSub = $"(SELECT TOP 1 E.Emp_ID FROM EMPLOYEE E JOIN ACCOUNT A ON E.Email = A.User_Name WHERE A.Acc_ID = '{_accId}')";

            switch (_currentMode)
            {
                case "MedicalRecord":
                    sql = $"SELECT * FROM MEDICAL_RECORD WHERE Employ_ID = {empSub}";
                    break;
                case "ExamForm":
                    sql = $"SELECT * FROM MEDICAL_FORM WHERE Emp_ID = {empSub}";
                    break;
                case "Monitoring":
                    sql = $"SELECT * FROM MONITORING_SHEET WHERE Emp_ID = {empSub}";
                    break;
                case "MyPatients":
                    sql = $@"SELECT DISTINCT P.* FROM PATIENT P WHERE P.Patient_ID IN (
                            SELECT Patient_ID FROM MEDICAL_RECORD WHERE Employ_ID = {empSub}
                            UNION SELECT Patient_ID FROM MEDICAL_FORM WHERE Emp_ID = {empSub}
                            UNION SELECT Patient_ID FROM MONITORING_SHEET WHERE Emp_ID = {empSub})";
                    break;
                case "Drug":
                    sql = "SELECT * FROM DRUG";
                    break;
            }

            if (!string.IsNullOrEmpty(sql))
            {
                dgData.ItemsSource = null;
                DataTable dt = db.GetData(sql);
                dgData.ItemsSource = dt?.DefaultView;
            }
        }

        // ===== CHỨC NĂNG THÊM / SỬA / XÓA =====
        private void btnAddRecord_Click(object sender, RoutedEventArgs e)
        {
            Window win = null;
            if (_currentMode == "MedicalRecord") win = new AddMedicalRecordWindow(_accId);
            else if (_currentMode == "ExamForm") win = new AddExamFormWindow(_accId);
            else if (_currentMode == "Monitoring") win = new AddMonitoringWindow(_accId);

            if (win != null && win.ShowDialog() == true)
            {
                Application.Current.Dispatcher.Invoke(() => { LoadData(); LoadStats(); });
            }
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (dgData.SelectedItem == null) { MessageBox.Show("Vui lòng chọn một dòng!"); return; }
            DataRowView row = (DataRowView)dgData.SelectedItem;
            Window updateWin = null;

            if (_currentMode == "MedicalRecord") updateWin = new UpdateMedicalRecordWindow(row, _accId);
            else if (_currentMode == "ExamForm") updateWin = new UpdateExamFormWindow(row, _accId);
            else if (_currentMode == "Monitoring") updateWin = new UpdateMonitoringWindow(row, _accId);

            if (updateWin != null && updateWin.ShowDialog() == true) LoadData();
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgData.SelectedItem == null) { MessageBox.Show("Vui lòng chọn dòng cần xóa!"); return; }
            DataRowView row = (DataRowView)dgData.SelectedItem;
            string table = "", idCol = "";

            if (_currentMode == "MedicalRecord") { table = "MEDICAL_RECORD"; idCol = "Med_Record_ID"; }
            else if (_currentMode == "ExamForm") { table = "MEDICAL_FORM"; idCol = "Med_Form_ID"; }
            else if (_currentMode == "Monitoring") { table = "MONITORING_SHEET"; idCol = "Moni_Sheet_ID"; }
            else return;

            if (MessageBox.Show($"Xóa mã {row[idCol]}?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (db.Execute($"DELETE FROM {table} WHERE {idCol} = '{row[idCol]}'")) { LoadData(); LoadStats(); }
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string key = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(key)) { LoadData(); return; }
            string empSub = $"(SELECT TOP 1 E.Emp_ID FROM EMPLOYEE E JOIN ACCOUNT A ON E.Email = A.User_Name WHERE A.Acc_ID = '{_accId}')";
            string sql = "";

            if (_currentMode == "Drug") sql = $"SELECT * FROM DRUG WHERE Drug_Name LIKE N'%{key}%'";
            else if (_currentMode == "MyPatients") sql = $"SELECT DISTINCT P.* FROM PATIENT P WHERE P.Patient_Name LIKE N'%{key}%' AND P.Patient_ID IN (SELECT Patient_ID FROM MEDICAL_RECORD WHERE Employ_ID = {empSub})";
            else LoadData();

            if (!string.IsNullOrEmpty(sql)) dgData.ItemsSource = db.GetData(sql).DefaultView;
        }

        // ===== HỆ THỐNG =====
        private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed) this.DragMove();
        }

        private void Min_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Close_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        private void Max_Click(object sender, RoutedEventArgs e) => WindowState = (WindowState == WindowState.Normal) ? WindowState.Maximized : WindowState.Normal;

        private void btnLogOut_Click(object sender, RoutedEventArgs e)
        {
            this.Effect = new System.Windows.Media.Effects.BlurEffect { Radius = 10 };
            var logoutWin = new LogoutWindow { Owner = this };
            if (logoutWin.ShowDialog() == true)
            {
                new LoginWindow().Show();
                this.Close();
            }
            else this.Effect = null;
        }
    }
}