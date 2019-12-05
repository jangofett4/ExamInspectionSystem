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
using System.Windows.Input;

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
            SetupSearch();
        }

        public void ClearSelected()
        {
            foreach (var data in Grid.Items)
            {
                var x = ((EISPeriod)data);
                if (x.Checked)
                    x.Checked = false;
            }
        }

        private List<EISPeriod> GetSelectedPeriods()
        {
            var lst = new List<EISPeriod>();
            foreach (var data in Grid.Items)
            {
                var x = ((EISPeriod)data);
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
            foreach (var p in EISSystem.Periods)
                    Grid.Items.Add(p);
        }

        private void TileAddClick(object sender, RoutedEventArgs e)
        {
            sideFlyout.Header = "Ekle";
            sideFlyout.IsOpen = true;

            var date = DateTime.Now;
            if (date.Month > 9 || date.Month <= 2)
            {
                if (date.Month <= 2)
                {
                    txtPeriodsName.Text = $"{date.Year - 1}-{ date.Year } Güz Yarıyılı";
                }
                else
                {
                    txtPeriodsName.Text = $"{ date.Year }-{ date.Year + 1 } Güz Yarıyılı";
                }
            }
            else
            {
                txtPeriodsName.Text = $"{ date.Year - 1 }-{ date.Year } Bahar Yarıyılı";
            }

            SideMenuState = FlyoutState.Add;
        }

        private async void TileDeleteClick(object sender, RoutedEventArgs e)
        {
            var dataToDelete = GetSelectedPeriods();

            if (dataToDelete.Count == 0)
            {
                ParentObject.NotifyWarning("Silmek istediğiniz verileri tablodan seçin (çoklu seçim için Control ya da Shift kullanın)");
                return;
            }

            var result = await ParentObject.ShowMessageAsync("Uyarı", $"{ dataToDelete.Count } dönem bilgisi silinecek (bağıntılı olan dersler ve alt bilgiler dahil).\nSistem olan en güncel dönemi aktif edecektir.\nEmin misiniz?", MessageDialogStyle.AffirmativeAndNegative);
            if (result == MessageDialogResult.Affirmative)
            {
                foreach (var data in dataToDelete) // for each data selected
                {
                    Grid.Items.Remove(data);            // Remove from grid
                    EISSystem.Periods.Remove(data);   // Remove from system in memory
                    data.Delete(EISSystem.Connection);  // Remove from database
                }
                ParentObject.NotifyInformation($"{ dataToDelete.Count } dönem bilgisi silindi.");
                var id = new EISSelectLastCommand("System", "ActivePeriod").Create(EISSystem.Connection).ExecuteScalar();
                if (id != null) // if there is more periods left
                {
                    EISSystem.ActivePeriod = new EISPeriod((int)((long)id)).SelectT(EISSystem.Connection);
                }
                else
                {
                    EISSystem.ActivePeriod = null;
                    ParentObject.NotifyWarning("Sistemde başka dönem kaldığından dolayı aktif dönem belirlenemedi. Sistem bu durumda karasız çalışabilir. Programı tekrar açmayı ya da yeni dönem eklemeyi deneyin.");
                }

                if (EISSystem.ActivePeriod != null)
                    foreach (var lec in EISSystem.Lecturers)
                    {
                        using (var asrd = lec.SelectAssociated(EISSystem.Connection, EISSystem.ActivePeriod))
                        {
                            while (asrd.Read())
                                lec.Associated.Add(EISSystem.GetLecture(asrd.GetInt32(3)));
                        }
                    }
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

        private async void TileFlyoutDoneClick(object sender, RoutedEventArgs e)
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
                var dialogresult = await ParentObject.ShowMessageAsync("Uyarı", "Yeni eklenecek dönem sistemin aktif dönemi olarak belirlenecek ve sınav düzenlemek mümkün olmayacak.\nEski sınavlar arşiv amacıyla silinmeyecektir.\nEmin misiniz?", MessageDialogStyle.AffirmativeAndNegative);
                if (dialogresult == MessageDialogResult.Negative)
                {
                    ParentObject.NotifyInformation("Dönem ekleme iptal edildi.");
                    return;
                }

                var per = new EISPeriod(txtPeriodsName.Text);
                var result = per.Insert(EISSystem.Connection);
                if (result == -1)
                {
                    ParentObject.NotifyError("Dönem isim çakışması, başka isim belirtin.");
                    return;
                }
               
                EISSystem.Periods.Add(per);
                Grid.Items.Add(per);
                ParentObject.NotifySuccess("Sistem yeni dönem için güncellendi!");

                
                EISSystem.ActivePeriod = per; // change active period
                
                foreach (var lec in EISSystem.Lecturers)
                {
                    using (var asrd = lec.SelectAssociated(EISSystem.Connection, EISSystem.ActivePeriod))
                    {
                        while (asrd.Read())
                            lec.Associated.Add(EISSystem.GetLecture(asrd.GetInt32(3)));
                    }
                }
                
                new EISInsertCommand("System").Create(EISSystem.Connection, "ActivePeriod", per.ID.ToString()).ExecuteNonQuery(); // change active period in database

                sideFlyout.IsOpen = false;
            }
            // Flyout is in edit mode
            else
            {
                if (itemToEdit == null)
                    return; // somehow??

                itemToEdit.Name = txtPeriodsName.Text;
               
                var result = itemToEdit.Update(EISSystem.Connection);
                if (result == -1)
                {
                    ParentObject.NotifyError("Dönem isim çakışması, başka isim belirtin.");
                    return;
                }

                itemToEdit.OnPropertyChanged("Name");

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
                    foreach (var data in EISSystem.Periods)
                    {
                        string sq = searchQuery.Text.ToLower();
                        if (data.Name.ToLower().Contains(sq))
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
