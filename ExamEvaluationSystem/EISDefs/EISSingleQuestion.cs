﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExamEvaluationSystem
{
    public class EISSingleQuestion : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public char Answer { get; set; }
        public List<EISEarning> Earnings { get; set; }

        public string FriendlyEarnings { get { return $"[{ Earnings.Count } soru kazanımı]"; } }
        public int Nth { get; set; }

        public EISSingleQuestion(char a, List<EISEarning> e, int nth = 0)
        {
            Nth = nth;
            Answer = a;
            Earnings = e;
        }

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
