using MahApps.Metro.Controls;

using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ExamEvaluationSystem
{
    /// <summary>
    /// MainWindow.xaml etkileşim mantığı
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            EISSystem.Connection = new SQLiteConnection($"Data Source=./data.db; Version=3;");
            EISSystem.Connection.Open();

            using (var rd = new EISFaculty(-1).SelectAll(EISSystem.Connection))
            {
                EISSystem.Faculties = new List<EISFaculty>();
                while (rd.Read())
                    EISSystem.Faculties.Add(new EISFaculty(rd.GetInt32(0), rd.GetString(1)));
            }

            using (var rd = new EISDepartment(-1).SelectAll(EISSystem.Connection))
            {
                EISSystem.Departments = new List<EISDepartment>();
                while (rd.Read())
                {
                    var dep = new EISDepartment(rd.GetInt32(0));
                    dep.Name = rd.GetString(2);
                    dep.Faculty = EISSystem.GetFaculty(rd.GetInt32(1));
                    var cmd = new EISSelectCommand("DepartmentEarnings", $"DepartmentID = { dep.ID }");

                    using (var sql = cmd.Create(EISSystem.Connection).ExecuteReader())
                    {
                        while (sql.Read())
                            dep.Earnings.Add(EISSystem.GetEarning(sql.GetInt32(1)));
                    }
                    EISSystem.Departments.Add(dep);
                }
            }

            using (var rd = new EISEarning(-1).SelectAll(EISSystem.Connection))
            {
                EISSystem.Earnings = new List<EISEarning>();
                while (rd.Read())
                    EISSystem.Earnings.Add(new EISEarning(rd.GetInt32(0), rd.GetString(1), (EISEarningType)rd.GetInt32(2)));
            }
        }

        private void ClickLoginButton(object sender, RoutedEventArgs e)
        {
            var select = new EISSelectCommand("UserLoginInfo", Where.Equals("Username", UsernameTextbox.Text.EncapsulateQuote(), "Password", PasswordTextbox.Password.EncapsulateQuote()));
            var sql = select.Create(EISSystem.Connection);
            using (var reader = sql.ExecuteReader())
            {
                if (!reader.HasRows)
                {
                    MessageBox.Show("Kullanıcı adı veya şifre hatalı!", "Hata", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    return;
                }

                reader.Read();

                int privilege = reader.GetInt32(4);
                int uid = reader.GetInt32(3);

                // Lecturer
                if (privilege == 1)
                {
                    var lec = new EISLecturer(uid);
                    lec = lec.SelectT(EISSystem.Connection);
                    var panel = new UserPanel(lec);
                    panel.Show();
                    Close();
                }
                // Admin
                else if (privilege == 2)
                {
                    var panel = new AdminPanel();
                    panel.Show();
                    Close();
                }
                // Debug
                else
                {
                    /*var panel = new DebugPanel();
                    panel.Show();
                    Close();*/
                }

                reader.Close();
            }
        }
    }
}
