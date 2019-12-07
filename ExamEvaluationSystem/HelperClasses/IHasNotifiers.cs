using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using ToastNotifications;
using ToastNotifications.Core;
using ToastNotifications.Position;

namespace ExamEvaluationSystem
{
    public interface IHasNotifiers
    {
        public Notifier Notifier { get; set; }

        public void NotifySuccess(string message);

        public void NotifyInformation(string message);

        public void NotifyWarning(string message);

        public void NotifyError(string message);

        public void NotifierClear();
    }
}
