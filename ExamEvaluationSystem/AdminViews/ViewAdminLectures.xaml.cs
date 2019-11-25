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
    /// ViewAdminLectures.xaml etkileşim mantığı
    /// </summary>
    public partial class ViewAdminLectures : IChildObject<AdminPanel>
    {
        public AdminPanel ParentObject { get; set; }

        public ViewAdminLectures(AdminPanel parent)
        {
            InitializeComponent();
            ParentObject = parent;
        }
    }
}
