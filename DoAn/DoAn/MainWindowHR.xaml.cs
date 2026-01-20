using DoAn.ClassData;
using DoAn.Windows;
using Microsoft.Data.SqlClient;
using Microsoft.Win32;
using System;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;

namespace DoAn
{
    public partial class MainWindowHR : Window
    {
        private readonly Database db = new Database();
        private string currentUser;
        private DispatcherTimer timer;
        private string currentHRMode = "Home"; // Khai báo biến này để dùng cho Search và Export

        public MainWindowHR(string userName, string id)
        {
            InitializeComponent();
            this.currentUser = userName;
            txtHRUsername.Text = userName;

            StartClock();

            this.Loaded += (s, e) => {
                btnHRHome.IsChecked = true;
                SetHRMode("Home");
            };
        }

        private void StartClock()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) => { txtClock.Text = DateTime.Now.ToString("HH:mm:ss"); };
            timer.Start();
        }

        #region ĐIỀU KHIỂN CHẾ ĐỘ (MODE) & BIỂU ĐỒ

        private void SetHRMode(string mode)
        {
            currentHRMode = mode; // Gán giá trị khi chuyển Tab
            if (mode == "Home")
            {
                brdDashboard.Visibility = Visibility.Visible;
                gridHRData.Visibility = Visibility.Collapsed;
                LoadDashboardStats();
            }
            else
            {
                brdDashboard.Visibility = Visibility.Collapsed;
                gridHRData.Visibility = Visibility.Visible;

                bool isManageable = (mode != "Khoa");
                btnHRAddNew.Visibility = isManageable ? Visibility.Visible : Visibility.Collapsed;
                stkHRActions.Visibility = isManageable ? Visibility.Visible : Visibility.Collapsed;

                LoadHRData(mode);
            }
        }

        private void LoadDashboardStats()
        {
            try
            {
         
                int total = Convert.ToInt32(db.ExecuteScalar("SELECT COUNT(*) FROM EMPLOYEE"));
                int doctors = Convert.ToInt32(db.ExecuteScalar("SELECT COUNT(*) FROM EMPLOYEE WHERE Emp_Type = 'DC002'"));
                int departs = Convert.ToInt32(db.ExecuteScalar("SELECT COUNT(*) FROM DEPARTMENT"));

                txtStatTotalEmp.Text = total.ToString();
                txtStatDoctors.Text = doctors.ToString();
                txtStatDeparts.Text = departs.ToString();

              
                int n1 = Convert.ToInt32(db.ExecuteScalar("SELECT COUNT(*) FROM EMPLOYEE WHERE Depart_ID = 'DP001'"));
                int n2 = Convert.ToInt32(db.ExecuteScalar("SELECT COUNT(*) FROM EMPLOYEE WHERE Depart_ID = 'DP008'"));
                int n3 = Convert.ToInt32(db.ExecuteScalar("SELECT COUNT(*) FROM EMPLOYEE WHERE Depart_ID =' 'DP005'"));
                int n4 = total - (n1 + n2 + n3);

                txtVal1.Text = n1.ToString();
                txtVal2.Text = n2.ToString();
                txtVal3.Text = n3.ToString();
                txtVal4.Text = n4.ToString();

             
                if (total > 0)
                {
                    bar1.Height = (n1 * 140.0 / total) + 5;
                    bar2.Height = (n2 * 140.0 / total) + 5;
                    bar3.Height = (n3 * 140.0 / total) + 5;
                    bar4.Height = (n4 * 140.0 / total) + 5;
                }
            }
            catch { }
        }

        private void LoadHRData(string mode)
        {
            string sql = "";
            switch (mode)
            {
                case "NhanVien":
                    // Lấy nhân viên (không phải bác sĩ)
                    sql = "SELECT Emp_ID, Emp_Name, Phone, Address, Depart_ID FROM EMPLOYEE WHERE Emp_Type != 'DC002'";
                    break;
                case "BacSi":
                    // Lấy bác sĩ
                    sql = "SELECT Emp_ID, Emp_Name, Phone, Address, Depart_ID FROM EMPLOYEE WHERE Emp_Type = 'DC002'";
                    break;
                case "Khoa":
                    // Lấy danh sách khoa
                    sql = "SELECT Depart_ID, Depart_Name FROM DEPARTMENT";
                    break;
            }

            try
            {
                if (!string.IsNullOrEmpty(sql))
                {
                    dgHRMain.ItemsSource = db.GetData(sql).DefaultView;
                }
            }
            catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
        }

        #endregion

        #region XUẤT FILE & TÌM KIẾM

        private void btnHRExport_Click(object sender, RoutedEventArgs e)
        {
            if (dgHRMain.ItemsSource == null || currentHRMode == "Home") return;

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                FileName = $"DS_{currentHRMode}_{DateTime.Now:MM_yyyy}.csv"
            };

            if (sfd.ShowDialog() == true)
            {
                try
                {
                    DataTable dt = ((DataView)dgHRMain.ItemsSource).ToTable();
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < dt.Columns.Count; i++)
                        sb.Append(dt.Columns[i].ColumnName + (i == dt.Columns.Count - 1 ? "" : ","));
                    sb.AppendLine();

                    foreach (DataRow row in dt.Rows)
                    {
                        for (int i = 0; i < dt.Columns.Count; i++)
                            sb.Append(row[i].ToString().Replace(",", " ") + (i == dt.Columns.Count - 1 ? "" : ","));
                        sb.AppendLine();
                    }
                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Xuất file tab " + currentHRMode + " thành công!");
                }
                catch { MessageBox.Show("Lỗi xuất file!"); }
            }
        }

        private void btnHRSearch_Click(object sender, RoutedEventArgs e)
        {
            string key = txtHRSearch.Text.Trim();
            if (string.IsNullOrEmpty(key)) { LoadHRData(currentHRMode); return; }

            string sql = currentHRMode switch
            {
                "NhanVien" => "SELECT * FROM EMPLOYEE WHERE (Emp_ID LIKE @k OR Emp_Name LIKE @k) AND Emp_Type != 'DC002'",
                "BacSi" => "SELECT * FROM EMPLOYEE WHERE (Emp_ID LIKE @k OR Emp_Name LIKE @k) AND Emp_Type = 'DC002'",
                "Khoa" => "SELECT * FROM DEPARTMENT WHERE Depart_Name LIKE @k",
                _ => ""
            };
            dgHRMain.ItemsSource = db.GetData(sql, new SqlParameter("@k", "%" + key + "%")).DefaultView;
        }

        #endregion

        #region SỬA - XÓA - MENU
        private void HRMenu_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not ToggleButton btn) return;
            btnHRHome.IsChecked = (btn == btnHRHome); btnHRNhanVien.IsChecked = (btn == btnHRNhanVien);
            btnHRBacSi.IsChecked = (btn == btnHRBacSi); btnHRKhoa.IsChecked = (btn == btnHRKhoa);

            string mode = btn.Name switch
            {
                "btnHRNhanVien" => "NhanVien",
                "btnHRBacSi" => "BacSi",
                "btnHRKhoa" => "Khoa",
                _ => "Home"
            };
            SetHRMode(mode);
        }

        private void btnHRAddNew_Click(object sender, RoutedEventArgs e)
        {
            if (new AddEmployeeWindow { Owner = this }.ShowDialog() == true) LoadHRData(currentHRMode);
        }

        private void btnHRUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (dgHRMain.SelectedItem is not DataRowView row)
            {
                MessageBox.Show("Vui lòng chọn một dòng để cập nhật!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Lấy dữ liệu từ dòng đang chọn
            string id = row["Emp_ID"].ToString();
            string name = row["Emp_Name"].ToString();
            string phone = row["Phone"].ToString();
            string address = row["Address"].ToString();
            string deptID = row["Depart_ID"].ToString(); 

            // Mở cửa sổ sửa và truyền data sang
            var updateWin = new Windows.UpdateEmployeeWindow(id, name, phone, address, deptID) { Owner = this };

            if (updateWin.ShowDialog() == true)
            {
                LoadHRData(currentHRMode); 
                LoadDashboardStats();     
            }
        }

        private void btnHRDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgHRMain.SelectedItem is not DataRowView row) return;
            if (MessageBox.Show("Xóa bản ghi này?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                db.Execute($"DELETE FROM EMPLOYEE WHERE Emp_ID = '{row[0]}'"); LoadHRData(currentHRMode);
            }
        }

        private void HRLogOut_Click(object sender, RoutedEventArgs e)
        {
            this.Effect = new System.Windows.Media.Effects.BlurEffect { Radius = 10 };
            if (new LogoutWindow { Owner = this }.ShowDialog() == true) { new RoleSelectionWindow().Show(); this.Close(); }
            else this.Effect = null;
        }
        private void Max_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
                btnMax.Content = "❐"; // Đổi icon sang 2 ô vuông
            }
            else
            {
                WindowState = WindowState.Normal;
                btnMax.Content = "☐"; // Đổi về 1 ô vuông
            }
        }
        private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { if (e.LeftButton == MouseButtonState.Pressed) DragMove(); }
        private void Min_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void Close_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
        #endregion
    }
}