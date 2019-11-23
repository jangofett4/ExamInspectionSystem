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
    /// AdminPanel.xaml etkileşim mantığı
    /// </summary>
    public partial class AdminPanel
    {
        public ViewAdminDepartment ViewAdminDepartment = new ViewAdminDepartment();
        public ViewAdminEarnings ViewAdminEarnings = new ViewAdminEarnings();
        public ViewAdminExams ViewAdminExams = new ViewAdminExams();
        public ViewAdminFaculty ViewAdminFaculty = new ViewAdminFaculty();
        public ViewAdminLecturers ViewAdminLecturers = new ViewAdminLecturers();
        public ViewAdminLectures ViewAdminLectures = new ViewAdminLectures();
        public ViewAdminPeriods ViewAdminPeriods = new ViewAdminPeriods();
        public ViewAdminStudents ViewAdminStudents = new ViewAdminStudents();

        public AdminPanel()
        {
            InitializeComponent();
            AdminHamburgerMenuFrame.Content = ViewAdminFaculty;
        }

        private void HamburgerMenu_ItemClick(object sender, MahApps.Metro.Controls.ItemClickEventArgs e)
        {
            var tag = ((MahApps.Metro.Controls.HamburgerMenuItem)e.ClickedItem).Tag.ToString();
            switch (tag)
            {
                case "1":
                    AdminHamburgerMenuFrame.Content = ViewAdminFaculty;
                    break;
                case "2":
                    AdminHamburgerMenuFrame.Content = ViewAdminDepartment;
                    break;
                case "3":
                    AdminHamburgerMenuFrame.Content = ViewAdminPeriods;
                    break;
                case "4":
                    AdminHamburgerMenuFrame.Content = ViewAdminLecturers;
                    break;
                case "5":
                    AdminHamburgerMenuFrame.Content = ViewAdminLectures;
                    break;
                case "6":
                    AdminHamburgerMenuFrame.Content = ViewAdminExams;
                    break;
                case "7":
                    AdminHamburgerMenuFrame.Content = ViewAdminEarnings;
                    break;
                case "8":
                    AdminHamburgerMenuFrame.Content = ViewAdminStudents;
                    break;
                

            }

        }
    }
}
