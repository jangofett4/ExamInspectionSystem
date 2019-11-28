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
    /// ExamEditPanel.xaml etkileşim mantığı
    /// </summary>
    public partial class ExamEditPanel : IChildObject<IHasNotifiers>
    {
        public List<EISQuestion> Questions;
        public EISLecture Lecture;

        private EISSingleQuestion current;
        private string group = "N/A";

        public IHasNotifiers ParentObject { get; set; }

        public List<List<EISSingleQuestion>> AllQuestions;

        public ExamEditPanel(IHasNotifiers parent)
        {
            ParentObject = parent;
            InitializeComponent();
            AllQuestions = new List<List<EISSingleQuestion>>();
        }

        private void ExamEditPanel_Loaded(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void ClearSelected()
        {
            foreach (var data in dgEarnings.Items)
            {
                var x = ((EISEarning)data);
                if (x.Checked)
                    x.Checked = false;
            }
        }

        private List<EISEarning> GetSelectedEarnings()
        {
            var lst = new List<EISEarning>();
            foreach (var data in dgEarnings.Items)
            {
                var x = ((EISEarning)data);
                if (x.Checked)
                {
                    x.Checked = false;
                    lst.Add(x);
                }
            }
            return lst;
        }

        public void RefreshDataGroups()
        {
            AllQuestions = new List<List<EISSingleQuestion>>();
            dgGroups.Items.Clear();
            foreach (var g in Questions)
            {
                var lst = new List<EISSingleQuestion>(g.Answer.Length);
                for (int i = 0; i < g.Answer.Length; i++)
                    lst.Add(new EISSingleQuestion(g.Answer[i], new List<EISEarning>(), i + 1));

                AllQuestions.Add(lst);
                dgGroups.Items.Add(g);
            }
        }

        public void RefreshDataSingleQuestions(int idx)
        {
            dgSingleAnswers.Items.Clear();

            for (int i = 0; i < AllQuestions[idx].Count; i++)
                dgSingleAnswers.Items.Add(AllQuestions[idx][i]);
        }

        public void RefreshDataEarnings()
        {
            dgEarnings.Items.Clear();
            foreach (var e in Lecture.Earnings)
                dgEarnings.Items.Add(e);
        }

        private void DataGridDoubleClickGroup(object sender, MouseButtonEventArgs e)
        {
            if (dgGroups.SelectedItem == null)
                return;

            RefreshDataSingleQuestions(dgGroups.SelectedIndex);
            group = ((EISQuestion)dgGroups.SelectedItem).Group;
            foreach (var er in dgEarnings.Items)
            {
                ((DataGridTemplateColumn)dgEarnings.Columns[0]).GetCellContent(er).FindChild<CheckBox>("Check").IsChecked = false;
                dgEarnings.SelectedItems.Clear();
            }
        }

        private void DataGridDoubleClickAnswers(object sender, MouseButtonEventArgs e)
        {
            if (dgSingleAnswers.SelectedItem == null)
                return;

            current = (EISSingleQuestion)dgSingleAnswers.SelectedItem;
            dgEarnings.SelectedItems.Clear();
            foreach (var er in current.Earnings)
            {
                dgEarnings.SelectedItems.Add(er);
                ((DataGridTemplateColumn)dgEarnings.Columns[0]).GetCellContent(er).FindChild<CheckBox>("Check").IsChecked = true;
            }
            lblSelectedQuestion.Content = $"Grup: { group }, Soru: { current.Nth }, { current.Answer }";
        }

        private void MenuCheckClick(object sender, RoutedEventArgs e)
        {
            if (!ProcessData())
                return;
            DialogResult = true;
        }

        public bool ProcessData()
        {
            List<int> groups = new List<int>();

            for (int i = 0; i < AllQuestions.Count; i++)
            {
                for (int j = 0; j < AllQuestions[i].Count; j++)
                {
                    if (AllQuestions[i][j].Earnings.Count == 0)
                    {
                        if (!groups.Contains(i))
                        {
                            groups.Add(i);
                            break;
                        }
                    }
                }
            }

            if (groups.Count > 0)
            {
                string groupsfriendly = "";
                foreach (var g in groups)
                    groupsfriendly += $"{ Questions[g].Group }, ";
                groupsfriendly = groupsfriendly.Remove(groupsfriendly.Length - 2);
                string message = groupsfriendly + " gruplarında kazanımlarla eşleştirilmemiş sorular bulunmakta.\nKaydetmeden çıkmak için sağ üstteki çıkış butonunu kullanın.";
                ParentObject.NotifyWarning(message);
                return false;
            }

            for (int i = 0; i < Questions.Count; i++)
                Questions[i].ConvertEarnings(AllQuestions[i]);

            return true;
        }

        private void TileCheckClick(object sender, RoutedEventArgs e)
        {
            if (current == null)
            {
                ParentObject.NotifyWarning("Soru seçilmedi!");
                return;
            }

            var selected = new List<EISEarning>();
            foreach (var item in dgEarnings.Items)
            {
                var x = ((DataGridTemplateColumn)dgEarnings.Columns[0]).GetCellContent(item).FindChild<CheckBox>("Check");
                if (x == null)
                    continue;
                if (x.IsChecked == true)
                    selected.Add((EISEarning)item);
                x.IsChecked = false;
            }
            current.Earnings = selected;
            ((TextBlock)(dgSingleAnswers.Columns[2].GetCellContent(current))).Text = current.FriendlyEarnings;
            ParentObject.NotifyInformation("Onaylandı!");
            if (current.Nth != dgSingleAnswers.Items.Count)
            {
                current = (EISSingleQuestion)dgSingleAnswers.Items[current.Nth];
                lblSelectedQuestion.Content = $"Soru: { current.Nth }, { current.Answer }";
                dgSingleAnswers.SelectedItem = current;
                dgEarnings.SelectedItems.Clear();
                foreach (var er in current.Earnings)
                {
                    dgEarnings.SelectedItems.Add(er);
                    ((DataGridTemplateColumn)dgEarnings.Columns[0]).GetCellContent(er).FindChild<CheckBox>("Check").IsChecked = true;
                }
            }
            else
            {
                lblSelectedQuestion.Content = "Seçilen Soru Yok";
                dgSingleAnswers.SelectedItems.Clear();
                current = null;
            }
        }
    }
}
