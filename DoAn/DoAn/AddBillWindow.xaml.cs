using DoAn.ClassData;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace DoAn.Windows
{
    public partial class AddBillWindow : Window
    {
        private readonly Database db = new Database();

        // Constructor nhận mã nhân viên từ MainWindow truyền sang
        public AddBillWindow(string empId)
        {
            InitializeComponent();

            // Gán mã nhân viên và khóa lại
            txtEmpId.Text = empId;

            // Tự động tạo mã hóa đơn và tải danh sách bệnh nhân
            GenerateNextBillID();
            LoadPatients();
        }

        // 1. Hàm tự động tăng mã hóa đơn (Dạng B00001)
        private void GenerateNextBillID()
        {
            try
            {
                // Lấy mã hóa đơn lớn nhất hiện có
                object result = db.ExecuteScalar("SELECT TOP 1 Bill_ID FROM BILL ORDER BY Bill_ID DESC");

                if (result == null || string.IsNullOrEmpty(result.ToString()))
                {
                    txtBillId.Text = "B001";
                }
                else
                {
                    string lastID = result.ToString(); // Ví dụ: "B00005"
                    // Cắt bỏ chữ 'B', chuyển phần còn lại sang số và cộng 1
                    int num = int.Parse(lastID.Substring(1)) + 1;
                    // Format lại thành chữ B + 5 chữ số (D5)
                    txtBillId.Text = "B" + num.ToString("D3");
                }
            }
            catch
            {
                // Nếu lỗi, tạo mã dựa trên thời gian để tránh trùng lặp
                txtBillId.Text = "B" + DateTime.Now.ToString("HHmmss");
            }
        }

        // 2. Hàm tải danh sách bệnh nhân từ SQL vào ComboBox
        private void LoadPatients()
        {
            try
            {
                DataTable dt = db.GetData("SELECT Patient_ID, Patient_Name FROM PATIENT");
                var patientList = new List<object>();

                foreach (DataRow row in dt.Rows)
                {
                    patientList.Add(new
                    {
                        Patient_ID = row["Patient_ID"].ToString(),
                        DisplayText = row["Patient_ID"].ToString() + " - " + row["Patient_Name"].ToString()
                    });
                }

                cboPatient.ItemsSource = patientList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách bệnh nhân: " + ex.Message);
            }
        }

        // 3. Sự kiện Lưu hóa đơn
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra nhập liệu
            if (cboPatient.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn bệnh nhân!", "Thông báo");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTotal.Text) || !decimal.TryParse(txtTotal.Text, out decimal totalValue))
            {
                MessageBox.Show("Vui lòng nhập tổng tiền hợp lệ!", "Thông báo");
                return;
            }

            try
            {
                string sql = @"INSERT INTO BILL (Bill_ID, Patient_ID, Emp_ID, Date, Total, Payment_Method, Payment_Status) 
                               VALUES (@id, @pID, @eID, @date, @total, @method, @status)";

                SqlParameter[] p = {
                    new SqlParameter("@id", txtBillId.Text),
                    new SqlParameter("@pID", cboPatient.SelectedValue.ToString()),
                    new SqlParameter("@eID", txtEmpId.Text),
                    new SqlParameter("@date", DateTime.Now),
                    new SqlParameter("@total", totalValue),
                    new SqlParameter("@method", (cboPaymentMethod.SelectedItem as ComboBoxItem).Content.ToString()),
                    new SqlParameter("@status", (cboStatus.SelectedItem as ComboBoxItem).Content.ToString())
                };

                // Thực thi lệnh INSERT
                db.Execute(sql, p);

                MessageBox.Show("Thêm hóa đơn thành công!", "Thành công");
                this.DialogResult = true; // Báo về MainWindow để load lại DataGrid
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi lưu hóa đơn: " + ex.Message, "Lỗi SQL");
            }
        }

        // 4. Sự kiện Hủy
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}