using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ExamEvaluationSystem
{
    /// <summary>
    /// App.xaml etkileşim mantığı
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Exit += (sender, e) =>
            {
                if (EISSystem.Connection != null)
                    EISSystem.Connection.Close();
            };
        }
    }
}
