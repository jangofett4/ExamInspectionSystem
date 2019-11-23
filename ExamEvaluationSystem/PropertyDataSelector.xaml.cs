using MahApps.Metro.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ExamEvaluationSystem
{
    /// <summary>
    /// PropertyDataSelector.xaml etkileşim mantığı
    /// </summary>
    public partial class PropertyDataSelector
    {
        public object SelectedData;
        
        public PropertyDataSelector(string title)
        {
            InitializeComponent();
            Title = title;
        }
    }

    public class DataGridBuilder<T>
    {
        public List<IPropertyCallAdapter<T>> PropertyProvider;
        public List<T> Data;

        public string Select;
        public (string, string)[] Show;

        public DataGridBuilder(List<T> data, string select, params (string, string)[] show)
        {
            Data = data;
            Select = select;
            Show = show;

            PropertyProvider = new List<IPropertyCallAdapter<T>>();

            PropertyProvider.Add(PropertyCallAdapterProvider<T>.GetInstance(select));
            foreach (var s in show)
                PropertyProvider.Add(PropertyCallAdapterProvider<T>.GetInstance(s.Item1));
        }

        public void BuildColumns(DataGrid grid)
        {
            grid.Columns.Add(new DataGridTextColumn() { Header = Select, Visibility = System.Windows.Visibility.Hidden });
            foreach (var s in Show)
                grid.Columns.Add(new DataGridTextColumn() { Header = s.Item2, Binding = new System.Windows.Data.Binding(s.Item1) });
        }

        public void BuildData(DataGrid grid)
        {
            foreach (var d in Data)
                    grid.Items.Add(d);
        }
    }
}
