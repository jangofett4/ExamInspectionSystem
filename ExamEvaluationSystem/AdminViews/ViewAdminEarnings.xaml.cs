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
    /// ViewEarnings.xaml etkileşim mantığı
    /// </summary>
    public partial class ViewAdminEarnings : IChildObject<AdminPanel>
    {
        public AdminPanel ParentObject { get; set; }

        public ViewAdminEarnings(AdminPanel parent)
        {
            InitializeComponent();
            ParentObject = parent;
            RefreshDataGrid();
        }

        public void RefreshDataGrid()
        {
            Grid.Items.Clear();
            foreach (var data in EISSystem.Earnings)
                Grid.Items.Add(data);
        }

        private void TileAddClick(object sender, RoutedEventArgs e)
        {

        }

        private void TileDeleteClick(object sender, RoutedEventArgs e)
        {

        }

        private void TileFlyoutDoneClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
