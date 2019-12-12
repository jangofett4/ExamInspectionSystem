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
    /// ViewStudents.xaml etkileşim mantığı
    /// </summary>
    public partial class ViewAdminLectureAssociate : IChildObject<AdminPanel>
    {
        public AdminPanel ParentObject { get; set; }

        public EISLecturer itemToEdit;

        public ViewAdminLectureAssociate(AdminPanel parent)
        {
            InitializeComponent();
            ParentObject = parent;
            RefreshDataGrid();

            selectorLecturerFaculty.ClickCallback = () =>
            {
                var s = new PropertyDataSelector("Atanacak Dersleri Seç");
                var builder = new MultiDataSelectorBuilder<EISLecture>(EISSystem.Lectures, s, "ID",
                    (l, q) => l.Name.ToLower().Contains(q) ? true : false, 
                    ("Name", "Ders Adı"));
                builder.DisableSearch = false;
                builder.BuildAll();

                if (selectorLecturerFaculty.SelectedData != null)
                {
                    builder.GridSelect(s.dgSelector, selectorLecturerFaculty.SelectedData); // Pre-Select data in grid
                    builder.SelectedData = (List<EISLecture>)selectorLecturerFaculty.SelectedData;
                }

                s.ShowDialog();

                if (builder.SelectedData == null)
                {
                    selectorLecturerFaculty.Text = "Atanan Ders Yok";
                    return;
                }

                selectorLecturerFaculty.SelectedData = builder.SelectedData;
                selectorLecturerFaculty.Text = $"[{ builder.SelectedData.Count } ders ataması]";
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

        /*private List<EISLecturer> GetSelectedLecturers()
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
        }*/

        public void RefreshDataGrid()
        {
            Grid.Items.Clear();
            foreach (var data in EISSystem.Lecturers)
                if (data.ID > 0)
                    Grid.Items.Add(data);
        }

        private void GridDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (Grid.SelectedItem == null)
                return;
            if (EISSystem.ActivePeriod == null)
            {
                ParentObject.NotifyWarning("Sistemde aktif dönem bulunmadığı için ders atama işlemi mümkün değil.");
                return;
            }

            var item = (EISLecturer)Grid.SelectedItem;
            itemToEdit = item;
            txtLecturerName.Text = item.Name + " " + item.Surname;
            txtLecturerID.Value = item.ID;
            txtPeriod.Text = EISSystem.ActivePeriod.Name;
            selectorLecturerFaculty.SelectedData = item.Associated;
            selectorLecturerFaculty.Text = $"[{ item.Associated.Count } ders ataması]";

            sideFlyout.Header = "Ders Atama";
            sideFlyout.IsOpen = true;
        }

        private void TileFlyoutDoneClick(object sender, RoutedEventArgs e)
        {
            itemToEdit.Associated = (List<EISLecture>)selectorLecturerFaculty.SelectedData;
            itemToEdit.InsertAssociated(EISSystem.Connection, EISSystem.ActivePeriod);
            ParentObject.NotifySuccess("Ders atama başarılı!");
            sideFlyout.IsOpen = false;
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
