using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ExamEvaluationSystem
{
    /// <summary>
    /// ViewAdminPeriods.xaml etkileşim mantığı
    /// </summary>
    public partial class ViewAdminPeriods : IChildObject<AdminPanel>
    {
        public AdminPanel ParentObject { get; set; }

        public ViewAdminPeriods(AdminPanel parent)
        {
            InitializeComponent();
            ParentObject = parent;
        }
    }
}
