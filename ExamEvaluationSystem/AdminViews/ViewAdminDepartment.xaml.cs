using MahApps.Metro.Controls;
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

        public ViewAdminDepartment(AdminPanel parent)
        {
            InitializeComponent();
            ParentObject = parent;
            RefreshDataGrid();

            selectorDepartmentFaculty.ClickCallback = () =>
            {
                var s = new PropertyDataSelector("Fakülte Seç");
                var builder = new SingleDataSelectorBuilder<EISFaculty>(EISSystem.Faculties, s, "ID", ("Name", "Fakülte Adı"), ("ID", "Fakülte Kodu"));
                builder.BuildAll();
                
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

        private void TileDeleteClick(object sender, RoutedEventArgs e)
        {

        }

        private void TileFlyoutDoneClick(object sender, RoutedEventArgs e)
        {
            /*if (!txtFacultyID.Value.HasValue)
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

                var elem = (TextBlock)Grid.Columns[1].GetCellContent(itemToEdit);
                itemToEdit.Name = txtFacultyName.Text;
                var result = itemToEdit.Update(EISSystem.Connection);
                if (result == -1)
                {
                    ParentObject.NotifyError("Fakülte isim çakışması, başka isim belirtin.");
                    return;
                }

                elem.Text = txtFacultyName.Text; // Edit the datagrid cell

                ParentObject.NotifySuccess("Düzenleme başarılı!");
                sideFlyout.IsOpen = false;
            }*/
        }
    }
}
