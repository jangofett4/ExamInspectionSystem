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

        public PropertyDataSelector(string title, int width)
        {
            InitializeComponent();
            Title = title;
            Width = width;
        }

        private void MenuSelectClick(object sender, RoutedEventArgs e)
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
        public bool DisableSearch { get; set; } = true;

        public PropertyDataSelector Form;

        public Func<T, string, bool> SearchPredicate;

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
        }

        public SingleDataSelectorBuilder(List<T> data, PropertyDataSelector form, string select, Func<T, string, bool> predicate, params (string, string)[] show)
        {
            Form = form;
            Data = data;
            Select = select;
            Show = show;
            DisableDoubleClickBehaviour = false;
            DisableMenu = false;
            SearchPredicate = predicate;
            SetupSearch();
        }

        public void ResetSearch()
        {
            if (buildDataPredicate != null)
                BuildData(Form.dgSelector, buildDataPredicate);
            else
                BuildData(Form.dgSelector);
        }

        private DelayedActionInvoker searchAction;
        public void SetupSearch()
        {
            if (SearchPredicate == null)
                return;
            if (searchAction != null)
                searchAction.Dispose();
            searchAction = new DelayedActionInvoker(() =>
            {
                Form.Dispatcher.Invoke(() =>
                {
                    Form.dgSelector.Items.Clear();
                    foreach (var data in Data)
                    {
                        string sq = Form.searchQuery.Text.ToLower();
                        var datapred = buildDataPredicate?.Invoke(data);
                        if (datapred.HasValue)
                        {
                            if (SearchPredicate(data, sq) && datapred.Value)
                                Form.dgSelector.Items.Add(data);
                        }
                        else if (SearchPredicate(data, sq))
                            Form.dgSelector.Items.Add(data);
                    }
                });
            }, 1000);
        }

        private void SearchKeyUp(object sender, KeyEventArgs e)
        {
            searchAction.Reset();
        }

        public void ClearSelected()
        {
            ResetSearch();
            foreach (var data in Form.dgSelector.Items)
            {
                var x = ((T)data);
                if (x.Checked)
                    x.Checked = false;
            }
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
            
            if (DisableSearch)
            {
                Form.searchQuery.Visibility = Visibility.Hidden;
                Form.lblSearch.Visibility = Visibility.Hidden;
                Form.lblSeperator.Visibility = Visibility.Hidden;
            }
            else
            {
                Form.searchQuery.KeyUp += SearchKeyUp;
            }

            if (!DisableDoubleClickBehaviour)
                grid.MouseDoubleClick += (object sender, MouseButtonEventArgs e) =>
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
            Form.Closing += (sender, e) => { ClearSelected(); };
        }

        public void BuildColumns(DataGrid grid)
        {
            grid.Columns.Add(new DataGridTextColumn() { Header = Select, Visibility = Visibility.Hidden });
            var star = new DataGridLength(1, DataGridLengthUnitType.Star);
            for (int i = 0; i < Show.Length; i++)
            {
                var item = Show[i];
                if (i == Show.Length - 1)
                    grid.Columns.Add(new DataGridTextColumn() { Header = item.Item2, Binding = new System.Windows.Data.Binding(item.Item1), Width = star });
                else
                    grid.Columns.Add(new DataGridTextColumn() { Header = item.Item2, Binding = new System.Windows.Data.Binding(item.Item1), Width = DataGridLength.SizeToCells });
            }
        }

        public void BuildData(DataGrid grid)
        {
            grid.Items.Clear();
            foreach (var d in Data)
                grid.Items.Add(d);
        }

        private Predicate<T> buildDataPredicate;
        public void BuildData(DataGrid grid, Predicate<T> predicate)
        {
            buildDataPredicate = predicate;
            grid.Items.Clear();
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

        public bool DisableSearch { get; set; } = true;

        public Func<T, string, bool> SearchPredicate;

        public string Select;
        public (string, string)[] Show;

        public MultiDataSelectorBuilder(List<T> data, PropertyDataSelector form, string select, params (string, string)[] show)
        {
            Form = form;
            Data = data;
            Select = select;
            Show = show;
        }

        public MultiDataSelectorBuilder(List<T> data, PropertyDataSelector form, string select, Func<T, string, bool> predicate, params (string, string)[] show)
        {
            Form = form;
            Data = data;
            Select = select;
            Show = show;
            SearchPredicate = predicate;
            SetupSearch();
        }

        public void ResetSearch()
        {
            BuildData(Form.dgSelector);
        }

        private DelayedActionInvoker searchAction;
        public void SetupSearch()
        {
            if (SearchPredicate == null)
                return;
            if (searchAction != null)
                searchAction.Dispose();
            searchAction = new DelayedActionInvoker(() =>
            {
                Form.Dispatcher.Invoke(() =>
                {
                    Form.dgSelector.Items.Clear();
                    foreach (var data in Data)
                    {
                        string sq = Form.searchQuery.Text.ToLower();
                        if (SearchPredicate(data, sq))
                            Form.dgSelector.Items.Add(data);
                    }
                });
            }, 1000);
        }

        private void SearchKeyUp(object sender, KeyEventArgs e)
        {
            searchAction.Reset();
        }

        public void ClearSelected()
        {
            ResetSearch();
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

            if (DisableSearch)
            {
                Form.searchQuery.Visibility = Visibility.Hidden;
                Form.lblSearch.Visibility = Visibility.Hidden;
                Form.lblSeperator.Visibility = Visibility.Hidden;
            }
            else
            {
                Form.searchQuery.KeyUp += SearchKeyUp;
            }

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
            Form.Closing += (sender, e) => { ClearSelected(); };
        }

        public void BuildColumns(DataGrid grid)
        {
            grid.Columns.Add(new DataGridTextColumn() { Header = Select, Visibility = Visibility.Hidden });
            var star = new DataGridLength(1, DataGridLengthUnitType.Star);
            for (int i = 0; i < Show.Length; i++)
            {
                var item = Show[i];
                if (i == Show.Length - 1)
                    grid.Columns.Add(new DataGridTextColumn() { Header = item.Item2, Binding = new System.Windows.Data.Binding(item.Item1), Width = star });
                else
                    grid.Columns.Add(new DataGridTextColumn() { Header = item.Item2, Binding = new System.Windows.Data.Binding(item.Item1), Width = DataGridLength.SizeToCells });
            }
        }

        public void BuildData(DataGrid grid)
        {
            foreach (var d in Data)
                grid.Items.Add(d);
        }

        public void GridSelect(DataGrid grid, object data)
        {
            // developer takes full reponsibility on these
            if (data is IList lst)
                foreach (var i in lst)
                    ((T)i).Checked = true;
        }
    }
}
