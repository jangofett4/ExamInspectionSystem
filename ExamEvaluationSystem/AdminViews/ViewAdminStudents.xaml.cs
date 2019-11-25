using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ExamEvaluationSystem
{
    /// <summary>
    /// ViewStudents.xaml etkileşim mantığı
    /// </summary>
    public partial class ViewAdminStudents : IChildObject<AdminPanel>
    {
        public AdminPanel ParentObject { get; set; }
        public FlyoutState SideMenuState { get; set; }

        public ViewAdminStudents(AdminPanel parent)
        {
            InitializeComponent();
            ParentObject = parent;
        }
    }
}
