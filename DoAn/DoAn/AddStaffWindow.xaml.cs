using DoAn.ClassData;
using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Windows;

namespace DoAn.Windows
{
    public partial class AddStaffWindow : Window
    {
        private readonly Database db = new Database();

        public AddStaffWindow()
        {
            InitializeComponent();
            LoadAccountTypes();
            GenerateNextAccID();
        }

        // 1. Tự sinh mã Acc_ID (AC001, AC002...)
        private void GenerateNextAccID()
        {
            try
            {
                object result = db.ExecuteScalar("SELECT TOP 1 Acc_ID FROM ACCOUNT ORDER BY Acc_ID DESC");
                if (result == null)
                {
                    txtAccID.Text = "AC001";
                }
                else
                {
                    string lastID = result.ToString();
                    int num = int.Parse(lastID.Substring(2)) + 1;
                    txtAccID.Text = "AC" + num.ToString("D3");
                }
            }
            catch
            {
                txtAccID.Text = "AC" + DateTime.Now.ToString("mmss");
            }
        }

        // 2. Load ComboBox từ bảng ACCOUNT_TYPE của bạn
        private void LoadAccountTypes()
        {
            try
            {
                DataTable dt = db.GetData("SELECT Acc_Type_ID, Acc_Type_Name FROM ACCOUNT_TYPE");
                cboRole.ItemsSource = dt.DefaultView;
                cboRole.DisplayMemberPath = "Acc_Type_Name";
                cboRole.SelectedValuePath = "Acc_Type_ID";
                cboRole.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi load loại tài khoản: " + ex.Message);
            }
        }

        // 3. Sự kiện Lưu
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUser.Text) || string.IsNullOrWhiteSpace(txtPass.Password))
            {
                MessageBox.Show("Vui lòng điền đủ thông tin!");
                return;
            }

            try
            {
                // Truy vấn khớp 100% tên bảng ACCOUNT và các cột của bạn
                string sql = @"INSERT INTO ACCOUNT (Acc_ID, User_Name, Password, Acc_Type, Date_Create_Acc, Acc_Status) 
                               VALUES (@id, @user, @pass, @type, @date, @status)";

                SqlParameter[] p = {
                    new SqlParameter("@id", txtAccID.Text),
                    new SqlParameter("@user", txtUser.Text.Trim()),
                    new SqlParameter("@pass", txtPass.Password),
                    new SqlParameter("@type", cboRole.SelectedValue.ToString()),
                    new SqlParameter("@date", DateTime.Now), // smalldatetime
                    new SqlParameter("@status", "Active")    // nvarchar
                };

                if (db.Execute(sql, p))
                {
                    MessageBox.Show("Thêm tài khoản thành công!", "Thông báo");
                    this.DialogResult = true;
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi SQL: " + ex.Message);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}