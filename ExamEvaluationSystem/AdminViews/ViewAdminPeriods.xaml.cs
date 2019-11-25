using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ExamEvaluationSystem
{
    /// <summary>
    /// ViewAdminPeriods.xaml etkileşim mantığı
    /// </summary>
    public partial class ViewAdminPeriods : IChildObject<AdminPanel>
    {
        public AdminPanel ParentObject { get; set; }
        public FlyoutState SideMenuState { get; set; }
        private EISPeriod itemToEdit;
        public ViewAdminPeriods(AdminPanel parent)
        {
            InitializeComponent();
            ParentObject = parent;
            RefreshDataGrid();
        }
        public void RefreshDataGrid()
        {
            Grid.Items.Clear();
            foreach (var p in EISSystem.Periods)
                    Grid.Items.Add(p);
        }
        private void TileAddClick(object sender, RoutedEventArgs e)
        {
            sideFlyout.Header = "Ekle";
            sideFlyout.IsOpen = true;
            SideMenuState = FlyoutState.Add;
        }
        private async void TileDeleteClick(object sender, RoutedEventArgs e)
        {
            var dataToDelete = new List<EISPeriod>();
            foreach (var item in Grid.Items)
            {
                var x = ((DataGridTemplateColumn)Grid.Columns[0]).GetCellContent(item).FindChild<CheckBox>("Check");
                if (x.IsChecked == true)
                    dataToDelete.Add((EISPeriod)item);
            }

            if (dataToDelete.Count == 0)
            {
                ParentObject.NotifyWarning("Silmek istediğiniz verileri tablodan seçin (çoklu seçim için Control ya da Shift kullanın)");
                return;
            }

            var result = await ParentObject.ShowMessageAsync("Uyarı", $"{ dataToDelete.Count } dönem bilgisi silinecek (bağıntılı olan dersler ve alt bilgiler dahil).\nEmin misiniz?", MessageDialogStyle.AffirmativeAndNegative);
            if (result == MessageDialogResult.Affirmative)
            {
                foreach (var data in dataToDelete) // for each data selected
                {
                    Grid.Items.Remove(data);            // Remove from grid
                    EISSystem.Periods.Remove(data);   // Remove from system in memory
                    data.Delete(EISSystem.Connection);  // Remove from database
                }
                ParentObject.NotifyInformation($"{ dataToDelete.Count } dönem bilgisi silindi.");
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

            var item = (EISPeriod)Grid.SelectedItem;
            itemToEdit = item;
            txtPeriodsName.Text = item.Name;

            sideFlyout.Header = "Düzenle";
            sideFlyout.IsOpen = true;
            SideMenuState = FlyoutState.Edit;
        }
        private void TileFlyoutDoneClick(object sender, RoutedEventArgs e)
        {
            

            txtPeriodsName.Text = txtPeriodsName.Text.Trim();
            if (string.IsNullOrEmpty(txtPeriodsName.Text))
            {
                ParentObject.NotifyWarning("Dönem ismi alanı boş bırakılamaz!");
                return;
            }

            // Flyout is in add mode
            if (SideMenuState == FlyoutState.Add)
            {
                var per = new EISPeriod(txtPeriodsName.Text);
                var result = per.Insert(EISSystem.Connection);
                if (result == -1)
                {
                    ParentObject.NotifyError("Dönem isim çakışması, başka isim belirtin.");
                    return;
                }
               
                EISSystem.Periods.Add(per);
                Grid.Items.Add(per);
                ParentObject.NotifySuccess("Dönem ekleme başarılı!");

                sideFlyout.IsOpen = false;
            }
            // Flyout is in edit mode
            else
            {
                if (itemToEdit == null)
                    return; // somehow??

                var elem = (TextBlock)Grid.Columns[2].GetCellContent(itemToEdit);
                itemToEdit.Name = txtPeriodsName.Text;
               
                var result = itemToEdit.Update(EISSystem.Connection);
                if (result == -1)
                {
                    ParentObject.NotifyError("Dönem isim çakışması, başka isim belirtin.");
                    return;
                }

                elem.Text = txtPeriodsName.Text;
                // Edit the datagrid cell

                ParentObject.NotifySuccess("Düzenleme başarılı!");
                sideFlyout.IsOpen = false;
            }
        }

        private void TileSearchClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
