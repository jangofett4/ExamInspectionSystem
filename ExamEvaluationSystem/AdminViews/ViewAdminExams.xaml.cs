using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ExamEvaluationSystem
{
    /// <summary>
    /// ViewExams.xaml etkileşim mantığı
    /// </summary>
    public partial class ViewAdminExams : IChildObject<AdminPanel>
    {
        public AdminPanel ParentObject { get; set; }
        public FlyoutState SideMenuState { get; set; }

        private EISExam itemToEdit;
        private List<EISExamResult> itemToEditResults;

        private List<EISQuestion> questions;

        public ViewAdminExams(AdminPanel parent)
        {
            InitializeComponent();
            ParentObject = parent;
            selectorExamLectures.ClickCallback = () =>
            {
                var s = new PropertyDataSelector("Ders Seç", 350);
                var builder = new SingleDataSelectorBuilder<EISLecture>(EISSystem.Lectures, s, "ID", (l, q) => l.Name.ToLower().Contains(q) ? true : false, ("Name", "Ders Adı"), ("Credit", "Kredi"));
                builder.DisableSearch = false;
                builder.BuildAll();

                if (selectorExamLectures.SelectedData != null)
                {
                    builder.GridSelect(s.dgSelector, selectorExamLectures.SelectedData); // Pre-Select data in grid
                    builder.SelectedData = (EISLecture)selectorExamLectures.SelectedData;
                }

                s.ShowDialog();

                if (builder.SelectedData == null)
                {
                    selectorExamLectures.Text = "";
                    return;
                }

                if (SideMenuState == FlyoutState.Edit)
                    itemToEdit.Lecture = builder.SelectedData;

                selectorExamLectures.SelectedData = builder.SelectedData;
                selectorExamLectures.Text = builder.SelectedData.Name;
            };

            selectorExamPeriods.SelectedData = EISSystem.ActivePeriod;
            selectorExamPeriods.Text = EISSystem.ActivePeriod.Name;
            selectorExamPeriods.ClickCallback = () =>
            {
                /*var s = new PropertyDataSelector("Dönem Seç");
                var builder = new SingleDataSelectorBuilder<EISPeriod>(EISSystem.Periods, s, "ID", ("Name", "Dönem"));
                builder.BuildAll();

                if (selectorExamPeriods.SelectedData != null)
                {
                    builder.GridSelect(s.dgSelector, selectorExamPeriods.SelectedData); // Pre-Select data in grid
                    builder.SelectedData = (EISPeriod)selectorExamPeriods.SelectedData;
                }

                s.ShowDialog();

                if (builder.SelectedData == null)
                {
                    selectorExamPeriods.Text = "";
                    return;
                }

                selectorExamPeriods.SelectedData = builder.SelectedData;
                selectorExamPeriods.Text = builder.SelectedData.Name;*/
            };

            selectorExamTypes.ClickCallback = () =>
            {
                var s = new PropertyDataSelector("Sınav Türü Seç", 300);
                var builder = new SingleDataSelectorBuilder<EISExamType>(EISSystem.ExamTypes, s, "ID", ("Name", "Sınav Türü")/*, ("Multiple", "Çoklu Sınav")*/);
                builder.BuildAll();

                if (selectorExamTypes.SelectedData != null)
                {
                    builder.GridSelect(s.dgSelector, selectorExamTypes.SelectedData); // Pre-Select data in grid
                    builder.SelectedData = (EISExamType)selectorExamTypes.SelectedData;
                }

                s.ShowDialog();

                if (builder.SelectedData == null)
                {
                    selectorExamTypes.Text = "";
                    return;
                }

                selectorExamTypes.SelectedData = builder.SelectedData;
                selectorExamTypes.Text = builder.SelectedData.Name;
            };

            selectorExamOptics.ClickCallback = () =>
            {
                if (SideMenuState == FlyoutState.Add)
                {
                    if (string.IsNullOrWhiteSpace(selectorExamAnswers.Text))
                    {
                        ParentObject.NotifyWarning("Cevaplar anahtarı eklenmeden sınav sonuçları eklemek mümkün değil!");
                        return;
                    }

                    OpenFileDialog dlg = new OpenFileDialog()
                    {
                        Title = "Sınav Sonuçlarını Seçin",
                        FileName = "*.txt",
                        Filter = "Sonuç Dosyaları|*.txt"
                    };

                    var d = dlg.ShowDialog();
                    if (d.HasValue && d.Value)
                    {
                        var res = EISExamResult.ParseResults(File.ReadAllText(dlg.FileName, Encoding.UTF8));
                        if (res == null)
                        {
                            ParentObject.NotifyWarning("Sonuç dosyası formatı hatalı! Yanlış dosya mı seçtiniz?");
                            return;
                        }

                        var panel = new ExamStudentPanel();
                        panel.ExamResults = res;
                        panel.Resources.Add("GroupItemSource", ((EISExam)selectorExamAnswers.SelectedData).Groups);
                        panel.RefreshStudents();
                        panel.Exam = (EISExam)selectorExamAnswers.SelectedData;
                        panel.ProcessAnswers();
                        var result = panel.ShowDialog();
                        if (result.HasValue && result.Value)
                        {
                            selectorExamOptics.Text = dlg.FileName;
                            selectorExamOptics.SelectedData = panel.ExamResults;
                            ParentObject.NotifyInformation("Sınav sonuçları onaylandı!");
                        }
                        else
                        {
                            selectorExamOptics.Text = "";
                            ParentObject.NotifyInformation("Sınav sonuçları onaylanmadı.");
                        }
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(selectorExamAnswers.Text))
                    {
                        ParentObject.NotifyWarning("Cevaplar anahtarı eklenmeden sınav sonuçları eklemek mümkün değil!");
                        return;
                    }

                    var panel = new ExamStudentPanel();
                    panel.ExamResults = itemToEditResults;
                    panel.Resources.Add("GroupItemSource", itemToEdit.Groups);
                    panel.RefreshStudents();
                    panel.Exam = itemToEdit;
                    panel.ProcessAnswers(true);
                    var result = panel.ShowDialog();
                    if (result.HasValue && result.Value)
                    {
                        ParentObject.NotifyInformation("Sınav sonuçlarındaki değişiklikler onaylandı!");
                        selectorExamOptics.Text = "[ ... ]";
                    }
                    else
                        ParentObject.NotifyInformation("Sınav sonuçlarındaki değişiklikler iptal edildi.");
                }
            };

            selectorExamAnswers.ClickCallback = () =>
            {
                if (SideMenuState == FlyoutState.Add)
                {
                    if (string.IsNullOrWhiteSpace(selectorExamLectures.Text))
                    {
                        ParentObject.NotifyWarning("Cevap anahtarı eklenmeden önce dersin seçilmesi gerek!");
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(selectorExamPeriods.Text))
                    {
                        ParentObject.NotifyWarning("Cevap anahtarı eklenmeden önce dönemin seçilmesi gerek!");
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(selectorExamTypes.Text))
                    {
                        ParentObject.NotifyWarning("Cevap anahtarı eklenmeden sınav türünün seçilmesi gerek!");
                        return;
                    }

                    OpenFileDialog dlg = new OpenFileDialog()
                    {
                        Title = "Cevap Anahtarı Seçin",
                        FileName = "*.txt",
                        Filter = "Sonuç Dosyaları|*.txt"
                    };
                    var d = dlg.ShowDialog();
                    if (d.HasValue && d.Value)
                    {
                        var res = EISExam.ParseAnswers(File.ReadAllText(dlg.FileName, Encoding.UTF8));
                        if (res == null)
                        {
                            ParentObject.NotifyWarning("Cevap anahtarı formatı hatalı! Yanlış dosya mı seçtiniz?");
                            return;
                        }

                        var panel = new ExamEditPanel();
                        panel.Lecture = (EISLecture)selectorExamLectures.SelectedData;
                        panel.lblLecture.Content = "Ders: " + panel.Lecture.Name;
                        panel.Questions = res;
                        panel.RefreshDataGroups();
                        panel.RefreshDataEarnings();
                        var result = panel.ShowDialog();

                        if (result.HasValue && result.Value)
                        {
                            selectorExamAnswers.Text = dlg.FileName;
                            selectorExamAnswers.SelectedData = new EISExam(-1, (EISLecture)selectorExamLectures.SelectedData, (EISPeriod)selectorExamPeriods.SelectedData, (EISExamType)selectorExamTypes.SelectedData, res);
                            questions = res;
                            ParentObject.NotifyInformation("Cevap anahtarı onaylandı!");
                        }
                        else
                            ParentObject.NotifyInformation("Cevap anahtarı onaylanmadı.");
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(selectorExamLectures.Text))
                    {
                        ParentObject.NotifyWarning("Cevap anahtarı eklenmeden önce dersin seçilmesi gerek!");
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(selectorExamPeriods.Text))
                    {
                        ParentObject.NotifyWarning("Cevap anahtarı eklenmeden önce dönemin seçilmesi gerek!");
                        return;
                    }

                    if (string.IsNullOrWhiteSpace(selectorExamTypes.Text))
                    {
                        ParentObject.NotifyWarning("Cevap anahtarı eklenmeden sınav türünün seçilmesi gerek!");
                        return;
                    }

                    var panel = new ExamEditPanel();
                    panel.Lecture = itemToEdit.Lecture;
                    panel.lblLecture.Content = "Ders: " + panel.Lecture.Name;
                    panel.Questions = itemToEdit.Questions;
                    panel.RefreshDataGroups(true);
                    panel.RefreshDataEarnings();
                    var result = panel.ShowDialog();

                    if (result.HasValue && result.Value)
                        ParentObject.NotifyInformation("Cevap anahtarındaki değişiklikler onaylandı!");
                    else
                        ParentObject.NotifyInformation("Cevap anahtarında yapılan değişiklikler iptal edildi.");
                }
            };
            RefreshDataGrid();
            SetupSearch();
        }

        public void RefreshDataGrid()
        {
            Grid.Items.Clear();
            foreach (var data in EISSystem.Exams)
                Grid.Items.Add(data);
        }

        private void GridDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (Grid.SelectedItem == null)
                return;

            var item = (EISExam)Grid.SelectedItem;
            itemToEdit = item;
            selectorExamLectures.SelectedData = item.Lecture;
            selectorExamLectures.Text = item.LectureName;

            selectorExamPeriods.SelectedData = item.Period;
            selectorExamPeriods.Text = item.PeriodName;

            selectorExamTypes.SelectedData = item.Type;
            selectorExamTypes.Text = item.ExamType;

            selectorExamAnswers.SelectedData = item.Questions;
            questions = item.Questions;
            selectorExamAnswers.Text = $"{ item.Questions.Count } grup";

            itemToEditResults = new EISExamResult(-1).SelectList(EISSystem.Connection, Where.Equals("ExamID", item.ID.ToString()));
            foreach (var r in itemToEditResults)
                r.Student.Store(); // store inital

            selectorExamOptics.SelectedData = itemToEditResults;
            selectorExamOptics.Text = "[ ... ]";

            // add answer editing and optic editing?

            SideMenuState = FlyoutState.Edit;
            sideFlyout.IsOpen = true;
            sideFlyout.Header = "Düzenle";
        }

        private void TileAddClick(object sender, RoutedEventArgs e)
        {
            selectorExamPeriods.SelectedData = EISSystem.ActivePeriod;
            selectorExamPeriods.Text = EISSystem.ActivePeriod.Name;
            sideFlyout.Header = "Ekle";
            sideFlyout.IsOpen = true;
            SideMenuState = FlyoutState.Add;
        }

        private async void TileDeleteClick(object sender, RoutedEventArgs e)
        {
            var dataToDelete = GetSelectedExams();

            if (dataToDelete.Count == 0)
            {
                ParentObject.NotifyWarning("Silmek istediğiniz verileri tablodan seçin");
                return;
            }

            var result = await ParentObject.ShowMessageAsync("Uyarı", $"{ dataToDelete.Count } sınav bilgisi silinecek (bağıntılı olan sınav sonuçları dahil).\nEmin misiniz?", MessageDialogStyle.AffirmativeAndNegative);
            if (result == MessageDialogResult.Affirmative)
            {
                foreach (var data in dataToDelete)
                {
                    if (data.Period.ID != EISSystem.ActivePeriod.ID)
                    {
                        ParentObject.NotifyWarning("Geçmiş dönem sınavları arşivlendiği için silinemez!");
                        return;
                    }
                }

                foreach (var data in dataToDelete) // for each data selected
                {
                    Grid.Items.Remove(data);            // Remove from grid
                    EISSystem.Exams.Remove(data);       // Remove from system in memory
                    data.Delete(EISSystem.Connection);  // Remove from database
                }
                ParentObject.ViewAdminInspectExam.Exams = ParentObject.GenerateExamTriple();
                ParentObject.ViewAdminInspectExam.RefreshData();
                ParentObject.NotifyInformation($"{ dataToDelete.Count } sınav bilgisi silindi.");
            }
            else
            {
                Grid.SelectedIndex = -1;
                ParentObject.NotifyInformation("Silme işlemi iptal edildi.");
            }
        }

        public void ClearSelected()
        {
            foreach (var data in Grid.Items)
            {
                var x = ((EISExam)data);
                if (x.Checked)
                    x.Checked = false;
            }
        }

        private List<EISExam> GetSelectedExams()
        {
            var lst = new List<EISExam>();
            foreach (var data in Grid.Items)
            {
                var x = ((EISExam)data);
                if (x.Checked)
                {
                    x.Checked = false;
                    lst.Add(x);
                }
            }
            return lst;
        }

        private void TileExportClick(object sender, RoutedEventArgs e)
        {
            var sel = GetSelectedExams();
            if (sel.Count == 0)
            {
                ParentObject.NotifyWarning("Listeden sınav seçin!");
                return;
            }
            if (sel.Count > 1)
            {
                ParentObject.NotifyWarning("Birden fazla sınav seçildi, ilk seçim kullanılacak.");
            }

            using (var dlg = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    ParentObject.NotifyWarning("Çıktı için klasör belirlenmedi!");
                    return;
                }

                var ex = sel[0];
                var res = new EISExamResult(-1).SelectList(EISSystem.Connection, Where.Equals("ExamID", ex.ID.ToString()));
                var exam = EISStatistics.InternalExam.FromExam(ex, res);

                try
                {
                    exam.ExportStatistics($"{ dlg.SelectedPath }/{ GetNonLocalizedFilename(ex.Period.Name) }_{ GetNonLocalizedFilename(ex.Type.Name) }_{ GetNonLocalizedFilename(ex.Lecture.Name) }_Soru_Bazli_Degerlendirme.xlsx", $"{ dlg.SelectedPath }/{ GetNonLocalizedFilename(ex.Period.Name) }_{ GetNonLocalizedFilename(ex.Type.Name) }_{ GetNonLocalizedFilename(ex.Lecture.Name) }_Kazanim_Bazli_Degerlendirme.xlsx");
                }
                catch (Exception)
                {
                    ParentObject.NotifyWarning("Değerlendirme çıktısı oluşturma başarısız. Dosya kullanımda olabilir mi?");
                    return;
                }
                ParentObject.NotifySuccess("Değerlendirme çıktısı kaydedildi!");
            }
        }

        private static string GetNonLocalizedFilename(string file)
        {
            return file.Replace(' ', '_')
                .Replace('İ', 'I')
                .Replace('ı', 'i')
                .Replace('ş', 's').Replace('Ş', 'S')
                .Replace('ç', 'c').Replace('Ç', 'C')
                .Replace('ö', 'o').Replace('Ö', 'O')
                .Replace('ğ', 'g').Replace('Ğ', 'G')
                .Replace('ü', 'u').Replace('Ü', 'u');
        }

        private void TileFlyoutDoneClick(object sender, RoutedEventArgs e)
        {
            if (selectorExamLectures.SelectedData == null)
            {
                ParentObject.NotifyWarning("Ders alanı boş bırakılamaz!");
                return;
            }
            if (selectorExamPeriods.SelectedData == null)
            {
                ParentObject.NotifyWarning("Dönem alanı boş bırakılamaz!");
                return;
            }
            if (selectorExamTypes.SelectedData == null)
            {
                ParentObject.NotifyWarning("Sınav türü alanı boş bırakılamaz!");
                return;
            }
            if (questions == null)
            {
                ParentObject.NotifyWarning("Cevap anahtarı alanı boş bırakılamaz!");
                return;
            }
            if (selectorExamOptics.SelectedData == null)
            {
                ParentObject.NotifyWarning("Optik alanı boş bırakılamaz!");
                return;
            }
            if (SideMenuState == FlyoutState.Add)
            {
                var trn = new EISTransaction();
                trn.Begin(EISSystem.Connection);

                var ex = new EISExam(
                    (EISLecture)selectorExamLectures.SelectedData,
                    (EISPeriod)selectorExamPeriods.SelectedData,
                    (EISExamType)selectorExamTypes.SelectedData,
                    questions
                );
                var result = ex.Insert(EISSystem.Connection);

                if (result == -1)
                {
                    ParentObject.NotifyError($"Sistemde { ex.PeriodName } dönemi için { ex.LectureName } için { ex.Type.Name } sınavı zaten mevcut!");
                    trn.Rollback(EISSystem.Connection);
                    return;
                }

                var ress = (List<EISExamResult>)selectorExamOptics.SelectedData;
                if (ress == null)
                {
                    ParentObject.NotifyError("Sonuç belgesi seçilmedi!");
                    trn.Rollback(EISSystem.Connection);
                    return;
                }
                foreach (var v in ress)
                {
                    if (v.Student.Insert(EISSystem.Connection) == -1)
                        v.Student.Update(EISSystem.Connection);
                    if (EISSystem.GetStudent(v.Student.ID) == null)
                        EISSystem.Students.Add(v.Student);
                    v.Exam = ex;
                    v.Insert(EISSystem.Connection);
                }

                trn.Commit(EISSystem.Connection);


                EISSystem.Exams.Add(ex);
                Grid.Items.Add(ex);
                ParentObject.ViewAdminInspectExam.Exams = ParentObject.GenerateExamTriple();
                ParentObject.ViewAdminInspectExam.RefreshData();
                ParentObject.NotifySuccess("Sınav ekleme başarılı!");

                sideFlyout.IsOpen = false;
                questions = null;
            }
            else
            {
                var trn = new EISTransaction();
                trn.Begin(EISSystem.Connection);
                itemToEdit.Store();
                itemToEdit.Lecture = (EISLecture)selectorExamLectures.SelectedData;
                itemToEdit.Type = (EISExamType)selectorExamTypes.SelectedData;
                var result = itemToEdit.Update(EISSystem.Connection);

                if (result == -1)
                {
                    ParentObject.NotifyError($"Düzenleme başarısız: Sistemde { itemToEdit.PeriodName } dönemi için { itemToEdit.LectureName } için { itemToEdit.Type.Name } sınavı zaten mevcut!");
                    itemToEdit.Restore();
                    trn.Rollback(EISSystem.Connection);
                    return;
                }

                var ress = (List<EISExamResult>)selectorExamOptics.SelectedData;
                if (ress == null)
                {
                    ParentObject.NotifyError("Sonuç belgesi seçilmedi!");
                    itemToEdit.Restore();
                    trn.Rollback(EISSystem.Connection);
                    return;
                }

                foreach (var v in ress)
                {
                    v.Student.UpdateWhere(EISSystem.Connection, Where.Equals("ID", (string)v.Student.GetStored("ID")));
                    v.Update(EISSystem.Connection);
                }

                trn.Commit(EISSystem.Connection);

                ParentObject.ViewAdminInspectExam.Exams = ParentObject.GenerateExamTriple();
                ParentObject.ViewAdminInspectExam.RefreshData();
                ParentObject.NotifySuccess("Sınav düzenleme başarılı!");

                sideFlyout.IsOpen = false;
            }
        }

        private void TileSearchClick(object sender, RoutedEventArgs e)
        {
            searchFlyout.IsOpen = true;
        }

        private void ResetSearchClick(object sender, RoutedEventArgs e)
        {
            RefreshDataGrid();
            TileResetSearch.Visibility = Visibility.Hidden;
        }

        private DelayedActionInvoker searchAction;
        private void SetupSearch()
        {
            searchAction = new DelayedActionInvoker(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    Grid.Items.Clear();
                    foreach (var ea in EISSystem.Exams)
                    {
                        var sq = searchQuery.Text.ToLower();
                        if (ea.PeriodName.ToLower().Contains(sq) || ea.LectureName.ToLower().Contains(sq) || ea.ExamType.ToLower().Contains(sq))
                            Grid.Items.Add(ea);
                    }
                    TileResetSearch.Visibility = Visibility.Visible;
                });
            }, 1000);
        }

        private void SearchKeyUp(object sender, KeyEventArgs e)
        {
            searchAction.Reset();
            if (e.Key == Key.Enter)
                searchFlyout.IsOpen = false;
        }

        public string GetGridLayout()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var c in Grid.Columns)
            {
                sb.Append(c.DisplayIndex);
                sb.Append('/');
                sb.Append(c.Width.Value);
                sb.Append('/');
                sb.Append((int)c.Width.UnitType);
                sb.Append('/');
            }
            return sb.ToString();
        }

        public void SetGridLayout(string lay)
        {
            var split = lay.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            int i = 0;
            foreach (var c in Grid.Columns)
            {
                c.DisplayIndex = int.Parse(split[i++]);
                c.Width = new DataGridLength(double.Parse(split[i++]), (DataGridLengthUnitType)int.Parse(split[i++]));
            }
        }
    }
}
