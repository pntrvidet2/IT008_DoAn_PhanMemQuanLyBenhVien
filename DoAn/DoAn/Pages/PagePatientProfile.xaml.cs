using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Data.SqlClient;
using DoAn.ClassData;

namespace DoAn.Pages
{
    public partial class PagePatientProfile : Page
    {
        private readonly Database db = new Database();
        private readonly string patientID;

        public PagePatientProfile(string patientID)
        {
            InitializeComponent();
            this.patientID = patientID;
            LoadPatient();
            SetEditMode(false);
        }

        private void LoadPatient()
        {
            string sql = @"
SELECT Patient_ID, Patient_Name, Gender, Day_Of_Birth, Phone, CID, Address
FROM PATIENT
WHERE Patient_ID = @id;
";
            DataTable dt = db.GetData(sql, new SqlParameter("@id", patientID));
            if (dt.Rows.Count == 0) return;

            DataRow r = dt.Rows[0];
            txtPatientId.Text = r["Patient_ID"].ToString();
            txtName.Text = r["Patient_Name"].ToString();
            txtPhone.Text = r["Phone"].ToString();
            txtCID.Text = r["CID"].ToString();
            txtAddress.Text = r["Address"].ToString();
            dpDOB.SelectedDate = Convert.ToDateTime(r["Day_Of_Birth"]);

            cbGender.SelectedIndex = r["Gender"].ToString() == "Female" ? 1 : 0;
        }

        private void SetEditMode(bool edit)
        {
            txtName.IsReadOnly = !edit;
            txtPhone.IsReadOnly = !edit;
            txtCID.IsReadOnly = !edit;
            txtAddress.IsReadOnly = !edit;

            cbGender.IsEnabled = edit;
            dpDOB.IsEnabled = edit;

            btnSave.IsEnabled = edit;
        }

        // ✅ BẮT BUỘC PHẢI CÓ
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            SetEditMode(true);
        }

        // ✅ BẮT BUỘC PHẢI CÓ
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string gender = (cbGender.SelectedItem as ComboBoxItem)?.Content.ToString();

            string sql = @"
UPDATE PATIENT
SET Patient_Name=@name,
    Gender=@gender,
    Day_Of_Birth=@dob,
    Phone=@phone,
    CID=@cid,
    Address=@addr
WHERE Patient_ID=@id;
";
            db.Execute(sql,
                new SqlParameter("@name", txtName.Text),
                new SqlParameter("@gender", gender),
                new SqlParameter("@dob", dpDOB.SelectedDate),
                new SqlParameter("@phone", txtPhone.Text),
                new SqlParameter("@cid", txtCID.Text),
                new SqlParameter("@addr", txtAddress.Text),
                new SqlParameter("@id", patientID)
            );

            MessageBox.Show("Cập nhật thành công!");
            SetEditMode(false);
            LoadPatient();
        }
    }
}
