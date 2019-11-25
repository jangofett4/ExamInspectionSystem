using MahApps.Metro.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

        private void MenuSelectClick(object sender, System.Windows.RoutedEventArgs e)
        {
            // actually nothing.
        }
    }

    public class SingleDataSelectorBuilder<T>
    {
        public List<T> Data;
        public T SelectedData;

        public PropertyDataSelector Form;

        public string Select;
        public (string, string)[] Show;

        public SingleDataSelectorBuilder(List<T> data, PropertyDataSelector form, string select, params (string, string)[] show)
        {
            Form = form;
            Data = data;
            Select = select;
            Show = show;
        }

        public void BuildAll()
        {
            BuildDataGrid(Form.dgSelector);
            BuildColumns(Form.dgSelector);
            BuildData(Form.dgSelector);
        }

        public void BuildDataGrid(DataGrid grid)
        {
            grid.Columns[0].Visibility = Visibility.Hidden;
            grid.IsReadOnly = true;                             // Restrict editing
            grid.SelectionMode = DataGridSelectionMode.Single;  // Restrict multiple selection

            grid.MouseDoubleClick += (object sender, System.Windows.Input.MouseButtonEventArgs e) =>
            {
                if (SelectedData != null && ReferenceEquals(SelectedData, grid.SelectedItem)) // If someone already set data
                {
                    Form.Close(); // Just use it
                    return;
                }

                SelectedData = (T)grid.SelectedItem; // Set data
                Form.Close();
            };

            Form.MenuSelectButton.Click += (object sender, RoutedEventArgs e) =>
            {
                if (SelectedData != null && ReferenceEquals(SelectedData, grid.SelectedItem)) // If someone already set data
                {
                    Form.Close(); // Just use it
                    return;
                }

                SelectedData = (T)grid.SelectedItem; // Set data
                Form.Close();
            };
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

        public void GridSelect(DataGrid grid, object data)
        {
            grid.SelectedItem = data; // developer takes full reponsibility on this one
        }
    }

    public class MultiDataSelectorBuilder<T>
    {
        public List<T> Data;
        public List<T> SelectedData;

        public PropertyDataSelector Form;

        public string Select;
        public (string, string)[] Show;

        public MultiDataSelectorBuilder(List<T> data, PropertyDataSelector form, string select, params (string, string)[] show)
        {
            Form = form;
            Data = data;
            Select = select;
            Show = show;
        }

        public void BuildAll()
        {
            BuildDataGrid(Form.dgSelector);
            BuildColumns(Form.dgSelector);
            BuildData(Form.dgSelector);
        }

        public void BuildDataGrid(DataGrid grid)
        {
            grid.IsReadOnly = true;                                 // Restrict editing
            grid.SelectionMode = DataGridSelectionMode.Extended;    // Enable multiple selection

            Form.MenuSelectButton.Click += (object sender, RoutedEventArgs e) =>
            {
                /*
                 * Wont work anyway. Since two different list is created their reference will never be same
                if (SelectedData != null && ReferenceEquals(SelectedData, grid.SelectedItem)) // If someone already set data
                {
                    Form.Close();
                    return;
                }
                */

                SelectedData = new List<T>();
                foreach (var item in grid.Items)
                {
                    var x = ((DataGridTemplateColumn)grid.Columns[0]).GetCellContent(item).FindChild<CheckBox>("Check");
                    if (x.IsChecked == true)
                        SelectedData.Add((T)item);
                }
                Form.Close();
            };
        }

        public void BuildColumns(DataGrid grid)
        {
            grid.Columns[0].IsReadOnly = false;
            grid.Columns.Add(new DataGridTextColumn() { Header = Select, Visibility = System.Windows.Visibility.Hidden });
            foreach (var s in Show)
                grid.Columns.Add(new DataGridTextColumn() { Header = s.Item2, Binding = new System.Windows.Data.Binding(s.Item1) });
        }

        public void BuildData(DataGrid grid)
        {
            foreach (var d in Data)
                grid.Items.Add(d);
        }

        public void GridSelect(DataGrid grid, object data)
        {
            // developer takes full reponsibility on these
            var lst = data as IList;
            if (lst != null)
            {
                grid.SelectedItems.Clear();
                for (int i = 0; i < lst.Count; i++)
                    grid.SelectedItems.Add(lst[i]);
            }
            else
            {
                // If some moron decides to send single data
                grid.SelectedItem = data;
                /*
                var x = ((DataGridTemplateColumn)grid.Columns[0]).GetCellContent(data).FindChild<CheckBox>("Check");
                x.IsChecked = true;
                */
            }
        }
    }
}
