﻿using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using ExamEvaluationSystem;

using System.Data.SQLite;

using Newtonsoft.Json;

namespace ExamEvaluationSystem
{
    /// <summary>
    /// MainWindow.xaml etkileşim mantığı
    /// </summary>
    public partial class MainWindow
    {
        [DllImport("Kernel32")]
        public static extern void AllocConsole();

        [DllImport("Kernel32")]
        public static extern void FreeConsole();

        public MainWindow()
        {
            /*AllocConsole();

            var dlg = new OpenFileDialog();
            dlg.Title = "Cevap Dosyası Seçin";
            dlg.Filter = "Cevap Dosyası|*.txt";
            dlg.FileName = "*.txt";
            dlg.ShowDialog();
            var file = dlg.FileName;
            EISExamResult.ParseResults(File.ReadAllText(file, Encoding.UTF8));

            FreeConsole();*/

            string configfile = "config.cfg";
            EISSystem.Config = new BasicConfig(configfile);
            if (!File.Exists(configfile))
            {
                try
                {
                    File.WriteAllText(configfile, "SaveLayouts = true\nLayoutsToAppdata = true\nDbToAppdata = true\nDbFile = \"data.db\"", Encoding.UTF8);
                }
                catch (Exception)
                {
                    // Probably UAC, just push default values, nothing we can do anyway
                    EISSystem.Config.Push("SaveLayouts", true, ConfigValueType.Boolean);
                    EISSystem.Config.Push("LayoutsToAppdata", true, ConfigValueType.Boolean);
                    EISSystem.Config.Push("DbToAppdata", true, ConfigValueType.Boolean);
                    EISSystem.Config.Push("DbFile", "data.db", ConfigValueType.Boolean);
                }
            }
            else
            {
                if (!EISSystem.Config.Read())
                {
                    MessageBox.Show("Hatalı konfigürasyon dosyası (config.cfg). Dosyayı sıfırlamak için silmeyi deneyin.\nVarsayılan değerler kullanılacak.", "Hata", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    EISSystem.Config.Push("SaveLayouts", true, ConfigValueType.Boolean);
                    EISSystem.Config.Push("LayoutsToAppdata", true, ConfigValueType.Boolean);
                    EISSystem.Config.Push("DbToAppdata", true, ConfigValueType.Boolean);
                    EISSystem.Config.Push("DbFile", "data.db", ConfigValueType.Boolean);
                }
            }


            string dbfile = "";

            if (EISSystem.Config.GetString("DbFile", out string dbfilename))
            {
                if (!File.Exists(dbfilename))
                {
                    MessageBox.Show("Konfigürasyonda belirtilen veritabanı dosyası ({ dbfilename }) bulunamadı!\nKonfigürasyonu kontrol edin ya da silip sıfırlamayı deneyin.", "Hata", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    Environment.Exit(1);
                }
                dbfile = dbfilename;
            }

            EISSystem.Config.If("DbToAppdata", () => {
                string appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/EIS/";
                if (!Directory.Exists(appdata))
                    Directory.CreateDirectory(appdata);
                appdata = appdata + dbfile;
                if (!File.Exists(appdata))
                    File.Copy(dbfile, appdata);
                dbfile = appdata;
            }, () => dbfile = "./" + dbfile);

            InitializeComponent();
            EISSystem.Connection = new SQLiteConnection($"Data Source={ dbfile }; Version=3;");
            EISSystem.Connection.Open();

            var fkcmd = new SQLiteCommand("PRAGMA foreign_keys = ON;", EISSystem.Connection);
            fkcmd.ExecuteNonQuery();
            fkcmd.Dispose();

            // Data fixers
            {
                var earningFix = new DataFixer.EarningCodeFix(EISSystem.Connection);
                if (earningFix.NeedFix())
                {
                    var res = MessageBox.Show("Görünüşe bakılırsa kullanılan veri tabanı eski kazanım biçimine göre formatlanmış.\nEvet seçeneğini seçerseniz uygulama otomatik olarak bu hataları düzeltecektir.\nHayır seçeneği progamı kapatacaktır\nHatalı kodlamalar yönetici panelinden düzenlenebilir.", "Veritabanı Düzeltme", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if (res == MessageBoxResult.No)
                        Environment.Exit(1);
                    var prog = new UpdateProgress("Kazanım Formatı Güncelleme", "Veri tabanı yeni formata dönüştürülürken lütfen bekleyin...");
                    prog.ExecuteProgress(earningFix.Fix);
                    while (!earningFix.FixDone) ;
                    if (earningFix.PossibleErrors.Count > 0)
                    {
                        string date = DateTime.Now.ToString("dd-MM-yyyy-HH.mm");
                        MessageBox.Show($"Bazı kazanımlar hatalı dönüştürülmüş olabilir.\nHatalı olabilecek kazanımlar listesi için masaüstünde bulunan 'VT.E.Update-{ date }.txt' dosyasından kontrol edebilirsiniz", "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
                        StringBuilder sb = new StringBuilder();
                        foreach (var e in earningFix.PossibleErrors)
                            sb.AppendLine(e.ToString());
                        File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + $"./VT.E.Update-{ date }.txt", sb.ToString());
                    }
                }
                var earningTableFix = new DataFixer.EarningTableFix(EISSystem.Connection);
                if (earningTableFix.NeedFix())
                {
                    earningTableFix.Fix(new Progress<int>((i) => { }));
                }
            }

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
                    var e = new EISEarning(rd.GetInt32(0), rd.GetString(1), rd.GetString(2), (EISEarningType)rd.GetInt32(3));
                    
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
                {
                    var lec = new EISLecture(rd.GetInt32(0));
                    lec.Name = rd.GetString(1);
                    lec.Credit = rd.GetInt32(2);
                    var cmd = new EISSelectCommand("LectureEarnings", $"LectureID = { lec.ID }");
                    using (var sql = cmd.Create(EISSystem.Connection).ExecuteReader())
                    {
                        while (sql.Read())
                            lec.Earnings.Add(EISSystem.GetEarning(sql.GetInt32(0)));
                    }
                    EISSystem.Lectures.Add(lec);
                }
                    
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
                    var date = DateTime.Now;
                    string newperiod = "";
                    if (date.Month > 9 || date.Month <= 2)
                    {
                        if (date.Month <= 2)
                        {
                            newperiod = $"{date.Year - 1}-{ date.Year } Güz Yarıyılı";
                        }
                        else
                        {
                            newperiod = $"{ date.Year }-{ date.Year + 1 } Güz Yarıyılı";
                        }
                    }
                    else
                    {
                        newperiod = $"{ date.Year - 1 }-{ date.Year } Bahar Yarıyılı";
                    }
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
                    EISSystem.Periods.Add(per);
                    EISSystem.ActivePeriod = per;
                }
                else
                    EISSystem.ActivePeriod = new EISPeriod((int)((long)new EISSelectLastCommand("System", "ActivePeriod").Create(EISSystem.Connection).ExecuteScalar())).SelectT(EISSystem.Connection);
            }

            using (var rd = new EISSelectCommand("Lecturers").Create(EISSystem.Connection).ExecuteReader())
            {
                EISSystem.Lecturers = new List<EISLecturer>();
                while (rd.Read())
                {
                    var lec = new EISLecturer(rd.GetInt32(0), rd.GetString(1), rd.GetString(2), EISSystem.GetFaculty(rd.GetInt32(3)));
                    using (var asrd = lec.SelectAssociated(EISSystem.Connection, EISSystem.ActivePeriod))
                    {
                        while (asrd.Read())
                            lec.Associated.Add(EISSystem.GetLecture(asrd.GetInt32(3)));
                        EISSystem.Lecturers.Add(lec);
                    }
                }
            }

            using (var rd = new EISSelectCommand("Exams").Create(EISSystem.Connection).ExecuteReader())
            {
                EISSystem.Exams = new List<EISExam>();
                while (rd.Read())
                {
                    var ex = new EISExam(rd.GetInt32(0), EISSystem.GetLecture(rd.GetInt32(1)), EISSystem.GetPeriod(rd.GetInt32(2)), EISSystem.GetExamType(rd.GetInt32(3)), new List<EISQuestion>());
                    ex.Questions = JsonConvert.DeserializeObject<List<EISQuestion>>(rd.GetString(4));
                    foreach (var q in ex.Questions)
                        q.ConvertEarnings();
                    EISSystem.Exams.Add(ex);
                }
            }

            using (var rd = new EISSelectCommand("Students").Create(EISSystem.Connection).ExecuteReader())
            {
                EISSystem.Students = new List<EISStudent>();
                while (rd.Read())
                {
                    var st = new EISStudent(rd.GetInt32(0).ToString(), rd.GetString(1), rd.GetString(2), EISSystem.GetDepartment(rd.GetInt32(3)));
                    EISSystem.Students.Add(st);
                }
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
                    var lec = EISSystem.GetLecturer(uid);
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
