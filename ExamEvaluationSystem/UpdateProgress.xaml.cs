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
using System.Windows.Shapes;

namespace ExamEvaluationSystem
{
    /// <summary>
    /// UpdateProgress.xaml etkileşim mantığı
    /// </summary>
    public partial class UpdateProgress : INotifyPropertyChanged
    {
        private string _progperc = "";
        public string ProgressPercent { 
            get 
            {
                return _progperc;
            } 
            set 
            {
                _progperc = value;
                OnPropertyChanged("ProgressPercent");
            }
        }

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public UpdateProgress(string title, string desc)
        {
            InitializeComponent();

            Title = title;
            lblDesc.Content = desc;
            ProgressPercent = "%0";

            DataContext = this;
        }

        public void ExecuteProgress(Action<IProgress<int>> fn)
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Task.Run(() =>
            {
                fn(new Progress<int>((i) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        progress.Value = i;
                        ProgressPercent = $"%{ i }";
                        if (i == 100)
                            Close();
                    });
                }));
            });
            ShowDialog();
        }
    }
}
