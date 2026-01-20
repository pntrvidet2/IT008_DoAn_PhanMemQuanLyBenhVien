using DoAn.ClassData;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;

namespace DoAn.Pages
{
    public partial class PagePatientBilling : Page
    {
        Database db = new Database();
        string _patientId;

        public PagePatientBilling(string patientId)
        {
            InitializeComponent();
            _patientId = patientId;
            LoadData();
        }

        public void LoadData()
        {
            string sql = "SELECT * FROM BILL WHERE Patient_ID = @id";
            DataTable dt = db.GetData(sql, new SqlParameter("@id", _patientId));
            dgBills.ItemsSource = dt?.DefaultView;
        }

        private void btnPay_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var row = dgBills.SelectedItem as DataRowView;
            if (row == null) return;

            string bId = row["Bill_ID"].ToString();
            decimal total = Convert.ToDecimal(row["Total"]);

            // Gọi Window QR chuẩn "vjp"
            PaymentQRWindow qrWin = new PaymentQRWindow(bId, total) { Owner = Window.GetWindow(this) };

            if (qrWin.ShowDialog() == true)
            {
                // 1. Cập nhật SQL
                db.Execute($"UPDATE BILL SET Payment_Status = 'Paid' WHERE Bill_ID = '{bId}'");

                // 2. Load lại bảng tại chỗ
                LoadData();

                // 3. Cập nhật lại con số ở Dashboard (Dòng này hết lỗi nhờ bước 1)
                var mainWin = Window.GetWindow(this) as MainWindowPatient;
                mainWin?.LoadDashboardStats();
            }
        }
    }
}