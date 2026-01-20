using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using DoAn.ClassData;

namespace DoAn.Windows
{
    public partial class AddEmployeeWindow : Window
    {
        Database db = new Database();

        public AddEmployeeWindow()
        {
            InitializeComponent();
            LoadDepartments();

            // Set giá trị mặc định khi mở form
            dpDOB.SelectedDate = DateTime.Now.AddYears(-20);
            dpStartDate.SelectedDate = DateTime.Now;
        }

        private void LoadDepartments()
        {
            try
            {
                // Truy vấn lấy dữ liệu khoa
                DataTable dt = db.GetData("SELECT Depart_ID, Depart_Name FROM DEPARTMENT");

                // Gán dữ liệu vào ComboBox
                cbDept.ItemsSource = dt.DefaultView;

                // QUAN TRỌNG: Phải đặt đúng tên cột để hiển thị chữ
                cbDept.DisplayMemberPath = "Depart_Name";
                cbDept.SelectedValuePath = "Depart_ID";

                if (cbDept.Items.Count > 0) cbDept.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi hiển thị danh sách khoa: " + ex.Message);
            }
        }

        // LOGIC: LẤY ID LỚN NHẤT + 1 (KHÔNG RANDOM)
        private string GenerateNextEmpID()
        {
            try
            {
                // Lấy ra phần số của mã ID lớn nhất hiện tại
                string sql = "SELECT TOP 1 Emp_ID FROM EMPLOYEE ORDER BY CAST(SUBSTRING(Emp_ID, 4, 10) AS INT) DESC";
                DataTable dt = db.GetData(sql);

                int nextNumber = 1;

                if (dt != null && dt.Rows.Count > 0)
                {
                    string currentMaxID = dt.Rows[0]["Emp_ID"].ToString(); // Ví dụ: EMP015
                    string numericPart = currentMaxID.Substring(3); // Cắt lấy "015"
                    nextNumber = int.Parse(numericPart) + 1; // 15 + 1 = 16
                }

                // Trả về định dạng EMP + 3 chữ số (EMP016)
                return "EMP" + nextNumber.ToString("D3");
            }
            catch
            {
                return "EMP001"; // Nếu bảng trống thì bắt đầu từ 001
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra dữ liệu trống
            if (string.IsNullOrWhiteSpace(txtName.Text) || cbDept.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng nhập tên và chọn Khoa!");
                return;
            }

            try
            {
                // 1. Tạo ID tăng dần
                string newID = GenerateNextEmpID();

                // 2. Thu thập dữ liệu từ form
                string name = txtName.Text.Trim();
                string type = ((ComboBoxItem)cbType.SelectedItem).Tag.ToString();
                string gender = ((ComboBoxItem)cbGender.SelectedItem).Content.ToString();
                DateTime dob = dpDOB.SelectedDate ?? DateTime.Now;
                DateTime start = dpStartDate.SelectedDate ?? DateTime.Now;
                string phone = txtPhone.Text.Trim();
                string cid = txtCID.Text.Trim();
                string addr = txtAddress.Text.Trim();
                string email = txtEmail.Text.Trim();
                decimal salary = decimal.TryParse(txtSalary.Text, out decimal s) ? s : 0;
                string departID = cbDept.SelectedValue.ToString();

                // 3. Câu lệnh SQL INSERT
                string sql = @"INSERT INTO EMPLOYEE (Emp_ID, Emp_Name, Emp_Type, Gender, Day_Of_Birth, Start_Date, Phone, CID, Address, Email, Salary, Depart_ID)
                               VALUES (@id, @name, @type, @gender, @dob, @start, @phone, @cid, @addr, @email, @salary, @dept)";

                SqlParameter[] p = new SqlParameter[]
                {
                    new SqlParameter("@id", newID),
                    new SqlParameter("@name", name),
                    new SqlParameter("@type", type),
                    new SqlParameter("@gender", gender),
                    new SqlParameter("@dob", dob),
                    new SqlParameter("@start", start),
                    new SqlParameter("@phone", phone),
                    new SqlParameter("@cid", cid),
                    new SqlParameter("@addr", addr),
                    new SqlParameter("@email", email),
                    new SqlParameter("@salary", salary),
                    new SqlParameter("@dept", departID)
                };

                if (db.Execute(sql, p))
                {
                    MessageBox.Show($"Thêm thành công nhân viên: {newID}");
                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu dữ liệu: " + ex.Message);
            }
        }
    }
}