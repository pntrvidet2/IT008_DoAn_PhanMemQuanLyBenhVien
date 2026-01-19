using System;
using System.Data;
using System.Windows;
using DoAn.ClassData;
using Microsoft.Data.SqlClient;

namespace DoAn.Windows
{
    public partial class AddAppointmentWindow : Window
    {
        Database db = new Database();
        string _accId;

        public AddAppointmentWindow(string accId)
        {
            InitializeComponent();
            _accId = accId;
            LoadPatients();
            dpDate.SelectedDate = DateTime.Now.AddDays(7);
        }

        private void LoadPatients()
        {
            cbPatient.ItemsSource = db.GetData("SELECT Patient_ID, Patient_Name FROM PATIENT").DefaultView;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (cbPatient.SelectedValue == null) { MessageBox.Show("Vui lòng chọn bệnh nhân!"); return; }

            string empSub = $"(SELECT TOP 1 Emp_ID FROM EMPLOYEE E JOIN ACCOUNT A ON E.Email = A.User_Name WHERE A.Acc_ID = '{_accId}')";
            string sql = "INSERT INTO APPOINTMENT (Patient_ID, Doctor_ID, App_Date, App_Note, Status) " +
                         $"VALUES ('{cbPatient.SelectedValue}', {empSub}, '{dpDate.SelectedDate:yyyy-MM-dd}', N'{txtNote.Text}', N'Pending')";

            if (db.Execute(sql))
            {
                this.DialogResult = true;
                this.Close();
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}