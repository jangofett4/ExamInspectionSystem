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

namespace ExamEvaluationSystem
{
    /// <summary>
    /// AdminPanel.xaml etkileşim mantığı
    /// </summary>
    public partial class AdminPanel : IDisposable, IHasNotifiers
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

        private Notifier Notifier;

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
                    parentWindow: Application.Current.MainWindow,
                    corner: Corner.BottomRight,
                    offsetX: 10,
                    offsetY: 10);

                cfg.LifetimeSupervisor = new TimeAndCountBasedLifetimeSupervisor(
                    notificationLifetime: TimeSpan.FromSeconds(3),
                    maximumNotificationCount: MaximumNotificationCount.FromCount(3));

                cfg.Dispatcher = Application.Current.Dispatcher;

                cfg.DisplayOptions.TopMost = false; // Needs to be set to false in order to toast to function properly
            });

            AdminHamburgerMenuFrame.Content = ViewAdminFaculty;
        }

        public List<EISExamTriple> GenerateExamTriple()
        {
            var distinctex = new List<EISExam>();
            foreach (var de in EISSystem.Exams)
            {
                bool found = false;
                foreach (var disc in distinctex)
                {
                    if (disc.LectureName == de.LectureName)
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
            Notifier.ShowWarning(message);
        }

        public void NotifyError(string message)
        {
            Notifier.ShowError(message);
        }

        public void Dispose()
        {
            Notifier.Dispose();
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
                case "9":
                    AdminHamburgerMenuFrame.Content = ViewAdminInspectExam;
                    break;
                case "7":
                    AdminHamburgerMenuFrame.Content = ViewAdminEarnings;
                    ViewAdminEarnings.ClearSelected();
                    break;
                case "8":
                    AdminHamburgerMenuFrame.Content = ViewAdminLectureAssociate;
                    break;
                default:
                    // impossible?
                    break;
            }

        }
    }
}
