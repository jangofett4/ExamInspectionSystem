using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ExamEvaluationSystem
{
    /// <summary>
    /// ViewExams.xaml etkileşim mantığı
    /// </summary>
    public partial class ViewAdminExams : IChildObject<AdminPanel>
    {
        public AdminPanel ParentObject { get; set; }
        public FlyoutState SideMenuState { get; set; }

        public ViewAdminExams(AdminPanel parent)
        {
            InitializeComponent();
            ParentObject = parent;
        }

        private void GridDoubleClick(object sender, MouseButtonEventArgs e)
        {

        }

        private void TileAddClick(object sender, RoutedEventArgs e)
        {

        }

        private void TileDeleteClick(object sender, RoutedEventArgs e)
        {

        }

        private void TileSearchClick(object sender, RoutedEventArgs e)
        {

        }

        private void TileFlyoutDoneClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
