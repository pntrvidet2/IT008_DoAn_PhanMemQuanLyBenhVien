using System;
using System.Data;
using System.Windows;
using Microsoft.Data.SqlClient;
using DoAn.ClassData;

namespace DoAn.Windows
{
    public partial class AddPrescriptionWindow : Window
    {
        Database db = new Database();
        decimal currentDrugPrice = 0;
        bool isEditMode = false;

        public AddPrescriptionWindow() // Dùng để THÊM
        {
            InitializeComponent();
            LoadData();
        }

        public AddPrescriptionWindow(string bID, string dID, int q) // Dùng để SỬA
        {
            InitializeComponent();
            LoadData();
            isEditMode = true;
            txtTitle.Text = "CẬP NHẬT CHI TIẾT";
            cboBill.SelectedValue = bID; cboBill.IsEnabled = false;
            cboDrug.SelectedValue = dID; cboDrug.IsEnabled = false;
            txtQty.Text = q.ToString();
        }

        private void LoadData()
        {
            cboBill.ItemsSource = db.GetData("SELECT Bill_ID FROM BILL").DefaultView;
            cboDrug.ItemsSource = db.GetData("SELECT Drug_ID, Drug_Name, Drug_Price FROM DRUG").DefaultView;
        }

        private void cboDrug_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (cboDrug.SelectedItem is DataRowView row)
            {
                currentDrugPrice = Convert.ToDecimal(row["Drug_Price"]);
                Calculate();
            }
        }

        private void txtQty_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) => Calculate();

        private void Calculate()
        {
            if (int.TryParse(txtQty.Text, out int q))
                lblTotal.Text = $"Thành tiền: {string.Format("{0:N0}", q * currentDrugPrice)} VNĐ";
            else lblTotal.Text = "Thành tiền: 0 VNĐ";
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string bID = cboBill.SelectedValue?.ToString();
            string dID = cboDrug.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(bID) || string.IsNullOrEmpty(dID) || !int.TryParse(txtQty.Text, out int q)) return;

            try
            {
                // 1. KIỂM TRA SỐ LƯỢNG TỒN KHO TRƯỚC
                int stockNow = Convert.ToInt32(db.ExecuteScalar($"SELECT Stock_Quantity FROM DRUG WHERE Drug_ID = '{dID}'"));

                // Nếu là chế độ SỬA, ta phải cộng lại số lượng cũ vào kho tạm thời để tính toán
                int oldQty = 0;
                if (isEditMode)
                {
                    oldQty = Convert.ToInt32(db.ExecuteScalar($"SELECT Drug_Quantity FROM PRESCRIPTION_DETAILS WHERE Bill_ID = '{bID}' AND Drug_ID = '{dID}'"));
                }

                if (q > (stockNow + oldQty))
                {
                    MessageBox.Show($"Không đủ thuốc trong kho! (Còn lại: {stockNow + oldQty})", "Cảnh báo");
                    return;
                }

                // 2. THỰC HIỆN CẬP NHẬT CHI TIẾT ĐƠN THUỐC
                decimal amount = q * currentDrugPrice;
                string sql = isEditMode
                    ? "UPDATE PRESCRIPTION_DETAILS SET Drug_Quantity = @q, Amount = @a WHERE Bill_ID = @b AND Drug_ID = @d"
                    : "INSERT INTO PRESCRIPTION_DETAILS (Bill_ID, Drug_ID, Drug_Quantity, Amount) VALUES (@b, @d, @q, @a)";

                SqlParameter[] p = { new SqlParameter("@b", bID), new SqlParameter("@d", dID), new SqlParameter("@q", q), new SqlParameter("@a", amount) };

                if (db.Execute(sql, p))
                {
                    // 3. CẬP NHẬT TRỪ/CỘNG LẠI TỒN KHO TRONG BẢNG DRUG
                    // Công thức: Kho mới = Kho hiện tại + Số lượng cũ - Số lượng mới
                    int change = oldQty - q;
                    db.Execute($"UPDATE DRUG SET Stock_Quantity = Stock_Quantity + ({change}) WHERE Drug_ID = '{dID}'");

                    // 4. CẬP NHẬT TỔNG TIỀN CHO BILL
                    db.Execute("UPDATE BILL SET Total = (SELECT ISNULL(SUM(Amount),0) FROM PRESCRIPTION_DETAILS WHERE Bill_ID = @b) WHERE Bill_ID = @b", new SqlParameter("@b", bID));

                    MessageBox.Show("Lưu thành công và đã cập nhật kho hàng!");
                    this.DialogResult = true;
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}