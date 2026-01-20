using System;
using System.Data;
using System.Windows;
using Microsoft.Data.SqlClient;
using DoAn.ClassData;

namespace DoAn.Windows
{
    public partial class AddMedicalRecordWindow : Window
    {
        private readonly Database db = new Database();
        private string _accId;

        public AddMedicalRecordWindow(string accId)
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
            object res = db.ExecuteScalar("SELECT TOP 1 Med_Record_ID FROM MEDICAL_RECORD ORDER BY Med_Record_ID DESC");
            if (res == null || res == DBNull.Value) txtMedID.Text = "MR001";
            else
            {
                int num = int.Parse(res.ToString().Substring(2)) + 1;
                txtMedID.Text = "MR" + num.ToString("D3");
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (cboPatient.SelectedValue == null) { MessageBox.Show("Vui lòng chọn bệnh nhân!"); return; }

            try
            {
                // Chú ý: Cột bác sĩ trong bảng Bệnh án bà đặt là Employ_ID
                string sql = @"INSERT INTO MEDICAL_RECORD (Med_Record_ID, Patient_ID, Employ_ID, Med_Date, Diagnosis, Theory_Plan, Health_Note) 
                               VALUES (@id, @pId, 
                               (SELECT TOP 1 E.Emp_ID FROM EMPLOYEE E JOIN ACCOUNT A ON E.Email = A.User_Name WHERE A.Acc_ID = @accId), 
                               GETDATE(), @diag, @plan, @note)";

                SqlParameter[] p = {
                    new SqlParameter("@id", txtMedID.Text),
                    new SqlParameter("@pId", cboPatient.SelectedValue),
                    new SqlParameter("@accId", _accId),
                    new SqlParameter("@diag", txtDiag.Text),
                    new SqlParameter("@plan", txtPlan.Text),
                    new SqlParameter("@note", txtNote.Text)
                };

                if (db.Execute(sql, p))
                {
                    MessageBox.Show("Lập hồ sơ bệnh án thành công!");
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