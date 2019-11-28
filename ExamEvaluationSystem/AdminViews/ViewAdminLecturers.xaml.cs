using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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
                var builder = new SingleDataSelectorBuilder<EISFaculty>(EISSystem.Faculties, s, "ID", ("Name", "Fakülte Adı"), ("ID", "Fakülte Kodu"));
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
            var dataToDelete = new List<EISLecturer>();
            foreach (var item in Grid.Items)
            {
                var x = ((DataGridTemplateColumn)Grid.Columns[0]).GetCellContent(item).FindChild<CheckBox>("Check");
                if (x.IsChecked == true)
                    dataToDelete.Add((EISLecturer)item);
            }

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
                ParentObject.NotifySuccess("Öğretim elemanı ekleme başarılı!");

                sideFlyout.IsOpen = false;
            }
            // Flyout is in edit mode
            else
            {
                if (itemToEdit == null)
                    return; // somehow??

                var elem = (TextBlock)Grid.Columns[1].GetCellContent(itemToEdit);
                var elem1 = (TextBlock)Grid.Columns[2].GetCellContent(itemToEdit);
                var elem2 = (TextBlock)Grid.Columns[3].GetCellContent(itemToEdit);
                var elem3 = (TextBlock)Grid.Columns[4].GetCellContent(itemToEdit);

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
                    elem1.Text = itemToEdit.Name;
                    elem2.Text = itemToEdit.Surname;
                    elem3.Text = itemToEdit.Faculty.Name;

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
                    // TODO: might add associated lectures

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
                    // Edit the datagrid cell
                    elem.Text = itemToEdit.ID.ToString(); 
                    elem1.Text = itemToEdit.Name;
                    elem2.Text = itemToEdit.Surname;
                    elem3.Text = itemToEdit.Faculty.Name;

                    ParentObject.NotifySuccess("Düzenleme başarılı!");
                    sideFlyout.IsOpen = false;
                }
            }
        }

        private void TileSearchClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
