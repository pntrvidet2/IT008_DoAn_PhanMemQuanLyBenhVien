using System;
using System.Data;
using System.Windows;
using Microsoft.Data.SqlClient;
using DoAn.ClassData;

namespace DoAn.Windows
{
    public partial class UpdateMedicalRecordWindow : Window
    {
        private Database db = new Database();
        private string _accId;

        public UpdateMedicalRecordWindow(DataRowView row, string accId)
        {
            InitializeComponent();
            this._accId = accId;

            // Đổ dữ liệu từ DataGrid vào các ô nhập liệu (Khớp Name trong XAML)
            txtRecordId.Text = row["Med_Record_ID"].ToString();
            txtPatientId.Text = row["Patient_ID"].ToString();
            txtDiagnosis.Text = row["Diagnosis"].ToString();
            txtTreatment.Text = row["Theory_Plan"].ToString();
            // Vì bảng Medical Record không có cột triệu chứng, bà có thể để trống hoặc dùng Note
            txtSymptoms.Text = row["Health_Note"]?.ToString();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string sql = @"UPDATE MEDICAL_RECORD 
                           SET Diagnosis = @diag, Theory_Plan = @plan, Health_Note = @note 
                           WHERE Med_Record_ID = @id";

            SqlParameter[] p = {
                new SqlParameter("@diag", txtDiagnosis.Text),
                new SqlParameter("@plan", txtTreatment.Text),
                new SqlParameter("@note", txtSymptoms.Text),
                new SqlParameter("@id", txtRecordId.Text)
            };

            if (db.Execute(sql, p))
            {
                MessageBox.Show("Cập nhật bệnh án thành công!");
                this.DialogResult = true;
                this.Close();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}