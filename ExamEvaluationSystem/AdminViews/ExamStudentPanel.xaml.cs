using MahApps.Metro.Controls;
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
    /// ExamStudentPanel.xaml etkileşim mantığı
    /// </summary>
    public partial class ExamStudentPanel : IHasNotifiers
    {
        public List<EISExamResult> ExamResults;
        public EISExam Exam;

        public Notifier Notifier { get; set; }

        public ExamStudentPanel()
        {
            InitializeComponent();

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

            Closing += (sender, e) => {
                Notifier.Dispose();
            };
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

        public void  RefreshStudents()
        {
            foreach (var r in ExamResults)
                dgStudents.Items.Add(r);
        }

        public List<List<EISSingleQuestion>> Questions;
        public void ProcessAnswers(bool prefill = false)
        {
            var answerlen = Exam.Questions[0].Answer.Length;
            Questions = new List<List<EISSingleQuestion>>();
            foreach (var res in ExamResults)
            {
                var lst = new List<EISSingleQuestion>();
                res.CutAnswer(0, answerlen);
                int i = 1;
                foreach (var q in res.Answers)
                    lst.Add(new EISSingleQuestion(q, null, i++));
                Questions.Add(lst);
            }
        }

        private void MenuCheckClick(object sender, RoutedEventArgs e)
        {
            var needno = new StringBuilder();
            var neednoi = 0;

            foreach (var i in dgStudents.Items)
            {
                var val = (EISExamResult)i;
                val.No = val.No.Trim().TrimStart('0'); // yes, we explicitly get rid of zeroes at the start
                val.Name = val.Name.Trim();
                val.Surname = val.Surname.Trim();

                if (neednoi == 4)
                    break;

                if ((string.IsNullOrWhiteSpace(val.No) || val.No.Length != 9) && neednoi < 4)
                {
                    if (neednoi == 3)
                        needno.Append($"{ val.Student.Name } { val.Student.Surname }");
                    else
                        needno.Append($"{ val.Student.Name } { val.Student.Surname }, ");
                    neednoi++;
                }
            }

            if (neednoi > 0)
            {
                string str = needno.ToString();
                if (neednoi == 4)
                    NotifyWarning($"Onaylama başarısız, numara alanlarında geçeriz veriler mevcut: { str }...");
                else
                    NotifyWarning($"Onaylama başarısız, numara alanlarında geçeriz veriler mevcut: { str }.");
                return;
            }

            for (int i = 0; i < Questions.Count; i++)
            {
                StringBuilder sb = new StringBuilder();
                var elem = ExamResults.ElementAt(i);
                sb.Append(elem.Group);
                foreach (var c in Questions[i])
                    sb.Append(c.Answer);
                elem.SetEncryptedAnswer(sb.ToString());
                elem.Student = ((EISExamResult)dgStudents.Items[i]).Student;
            }

            DialogResult = true;
        }

        private int selectedIndex = -1;
        private void TileCheckClick(object sender, RoutedEventArgs e)
        {
            if (selectedIndex == -1)
            {
                NotifyWarning("Tablodan öğrenci seçin!");
                return;
            }

            NotifyInformation("Onaylandı!");
            if (selectedIndex + 1 != dgStudents.Items.Count)
            {
                selectedIndex++;
                dgStudents.SelectedIndex = selectedIndex;
                dgAnswers.Items.Clear();
                foreach (var v in Questions[selectedIndex])
                    dgAnswers.Items.Add(v);
            }
            else
            {
                dgAnswers.Items.Clear();
                selectedIndex = -1;
            }
        }

        private void DataGridButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var sel = dgStudents.SelectedItem;
                if (sel != null)
                {
                    var res = (EISExamResult)sel;
                    dgAnswers.Items.Clear();

                    if (Exam.Questions.Count == 0)
                    {
                        NotifyError($"Cevap anahtarında grup bulunamadı, tekrar yüklemeyi deneyin!");
                        return;
                    }

                    dgAnswers.Items.Clear();
                    selectedIndex = dgStudents.SelectedIndex;
                    foreach (var v in Questions[dgStudents.SelectedIndex])
                        dgAnswers.Items.Add(v);
                }
            }
            catch (Exception ex)
            {
                NotifyError($"Cevap anahtarında grup bulunamadı, tekrar yüklemeyi deneyin!\nGeliştirici hata mesajı: { ex.Message }");
                Close();
            }
        }

        private void MenuSelectButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
