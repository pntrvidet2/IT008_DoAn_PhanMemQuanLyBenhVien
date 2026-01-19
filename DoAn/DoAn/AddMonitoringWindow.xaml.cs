using System;
using System.Data;
using System.Windows;
using Microsoft.Data.SqlClient;
using DoAn.ClassData;

namespace DoAn.Windows
{
    public partial class AddMonitoringWindow : Window
    {
        private readonly Database db = new Database();
        private string _accId; // Biến này nhận Acc001, Acc002... từ MainWindow

        public AddMonitoringWindow(string accId)
        {
            InitializeComponent();
            this._accId = accId; // Lưu lại mã tài khoản
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
            object res = db.ExecuteScalar("SELECT TOP 1 Moni_Sheet_ID FROM MONITORING_SHEET ORDER BY Moni_Sheet_ID DESC");
            if (res == null || res == DBNull.Value) txtMoniID.Text = "MS001";
            else
            {
                int num = int.Parse(res.ToString().Substring(2)) + 1;
                txtMoniID.Text = "MS" + num.ToString("D3");
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (cboPatient.SelectedValue == null) { MessageBox.Show("Vui lòng chọn bệnh nhân!"); return; }

            try
            {
                // Sửa lại câu SQL: Join từ ACCOUNT qua EMPLOYEE để lấy Emp_ID dựa trên Acc_ID
                string sql = @"
                    INSERT INTO MONITORING_SHEET (Moni_Sheet_ID, Patient_ID, Emp_ID, Start_Date, Curr_Condition) 
                    VALUES (
                        @id, 
                        @pId, 
                        (SELECT TOP 1 E.Emp_ID FROM EMPLOYEE E JOIN ACCOUNT A ON E.Email = A.User_Name WHERE A.Acc_ID = @accId), 
                        GETDATE(), 
                        @cond
                    )";

                SqlParameter[] p = {
                    new SqlParameter("@id", txtMoniID.Text),
                    new SqlParameter("@pId", cboPatient.SelectedValue),
                    new SqlParameter("@accId", _accId), // Đảm bảo chữ @accId viết thường khớp với SQL ở trên
                    new SqlParameter("@cond", txtCondition.Text)
                };

                if (db.Execute(sql, p))
                {
                    MessageBox.Show("Lập phiếu theo dõi thành công!");
                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    // Nếu vào đây là do Subquery trả về NULL (Dữ liệu Email không khớp giữa 2 bảng)
                    MessageBox.Show("Lỗi: Tài khoản đăng nhập không khớp với thông tin nhân viên trong bảng EMPLOYEE.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}