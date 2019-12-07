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
    /// ExamEditPanel.xaml etkileşim mantığı
    /// </summary>
    public partial class ExamEditPanel : IHasNotifiers
    {
        public List<EISQuestion> Questions;
        public EISLecture Lecture;

        private EISSingleQuestion current;
        private string group = "N/A";

        public List<List<EISSingleQuestion>> AllQuestions;

        public Notifier Notifier { get; set; }

        public ExamEditPanel()
        {
            InitializeComponent();
            AllQuestions = new List<List<EISSingleQuestion>>();

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

            Closing += (sender, e) => { ClearSelected(); Notifier.Dispose();  };
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
            ClearSelected();
        }

        private void DataGridDoubleClickAnswers(object sender, MouseButtonEventArgs e)
        {
            if (dgSingleAnswers.SelectedItem == null)
                return;

            current = (EISSingleQuestion)dgSingleAnswers.SelectedItem;
            dgEarnings.SelectedItems.Clear();
            foreach (var er in dgEarnings.Items)
                ((EISEarning)er).Checked = false;
            foreach (var er in current.Earnings)
                er.Checked = true;
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
                NotifyWarning(message);
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
                NotifyWarning("Soru seçilmedi!");
                return;
            }

            var selected = GetSelectedEarnings();
            current.Earnings = selected; current.OnPropertyChanged("FriendlyEarnings"); // trigger this, so datagrid is updated properly
            NotifyInformation("Onaylandı!");
            if (current.Nth != dgSingleAnswers.Items.Count)
            {
                current = (EISSingleQuestion)dgSingleAnswers.Items[current.Nth];
                lblSelectedQuestion.Content = $"Soru: { current.Nth }, { current.Answer }";
                dgSingleAnswers.SelectedItem = current;
                dgEarnings.SelectedItems.Clear();
                foreach (var er in current.Earnings)
                    er.Checked = true;
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
