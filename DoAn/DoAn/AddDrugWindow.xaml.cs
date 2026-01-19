using DoAn.ClassData;
using Microsoft.Data.SqlClient;
using System;
using System.Windows;

namespace DoAn
{
    public partial class AddDrugWindow : Window
    {
        private readonly Database db = new Database();

        public AddDrugWindow()
        {
            InitializeComponent();

            // Tự động chạy khi cửa sổ mở lên
            this.Loaded += (s, e) => {
                GenerateDrugID();
            };
        }

        // HÀM TỰ ĐỘNG TẠO MÃ THUỐC
        private void GenerateDrugID()
        {
            try
            {
                // Lấy mã thuốc lớn nhất hiện có (ví dụ D010)
                string sql = "SELECT TOP 1 Drug_ID FROM DRUG ORDER BY Drug_ID DESC";
                object result = db.ExecuteScalar(sql);

                if (result != null && result != DBNull.Value)
                {
                    string lastId = result.ToString(); // Ví dụ: "D010"
                    // Cắt lấy phần số (bỏ chữ 'D' ở đầu)
                    string numericPart = lastId.Substring(1);
                    if (int.TryParse(numericPart, out int lastNumber))
                    {
                        // Tăng lên 1 và định dạng lại 3 chữ số (D + 011)
                        txtId.Text = "D" + (lastNumber + 1).ToString("D3");
                    }
                }
                else
                {
                    // Nếu bảng thuốc trống hoàn toàn
                    txtId.Text = "D001";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tự động tạo mã thuốc: " + ex.Message);
                txtId.Text = "D001";
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra nhập liệu cơ bản
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtPrice.Text))
            {
                txtError.Text = "Vui lòng nhập tên thuốc và giá!";
                return;
            }

            try
            {
                string sql = "INSERT INTO DRUG (Drug_ID, Drug_Name, Drug_Unit, Drug_Price, Stock_Quantity) VALUES (@id, @n, @u, @p, @q)";

                SqlParameter[] p = {
                    new SqlParameter("@id", txtId.Text),
                    new SqlParameter("@n", txtName.Text.Trim()),
                    new SqlParameter("@u", txtUnit.Text.Trim()),
                    new SqlParameter("@p", decimal.Parse(txtPrice.Text)),
                    new SqlParameter("@q", int.Parse(txtQty.Text))
                };

                if (db.Execute(sql, p))
                {
                    MessageBox.Show("Thêm thuốc thành công!", "Thông báo");
                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                txtError.Text = "Lỗi dữ liệu: " + ex.Message;
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}