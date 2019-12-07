using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using ToastNotifications;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;

namespace ExamEvaluationSystem
{
    /// <summary>
    /// WindowLecturerInpectExam.xaml etkileşim mantığı
    /// </summary>
    public partial class WindowLecturerInpectExam : IHasNotifiers
    {

        private struct BasicResultType
        {
            public string Name { get; set; }
            public string Surname { get; set; }
            public string No { get; set; }
            public float Result { get; set; }
        }

        EISStatistics.InternalExam Exam;
        EISExam BaseExam;

        List<BasicResultType> Results;

        public Notifier Notifier { get; set; }

        public WindowLecturerInpectExam(EISExam ex, List<EISExamResult> res)
        {
            BaseExam = ex;
            InitializeComponent();
            var iex = EISStatistics.InternalExam.FromExam(ex, res);
            Results = new List<BasicResultType>();
            foreach (var g in iex.Groups)
            {
                var m = g.GetResultsForQuestions();
                var totals = g.GetPointSumsForQuestions(m);

                for (int i = 0; i < g.Students.Count; i++)
                {
                    var st = g.Students[i];
                    Results.Add(new BasicResultType()
                    {
                        Name = st.Name,
                        Surname = st.Surname,
                        No = st.No,
                        Result = totals[i]
                    });
                }
            }
            Exam = iex;

            Notifier = new Notifier(cfg =>
            {
                cfg.PositionProvider = new WindowPositionProvider(
                    parentWindow: this,
                    corner: Corner.BottomRight,
                    offsetX: 10,
                    offsetY: 10);

                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                    notificationLifetime: TimeSpan.FromSeconds(3),
                    maximumNotificationCount: MaximumNotificationCount.FromCount(3));

                cfg.Dispatcher = Dispatcher;

                cfg.DisplayOptions.TopMost = false; // Needs to be set to false in order to toast to function properly
            });

            RefreshDataGrid();
            SetupSearch();
        }

        public void NotifySuccess(string message)
        {
            Notifier.ShowSuccess(message);
        }

        public void NotifyInformation(string message)
        {
            Notifier.ShowInformation(message);
        }

        public void NotifyWarning(string message)
        {
            NotifierClear();
            Notifier.ShowWarning(message);
        }

        public void NotifyError(string message)
        {
            NotifierClear();
            Notifier.ShowError(message);
        }

        public void NotifierClear()
        {
            Notifier.ClearMessages(new ToastNotifications.Lifetime.Clear.ClearAll());
        }

        public void RefreshDataGrid()
        {
            Grid.Items.Clear();
            foreach (var data in Results)
                Grid.Items.Add(data);
        }

        private void MenuSelectButton(object sender, RoutedEventArgs e)
        {
            using (var dlg = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    NotifyWarning("Çıktı için klasör belirlenmedi!");
                    return;
                }

                /*try
                {*/
                    Exam.ExportStatistics($"{ dlg.SelectedPath }/{ GetNonLocalizedFilename(BaseExam.Period.Name) }_{ GetNonLocalizedFilename(BaseExam.Type.Name) }_{ GetNonLocalizedFilename(BaseExam.Lecture.Name) }_Soru_Bazli_Degerlendirme.xlsx", $"{ dlg.SelectedPath }/{ GetNonLocalizedFilename(BaseExam.Period.Name) }_{ GetNonLocalizedFilename(BaseExam.Type.Name) }_{ GetNonLocalizedFilename(BaseExam.Lecture.Name) }_Kazanim_Bazli_Degerlendirme.xlsx");
                /*}
                catch (Exception ee)
                {
                    NotifyWarning("Değerlendirme çıktısı oluşturma başarısız. Dosya kullanımda olabilir mi?");
                    throw;
                }*/
                NotifySuccess("Değerlendirme çıktısı kaydedildi!");
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

        private void ResetSearchClick(object sender, RoutedEventArgs e)
        {
            RefreshDataGrid();
        }

        private DelayedActionInvoker searchAction;
        private void SetupSearch()
        {
            searchAction = new DelayedActionInvoker(() => {
                Dispatcher.Invoke(() =>
                {
                    Grid.Items.Clear();
                    foreach (var ea in Results)
                        if (ea.No.Contains(searchQuery.Text) || (ea.Name + ea.Surname).ToLower().Contains(searchQuery.Text.ToLower()))
                            Grid.Items.Add(ea);
                });
            }, 1000);
        }

        private void SearchKeyUp(object sender, KeyEventArgs e)
        {
            searchAction.Reset();
        }
    }
}
