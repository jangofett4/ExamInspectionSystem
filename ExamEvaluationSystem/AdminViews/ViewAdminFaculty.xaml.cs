using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using ToastNotifications;
using ToastNotifications.Messages;
using ToastNotifications.Lifetime;
using ToastNotifications.Position;
using System.Windows.Input;

namespace ExamEvaluationSystem
{
    /// <summary>
    /// ViewAdminFaculty.xaml etkileşim mantığı
    /// </summary>
    public partial class ViewAdminFaculty : IChildObject<AdminPanel>
    {
        public AdminPanel ParentObject { get; set; }
        public FlyoutState SideMenuState { get; set; }

        private EISFaculty itemToEdit;

        public ViewAdminFaculty(AdminPanel parent)
        {
            InitializeComponent();
            ParentObject = parent;
            RefreshDataGrid();
            SetupSearch();
        }

        public void ClearSelected()
        {
            foreach (var data in Grid.Items)
            {
                var x = ((EISFaculty)data);
                if (x.Checked)
                    x.Checked = false;
            }
        }

        private List<EISFaculty> GetSelectedFacultys()
        {
            var lst = new List<EISFaculty>();
            foreach (var data in Grid.Items)
            {
                var x = ((EISFaculty)data);
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
            foreach (var f in EISSystem.Faculties)
                if (f.ID >= 10) // <10 are dummy and debug faculties
                    Grid.Items.Add(f);
        }

        private void TileAddClick(object sender, RoutedEventArgs e)
        {
            sideFlyout.Header = "Ekle";
            sideFlyout.IsOpen = true;
            SideMenuState = FlyoutState.Add;
        }

        private async void TileDeleteClick(object sender, RoutedEventArgs e)
        {
            var dataToDelete = GetSelectedFacultys();

            if (dataToDelete.Count == 0)
            {
                ParentObject.NotifyWarning("Silmek istediğiniz verileri tablodan seçin");
                return;
            }

            var result = await ParentObject.ShowMessageAsync("Uyarı", $"{ dataToDelete.Count } fakülte bilgisi silinecek (bağıntılı olan bölümler ve alt bilgiler dahil).\nEmin misiniz?", MessageDialogStyle.AffirmativeAndNegative);
            if (result == MessageDialogResult.Affirmative)
            {
                foreach (var data in dataToDelete) // for each data selected
                {
                    Grid.Items.Remove(data);            // Remove from grid
                    EISSystem.Faculties.Remove(data);   // Remove from system in memory
                    data.Delete(EISSystem.Connection);  // Remove from database
                }
                ParentObject.NotifyInformation($"{ dataToDelete.Count } fakülte bilgisi silindi.");
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

            var item = (EISFaculty)Grid.SelectedItem;
            itemToEdit = item;
            txtFacultyName.Text = item.Name;
            txtFacultyID.Value = item.ID;
            
            sideFlyout.Header = "Düzenle";
            sideFlyout.IsOpen = true;
            SideMenuState = FlyoutState.Edit;
        }
        
        private void TileFlyoutDoneClick(object sender, RoutedEventArgs e)
        {
            if (!txtFacultyID.Value.HasValue)
            {
                ParentObject.NotifyWarning("Fakülte kodu alanı boş burakılamaz!");
                return;
            }

            txtFacultyName.Text = txtFacultyName.Text.Trim();
            if (string.IsNullOrEmpty(txtFacultyName.Text))
            {
                ParentObject.NotifyWarning("Fakülte ismi alanı boş bırakılamaz!");
                return;
            }

            // Flyout is in add mode
            if (SideMenuState == FlyoutState.Add)
            {
                var fac = new EISFaculty((int)txtFacultyID.Value.Value, txtFacultyName.Text);
                var result = fac.Insert(EISSystem.Connection);
                if (result == -1)
                {
                    ParentObject.NotifyError("Fakülte isim çakışması, başka isim belirtin.");
                    return;
                }
                else if (result == -2)
                {
                    ParentObject.NotifyError("Fakülte kodu çakışması, başka kod belirtin.");
                    return;
                }

                EISSystem.Faculties.Add(fac);
                Grid.Items.Add(fac);
                ParentObject.NotifySuccess("Fakülte ekleme başarılı!");

                sideFlyout.IsOpen = false;
            }
            // Flyout is in edit mode
            else
            {
                if (itemToEdit == null)
                    return; // somehow??

                int id = itemToEdit.ID;
                itemToEdit.Store();

                if (id == (int)txtFacultyID.Value.Value)
                {
                    itemToEdit.Name = txtFacultyName.Text;
                    var result = itemToEdit.Update(EISSystem.Connection);
                    if (result == -1)
                    {
                        ParentObject.NotifyError("Fakülte isim çakışması, başka isim belirtin.");
                        itemToEdit.Restore();
                        return;
                    }
                }
                else
                {
                    string name = itemToEdit.Name;
                    itemToEdit.ID = (int)txtFacultyID.Value.Value;
                    itemToEdit.Name = txtFacultyName.Text;
                    var result = itemToEdit.UpdateWhere(EISSystem.Connection, Where.Equals("ID", id.ToString()));
                    if (result == -1)
                    {
                        ParentObject.NotifyError("Fakülte isim çakışması, başka isim belirtin.");
                        itemToEdit.Restore();
                        return;
                    }
                    else if (result == -2)
                    {
                        ParentObject.NotifyError("Fakülte kodu çakışması, başka kod belirtin.");
                        itemToEdit.Restore();
                        return;
                    }
                }

                ParentObject.HandleUpdate("faculty");

                // Edit the datagrid cell
                itemToEdit.OnPropertyChanged("ID", "Name");

                ParentObject.NotifySuccess("Düzenleme başarılı!");
                sideFlyout.IsOpen = false;
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
                    foreach (var data in EISSystem.Faculties)
                    {
                        string sq = searchQuery.Text.ToLower();
                        if (data.ID >= 10 && data.Name.ToLower().Contains(sq))
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
