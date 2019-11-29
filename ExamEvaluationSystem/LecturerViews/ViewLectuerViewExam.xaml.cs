using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public class EISExamTriple : INotifyPropertyChanged
    {
        private bool _checked;
        public bool Checked
        {
            get
            {
                return _checked;
            }
            set
            {
                _checked = value;
                OnPropertyChanged("Checked");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public EISLecture Lecture;
        public EISPeriod Period;
        public EISExam Visa;
        public EISExam Final;
        public EISExam Complement;

        public string LectureName { get { return Lecture.Name; } }
        public string PeriodName { get { return Period.Name; } }

        public string VisaFriendlyName { get { return "Vize"; } }
        public string FinalFriendlyName { get { return "Final"; } }
        public string ComplementFriendlyName { get { return "Bütünleme"; } }

        public bool IsVisaDone { get { return Visa != null; } }
        public bool IsFinalDone { get { return Final != null; } }
        public bool IsComplementDone { get { return Complement != null; } }
    }
    /// <summary>
    /// ViewLectuerViewExam.xaml etkileşim mantığı
    /// </summary>
    public partial class ViewLecturerViewExam : IChildObject<UserPanel>
    {
        public List<EISExamTriple> Exams;

        public UserPanel ParentObject { get; set; }

        public ViewLecturerViewExam(List<EISExamTriple> triple, UserPanel parent)
        {
            InitializeComponent();
            Exams = triple;
            ParentObject = parent;
            RefreshData();
        }

        public void RefreshData()
        {
            Grid.Items.Clear();
            foreach (var e in Exams)
                foreach (var l in ParentObject.Lecturer.Associated)
                    if (l.ID == e.Lecture.ID)
                    {
                        Grid.Items.Add(e);
                        break;
                    }
        }

        private void ClickVisa(object sender, RoutedEventArgs e)
        {
            var sel = Grid.SelectedItem;
            if (sel == null) return;
            var d = (EISExamTriple)sel;
            if (d.Visa == null) return;
            var res = new EISExamResult(-1).SelectList(EISSystem.Connection, Where.Equals("ExamID", d.Visa.ID.ToString()));
            new WindowLecturerInpectExam(d.Visa, res, ParentObject).ShowDialog();
        }

        private void ClickFinal(object sender, RoutedEventArgs e)
        {
            var sel = Grid.SelectedItem;
            if (sel == null) return;
            var d = (EISExamTriple)sel;
            if (d.Final == null) return;
            var res = new EISExamResult(-1).SelectList(EISSystem.Connection, Where.Equals("ExamID", d.Final.ID.ToString()));
            new WindowLecturerInpectExam(d.Final, res, ParentObject).ShowDialog();
        }

        private void ClickComplement(object sender, RoutedEventArgs e)
        {
            var sel = Grid.SelectedItem;
            if (sel == null) return;
            var d = (EISExamTriple)sel;
            if (d.Complement == null) return;
            var res = new EISExamResult(-1).SelectList(EISSystem.Connection, Where.Equals("ExamID", d.Complement.ID.ToString()));
            new WindowLecturerInpectExam(d.Complement, res, ParentObject).ShowDialog();
        }
    }
}
