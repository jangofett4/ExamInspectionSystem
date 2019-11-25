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
    /// DataSelector.xaml etkileşim mantığı
    /// </summary>
    public partial class DataSelector : UserControl
    {
        public bool IsTextReadOnly { get { return txtData.IsReadOnly; } set { txtData.IsReadOnly = value; } }
        public string Text { get { return txtData.Text; } set { txtData.Text = value; } }
        public object SelectedData { get; set; }

        public Action ClickCallback { get; set; }

        public DataSelector()
        {
            InitializeComponent();
            btnDataSelect.Click += (object sender, RoutedEventArgs e) =>
            {
                ClickCallback?.Invoke();
            };
        }
    }
}
