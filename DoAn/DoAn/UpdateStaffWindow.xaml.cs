using DoAn.ClassData;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace DoAn.Windows
{
    public partial class UpdateStaffWindow : Window
    {
        private readonly Database db = new Database();

        public UpdateStaffWindow(DataRowView row)
        {
            InitializeComponent();

            // 1. Load ComboBox Loại tài khoản từ bảng ACCOUNT_TYPE
            LoadAccountTypes();

            // 2. Điền dữ liệu từ SQL vào Form
            if (row != null)
            {
                txtAccID.Text = row["Acc_ID"].ToString();
                txtUser.Text = row["User_Name"].ToString();
                txtPass.Text = row["Password"].ToString();
                cboRole.SelectedValue = row["Acc_Type"].ToString();

                // Gán trạng thái
                string status = row["Acc_Status"].ToString();
                foreach (ComboBoxItem item in cboStatus.Items)
                {
                    if (item.Content.ToString() == status)
                    {
                        cboStatus.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void LoadAccountTypes()
        {
            try
            {
                // Truy vấn đúng tên bảng ACCOUNT_TYPE và các cột Acc_Type_ID, Acc_Type_Name
                DataTable dt = db.GetData("SELECT Acc_Type_ID, Acc_Type_Name FROM ACCOUNT_TYPE");
                cboRole.ItemsSource = dt.DefaultView;
                cboRole.DisplayMemberPath = "Acc_Type_Name";
                cboRole.SelectedValuePath = "Acc_Type_ID";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi load loại tài khoản: " + ex.Message);
            }
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUser.Text) || string.IsNullOrWhiteSpace(txtPass.Text) || cboRole.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin!");
                return;
            }

            try
            {
                // Câu lệnh SQL khớp với cấu trúc bảng ACCOUNT của bạn
                string sql = @"UPDATE ACCOUNT 
                               SET User_Name = @user, 
                                   Password = @pass, 
                                   Acc_Type = @type, 
                                   Acc_Status = @status 
                               WHERE Acc_ID = @id";

                SqlParameter[] p = {
                    new SqlParameter("@user", txtUser.Text.Trim()),
                    new SqlParameter("@pass", txtPass.Text.Trim()),
                    new SqlParameter("@type", cboRole.SelectedValue.ToString()),
                    new SqlParameter("@status", (cboStatus.SelectedItem as ComboBoxItem).Content.ToString()),
                    new SqlParameter("@id", txtAccID.Text)
                };

                if (db.Execute(sql, p))
                {
                    MessageBox.Show("Cập nhật tài khoản thành công!", "Thành công");
                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi cập nhật SQL: " + ex.Message);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}