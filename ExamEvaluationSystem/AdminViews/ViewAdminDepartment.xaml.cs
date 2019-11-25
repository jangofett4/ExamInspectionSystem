using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
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
                var s = new PropertyDataSelector("Fakülte Seç");
                var builder = new SingleDataSelectorBuilder<EISFaculty>(EISSystem.Faculties, s, "ID", ("Name", "Fakülte Adı"), ("ID", "Fakülte Kodu"));
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
                var s = new PropertyDataSelector("Bölüm Kazanımlarını Seç");
                var builder = new MultiDataSelectorBuilder<EISEarning>(EISSystem.DepartmentEarnings, s, "ID", ("Name", "Kazanım"));
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
        }

        public void RefreshDataGrid()
        {
            foreach (var data in EISSystem.Departments)
                if (data.ID >= 10)
                    Grid.Items.Add(data);
        }

        private void TileAddClick(object sender, RoutedEventArgs e)
        {
            sideFlyout.Header = "Ekle";
            sideFlyout.IsOpen = true;
            txtDepartmentID.IsReadOnly = false; // might be artifact from edit mode
            SideMenuState = FlyoutState.Add;
        }

        private async void TileDeleteClick(object sender, RoutedEventArgs e)
        {
            var dataToDelete = new List<EISDepartment>();
            foreach (var item in Grid.Items)
            {
                var x = ((DataGridTemplateColumn)Grid.Columns[0]).GetCellContent(item).FindChild<CheckBox>("Check");
                if (x.IsChecked == true)
                    dataToDelete.Add((EISDepartment)item);
            }

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
            txtDepartmentID.IsReadOnly = true; // cannot change this, lots of foreign key erors
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

                var elem = (TextBlock)Grid.Columns[1].GetCellContent(itemToEdit);
                var elem1 = (TextBlock)Grid.Columns[3].GetCellContent(itemToEdit);
                itemToEdit.Name = txtDepartmentName.Text;
                itemToEdit.Earnings = (List<EISEarning>)selectorDepartmentEarnings.SelectedData;
                itemToEdit.Faculty = (EISFaculty)selectorDepartmentFaculty.SelectedData;

                var result = itemToEdit.Update(EISSystem.Connection);
                if (result == -1)
                {
                    ParentObject.NotifyError("Bölüm isim çakışması, başka isim belirtin.");
                    return;
                }

                elem.Text = itemToEdit.Name; // Edit the datagrid cell
                elem1.Text = itemToEdit.Faculty.Name;
                itemToEdit.InsertEarnings(EISSystem.Connection);

                ParentObject.NotifySuccess("Düzenleme başarılı!");
                sideFlyout.IsOpen = false;
            }
        }
    }
}
