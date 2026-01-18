using DoAn.ClassData;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace DoAn.Windows
{
    public partial class AddPatientWindow : Window
    {
        private readonly Database db = new Database();

        public AddPatientWindow()
        {
            InitializeComponent();
            this.Loaded += (s, e) => {
                GeneratePatientID();
                LoadDoctorList();
                dpDob.SelectedDate = DateTime.Now.AddYears(-20);
            };
        }

        private void GeneratePatientID()
        {
            try
            {
                string sql = "SELECT TOP 1 Patient_ID FROM PATIENT ORDER BY Patient_ID DESC";
                object result = db.ExecuteScalar(sql);
                if (result != null && result != DBNull.Value)
                {
                    string lastId = result.ToString();
                    int lastNumber = int.Parse(lastId.Substring(3));
                    txtId.Text = "PAT" + (lastNumber + 1).ToString("D3");
                }
                else
                {
                    txtId.Text = "PAT001";
                }
            }
            catch { txtId.Text = "PAT001"; }
        }

        private void LoadDoctorList()
        {
            try
            {
                string sql = "SELECT Emp_ID, Emp_Name FROM EMPLOYEE WHERE Emp_Type = 'DC002' ORDER BY Emp_Name ASC";
                DataTable dt = db.GetData(sql);
                cbDoctor.ItemsSource = dt.DefaultView;
                cbDoctor.DisplayMemberPath = "Emp_Name";
                cbDoctor.SelectedValuePath = "Emp_ID";
                if (dt.Rows.Count > 0) cbDoctor.SelectedIndex = 0;
            }
            catch (Exception ex) { txtError.Text = "Lỗi tải bác sĩ: " + ex.Message; }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            txtError.Text = "";
            if (string.IsNullOrEmpty(txtName.Text) || string.IsNullOrEmpty(txtPhone.Text) || string.IsNullOrEmpty(txtCID.Text))
            {
                txtError.Text = "Vui lòng nhập đầy đủ các thông tin bắt buộc (*)!";
                return;
            }

            try
            {
                string sql = @"INSERT INTO PATIENT (Patient_ID, Patient_Name, Gender, Day_Of_Birth, Phone, CID, Address, Curr_Condition, Emp_ID) 
                               VALUES (@id, @name, @gender, @dob, @phone, @cid, @addr, @cond, @empid)";

                SqlParameter[] p = {
                    new SqlParameter("@id", txtId.Text),
                    new SqlParameter("@name", txtName.Text.Trim()),
                    new SqlParameter("@gender", (cbGender.SelectedItem as ComboBoxItem).Content.ToString()),
                    new SqlParameter("@dob", dpDob.SelectedDate.Value),
                    new SqlParameter("@phone", txtPhone.Text.Trim()),
                    new SqlParameter("@cid", txtCID.Text.Trim()),
                    new SqlParameter("@addr", txtAddress.Text.Trim()),
                    new SqlParameter("@cond", txtCondition.Text.Trim()),
                    new SqlParameter("@empid", cbDoctor.SelectedValue?.ToString() ?? (object)DBNull.Value)
                };

                if (db.Execute(sql, p))
                {
                    MessageBox.Show("Thêm bệnh nhân thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
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