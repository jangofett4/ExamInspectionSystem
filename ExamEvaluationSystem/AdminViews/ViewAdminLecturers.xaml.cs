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
        public void RefreshDataGrid()
        {
            foreach (var data in EISSystem.Lecturers)
                if (data.ID < 10)
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
            txtLecturerName.Text = item.Name;
            txtLecturerID.Value = item.ID;
            txtLecturerSurname.Text = item.Surname;
            //txtLecturerUsername.Text = zıkkım.Username;
            //txtLecturerPassword.Password = zıkkım.Password;
            txtLecturerID.IsReadOnly = true; // cannot change this, lots of foreign key erors
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
            if (string.IsNullOrEmpty(txtLecturerName.Text))
            {
                ParentObject.NotifyWarning("İsim alanı boş bırakılamaz!");
                return;
            }

            if (selectorLecturerFaculty.SelectedData == null)
            {
                ParentObject.NotifyWarning("Fakülte alanı boş bırakılamaz!");
                return;
            }

            if (SideMenuState == FlyoutState.Add)
            {
                var lec = new EISLecturer((int)txtLecturerID.Value.Value, txtLecturerName.Text,txtLecturerSurname.Text, (EISFaculty)selectorLecturerFaculty.SelectedData);
                var result = lec.Insert(EISSystem.Connection);
                if (result == -1)
                {
                    ParentObject.NotifyError("Sicil numarası çakışması, başka numara belirtin.");
                    return;
                }
                var cmd = new EISInsertCommand("UserLoginInfo");
                var sqlcmd = cmd.Create(EISSystem.Connection, "Username", $"'{ txtLecturerUsername.Text }'", "Password", txtLecturerPassword.Password.EncapsulateQuote(),"UserID",lec.ID.ToString(),"MemberPrivilege","1"); 
                /*try
                {
                    sqlcmd.ExecuteNonQuery();
                }
                catch (SQLiteException e)
                {
                    if (e.Message.Contains(".Name"))

                      
                }*/

                EISSystem.Lecturers.Add(lec);
                Grid.Items.Add(lec);
                ParentObject.NotifySuccess("Öğretim elemanı ekleme başarılı!");

                sideFlyout.IsOpen = false;
            }
            // Flyout is in edit mode
            else
            {
                /* if (itemToEdit == null)
                    return; // somehow??

                var elem = (TextBlock)Grid.Columns[1].GetCellContent(itemToEdit);
                var elem1 = (TextBlock)Grid.Columns[3].GetCellContent(itemToEdit);
                itemToEdit.Name = txtLecturerName.Text;
                
                itemToEdit.Faculty = (EISFaculty)selectorLecturerFaculty.SelectedData;

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
                sideFlyout.IsOpen = false; */
            }
        }
        private void TileSearchClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
