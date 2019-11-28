﻿using System;
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
using System.Windows.Shapes;

namespace ExamEvaluationSystem
{
    /// <summary>
    /// UserPanel.xaml etkileşim mantığı
    /// </summary>
    public partial class UserPanel
    {
        public EISLecturer Lecturer { get; set; }

        // Views
        public ViewLecturerAddExam ExamView;

        public UserPanel(EISLecturer lec)
        {
            InitializeComponent();

            /*
            var s = new PropertyDataSelector("Title");
            var builder = new SingleDataSelectorBuilder<EISLecture>(EISSystem.Lectures, s, "ID", ("Name", "Ders Adı"), ("Credit", "Kredi"));
            builder.BuildAll();
            s.ShowDialog();
            */
            ExamView = new ViewLecturerAddExam(this);
            Lecturer = lec;
            Resources.Add("CurrentLecturerID", lec.ID.ToString());
            Resources.Add("CurrentLecturerName", lec.Name + " " + lec.Surname);
            // UserHamburgerMenuFrame.Navigate(new Uri("ViewLecturerAddExam.xaml", UriKind.RelativeOrAbsolute));

            /* Initialize views */
            ExamView = new ViewLecturerAddExam(this);
            /* End of view initialization */
            
            
            UserHamburgerMenuFrame.Content = ExamView; // Initial view
        }

        private void HamburgerItemClick(object sender, MahApps.Metro.Controls.ItemClickEventArgs e)
        {
            var tag = ((MahApps.Metro.Controls.HamburgerMenuItem)e.ClickedItem).Tag.ToString();
            switch (tag)
            {
                case "1":
                     UserHamburgerMenuFrame.Content = ExamView;
                    break;
                default:
                    break;
            }
        }
    }
}
