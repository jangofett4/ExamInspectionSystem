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
    /// ViewAdminLecturers.xaml etkileşim mantığı
    /// </summary>
    public partial class ViewAdminLecturers : IChildObject<AdminPanel>
    {
        public AdminPanel ParentObject { get; set; }

        public ViewAdminLecturers(AdminPanel parent)
        {
            InitializeComponent();
            ParentObject = parent;
        }
    }
}
