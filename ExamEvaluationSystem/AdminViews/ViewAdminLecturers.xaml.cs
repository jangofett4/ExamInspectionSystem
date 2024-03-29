﻿using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ExamEvaluationSystem
{
    /// <summary>
    /// ViewAdminLecturers.xaml etkileşim mantığı
    /// </summary>
    public partial class ViewAdminLecturers : IChildObject<AdminPanel>
    {
        public AdminPanel ParentObject { get; set; }
        public FlyoutState SideMenuState { get; set; }
        
        private EISLecturer itemToEdit;
        private EISUserLoginInfo loginToEdit;

        public ViewAdminLecturers(AdminPanel parent)
        {
            InitializeComponent();
            ParentObject = parent;
            RefreshDataGrid();

            selectorLecturerFaculty.ClickCallback = () =>
            {
                var s = new PropertyDataSelector("Fakülte Seç");
                var builder = new SingleDataSelectorBuilder<EISFaculty>(EISSystem.Faculties, s, "ID",
                    (f, q) => (f.Name.ToLower().Contains(q) || f.ID.ToString().Contains(q)) ? true : false, 
                    ("Name", "Fakülte Adı"), ("ID", "Fakülte Kodu"));
                builder.DisableSearch = false;
                builder.BuildAll((EISFaculty f) => f.ID >= 10);

                if (selectorLecturerFaculty.SelectedData != null)
                {
                    builder.GridSelect(s.dgSelector, selectorLecturerFaculty.SelectedData); // Pre-Select data in grid
                    builder.SelectedData = (EISFaculty)selectorLecturerFaculty.SelectedData;
                }

                s.ShowDialog();

                if (builder.SelectedData == null)
                {
                    selectorLecturerFaculty.Text = "";
                    return;
                }

                selectorLecturerFaculty.SelectedData = builder.SelectedData;
                selectorLecturerFaculty.Text = builder.SelectedData.Name;
            };
            SetupSearch();
        }

        public void ClearSelected()
        {
            foreach (var data in Grid.Items)
            {
                var x = ((EISLecturer)data);
                if (x.Checked)
                    x.Checked = false;
            }
        }

        private List<EISLecturer> GetSelectedLecturers()
        {
            var lst = new List<EISLecturer>();
            foreach (var data in Grid.Items)
            {
                var x = ((EISLecturer)data);
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
            foreach (var data in EISSystem.Lecturers)
                if (data.ID > 0)
                    Grid.Items.Add(data);
        }

        private void TileAddClick(object sender, RoutedEventArgs e)
        {
            sideFlyout.Header = "Ekle";
            sideFlyout.IsOpen = true;
            //txtDepartmentID.IsReadOnly = false; // might be artifact from edit mode
            SideMenuState = FlyoutState.Add;
        }

        private async void TileDeleteClick(object sender, RoutedEventArgs e)
        {
            var dataToDelete = GetSelectedLecturers();

            if (dataToDelete.Count == 0)
            {
                ParentObject.NotifyWarning("Silmek istediğiniz verileri tablodan seçin");
                return;
            }

            var result = await ParentObject.ShowMessageAsync("Uyarı", $"{ dataToDelete.Count } öğretim görevlisi bilgisi silinecek (bağıntılı olan dersler ve sınavlar vb. dahil).\nEmin misiniz?", MessageDialogStyle.AffirmativeAndNegative);
            if (result == MessageDialogResult.Affirmative)
            {
                foreach (var data in dataToDelete) // for each data selected
                {
                    Grid.Items.Remove(data);            // Remove from grid
                    EISSystem.Lecturers.Remove(data); // Remove from system in memory
                    data.Delete(EISSystem.Connection);  // Remove from database
                }
                ParentObject.ViewAdminLectureAssociate.RefreshDataGrid();
                ParentObject.NotifyInformation($"{ dataToDelete.Count } öğretim elemanı bilgisi silindi.");
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

            var item = (EISLecturer)Grid.SelectedItem;
            
            itemToEdit = item;
            loginToEdit = new EISUserLoginInfo(-1).SelectT(EISSystem.Connection, Where.Equals("UserID", itemToEdit.ID.ToString()));

            txtLecturerName.Text = item.Name;
            txtLecturerID.Value = item.ID;
            txtLecturerSurname.Text = item.Surname;
            txtLecturerUsername.Text = loginToEdit.Username;
            txtLecturerPassword.Password = loginToEdit.Password;
            selectorLecturerFaculty.SelectedData = item.Faculty;
            selectorLecturerFaculty.Text = item.Faculty.Name;

            sideFlyout.Header = "Düzenle";
            sideFlyout.IsOpen = true;
            SideMenuState = FlyoutState.Edit;
        }

        private void TileFlyoutDoneClick(object sender, RoutedEventArgs e)
        {
            if (!txtLecturerID.Value.HasValue)
            {
                ParentObject.NotifyWarning("Sicil numarası alanı boş burakılamaz!");
                return;
            }

            txtLecturerName.Text = txtLecturerName.Text.Trim();
            txtLecturerSurname.Text = txtLecturerSurname.Text.Trim();
            txtLecturerUsername.Text = txtLecturerUsername.Text.Trim();
            txtLecturerPassword.Password = txtLecturerPassword.Password.Trim();
            if (string.IsNullOrEmpty(txtLecturerName.Text) || string.IsNullOrEmpty(txtLecturerSurname.Text))
            {
                ParentObject.NotifyWarning("İsim/Soyisim alanı boş bırakılamaz!");
                return;
            }

            if (string.IsNullOrEmpty(txtLecturerUsername.Text) || string.IsNullOrEmpty(txtLecturerPassword.Password))
            {
                ParentObject.NotifyWarning("Kullanıcı adı / Şifre alanı boş bırakılamaz!");
                return;
            }

            if (selectorLecturerFaculty.SelectedData == null)
            {
                ParentObject.NotifyWarning("Fakülte alanı boş bırakılamaz!");
                return;
            }

            if (SideMenuState == FlyoutState.Add)
            {
                var trn = new EISTransaction();
                trn.Begin(EISSystem.Connection);

                var lec = new EISLecturer((int)txtLecturerID.Value.Value, txtLecturerName.Text,txtLecturerSurname.Text, (EISFaculty)selectorLecturerFaculty.SelectedData);
                var result = lec.Insert(EISSystem.Connection);
                if (result == -1)
                {
                    trn.Rollback(EISSystem.Connection);
                    ParentObject.NotifyError("Sicil numarası çakışması, başka numara belirtin.");
                    return;
                }
                var inf = new EISUserLoginInfo(-1, txtLecturerUsername.Text, txtLecturerPassword.Password, lec.ID, 1);
                var result2 = inf.Insert(EISSystem.Connection);
                if (result2 == -1)
                {
                    trn.Rollback(EISSystem.Connection);
                    ParentObject.NotifyError("Aynı kullanıcı adına sahip öğretim üyesi sistemde mevcut, başka kullanıcı adı belirleyin.");
                    return;
                }
                trn.Commit(EISSystem.Connection);

                EISSystem.Lecturers.Add(lec);
                Grid.Items.Add(lec);
                ParentObject.ViewAdminLectureAssociate.RefreshDataGrid();
                ParentObject.NotifySuccess("Öğretim elemanı ekleme başarılı!");

                sideFlyout.IsOpen = false;
            }
            // Flyout is in edit mode
            else
            {
                if (itemToEdit == null)
                    return; // somehow??

                int id = itemToEdit.ID;
                itemToEdit.Store();

                if ((int)txtLecturerID.Value.Value == id) // ID is NOT changed
                {
                    var trn = new EISTransaction();
                    trn.Begin(EISSystem.Connection);

                    itemToEdit.Name = txtLecturerName.Text;
                    itemToEdit.Surname = txtLecturerSurname.Text;
                    itemToEdit.Faculty = (EISFaculty)selectorLecturerFaculty.SelectedData;

                    var result = itemToEdit.Update(EISSystem.Connection);
                    if (result == -1)
                    {
                        ParentObject.NotifyError("Sicil numarası çakışması, başka numara belirtin.");
                        itemToEdit.Restore();
                        trn.Rollback(EISSystem.Connection);
                        return;
                    }

                    // Edit the datagrid cells
                    itemToEdit.OnPropertyChanged("Name", "Surname", "FacultyName");

                    loginToEdit.Store();
                    loginToEdit.Username = txtLecturerUsername.Text;
                    loginToEdit.Password = txtLecturerPassword.Password;

                    result = loginToEdit.Update(EISSystem.Connection);
                    if (result == -1)
                    {
                        ParentObject.NotifyError("Aynı kullanıcı adına sahip öğretim üyesi sistemde mevcut, başka kullanıcı adı belirleyin.");
                        loginToEdit.Restore();
                        itemToEdit.Restore();
                        trn.Rollback(EISSystem.Connection);
                        return;
                    }

                    trn.Commit(EISSystem.Connection);

                    ParentObject.NotifySuccess("Düzenleme başarılı!");
                    sideFlyout.IsOpen = false;
                }
                else
                {
                    var trn = new EISTransaction();
                    trn.Begin(EISSystem.Connection);

                    itemToEdit.ID = (int)txtLecturerID.Value.Value;
                    itemToEdit.Name = txtLecturerName.Text;
                    itemToEdit.Surname = txtLecturerSurname.Text;
                    itemToEdit.Faculty = (EISFaculty)selectorLecturerFaculty.SelectedData;

                    var result = itemToEdit.UpdateWhere(EISSystem.Connection, Where.Equals("ID", id.ToString()));
                    if (result == -1)
                    {
                        ParentObject.NotifyError("Sicil numarası çakışması, başka numara belirtin.");
                        itemToEdit.Restore();
                        trn.Rollback(EISSystem.Connection);
                        return;
                    }

                    loginToEdit.Store();
                    loginToEdit.UserID = itemToEdit.ID;
                    loginToEdit.Username = txtLecturerUsername.Text;
                    loginToEdit.Password = txtLecturerPassword.Password;

                    result = loginToEdit.Update(EISSystem.Connection);
                    if (result == -1)
                    {
                        ParentObject.NotifyError("Aynı kullanıcı adına sahip öğretim üyesi sistemde mevcut, başka kullanıcı adı belirleyin.");
                        loginToEdit.Restore();
                        itemToEdit.Restore();
                        trn.Rollback(EISSystem.Connection);
                        return;
                    }

                    trn.Commit(EISSystem.Connection);

                    itemToEdit.OnPropertyChanged("ID", "Name", "Surname", "FacultyName");

                    ParentObject.ViewAdminLectureAssociate.RefreshDataGrid();
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
                    foreach (var data in EISSystem.Lecturers)
                    {
                        string sq = searchQuery.Text.ToLower();
                        string ns = data.Name + data.Surname; ns = ns.ToLower();
                        if (data.ID > 10 && (ns.Contains(sq) || data.FacultyName.ToLower().Contains(sq)))
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
