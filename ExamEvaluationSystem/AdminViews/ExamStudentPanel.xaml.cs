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

namespace ExamEvaluationSystem
{
    /// <summary>
    /// ExamStudentPanel.xaml etkileşim mantığı
    /// </summary>
    public partial class ExamStudentPanel : IChildObject<IHasNotifiers>
    {
        public IHasNotifiers ParentObject { get; set; }

        public Dictionary<EISStudent, EISExamResult> ExamResults;
        public EISExam Exam;

        public ExamStudentPanel(IHasNotifiers parent)
        {
            ParentObject = parent;
            InitializeComponent();
        }

        public void RefreshStudents()
        {
            foreach (var r in ExamResults)
                dgStudents.Items.Add(r.Value);
        }

        public List<List<EISSingleQuestion>> Questions;
        public void ProcessAnswers()
        {
            var answerlen = Exam.Questions[0].Answer.Length;
            Questions = new List<List<EISSingleQuestion>>();
            foreach (var res in ExamResults)
            {
                var lst = new List<EISSingleQuestion>();
                res.Value.CutAnswer(0, answerlen);
                int i = 1;
                foreach (var q in res.Value.Answers)
                    lst.Add(new EISSingleQuestion(q, null, i++));
                Questions.Add(lst);
            }
        }

        private void MenuCheckClick(object sender, RoutedEventArgs e)
        {
            var c1 = ((DataGridTemplateColumn)dgAnswers.Columns[0]);
            var needno = new StringBuilder();
            var needgroup = new StringBuilder();
            var neednoi = 0;
            var needgroupi = 0;

            foreach (var i in dgStudents.Items)
            {
                var val = (EISExamResult)i;
                
                if (neednoi > 3 && needgroupi > 3)
                    break;

                if (val.No.Length != 9 && neednoi < 3)
                {
                    needno.Append($"{ val.Student.Name } { val.Student.Surname }, ");
                    neednoi++;
                }

                if (string.IsNullOrWhiteSpace(val.Group.ToString()) && needgroupi < 3)
                {
                    needgroup.Append($"{ val.Student.Name } { val.Student.Surname }, ");
                    needgroupi++;
                }
            }

            if (neednoi > 0)
            {
                string str = needno.ToString();
                str.Substring(0, str.Length - 2);
                if (neednoi == 3)
                    ParentObject.NotifyWarning($"Onaylama başarısız, numara alanlarında geçeriz veriler mevcut: { str }...");
                else
                    ParentObject.NotifyWarning($"Onaylama başarısız, numara alanlarında geçeriz veriler mevcut: { str }.");
                return;
            }

            if (needgroupi > 0)
            {
                string str = needgroup.ToString();
                str.Substring(0, str.Length - 2);
                if (neednoi == 3)
                    ParentObject.NotifyWarning($"Onaylama başarısız, grup alanlarında geçeriz veriler mevcut: { str }...");
                else
                    ParentObject.NotifyWarning($"Onaylama başarısız, grup alanlarında geçeriz veriler mevcut: { str }.");
                return;
            }

            for (int i = 0; i < Questions.Count; i++)
            {
                StringBuilder sb = new StringBuilder();
                var elem = ExamResults.ElementAt(i);
                sb.Append(elem.Value.Group);
                foreach (var c in Questions[i])
                    sb.Append(c.Answer);
                elem.Value.SetEncryptedAnswer(sb.ToString());
            }

            DialogResult = true;
        }

        private int selectedIndex = -1;
        private void TileCheckClick(object sender, RoutedEventArgs e)
        {
            if (selectedIndex == -1)
            {
                ParentObject.NotifyWarning("Tablodan öğrenci seçin!");
                return;
            }

            var c1 = ((DataGridTemplateColumn)dgAnswers.Columns[0]);
            foreach (var a in dgAnswers.Items)
            {
                var t = c1.GetCellContent(a).FindChild<TextBox>("Text").Text;
                var s = ' ';
                if (!string.IsNullOrWhiteSpace(t))
                    s = t[0];
                ((EISSingleQuestion)a).Answer = s;
            }
            ParentObject.NotifyInformation("Onaylandı!");
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
                        ParentObject.NotifyError($"Cevap anahtarında grup bulunamadı, tekrar yüklemeyi deneyin!");
                        return;
                    }
                    
                    dgAnswers.Items.Clear();
                    selectedIndex = dgStudents.SelectedIndex;
                    foreach (var v in Questions[dgStudents.SelectedIndex])
                        dgAnswers.Items.Add(v);
                }
            }
            catch(Exception ex)
            {
                ParentObject.NotifyError($"Cevap anahtarında grup bulunamadı, tekrar yüklemeyi deneyin!\nGeliştirici hata mesajı: { ex.Message }");
                Close();
            }
        }

        private void MenuSelectButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
