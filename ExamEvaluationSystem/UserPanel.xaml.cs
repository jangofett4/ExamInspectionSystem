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
    public partial class UserPanel : Window
    {
        public EISLecturer Lecturer { get; set; }

        public UserPanel(EISLecturer lec)
        {
            InitializeComponent();
            Lecturer = lec;
            Resources.Add("CurrentLecturerID", lec.ID.ToString());
            Resources.Add("CurrentLecturerName", lec.Name + " " + lec.Surname);
            UserHamburgerMenuFrame.Navigate(new Uri("ViewLecturerAddExam.xaml",UriKind.RelativeOrAbsolute));
        }
    }
}