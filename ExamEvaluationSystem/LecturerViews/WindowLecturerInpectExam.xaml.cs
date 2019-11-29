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

namespace ExamEvaluationSystem
{
    /// <summary>
    /// WindowLecturerInpectExam.xaml etkileşim mantığı
    /// </summary>
    public partial class WindowLecturerInpectExam : IChildObject<IHasNotifiers>
    {
        public IHasNotifiers ParentObject { get; set; }

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

        public WindowLecturerInpectExam(EISExam ex, List<EISExamResult> res, IHasNotifiers parent)
        {
            BaseExam = ex;
            ParentObject = parent;
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
            RefreshData();
        }

        public void RefreshData()
        {
            foreach (var data in Results)
                Grid.Items.Add(data);
        }

        private void MenuSelectButton(object sender, RoutedEventArgs e)
        {
            using (var dlg = new System.Windows.Forms.FolderBrowserDialog())
            {
                if (dlg.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                {
                    ParentObject.NotifyWarning("Çıktı için klasör belirlenmedi!");
                    return;
                }

                try
                {
                    Exam.ExportStatistics($"{ dlg.SelectedPath }/{ GetNonLocalizedFilename(BaseExam.Period.Name) }_{ GetNonLocalizedFilename(BaseExam.Type.Name) }_{ GetNonLocalizedFilename(BaseExam.Lecture.Name) }_Soru_Bazli_Degerlendirme.xlsx", $"{ dlg.SelectedPath }/{ GetNonLocalizedFilename(BaseExam.Period.Name) }_{ GetNonLocalizedFilename(BaseExam.Type.Name) }_{ GetNonLocalizedFilename(BaseExam.Lecture.Name) }_Kazanim_Bazli_Degerlendirme.xlsx");
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
    }
}
