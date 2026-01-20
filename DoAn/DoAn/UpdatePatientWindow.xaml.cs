using System;
using System.Data;
using System.Windows;
using Microsoft.Data.SqlClient;
using DoAn.ClassData;
using System.Windows.Controls;

namespace DoAn.Windows
{
    public partial class UpdatePatientWindow : Window
    {
        private Database db = new Database();

        // Constructor nhận đủ 7 tham số để sửa lỗi "does not contain a constructor that takes 7 arguments"
        public UpdatePatientWindow(string id, string name, string gender, string phone, string cid, string address, string condition)
        {
            InitializeComponent();

            // 1. Gán dữ liệu vào giao diện
            txtId.Text = id;
            txtName.Text = name;
            txtPhone.Text = phone;
            txtCID.Text = cid;
            txtAddress.Text = address;
            txtCondition.Text = condition;

            // 2. Chọn lại giới tính đúng
            foreach (ComboBoxItem item in cbGender.Items)
            {
                if (item.Content.ToString() == gender)
                {
                    cbGender.SelectedItem = item;
                    break;
                }
            }

            // 3. Tải danh sách bác sĩ
            this.Loaded += (s, e) => LoadDoctorList();
        }

        private void LoadDoctorList()
        {
            try
            {
                string sql = "SELECT Emp_ID, Emp_Name FROM EMPLOYEE WHERE Emp_Type = 'DC002'";
                DataTable dt = db.GetData(sql);
                cbDoctor.ItemsSource = dt.DefaultView;
                cbDoctor.DisplayMemberPath = "Emp_Name";
                cbDoctor.SelectedValuePath = "Emp_ID";

                // Chọn bác sĩ hiện tại của bệnh nhân
                string currentDocSql = "SELECT Emp_ID FROM PATIENT WHERE Patient_ID = @id";
                object docId = db.ExecuteScalar(currentDocSql, new SqlParameter("@id", txtId.Text));
                if (docId != null) cbDoctor.SelectedValue = docId.ToString();
            }
            catch (Exception ex) { txtError.Text = "Lỗi tải bác sĩ: " + ex.Message; }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            txtError.Text = "";

            if (string.IsNullOrEmpty(txtName.Text) || string.IsNullOrEmpty(txtPhone.Text))
            {
                txtError.Text = "Họ tên và SĐT không được để trống!";
                return;
            }

            try
            {
                string sql = @"UPDATE PATIENT SET 
                                Patient_Name = @name, 
                                Gender = @gender,
                                Phone = @phone, 
                                CID = @cid, 
                                Address = @addr, 
                                Curr_Condition = @cond,
                                Emp_ID = @empid
                                WHERE Patient_ID = @id";

                SqlParameter[] p = {
                    new SqlParameter("@id", txtId.Text),
                    new SqlParameter("@name", txtName.Text.Trim()),
                    new SqlParameter("@gender", (cbGender.SelectedItem as ComboBoxItem)?.Content.ToString()),
                    new SqlParameter("@phone", txtPhone.Text.Trim()),
                    new SqlParameter("@cid", txtCID.Text.Trim()),
                    new SqlParameter("@addr", txtAddress.Text.Trim()),
                    new SqlParameter("@cond", txtCondition.Text.Trim()),
                    new SqlParameter("@empid", cbDoctor.SelectedValue ?? (object)DBNull.Value)
                };

                if (db.Execute(sql, p))
                {
                    MessageBox.Show("Cập nhật thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}