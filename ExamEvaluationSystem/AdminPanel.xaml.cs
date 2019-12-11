using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using ToastNotifications;
using ToastNotifications.Messages;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using System.IO;

namespace ExamEvaluationSystem
{
    /// <summary>
    /// AdminPanel.xaml etkileşim mantığı
    /// </summary>
    public partial class AdminPanel : IHasNotifiers
    {
        public ViewAdminDepartment ViewAdminDepartment;
        public ViewAdminEarnings ViewAdminEarnings;
        public ViewAdminExams ViewAdminExams;
        public ViewAdminFaculty ViewAdminFaculty;
        public ViewAdminLecturers ViewAdminLecturers;
        public ViewAdminLectures ViewAdminLectures;
        public ViewAdminPeriods ViewAdminPeriods;
        public ViewAdminLectureAssociate ViewAdminLectureAssociate;
        public ViewAdminInspectExam ViewAdminInspectExam;

        public Notifier Notifier { get; set; }

        public AdminPanel()
        {
            InitializeComponent();

            /**/
            ViewAdminDepartment = new ViewAdminDepartment(this);
            ViewAdminEarnings = new ViewAdminEarnings(this);
            ViewAdminExams = new ViewAdminExams(this);
            ViewAdminFaculty = new ViewAdminFaculty(this);
            ViewAdminLecturers = new ViewAdminLecturers(this);
            ViewAdminLectures = new ViewAdminLectures(this);
            ViewAdminPeriods = new ViewAdminPeriods(this);
            ViewAdminLectureAssociate = new ViewAdminLectureAssociate(this);
            ViewAdminInspectExam = new ViewAdminInspectExam(GenerateExamTriple(), this);
            /**/

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

            AdminHamburgerMenuFrame.Content = ViewAdminFaculty;

            Loaded += (sender, e) => EISSystem.Config.If("SaveLayouts", LoadLayouts);
            Closing += (sender, e) => EISSystem.Config.If("SaveLayouts", SaveLayouts);
        }

        public void LoadLayouts()
        {
            try
            {
                var path = ".";
                EISSystem.Config.If("LayoutsToAppdata", () => path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/EIS/");
                string file = path + "/admin.layout";
                if (!File.Exists(file))
                    return;
                var content = File.ReadAllText(file, Encoding.UTF8);
                var split = content.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                var i = 0;
                ViewAdminDepartment.SetGridLayout(split[i++]);
                ViewAdminEarnings.SetGridLayout(split[i++]);
                ViewAdminExams.SetGridLayout(split[i++]);
                ViewAdminFaculty.SetGridLayout(split[i++]);
                ViewAdminLecturers.SetGridLayout(split[i++]);
                ViewAdminLectures.SetGridLayout(split[i++]);
                ViewAdminPeriods.SetGridLayout(split[i++]);
                ViewAdminLectureAssociate.SetGridLayout(split[i++]);
                ViewAdminInspectExam.SetGridLayout(split[i++]);
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
                // Render the grids, so columns are actually have index at least once
                MemoryRenderer.Render(
                    ViewAdminDepartment.Grid,
                    ViewAdminEarnings.Grid,
                    ViewAdminExams.Grid,
                    ViewAdminFaculty.Grid,
                    ViewAdminLecturers.Grid,
                    ViewAdminLectures.Grid,
                    ViewAdminPeriods.Grid,
                    ViewAdminLectureAssociate.Grid,
                    ViewAdminInspectExam.Grid
                );
                save.AppendLine(ViewAdminDepartment.GetGridLayout());
                save.AppendLine(ViewAdminEarnings.GetGridLayout());
                save.AppendLine(ViewAdminExams.GetGridLayout());
                save.AppendLine(ViewAdminFaculty.GetGridLayout());
                save.AppendLine(ViewAdminLecturers.GetGridLayout());
                save.AppendLine(ViewAdminLectures.GetGridLayout());
                save.AppendLine(ViewAdminPeriods.GetGridLayout());
                save.AppendLine(ViewAdminLectureAssociate.GetGridLayout());
                save.AppendLine(ViewAdminInspectExam.GetGridLayout());
                var path = ".";
                EISSystem.Config.If("LayoutsToAppdata", () => path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "/EIS/");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                File.WriteAllText(path + "/admin.layout", save.ToString(), Encoding.UTF8);
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

        public void HandleUpdate(string part)
        {
            switch (part)
            {
                case "faculty":
                    ViewAdminDepartment.RefreshDataGrid();
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

        // Hamburger menu switcher
        private void HamburgerMenu_ItemClick(object sender, MahApps.Metro.Controls.ItemClickEventArgs e)
        {
            if (e.ClickedItem == null) return;
            var tag = ((HamburgerMenuItem)e.ClickedItem).Tag.ToString();
            switch (tag)
            {
                case "1":
                    AdminHamburgerMenuFrame.Content = ViewAdminFaculty;
                    ViewAdminFaculty.ClearSelected();
                    break;
                case "2":
                    AdminHamburgerMenuFrame.Content = ViewAdminDepartment;
                    ViewAdminDepartment.ClearSelected();
                    break;
                case "3":
                    AdminHamburgerMenuFrame.Content = ViewAdminPeriods;
                    ViewAdminPeriods.ClearSelected();
                    break;
                case "4":
                    AdminHamburgerMenuFrame.Content = ViewAdminLecturers;
                    ViewAdminLecturers.ClearSelected();
                    break;
                case "5":
                    AdminHamburgerMenuFrame.Content = ViewAdminLectures;
                    ViewAdminLectures.ClearSelected();
                    break;
                case "6":
                    AdminHamburgerMenuFrame.Content = ViewAdminExams;
                    ViewAdminExams.ClearSelected();
                    break;
                case "7":
                    AdminHamburgerMenuFrame.Content = ViewAdminEarnings;
                    ViewAdminEarnings.ClearSelected();
                    break;
                case "8":
                    AdminHamburgerMenuFrame.Content = ViewAdminLectureAssociate;
                    break;
                case "9":
                    AdminHamburgerMenuFrame.Content = ViewAdminInspectExam;
                    break;
                case "10":
                    helpFlyout.IsOpen = true;
                    break;
                default:
                    // impossible?
                    break;
            }

        }
    }
}
