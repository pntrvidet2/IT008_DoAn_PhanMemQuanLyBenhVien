using System;
using System.Data;
using System.Windows;
using DoAn.ClassData;

namespace DoAn
{
    public partial class UpdateAppointmentWindow : Window
    {
        private Database db = new Database();
        private DataRowView _row;

        public UpdateAppointmentWindow(DataRowView row, string accId)
        {
            InitializeComponent();
            _row = row;

          
            DataTable dt = db.GetData("SELECT Patient_ID, Patient_Name FROM PATIENT");
            cbPatient.ItemsSource = dt?.DefaultView;

    
            cbPatient.SelectedValue = _row["Patient_ID"].ToString();
            dpDate.SelectedDate = Convert.ToDateTime(_row["App_Date"]);
            txtNote.Text = _row["App_Note"]?.ToString();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (dpDate.SelectedDate == null) { MessageBox.Show("Vui lòng chọn ngày!"); return; }

            
            string id = _row["App_ID"].ToString();
            string dateStr = dpDate.SelectedDate.Value.ToString("yyyy-MM-dd");
            string noteStr = txtNote.Text.Replace("'", "''"); 

            string sql = $"UPDATE APPOINTMENT SET App_Date = '{dateStr}', App_Note = N'{noteStr}' WHERE App_ID = '{id}'";

            if (db.Execute(sql))
            {
                MessageBox.Show("Cập nhật lịch hẹn thành công!");
                this.DialogResult = true;
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}