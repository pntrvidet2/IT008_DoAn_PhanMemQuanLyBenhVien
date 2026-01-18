using DoAn.ClassData;
using Microsoft.Data.SqlClient;
using System;
using System.Windows;
using System.Windows.Controls;

namespace DoAn.Windows
{
    public partial class UpdateBillWindow : Window
    {
        private readonly Database db = new Database();

        // Constructor nhận dữ liệu từ MainWindow truyền qua
        public UpdateBillWindow(string id, string pID, string eID, decimal total, string method, string status)
        {
            InitializeComponent();

            // Đổ dữ liệu vào các ô nhập
            txtBillId.Text = id;
            txtPatientId.Text = pID;
            txtEmpId.Text = eID;
            txtTotal.Text = total.ToString();

            // Thiết lập giá trị cho ComboBox dựa trên chuỗi text
            SetComboBoxValue(cboPaymentMethod, method);
            SetComboBoxValue(cboStatus, status);
        }

        private void SetComboBoxValue(ComboBox cb, string value)
        {
            foreach (ComboBoxItem item in cb.Items)
            {
                if (item.Content.ToString() == value)
                {
                    cb.SelectedItem = item;
                    break;
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTotal.Text))
            {
                MessageBox.Show("Vui lòng nhập số tiền!");
                return;
            }

            try
            {
                // Chỉ UPDATE những trường được phép thay đổi
                string sql = @"UPDATE BILL SET 
                                Total = @total, 
                                Payment_Method = @method, 
                                Payment_Status = @status 
                                WHERE Bill_ID = @id";

                SqlParameter[] p = {
                    new SqlParameter("@total", decimal.Parse(txtTotal.Text)),
                    new SqlParameter("@method", (cboPaymentMethod.SelectedItem as ComboBoxItem).Content.ToString()),
                    new SqlParameter("@status", (cboStatus.SelectedItem as ComboBoxItem).Content.ToString()),
                    new SqlParameter("@id", txtBillId.Text)
                };

                if (db.Execute(sql, p))
                {
                    MessageBox.Show("Cập nhật hóa đơn thành công!", "Thông báo");
                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}