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

        private List<EISQuestion> questions;

        public ViewAdminExams(AdminPanel parent)
        {
            InitializeComponent();
            ParentObject = parent;
            selectorExamLectures.ClickCallback = () =>
            {
                var s = new PropertyDataSelector("Ders Seç");
                var builder = new SingleDataSelectorBuilder<EISLecture>(EISSystem.Lectures, s, "ID", ("Name", "Ders Adı"), ("Credit", "Kredi"));
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

                selectorExamLectures.SelectedData = builder.SelectedData;
                selectorExamLectures.Text = builder.SelectedData.Name;
            };

            selectorExamPeriods.ClickCallback = () =>
            {
                var s = new PropertyDataSelector("Dönem Seç");
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
                selectorExamPeriods.Text = builder.SelectedData.Name;
            };

            selectorExamTypes.ClickCallback = () =>
            {
                var s = new PropertyDataSelector("Sınav Türü Seç");
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

                    var panel = new ExamStudentPanel(ParentObject);
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
                    }
                }
            };

            selectorExamAnswers.ClickCallback = () =>
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

                    var panel = new ExamEditPanel(ParentObject);
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
                    }
                    else
                        ParentObject.NotifyInformation("Cevap anahtarı onaylanmadı.");
                }
            };
            RefreshDataGrid();
        }

        public void RefreshDataGrid()
        {
            Grid.Items.Clear();
            foreach (var data in EISSystem.Exams)
                Grid.Items.Add(data);
        }

        private void GridDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void TileAddClick(object sender, RoutedEventArgs e)
        {
            sideFlyout.Header = "Ekle";
            sideFlyout.IsOpen = true;
            SideMenuState = FlyoutState.Add;
        }

        private async void TileDeleteClick(object sender, RoutedEventArgs e)
        {
            var dataToDelete = new List<EISExam>();
            foreach (var item in Grid.Items)
            {
                var x = ((DataGridTemplateColumn)Grid.Columns[0]).GetCellContent(item).FindChild<CheckBox>("Check");
                if (x.IsChecked == true)
                    dataToDelete.Add((EISExam)item);
            }

            if (dataToDelete.Count == 0)
            {
                ParentObject.NotifyWarning("Silmek istediğiniz verileri tablodan seçin");
                return;
            }

            var result = await ParentObject.ShowMessageAsync("Uyarı", $"{ dataToDelete.Count } sınav bilgisi silinecek (bağıntılı olan sınav sonuçları dahil).\nEmin misiniz?", MessageDialogStyle.AffirmativeAndNegative);
            if (result == MessageDialogResult.Affirmative)
            {
                foreach (var data in dataToDelete) // for each data selected
                {
                    Grid.Items.Remove(data);            // Remove from grid
                    EISSystem.Exams.Remove(data);       // Remove from system in memory
                    data.Delete(EISSystem.Connection);  // Remove from database
                }
                ParentObject.NotifyInformation($"{ dataToDelete.Count } sınav bilgisi silindi.");
            }
            else
            {
                Grid.SelectedIndex = -1;
                ParentObject.NotifyInformation("Silme işlemi iptal edildi.");
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

            var ex = sel[0];
            var res = new EISExamResult(-1).SelectList(EISSystem.Connection, Where.Equals("ExamID", ex.ID.ToString()));
            var lmao = EISStatistics.InternalExam.FromExam(ex, res);
        }


        private void UpdateCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            ((CheckBox)sender).GetBindingExpression(CheckBox.IsCheckedProperty).UpdateTarget();
        }
        private void UpdateCheckBoxUnchecked(object sender, RoutedEventArgs e)
        {
            ((CheckBox)sender).GetBindingExpression(CheckBox.IsCheckedProperty).UpdateTarget();
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

            var trn = new EISTransaction();
            trn.Begin(EISSystem.Connection);

            var ex = new EISExam(
                (EISLecture)selectorExamLectures.SelectedData,
                (EISPeriod)selectorExamPeriods.SelectedData,
                (EISExamType)selectorExamTypes.SelectedData,
                questions
            );
            var result = ex.Insert(EISSystem.Connection);

            var dict = (Dictionary<EISStudent, EISExamResult>)selectorExamOptics.SelectedData;
            if (dict == null)
            {
                ParentObject.NotifyError("Sonuç belgesi seçilmedi!");
                return;
            }
            foreach (var v in dict)
            {
                if (v.Key.Insert(EISSystem.Connection) == -1)
                    v.Key.Update(EISSystem.Connection);
                v.Value.Exam = ex;
                v.Value.Insert(EISSystem.Connection);
            }

            trn.Commit(EISSystem.Connection);


            EISSystem.Exams.Add(ex);
            Grid.Items.Add(ex);
            ParentObject.NotifySuccess("Sınav ekleme başarılı!");

            sideFlyout.IsOpen = false;
            questions = null;
        }
    }
}
