using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
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
    /// ViewAdminDepartment.xaml etkileşim mantığı
    /// </summary>
    public partial class ViewAdminDepartment : IChildObject<AdminPanel>
    {
        public AdminPanel ParentObject { get; set; }
        public FlyoutState SideMenuState { get; set; }

        private EISDepartment itemToEdit;

        public ViewAdminDepartment(AdminPanel parent)
        {
            InitializeComponent();
            ParentObject = parent;
            RefreshDataGrid();

            selectorDepartmentFaculty.ClickCallback = () =>
            {
                var s = new PropertyDataSelector("Fakülte Seç", 300);
                var builder = new SingleDataSelectorBuilder<EISFaculty>(EISSystem.Faculties, s, "ID", 
                    (f, q) => (f.Name.ToLower().Contains(q) || f.ID.ToString().Contains(q)) ? true : false, 
                    ("Name", "Fakülte Adı"), ("ID", "Fakülte Kodu"));
                builder.DisableSearch = false;
                builder.BuildAll((EISFaculty f) => f.ID >= 10);
                
                if (selectorDepartmentFaculty.SelectedData != null)
                {
                    builder.GridSelect(s.dgSelector, selectorDepartmentFaculty.SelectedData); // Pre-Select data in grid
                    builder.SelectedData = (EISFaculty)selectorDepartmentFaculty.SelectedData;
                }

                s.ShowDialog();

                if (builder.SelectedData == null)
                {
                    selectorDepartmentFaculty.Text = "";
                    return;
                }

                selectorDepartmentFaculty.SelectedData = builder.SelectedData;
                selectorDepartmentFaculty.Text = builder.SelectedData.Name;
            };

            selectorDepartmentEarnings.ClickCallback = () =>
            {
                var s = new PropertyDataSelector("Bölüm Kazanımlarını Seç") { WindowState = WindowState.Maximized };
                var builder = new MultiDataSelectorBuilder<EISEarning>(EISSystem.DepartmentEarnings, s, "ID", 
                    (e, q) =>  (e.Name.ToLower().Contains(q) || e.Code.ToLower().Contains(q)) ? true : false, 
                    ("Code", "Kod"), ("Name", "Kazanım"));
                builder.DisableSearch = false;
                builder.BuildAll();

                if (selectorDepartmentEarnings.SelectedData != null)
                {
                    builder.GridSelect(s.dgSelector, selectorDepartmentEarnings.SelectedData); // Pre-Select data in grid
                    builder.SelectedData = (List<EISEarning>)selectorDepartmentEarnings.SelectedData;
                }

                s.ShowDialog();

                if (builder.SelectedData == null)
                {
                    selectorDepartmentEarnings.Text = "Seçilen Kazanım Yok";
                    return;
                }

                selectorDepartmentEarnings.SelectedData = builder.SelectedData;
                selectorDepartmentEarnings.Text = $"[{ builder.SelectedData.Count } bölüm kazanımı]";
            };
            SetupSearch();
        }

        public void ClearSelected()
        {
            foreach (var data in Grid.Items)
            {
                var x = ((EISDepartment)data);
                if (x.Checked)
                    x.Checked = false;
            }
        }

        private List<EISDepartment> GetSelectedDepartments()
        {
            var lst = new List<EISDepartment>();
            foreach (var data in Grid.Items)
            {
                var x = ((EISDepartment)data);
                if (x.Checked)
                {
                    x.Checked = false;
                    lst.Add(x);
                }
            }
            return lst;
        }

        public void RefreshDataGrid()
        {
            Grid.Items.Clear();
            foreach (var data in EISSystem.Departments)
                if (data.ID >= 10)
                    Grid.Items.Add(data);
        }

        private void TileAddClick(object sender, RoutedEventArgs e)
        {
            sideFlyout.Header = "Ekle";
            sideFlyout.IsOpen = true;
            SideMenuState = FlyoutState.Add;
        }

        private async void TileDeleteClick(object sender, RoutedEventArgs e)
        {
            var dataToDelete = GetSelectedDepartments();

            if (dataToDelete.Count == 0)
            {
                ParentObject.NotifyWarning("Silmek istediğiniz verileri tablodan seçin");
                return;
            }

            var result = await ParentObject.ShowMessageAsync("Uyarı", $"{ dataToDelete.Count } bölüm bilgisi silinecek (bağıntılı olan öğrenciler ve sınavlar vb. dahil).\nEmin misiniz?", MessageDialogStyle.AffirmativeAndNegative);
            if (result == MessageDialogResult.Affirmative)
            {
                foreach (var data in dataToDelete) // for each data selected
                {
                    Grid.Items.Remove(data);            // Remove from grid
                    EISSystem.Departments.Remove(data); // Remove from system in memory
                    data.Delete(EISSystem.Connection);  // Remove from database
                }
                ParentObject.NotifyInformation($"{ dataToDelete.Count } bölüm bilgisi silindi.");
            }
            else
            {
                Grid.SelectedIndex = -1;
                ParentObject.NotifyInformation("Silme işlemi iptal edildi.");
            }
        }

        private void GridDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Grid.SelectedItem == null)
                return;

            var item = (EISDepartment)Grid.SelectedItem;
            itemToEdit = item;
            txtDepartmentName.Text = item.Name;
            txtDepartmentID.Value = item.ID;
            // txtDepartmentID.IsReadOnly = true; // cannot change this, lots of foreign key erors
            selectorDepartmentEarnings.SelectedData = item.Earnings;
            selectorDepartmentEarnings.Text = $"[{ item.Earnings.Count } bölüm kazanımı]";
            selectorDepartmentFaculty.SelectedData = item.Faculty;
            selectorDepartmentFaculty.Text = item.Faculty.Name;

            sideFlyout.Header = "Düzenle";
            sideFlyout.IsOpen = true;
            SideMenuState = FlyoutState.Edit;
        }

        private void TileFlyoutDoneClick(object sender, RoutedEventArgs e)
        {
            if (!txtDepartmentID.Value.HasValue)
            {
                ParentObject.NotifyWarning("Bölüm kodu alanı boş burakılamaz!");
                return;
            }
            
            txtDepartmentName.Text = txtDepartmentName.Text.Trim();
            if (string.IsNullOrEmpty(txtDepartmentName.Text))
            {
                ParentObject.NotifyWarning("Bölüm ismi alanı boş bırakılamaz!");
                return;
            }

            if (selectorDepartmentFaculty.SelectedData == null)
            {
                ParentObject.NotifyWarning("Fakülte alanı boş bırakılamaz!");
                return;
            }

            if (selectorDepartmentEarnings.SelectedData == null)
            {
                ParentObject.NotifyWarning("Hiç kazanım seçilmedi!");
                return;
            }

            var earnings = (List<EISEarning>)selectorDepartmentEarnings.SelectedData;
            if (earnings.Count == 0)
            {
                ParentObject.NotifyWarning("Hiç kazanım seçilmedi!");
                return;
            }

            if (SideMenuState == FlyoutState.Add)
            {
                var dep = new EISDepartment((int)txtDepartmentID.Value.Value, txtDepartmentName.Text, (EISFaculty)selectorDepartmentFaculty.SelectedData, earnings);
                var result = dep.Insert(EISSystem.Connection);
                if (result == -1)
                {
                    ParentObject.NotifyError("Bölüm isim çakışması, başka isim belirtin.");
                    return;
                }
                else if (result == -2)
                {
                    ParentObject.NotifyError("Bölüm kodu çakışması, başka kod belirtin.");
                    return;
                }
                
                dep.InsertEarnings(EISSystem.Connection);

                EISSystem.Departments.Add(dep);
                Grid.Items.Add(dep);
                ParentObject.NotifySuccess("Bölüm ekleme başarılı!");

                sideFlyout.IsOpen = false;
            }
            // Flyout is in edit mode
            else
            {
                if (itemToEdit == null)
                    return; // somehow??

                int id = itemToEdit.ID;

                if ((int)txtDepartmentID.Value.Value == id) // ID is NOT changed
                {
                    itemToEdit.Name = txtDepartmentName.Text;
                    itemToEdit.Earnings = (List<EISEarning>)selectorDepartmentEarnings.SelectedData;
                    itemToEdit.Faculty = (EISFaculty)selectorDepartmentFaculty.SelectedData;

                    var result = itemToEdit.Update(EISSystem.Connection);
                    if (result == -1)
                    {
                        ParentObject.NotifyError("Bölüm isim çakışması, başka isim belirtin.");
                        return;
                    }

                    // Edit the datagrid cell
                    itemToEdit.OnPropertyChanged("Name", "FacultyName"); // update datagrid

                    itemToEdit.InsertEarnings(EISSystem.Connection);

                    ParentObject.NotifySuccess("Düzenleme başarılı!");
                    sideFlyout.IsOpen = false;
                }
                else
                {
                    itemToEdit.ID = (int)txtDepartmentID.Value.Value;
                    itemToEdit.Name = txtDepartmentName.Text;
                    itemToEdit.Earnings = (List<EISEarning>)selectorDepartmentEarnings.SelectedData;
                    itemToEdit.Faculty = (EISFaculty)selectorDepartmentFaculty.SelectedData;

                    var result = itemToEdit.UpdateWhere(EISSystem.Connection, Where.Equals("ID", id.ToString()));
                    if (result == -1)
                    {
                        ParentObject.NotifyError("Bölüm isim çakışması, başka isim belirtin.");
                        return;
                    }
                    else if (result == -2)
                    {
                        ParentObject.NotifyError("Bölüm kodu çakışması, başka kod belirtin.");
                        return;
                    }

                    itemToEdit.OnPropertyChanged("ID", "Name", "FacultyName");

                    itemToEdit.InsertEarnings(EISSystem.Connection);

                    ParentObject.NotifySuccess("Düzenleme başarılı!");
                    sideFlyout.IsOpen = false;
                }
            }
        }

        private void TileSearchClick(object sender, RoutedEventArgs e)
        {
            searchFlyout.IsOpen = true;
        }

        private void ResetSearchClick(object sender, RoutedEventArgs e)
        {
            RefreshDataGrid();
            TileResetSearch.Visibility = Visibility.Hidden;
        }

        private DelayedActionInvoker searchAction;
        private void SetupSearch()
        {
            if (searchAction != null)
                searchAction.Dispose();
            searchAction = new DelayedActionInvoker(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    Grid.Items.Clear();
                    foreach (var data in EISSystem.Departments)
                    {
                        string sq = searchQuery.Text.ToLower();
                        if (data.ID >= 10 && (data.Name.ToLower().Contains(sq) || data.FacultyName.ToLower().Contains(sq)))
                            Grid.Items.Add(data);
                    }
                    TileResetSearch.Visibility = Visibility.Visible;
                });
            }, 1000);
        }

        private void SearchKeyUp(object sender, KeyEventArgs e)
        {
            searchAction.Reset();
            if (e.Key == Key.Enter)
                searchFlyout.IsOpen = false;
        }

        public string GetGridLayout()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var c in Grid.Columns)
            {
                sb.Append(c.DisplayIndex);
                sb.Append('/');
                sb.Append(c.Width.Value);
                sb.Append('/');
                sb.Append((int)c.Width.UnitType);
                sb.Append('/');
            }
            return sb.ToString();
        }

        public void SetGridLayout(string lay)
        {
            var split = lay.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            int i = 0;
            foreach (var c in Grid.Columns)
            {
                c.DisplayIndex = int.Parse(split[i++]);
                c.Width = new DataGridLength(double.Parse(split[i++]), (DataGridLengthUnitType)int.Parse(split[i++]));
            }
        }
    }
}
