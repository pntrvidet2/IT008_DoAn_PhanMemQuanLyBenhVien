using System;
using System.Data;
using System.Windows;
using Microsoft.Data.SqlClient;
using DoAn.ClassData;

namespace DoAn.Windows
{
    public partial class UpdateExamFormWindow : Window
    {
        private Database db = new Database();
        private string _accId;

        public UpdateExamFormWindow(DataRowView row, string accId)
        {
            InitializeComponent();
            this._accId = accId;

            txtFormID.Text = row["Med_Form_ID"].ToString();
            txtSymptom.Text = row["Symptom"].ToString();
            txtConclusion.Text = row["Conclusion"].ToString();
            // Load lại ComboBox bệnh nhân nếu cần, hoặc chỉ để hiển thị
            cboPatient.Text = row["Patient_ID"].ToString();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string sql = "UPDATE MEDICAL_FORM SET Symptom = @s, Conclusion = @c WHERE Med_Form_ID = @id";
            SqlParameter[] p = {
                new SqlParameter("@s", txtSymptom.Text),
                new SqlParameter("@c", txtConclusion.Text),
                new SqlParameter("@id", txtFormID.Text)
            };

            if (db.Execute(sql, p))
            {
                MessageBox.Show("Cập nhật phiếu khám thành công!");
                this.DialogResult = true;
                this.Close();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}