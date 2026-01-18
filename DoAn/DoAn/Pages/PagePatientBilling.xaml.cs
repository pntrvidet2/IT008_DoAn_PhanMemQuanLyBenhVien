using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using DoAn.ClassData;

namespace DoAn.Pages
{
    public partial class PagePatientBilling : Page
    {
        private readonly Database db = new Database();
        private readonly string _patientId;

        public PagePatientBilling(string patientId)
        {
            InitializeComponent();
            _patientId = patientId;
            LoadBills();
        }

        private void LoadBills()
        {
            try
            {
                string sql = @"
SELECT Bill_ID, Patient_ID, Emp_ID, Date, Total, Payment_Method, Payment_Status
FROM BILL
WHERE Patient_ID = @pid
ORDER BY Date DESC;
";
                DataTable dt = db.GetData(sql, new SqlParameter("@pid", _patientId));
                dgBills.ItemsSource = dt.DefaultView;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi load hóa đơn: " + ex.Message);
            }
        }
    }
}
