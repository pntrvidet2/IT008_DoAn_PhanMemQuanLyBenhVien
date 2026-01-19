using DoAn.ClassData;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows;

namespace DoAn.Windows
{
    public partial class UpdateEmployeeWindow : Window
    {
        private readonly Database db = new Database();

        // Hàm khởi tạo nhận 5 tham số để điền dữ liệu cũ
        public UpdateEmployeeWindow(string id, string name, string phone, string address, string currentDeptID)
        {
            InitializeComponent();

            // 1. Load ComboBox Khoa trước khi gán giá trị
            LoadDepartments();

            // 2. Gán dữ liệu vào các ô nhập
            txtID.Text = id;
            txtName.Text = name;
            txtPhone.Text = phone;
            txtAddress.Text = address;

            // 3. Chọn đúng khoa hiện tại của nhân viên trong ComboBox
            cbDepartment.SelectedValue = currentDeptID;
        }

        private void LoadDepartments()
        {
            try
            {
                // 1. Lấy dữ liệu từ bảng DEPARTMENT
                string sql = "SELECT Depart_ID, Depart_Name FROM DEPARTMENT";
                DataTable dt = db.GetData(sql);

                if (dt != null && dt.Rows.Count > 0)
                {
                    // 2. Gán nguồn dữ liệu cho ComboBox
                    cbDepartment.ItemsSource = dt.DefaultView;

                    // 3. CHỈNH VIEW: Thiết lập cột hiển thị và cột giá trị
                    // Hiển thị tên khoa cho người dùng chọn
                    cbDepartment.DisplayMemberPath = "Depart_Name";

                    // Giá trị ngầm định bên dưới là mã khoa (để lưu vào bảng Employee)
                    cbDepartment.SelectedValuePath = "Depart_ID";

                    // 4. (Tùy chọn) Tự động chọn dòng đầu tiên để ComboBox không bị trống
                    cbDepartment.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show("Không có dữ liệu khoa trong hệ thống!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi nạp danh sách khoa: " + ex.Message);
            }
        }
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(txtName.Text) || cbDepartment.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng nhập đầy đủ Tên và chọn Khoa!");
                return;
            }

            try
            {
                // Câu lệnh cập nhật SQL
                string sql = @"UPDATE EMPLOYEE 
                               SET Emp_Name = @name, 
                                   Phone = @phone, 
                                   Address = @address, 
                                   Depart_ID = @deptID 
                               WHERE Emp_ID = @id";

                SqlParameter[] parameters = {
                    new SqlParameter("@id", txtID.Text),
                    new SqlParameter("@name", txtName.Text),
                    new SqlParameter("@phone", txtPhone.Text),
                    new SqlParameter("@address", txtAddress.Text),
                    new SqlParameter("@deptID", cbDepartment.SelectedValue.ToString())
                };

                db.Execute(sql, parameters);

                MessageBox.Show("Cập nhật thông tin nhân viên thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true; // Trả về true để MainWindow load lại lưới
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật dữ liệu: " + ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}