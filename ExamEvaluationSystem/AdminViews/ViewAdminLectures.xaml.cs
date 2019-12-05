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
    /// ViewAdminLectures.xaml etkileşim mantığı
    /// </summary>
    public partial class ViewAdminLectures : IChildObject<AdminPanel>
    {
        public AdminPanel ParentObject { get; set; }
        public FlyoutState SideMenuState { get; set; }
        private EISLecture itemToEdit;

        public ViewAdminLectures(AdminPanel parent)
        {
            InitializeComponent();
            ParentObject = parent;
            RefreshDataGrid();

            selectorLectureEarnings.ClickCallback = () =>
            {
                var s = new PropertyDataSelector("Ders Kazanımlarını Seç");
                var builder = new MultiDataSelectorBuilder<EISEarning>(EISSystem.LectureEarnings, s, "ID",
                    (e, q) => e.Name.ToLower().Contains(q) ? true : false,
                    ("Name", "Kazanım"));
                builder.DisableSearch = false;
                builder.BuildAll();

                if (selectorLectureEarnings.SelectedData != null)
                {
                    builder.GridSelect(s.dgSelector, selectorLectureEarnings.SelectedData); // Pre-Select data in grid
                    builder.SelectedData = (List<EISEarning>)selectorLectureEarnings.SelectedData;
                }

                s.ShowDialog();

                if (builder.SelectedData == null)
                {
                    selectorLectureEarnings.Text = "Seçilen Kazanım Yok";
                    return;
                }

                selectorLectureEarnings.SelectedData = builder.SelectedData;
                selectorLectureEarnings.Text = $"[{ builder.SelectedData.Count } ders kazanımı]";
            };
            SetupSearch();
        }

        public void ClearSelected()
        {
            foreach (var data in Grid.Items)
            {
                var x = ((EISLecture)data);
                if (x.Checked)
                    x.Checked = false;
            }
        }

        private List<EISLecture> GetSelectedLectures()
        {
            var lst = new List<EISLecture>();
            foreach (var data in Grid.Items)
            {
                var x = ((EISLecture)data);
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
            foreach (var f in EISSystem.Lectures)
                Grid.Items.Add(f);
        }
        private void TileAddClick(object sender, RoutedEventArgs e)
        {
            sideFlyout.Header = "Ekle";
            sideFlyout.IsOpen = true;
            // might be artifact from edit mode
            SideMenuState = FlyoutState.Add;
        }

        private void GridDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Grid.SelectedItem == null)
                return;

            var item = (EISLecture)Grid.SelectedItem;
            itemToEdit = item;
            txtLecturesName.Text = item.Name;
            txtLecturesCredit.Value = item.Credit;
            selectorLectureEarnings.SelectedData = item.Earnings;
            selectorLectureEarnings.Text = $"[{ item.Earnings.Count } ders kazanımı]";

            sideFlyout.Header = "Düzenle";
            sideFlyout.IsOpen = true;
            SideMenuState = FlyoutState.Edit;
        }
        private void TileFlyoutDoneClick(object sender, RoutedEventArgs e)
        {
            if (!txtLecturesCredit.Value.HasValue)
            {
                ParentObject.NotifyWarning("Ders kredi alanı boş burakılamaz!");
                return;
            }

            txtLecturesName.Text = txtLecturesName.Text.Trim();

            if (string.IsNullOrEmpty(txtLecturesName.Text))
            {
                ParentObject.NotifyWarning("Ders ismi alanı boş bırakılamaz!");
                return;
            }

            if (selectorLectureEarnings.SelectedData == null)
            {
                ParentObject.NotifyWarning("Hiç kazanım seçilmedi!");
                return;
            }

            var earnings = (List<EISEarning>)selectorLectureEarnings.SelectedData;
            if (earnings.Count == 0)
            {
                ParentObject.NotifyWarning("Hiç kazanım seçilmedi!");
                return;
            }

            // Flyout is in add mode
            if (SideMenuState == FlyoutState.Add)
            {
                var lec = new EISLecture(-1, txtLecturesName.Text, (int)txtLecturesCredit.Value.Value);
                var result = lec.Insert(EISSystem.Connection);
                lec.Earnings = earnings;
                lec.InsertEarnings(EISSystem.Connection);

                EISSystem.Lectures.Add(lec);
                Grid.Items.Add(lec);
                ParentObject.NotifySuccess("Ders ekleme başarılı!");

                sideFlyout.IsOpen = false;
            }
            // Flyout is in edit mode
            else
            {
                if (itemToEdit == null)
                    return; // somehow??

                itemToEdit.Name = txtLecturesName.Text;
                itemToEdit.Credit = (int)txtLecturesCredit.Value.Value;
                itemToEdit.Earnings = (List<EISEarning>)selectorLectureEarnings.SelectedData;
                var result = itemToEdit.Update(EISSystem.Connection);


                itemToEdit.OnPropertyChanged("Name", "Credit");

                itemToEdit.InsertEarnings(EISSystem.Connection);
                ParentObject.NotifySuccess("Düzenleme başarılı!");
                sideFlyout.IsOpen = false;
            }
        }
        private async void TileDeleteClick(object sender, RoutedEventArgs e)
        {
            var dataToDelete = GetSelectedLectures();

            if (dataToDelete.Count == 0)
            {
                ParentObject.NotifyWarning("Silmek istediğiniz verileri tablodan seçin");
                return;
            }

            var result = await ParentObject.ShowMessageAsync("Uyarı", $"{ dataToDelete.Count } ders bilgisi silinecek (bağıntılı olan alınan dersler bilgisi).\nEmin misiniz?", MessageDialogStyle.AffirmativeAndNegative);
            if (result == MessageDialogResult.Affirmative)
            {
                foreach (var data in dataToDelete) // for each data selected
                {
                    Grid.Items.Remove(data);            // Remove from grid
                    EISSystem.Lectures.Remove(data);   // Remove from system in memory
                    data.Delete(EISSystem.Connection);  // Remove from database
                }
                ParentObject.NotifyInformation($"{ dataToDelete.Count } ders bilgisi silindi.");
            }
            else
            {
                Grid.SelectedIndex = -1;
                ParentObject.NotifyInformation("Silme işlemi iptal edildi.");
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
                    foreach (var data in EISSystem.Lectures)
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
