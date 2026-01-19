using System;
using System.Windows;
using Microsoft.Data.SqlClient;
using DoAn.ClassData;

namespace DoAn.Windows
{
    public partial class UpdateDrugWindow : Window
    {
        private readonly Database db = new Database();

        public string DrugId { get; private set; } = "";
        public decimal NewPrice { get; private set; }
        public int NewQty { get; private set; }

        public UpdateDrugWindow(string drugId, decimal currentPrice, int currentQty)
        {
            InitializeComponent();

            DrugId = drugId;

            txtDrugId.Text = drugId;
            txtPrice.Text = currentPrice.ToString();
            txtQty.Text = currentQty.ToString();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            txtError.Text = "";

            // Validate
            if (!decimal.TryParse(txtPrice.Text.Trim(), out var price) || price < 0)
            {
                txtError.Text = "Giá không hợp lệ (phải là số >= 0).";
                return;
            }

            if (!int.TryParse(txtQty.Text.Trim(), out var qty) || qty < 0)
            {
                txtError.Text = "Hàng tồn không hợp lệ (phải là số nguyên >= 0).";
                return;
            }

            try
            {
                db.Execute(
                    @"UPDATE DRUG
                      SET Drug_Price = @price, Stock_Quantity = @qty
                      WHERE Drug_ID = @id",
                    new SqlParameter("@price", price),
                    new SqlParameter("@qty", qty),
                    new SqlParameter("@id", DrugId)
                );

                NewPrice = price;
                NewQty = qty;

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                txtError.Text = "Lỗi cập nhật: " + ex.Message;
            }
        }
    }
}
