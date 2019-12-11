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
    /// ViewEarnings.xaml etkileşim mantığı
    /// </summary>
    public partial class ViewAdminEarnings : IChildObject<AdminPanel>
    {
        public AdminPanel ParentObject { get; set; }
        public FlyoutState SideMenuState { get; set; }

        private EISEarning itemToEdit;

        public ViewAdminEarnings(AdminPanel parent)
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
                var x = ((EISEarning)data);
                if (x.Checked)
                    x.Checked = false;
            }
        }

        private List<EISEarning> GetSelectedEarnings()
        {
            var lst = new List<EISEarning>();
            foreach (var data in Grid.Items)
            {
                var x = ((EISEarning)data);
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
            foreach (var data in EISSystem.Earnings)
                Grid.Items.Add(data);
        }

        private void TileAddClick(object sender, RoutedEventArgs e)
        {
            sideFlyout.Header = "Ekle";
            sideFlyout.IsOpen = true;
            SideMenuState = FlyoutState.Add;
        }

        private void GridDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Grid.SelectedItem == null)
                return;

            var item = (EISEarning)Grid.SelectedItem;
            itemToEdit = item;
            txtEarningName.Text = item.Name;
            txtEarningCode.Text = item.Code;
            comboEarningType.SelectedIndex = (int)item.EarningType;

            sideFlyout.Header = "Düzenle";
            sideFlyout.IsOpen = true;
            SideMenuState = FlyoutState.Edit;
        }

        private async void TileDeleteClick(object sender, RoutedEventArgs e)
        {
            var dataToDelete = GetSelectedEarnings();

            if (dataToDelete.Count == 0)
            {
                ParentObject.NotifyWarning("Silmek istediğiniz verileri tablodan seçin");
                return;
            }

            var result = await ParentObject.ShowMessageAsync("Uyarı", $"{ dataToDelete.Count } kazanım bilgisi silinecek (bağıntılı olan ders kazanımları ve bölüm kazanımları dahil).\nEmin misiniz?", MessageDialogStyle.AffirmativeAndNegative);
            if (result == MessageDialogResult.Affirmative)
            {
                foreach (var data in dataToDelete) // for each data selected
                {
                    Grid.Items.Remove(data);            // Remove from grid
                    EISSystem.Earnings.Remove(data);    // Remove from system in memory

                    if (data.EarningType == EISEarningType.Department)
                        EISSystem.DepartmentEarnings.Remove(data);
                    else
                        EISSystem.LectureEarnings.Remove(data);

                    data.Delete(EISSystem.Connection);  // Remove from database
                }
                ParentObject.NotifyInformation($"{ dataToDelete.Count } kazanım silindi.");
            }
            else
            {
                Grid.SelectedIndex = -1;
                ParentObject.NotifyInformation("Silme işlemi iptal edildi.");
            }
        }

        private void TileFlyoutDoneClick(object sender, RoutedEventArgs e)
        {
            txtEarningName.Text = txtEarningName.Text.Trim();
            if (string.IsNullOrEmpty(txtEarningName.Text))
            {
                ParentObject.NotifyWarning("Kazanım açıklaması/ismi alanı boş bırakılamaz!");
                return;
            }
            txtEarningCode.Text = txtEarningCode.Text.Trim();
            if (string.IsNullOrEmpty(txtEarningCode.Text))
            {
                ParentObject.NotifyWarning("Kazanum kodu alanı boş bırakılamaz!");
                return;
            }

            // Flyout is in add mode
            if (SideMenuState == FlyoutState.Add)
            {
                var ern = new EISEarning(txtEarningCode.Text, txtEarningName.Text, (EISEarningType)comboEarningType.SelectedIndex);
                ern.Insert(EISSystem.Connection);


                EISSystem.Earnings.Add(ern);
                Grid.Items.Add(ern);
                ParentObject.NotifySuccess("Kazanım ekleme başarılı!");

                if (ern.EarningType == EISEarningType.Department)
                    EISSystem.DepartmentEarnings.Add(ern);
                else
                    EISSystem.LectureEarnings.Add(ern);

                sideFlyout.IsOpen = false;
            }
            // Flyout is in edit mode
            else
            {
                if (itemToEdit == null)
                    return; // somehow??

                var type = itemToEdit.EarningType;
                var newtype = (EISEarningType)comboEarningType.SelectedIndex;
                var idxd = type == newtype ? -1 : type == EISEarningType.Department ? EISSystem.DepartmentEarnings.IndexOf(itemToEdit) : EISSystem.LectureEarnings.IndexOf(itemToEdit);

                itemToEdit.Name = txtEarningName.Text;
                itemToEdit.Code = txtEarningCode.Text;
                itemToEdit.EarningType = newtype;
                itemToEdit.Update(EISSystem.Connection);

                if (idxd != -1)
                {
                    if (type == EISEarningType.Department)
                    {
                        EISSystem.DepartmentEarnings.RemoveAt(idxd);
                        EISSystem.LectureEarnings.Add(itemToEdit);
                    }
                    else
                    {
                        EISSystem.LectureEarnings.RemoveAt(idxd);
                        EISSystem.DepartmentEarnings.Add(itemToEdit);
                    }
                }

                // Edit the datagrid cell
                itemToEdit.OnPropertyChanged("Name", "FriendlyEarningTypeName");

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
            searchAction = new DelayedActionInvoker(() => {
                Dispatcher.Invoke(() =>
                {
                    var sq = searchQuery.Text.ToLower();
                    Grid.Items.Clear();
                    foreach (var ea in EISSystem.Earnings)
                        if (ea.Name.ToLower().Contains(sq) || ea.Code.ToLower().Contains(sq))
                            Grid.Items.Add(ea);
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
            try
            {
                var split = lay.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                int i = 0;
                foreach (var c in Grid.Columns)
                {
                    c.DisplayIndex = int.Parse(split[i++]);
                    c.Width = new DataGridLength(double.Parse(split[i++]), (DataGridLengthUnitType)int.Parse(split[i++]));
                }
            }
            catch (Exception)
            {
                return; // skip it for now, next save will correct the file
            }
        }
    }
}
