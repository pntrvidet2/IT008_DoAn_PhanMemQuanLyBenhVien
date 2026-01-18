using System;
using System.Data;
using System.Windows;
using Microsoft.Data.SqlClient;
using DoAn.ClassData;

namespace DoAn.Windows
{
    public partial class UpdateMonitoringWindow : Window
    {
        private Database db = new Database();
        private string _accId;

        public UpdateMonitoringWindow(DataRowView row, string accId)
        {
            InitializeComponent();
            this._accId = accId;

            txtMoniID.Text = row["Moni_Sheet_ID"].ToString();
            txtCondition.Text = row["Curr_Condition"].ToString();
            cboPatient.Text = row["Patient_ID"].ToString();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string sql = "UPDATE MONITORING_SHEET SET Curr_Condition = @cond WHERE Moni_Sheet_ID = @id";
            SqlParameter[] p = {
                new SqlParameter("@cond", txtCondition.Text),
                new SqlParameter("@id", txtMoniID.Text)
            };

            if (db.Execute(sql, p))
            {
                MessageBox.Show("Cập nhật phiếu theo dõi thành công!");
                this.DialogResult = true;
                this.Close();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}