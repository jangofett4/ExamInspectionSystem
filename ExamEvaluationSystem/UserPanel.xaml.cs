using System;
using System.Collections.Generic;
using System.IO;
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
using ToastNotifications.Core;
using ToastNotifications.Lifetime;
using ToastNotifications.Messages;
using ToastNotifications.Position;

namespace ExamEvaluationSystem
{
    /// <summary>
    /// UserPanel.xaml etkileşim mantığı
    /// </summary>
    public partial class UserPanel : IHasNotifiers
    {
        public EISLecturer Lecturer { get; set; }

        // Views
        public ViewLecturerAddExam ExamView;
        public ViewLecturerViewExam InspectView;

        public Notifier Notifier { get; set; }

        public UserPanel(EISLecturer lec)
        {
            InitializeComponent();

            /*
            var s = new PropertyDataSelector("Title");
            var builder = new SingleDataSelectorBuilder<EISLecture>(EISSystem.Lectures, s, "ID", ("Name", "Ders Adı"), ("Credit", "Kredi"));
            builder.BuildAll();
            s.ShowDialog();
            */
            Lecturer = lec;
            Resources.Add("CurrentLecturerID", lec.ID.ToString());
            Resources.Add("CurrentLecturerName", lec.Name + " " + lec.Surname);
            // UserHamburgerMenuFrame.Navigate(new Uri("ViewLecturerAddExam.xaml", UriKind.RelativeOrAbsolute));

            /* Initialize views */
            ExamView = new ViewLecturerAddExam(this);
            InspectView = new ViewLecturerViewExam(GenerateExamTriple(), this);
            /* End of view initialization */

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

            UserHamburgerMenuFrame.Content = ExamView; // Initial view

            Loaded += (sender, e) => EISSystem.Config.If("SaveLayouts", LoadLayouts);
            Closing += (sender, e) => EISSystem.Config.If("SaveLayouts", SaveLayouts);
        }

        public void LoadLayouts()
        {
            try
            {
                var path = ".";
                EISSystem.Config.If("LayoutsToAppdata", () => path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/EIS/");
                string file = path + "/user.layout";
                if (!File.Exists(file))
                    return;
                var content = File.ReadAllText(file, Encoding.UTF8);
                var split = content.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var i = 0;
                ExamView.SetGridLayout(split[i++]);
                InspectView.SetGridLayout(split[i++]);
            }
            catch (Exception)
            {
                return;
            }
        }

        public void SaveLayouts()
        {
            try
            {
                StringBuilder save = new StringBuilder();
                MemoryRenderer.Render(
                    ExamView,
                    InspectView
                );
                save.AppendLine(ExamView.GetGridLayout());
                save.AppendLine(InspectView.GetGridLayout());
                var path = ".";
                EISSystem.Config.If("LayoutsToAppdata", () => path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/EIS/");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                File.WriteAllText(path + "/user.layout", save.ToString(), Encoding.UTF8);
            }
            catch (Exception)
            {
                return;
            }
        }

        public List<EISExamTriple> GenerateExamTriple()
        {
            var distinctex = new List<EISExam>();
            foreach (var de in EISSystem.Exams)
            {
                bool found = false;
                foreach (var disc in distinctex)
                {
                    if (disc.LectureName == de.LectureName && disc.PeriodName == de.PeriodName)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                    distinctex.Add(de);
            }

            var lst = new List<EISExamTriple>();
            foreach (var e in distinctex)
            {
                lst.Add(new EISExamTriple()
                {
                    Lecture = e.Lecture,
                    Period = e.Period,
                    Visa = EISSystem.GetExam(e.Lecture, e.Period, 1),
                    Final = EISSystem.GetExam(e.Lecture, e.Period, 2),
                    Complement = EISSystem.GetExam(e.Lecture, e.Period, 3),
                });
            }
            return lst;
        }

        private void HamburgerItemClick(object sender, MahApps.Metro.Controls.ItemClickEventArgs e)
        {
            if (e.ClickedItem == null) return;
            var tag = ((MahApps.Metro.Controls.HamburgerMenuItem)e.ClickedItem).Tag.ToString();
            switch (tag)
            {
                case "1":
                     UserHamburgerMenuFrame.Content = ExamView;
                    break;
                case "2":
                    UserHamburgerMenuFrame.Content = InspectView;
                    break;
                case "3":
                    helpFlyout.IsOpen = true;
                    break;
                default:
                    break;
            }
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
    }
}
