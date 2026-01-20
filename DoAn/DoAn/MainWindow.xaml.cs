using DoAn.ClassData;
using DoAn.Pages;
using DoAn.Windows;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using System;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace DoAn
{
    public partial class MainWindow : Window
    {
        Database db = new Database();
        DispatcherTimer timer;
        string currentMode = "Home";
        string currentUser;

        public MainWindow(string username)
        {
            InitializeComponent();
            currentUser = username;
            txtUsername.Text = username;
            StartClock();
            this.Loaded += (s, e) => SetMode("Home");
        }

        private void StartClock()
        {
            timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += (s, e) => txtClock.Text = DateTime.Now.ToString("HH:mm:ss");
            timer.Start();
        }

        private void SetMode(string mode)
        {
            currentMode = mode;
            if (mode == "Home")
            {
                brdDashboard.Visibility = Visibility.Visible; gridDataDisplay.Visibility = Visibility.Collapsed;
                LoadDashboardStats();
            }
            else
            {
                brdDashboard.Visibility = Visibility.Collapsed; gridDataDisplay.Visibility = Visibility.Visible;
                bool canEdit = (mode == "Patient" || mode == "Drug" || mode == "Bill" || mode == "Prescription");
                btnAdd.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
                stkFooterActions.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
                txtCurrentTabName.Text = mode.ToUpper();
                LoadCurrentList(mode);
            }
        }

        private void LoadDashboardStats()
        {
            try
            {
                txtStatPatient.Text = db.ExecuteScalar("SELECT COUNT(*) FROM PATIENT").ToString();
                txtStatDrug.Text = db.ExecuteScalar("SELECT COUNT(*) FROM DRUG").ToString();
                object rev = db.ExecuteScalar("SELECT SUM(Total) FROM BILL");
                txtStatRevenue.Text = rev != DBNull.Value ? string.Format("{0:N0}", rev) : "0";
            }
            catch { }
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as ToggleButton;
            btnHome.IsChecked = (btn == btnHome); btnPatient.IsChecked = (btn == btnPatient);
            btnDrug.IsChecked = (btn == btnDrug); btnBill.IsChecked = (btn == btnBill);
            btnPrescription.IsChecked = (btn == btnPrescription); btnMedicalRecord.IsChecked = (btn == btnMedicalRecord);
            btnMedicalForm.IsChecked = (btn == btnMedicalForm); btnMonitoring.IsChecked = (btn == btnMonitoring);
            SetMode(btn.Name.Replace("btn", ""));
        }

        private void LoadCurrentList(string mode)
        {
            string sql = mode switch
            {
                "Patient" => "SELECT * FROM PATIENT",
                "Drug" => "SELECT * FROM DRUG",
                "Bill" => "SELECT * FROM BILL",
                "Prescription" => "SELECT PD.Bill_ID, D.Drug_Name, PD.Drug_Quantity, PD.Amount FROM PRESCRIPTION_DETAILS PD JOIN DRUG D ON PD.Drug_ID = D.Drug_ID",
                "MedicalRecord" => "SELECT * FROM MEDICAL_RECORD",
                "MedicalForm" => "SELECT * FROM MEDICAL_FORM",
                "Monitoring" => "SELECT * FROM MONITORING_SHEET",
                _ => ""
            };
            if (!string.IsNullOrEmpty(sql)) dgData.ItemsSource = db.GetData(sql).DefaultView;
        }

        // --- PHẦN THÊM MỚI (ADD) ---
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            bool? result = false;

            switch (currentMode)
            {
                case "Patient":
                    var addPatient = new AddPatientWindow { Owner = this };
                    result = addPatient.ShowDialog();
                    break;
                case "Drug":
                    var addDrug = new AddDrugWindow { Owner = this };
                    result = addDrug.ShowDialog();
                    break;
                case "Bill":
                    var addBill = new AddBillWindow(currentUser) { Owner = this };
                    result = addBill.ShowDialog();
                    break;
                case "Prescription":
                    var addPres = new AddPrescriptionWindow { Owner = this };
                    result = addPres.ShowDialog();
                    break;
            }

            // Nếu lưu thành công (DialogResult = true) thì load lại danh sách
            if (result == true) LoadCurrentList(currentMode);
        }

        // --- PHẦN CẬP NHẬT (UPDATE) ---
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (dgData.SelectedItem is not DataRowView row)
            {
                MessageBox.Show("Vui lòng chọn một dòng để sửa!");
                return;
            }

            bool? result = false;

            switch (currentMode)
            {
                case "Patient":
                    // Truyền dữ liệu sang Window sửa (Bà chỉnh tham số cho khớp với Constructor của bà nha)
                    var editPatient = new UpdatePatientWindow(
                        row["Patient_ID"].ToString(),
                        row["Patient_Name"].ToString(),
                        row["Gender"].ToString(),
                        row["Phone"].ToString(),
                        row["CID"].ToString(),
                        row["Address"].ToString(),
                        "")
                    { Owner = this };
                    result = editPatient.ShowDialog();
                    break;

                case "Drug":
                    var editDrug = new UpdateDrugWindow(
                        row["Drug_ID"].ToString(),
                        Convert.ToDecimal(row["Drug_Price"]),
                        Convert.ToInt32(row["Stock_Quantity"]))
                    { Owner = this };
                    result = editDrug.ShowDialog();
                    break;

                case "Bill":
                    var editBill = new UpdateBillWindow(
                        row["Bill_ID"].ToString(),
                        row["Patient_ID"].ToString(),
                        row["Emp_ID"].ToString(),
                        Convert.ToDecimal(row["Total"]),
                        row["Payment_Method"].ToString(),
                        row["Payment_Status"].ToString())
                    { Owner = this };
                    result = editBill.ShowDialog();
                    break;

                case "Prescription":
                    // Sửa chi tiết đơn thuốc (Sử dụng mã Bill và mã Thuốc để định danh)
                    string dID = db.ExecuteScalar($"SELECT Drug_ID FROM DRUG WHERE Drug_Name = N'{row["Drug_Name"]}'").ToString();
                    var editPres = new AddPrescriptionWindow(
                        row["Bill_ID"].ToString(),
                        dID,
                        Convert.ToInt32(row["Drug_Quantity"]))
                    { Owner = this };
                    result = editPres.ShowDialog();
                    break;
            }

            if (result == true) LoadCurrentList(currentMode);
        }

        // --- PHẦN XÓA (DELETE) ---
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgData.SelectedItem is not DataRowView row) return;

            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa bản ghi này?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    // Trong hàm btnDelete_Click chỗ xử lý Prescription:
                    if (currentMode == "Prescription")
                    {
                        string bID = row["Bill_ID"].ToString();
                        string dName = row["Drug_Name"].ToString();
                        string dID = db.ExecuteScalar($"SELECT Drug_ID FROM DRUG WHERE Drug_Name = N'{dName}'").ToString();
                        int qtyToDelete = Convert.ToInt32(row["Drug_Quantity"]);

                        // 1. Trả lại thuốc vào kho
                        db.Execute($"UPDATE DRUG SET Stock_Quantity = Stock_Quantity + {qtyToDelete} WHERE Drug_ID = '{dID}'");

                        // 2. Xóa dòng chi tiết
                        db.Execute($"DELETE FROM PRESCRIPTION_DETAILS WHERE Bill_ID='{bID}' AND Drug_ID='{dID}'");

                        // 3. Tính lại tiền Bill
                        db.Execute($"UPDATE BILL SET Total = (SELECT ISNULL(SUM(Amount),0) FROM PRESCRIPTION_DETAILS WHERE Bill_ID='{bID}') WHERE Bill_ID='{bID}'");
                    }
                    else
                    {
                        // Lấy tên cột ID tự động từ DataGrid để xóa các bảng khác
                        string idColumn = dgData.Columns[0].Header.ToString();
                        string idValue = row[0].ToString();
                        db.Execute($"DELETE FROM {currentMode.ToUpper()} WHERE {idColumn} = '{idValue}'");
                    }

                    LoadCurrentList(currentMode);
                    LoadDashboardStats(); // Cập nhật lại con số trên Dashboard
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể xóa bản ghi này do có dữ liệu liên quan!");
                }
            }
        }

  
        private void btnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            if (dgData.ItemsSource == null) return;

    
            string defaultName = $"KiemKe_{currentMode}_{DateTime.Now:MM_yyyy}.csv";

            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "CSV files (*.csv)|*.csv";
            sfd.FileName = defaultName;

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    DataTable dt = ((DataView)dgData.ItemsSource).ToTable();
                    using (StreamWriter sw = new StreamWriter(sfd.FileName, false, System.Text.Encoding.UTF8))
                    {
                
                        for (int i = 0; i < dt.Columns.Count; i++)
                        {
                            sw.Write(dt.Columns[i]);
                            if (i < dt.Columns.Count - 1) sw.Write(",");
                        }
                        sw.Write(sw.NewLine);

                    
                        foreach (DataRow row in dt.Rows)
                        {
                            for (int i = 0; i < dt.Columns.Count; i++)
                            {
                                if (!Convert.IsDBNull(row[i]))
                                    sw.Write(row[i].ToString().Replace(",", " ")); 
                                if (i < dt.Columns.Count - 1) sw.Write(",");
                            }
                            sw.Write(sw.NewLine);
                        }
                    }
                    MessageBox.Show($"Đã lưu file {sfd.FileName} thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi xuất file: " + ex.Message);
                }
            }
        }
        private void btnLogOut_Click(object sender, RoutedEventArgs e) {
            this.Effect = new System.Windows.Media.Effects.BlurEffect { Radius = 10 };
            if (new LogoutWindow { Owner = this }.ShowDialog() == true)
            {
                new RoleSelectionWindow().Show();
                this.Close();
            }
            else this.Effect = null;
        }
        private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); }
        private void Min_Click(object sender, RoutedEventArgs e)
     => WindowState = WindowState.Minimized;

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

        private void Close_Click(object sender, RoutedEventArgs e)
            => Application.Current.Shutdown();
       
    }
}