using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ExamEvaluationSystem
{
    /// <summary>
    /// ViewAdminInspectExam.xaml etkileşim mantığı
    /// </summary>
    public partial class ViewAdminInspectExam : IChildObject<AdminPanel>
    {
        public List<EISExamTriple> Exams;

        public AdminPanel ParentObject { get; set; }

        public ViewAdminInspectExam(List<EISExamTriple> triple, AdminPanel parent)
        {
            ParentObject = parent;
            InitializeComponent();
            Exams = triple;
            RefreshData();
        }

        public void RefreshData()
        {
            Grid.Items.Clear();
            foreach (var e in Exams)
                Grid.Items.Add(e);
        }

        private void ClickVisa(object sender, RoutedEventArgs e)
        {
            var sel = Grid.SelectedItem;
            if (sel == null) return;
            var d = (EISExamTriple)sel;
            if (d.Visa == null) return;
            var res = new EISExamResult(-1).SelectList(EISSystem.Connection, Where.Equals("ExamID", d.Visa.ID.ToString()));
            new WindowLecturerInpectExam(d.Visa, res).ShowDialog();
        }

        private void ClickFinal(object sender, RoutedEventArgs e)
        {
            var sel = Grid.SelectedItem;
            if (sel == null) return;
            var d = (EISExamTriple)sel;
            if (d.Final == null) return;
            var res = new EISExamResult(-1).SelectList(EISSystem.Connection, Where.Equals("ExamID", d.Final.ID.ToString()));
            new WindowLecturerInpectExam(d.Final, res).ShowDialog();
        }

        private void ClickComplement(object sender, RoutedEventArgs e)
        {
            var sel = Grid.SelectedItem;
            if (sel == null) return;
            var d = (EISExamTriple)sel;
            if (d.Complement == null) return;
            var res = new EISExamResult(-1).SelectList(EISSystem.Connection, Where.Equals("ExamID", d.Complement.ID.ToString()));
            new WindowLecturerInpectExam(d.Complement, res).ShowDialog();
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
