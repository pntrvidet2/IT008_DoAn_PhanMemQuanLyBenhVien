using System;
using System.Data;
using System.Windows;
using Microsoft.Data.SqlClient;
using DoAn.ClassData;

namespace DoAn.Windows
{
    public partial class AddExamFormWindow : Window
    {
        private readonly Database db = new Database();
        private string _accId; // Nhận Acc001, Acc002... từ MainWindow

        public AddExamFormWindow(string accId)
        {
            InitializeComponent();
            this._accId = accId;
            LoadPatients();
            GenerateID();
        }

        private void LoadPatients()
        {
            DataTable dt = db.GetData("SELECT Patient_ID, Patient_Name FROM PATIENT");
            cboPatient.ItemsSource = dt.DefaultView;
            cboPatient.DisplayMemberPath = "Patient_Name";
            cboPatient.SelectedValuePath = "Patient_ID";
        }

        private void GenerateID()
        {
            object res = db.ExecuteScalar("SELECT TOP 1 Med_Form_ID FROM MEDICAL_FORM ORDER BY Med_Form_ID DESC");
            if (res == null || res == DBNull.Value) txtFormID.Text = "MF001";
            else
            {
                int num = int.Parse(res.ToString().Substring(2)) + 1;
                txtFormID.Text = "MF" + num.ToString("D3");
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (cboPatient.SelectedValue == null) { MessageBox.Show("Vui lòng chọn bệnh nhân!"); return; }

            try
            {
                // Logic JOIN lồng để tìm Emp_ID từ Acc_ID bà đang có
                string sql = @"INSERT INTO MEDICAL_FORM (Med_Form_ID, Patient_ID, Emp_ID, Date, Symptom, Conclusion) 
                               VALUES (@id, @pId, 
                               (SELECT TOP 1 E.Emp_ID FROM EMPLOYEE E JOIN ACCOUNT A ON E.Email = A.User_Name WHERE A.Acc_ID = @accId), 
                               GETDATE(), @symp, @conc)";

                SqlParameter[] p = {
                    new SqlParameter("@id", txtFormID.Text),
                    new SqlParameter("@pId", cboPatient.SelectedValue),
                    new SqlParameter("@accId", _accId),
                    new SqlParameter("@symp", txtSymptom.Text),
                    new SqlParameter("@conc", txtConclusion.Text)
                };

                if (db.Execute(sql, p))
                {
                    MessageBox.Show("Lập phiếu khám thành công!");
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Lỗi: Không tìm thấy nhân viên liên kết với tài khoản này.");
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi hệ thống: " + ex.Message); }
        }


        private void Cancel_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}