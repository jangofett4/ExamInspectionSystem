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

            var fkcmd = new SQLiteCommand("PRAGMA foreign_keys = ON;", EISSystem.Connection);
            fkcmd.ExecuteNonQuery();

            using (var rd = new EISFaculty(-1).SelectAll(EISSystem.Connection))
            {
                EISSystem.Faculties = new List<EISFaculty>();
                while (rd.Read())
                    EISSystem.Faculties.Add(new EISFaculty(rd.GetInt32(0), rd.GetString(1)));
            }
            
            using (var rd = new EISEarning(-1).SelectAll(EISSystem.Connection))
            {
                EISSystem.Earnings = new List<EISEarning>();
                EISSystem.DepartmentEarnings = new List<EISEarning>();
                EISSystem.LectureEarnings = new List<EISEarning>();

                while (rd.Read())
                {
                    var e = new EISEarning(rd.GetInt32(0), rd.GetString(1), (EISEarningType)rd.GetInt32(2));
                    
                    // Categorize earnings so they will be easier to use later
                    if (e.EarningType == EISEarningType.Department)
                        EISSystem.DepartmentEarnings.Add(e);
                    else
                        EISSystem.LectureEarnings.Add(e);

                    EISSystem.Earnings.Add(e);
                }
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

            using (var rd = new EISPeriod(-1).SelectAll(EISSystem.Connection))
            {
                EISSystem.Periods = new List<EISPeriod>();
                while (rd.Read())
                    EISSystem.Periods.Add(new EISPeriod(rd.GetInt32(0), rd.GetString(1)));
            }

            using (var rd = new EISLecture(-1).SelectAll(EISSystem.Connection))
            {
                EISSystem.Lectures = new List<EISLecture>();
                while (rd.Read())
                    EISSystem.Lectures.Add(new EISLecture(rd.GetInt32(0), rd.GetString(1), rd.GetInt32(2)));
            }

            using (var rd = new EISExamType(-1).SelectAll(EISSystem.Connection))
            {
                EISSystem.ExamTypes = new List<EISExamType>();
                while (rd.Read())
                    EISSystem.ExamTypes.Add(new EISExamType(rd.GetInt32(0), rd.GetString(1), rd.GetBoolean(2)));
            }

            using (var rd = new EISSelectCommand("System").Create(EISSystem.Connection).ExecuteReader())
            {
                if (!rd.Read())
                {
                    int y = DateTime.Now.Year;
                    string newperiod = y + "-" + (y + 1) + " Dönemi";
                    var result = MessageBox.Show(
                        "Sistemde aktif dönem bulunamadı, bu sistemin ilk defa çalıştığı anlamına gelebilir.\n" +
                        "Yeni aktif dönem oluşturmak için 'OK/Evet' seçeneğini, uygulamadan çıkmak için 'Cancel/İptal' seçeneğini kullanın.\n" +
                        " Oluşturulacak Yeni Dönem: " + newperiod
                    , "Aktif Dönem Bulunamadı", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                    if (result == MessageBoxResult.Cancel)
                    {
                        Environment.Exit(1);    // exit from application without creating a new active period
                        return;                 // SOMEHOW?
                    }

                    var per = new EISPeriod(-1, newperiod);
                    if (per.Insert(EISSystem.Connection) <= 0)
                    {
                        MessageBox.Show("Sisteme yeni dönem ekleme başarısız. Kurulumda eksiklik mi var?", "Hata", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        Environment.Exit(1);    // exit from application without creating a new active period
                        return;                 // SOMEHOW?
                    }

                    new EISInsertCommand("System").Create(EISSystem.Connection, "ActivePeriod", per.ID.ToString()).ExecuteNonQuery();
                    EISSystem.ActivePeriod = per;
                }
                else
                    EISSystem.ActivePeriod = new EISPeriod((int)((long)new EISSelectLastCommand("System", "ActivePeriod").Create(EISSystem.Connection).ExecuteScalar())).SelectT(EISSystem.Connection);
            }

            using (var rd = new EISSelectCommand("Lecturers").Create(EISSystem.Connection).ExecuteReader())
            {
                EISSystem.Lecturers = new List<EISLecturer>();
                while (rd.Read())
                    EISSystem.Lecturers.Add(new EISLecturer(rd.GetInt32(0), rd.GetString(1), rd.GetString(2), EISSystem.GetFaculty(rd.GetInt32(3))));
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
