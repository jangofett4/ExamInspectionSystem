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
        public Action OnShow;

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

    public class SingleDataSelectorBuilder<T> where T:EISDataPoint<T>
    {
        public List<T> Data;
        public T SelectedData;

        public bool DisableDoubleClickBehaviour { get; set; } = false;
        public bool CloseAfterSelection { get; set; } = true;
        public Action DataDoubleClickCallback { get; set; } = null;
        public bool DisableMenu { get; set; } = false;

        public PropertyDataSelector Form;

        public string Select;
        public (string, string)[] Show;

        public SingleDataSelectorBuilder(List<T> data, PropertyDataSelector form, string select, params (string, string)[] show)
        {
            Form = form;
            Data = data;
            Select = select;
            Show = show;
            DisableDoubleClickBehaviour = false;
            DisableMenu = false;

            Form.Closing += (sender, e) =>
            {
                ClearSelected();
            };
        }

        public void ClearSelected()
        {
            foreach (var data in Form.dgSelector.Items)
            {
                var x = ((T)data);
                if (x.Checked)
                    x.Checked = false;
            }
        }

        private List<T> GetSelectedPeriods()
        {
            var lst = new List<T>();
            foreach (var data in Form.dgSelector.Items)
            {
                var x = ((T)data);
                if (x.Checked)
                {
                    x.Checked = false;
                    lst.Add(x);
                }
            }
            return lst;
        }

        public void BuildAll()
        {
            BuildDataGrid(Form.dgSelector);
            BuildColumns(Form.dgSelector);
            BuildData(Form.dgSelector);
        }

        public void BuildAll(Predicate<T> predicate)
        {
            BuildDataGrid(Form.dgSelector);
            BuildColumns(Form.dgSelector);
            BuildData(Form.dgSelector, predicate);
        }

        public void BuildDataGrid(DataGrid grid)
        {
            grid.Columns[0].Visibility = Visibility.Hidden;
            grid.IsReadOnly = true;                             // Restrict editing
            grid.SelectionMode = DataGridSelectionMode.Single;  // Restrict multiple selection

            if (!DisableDoubleClickBehaviour)
                grid.MouseDoubleClick += (object sender, System.Windows.Input.MouseButtonEventArgs e) =>
                {
                    if (SelectedData != null && ReferenceEquals(SelectedData, grid.SelectedItem)) // If someone already set data
                    {
                        DataDoubleClickCallback?.Invoke();
                        if (CloseAfterSelection)
                            Form.Close(); // Just use it
                        return;
                    }

                    SelectedData = (T)grid.SelectedItem; // Set data
                    DataDoubleClickCallback?.Invoke();
                    if (CloseAfterSelection)
                        Form.Close();
                };

            if (!DisableMenu)
                Form.MenuSelectButton.Click += (object sender, RoutedEventArgs e) =>
                {
                    if (SelectedData != null && ReferenceEquals(SelectedData, grid.SelectedItem)) // If someone already set data
                    {
                        DataDoubleClickCallback?.Invoke();
                        if (CloseAfterSelection)
                            Form.Close(); // Just use it
                        return;
                    }
                    DataDoubleClickCallback?.Invoke();
                    if (CloseAfterSelection)
                        SelectedData = (T)grid.SelectedItem; // Set data
                    Form.Close();
                };
            else
                Form.MainMenu.Visibility = Visibility.Hidden;
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

        public void BuildData(DataGrid grid, Predicate<T> predicate)
        {
            foreach (var d in Data)
                if (predicate(d))
                    grid.Items.Add(d);
        }

        public void GridSelect(DataGrid grid, object data)
        {
            ((T)data).Checked = true;
            grid.SelectedItem = data; // developer takes full reponsibility on this one
        }
    }

    public class MultiDataSelectorBuilder<T> where T : EISDataPoint<T>
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

        public void ClearSelected()
        {
            foreach (var data in Form.dgSelector.Items)
            {
                var x = ((T)data);
                if (x.Checked)
                    x.Checked = false;
            }
        }

        private List<T> GetSelected()
        {
            var lst = new List<T>();
            foreach (var data in Form.dgSelector.Items)
            {
                var x = ((T)data);
                if (x.Checked)
                {
                    x.Checked = false;
                    lst.Add(x);
                }
            }
            return lst;
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

                SelectedData = GetSelected();
                
                /*if (SelectedData.Count == 0) // no checkboxes are used
                {
                    foreach (var v in grid.SelectedItems)
                        SelectedData.Add((T)v);
                }*/

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
                foreach (var i in lst)
                    ((T)i).Checked = true;
            }
            else
            {
            }
        }
    }
}
