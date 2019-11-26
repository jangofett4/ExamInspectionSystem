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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ExamEvaluationSystem
{
    /// <summary>
    /// ViewLecturerAddExam.xaml etkileşim mantığı
    /// </summary>
    public partial class ViewLecturerAddExam : Page
    {
        public AdminPanel ParentObject { get; set; }
        public FlyoutState SideMenuState { get; set; }

        private EISDepartment itemToEdit;

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
