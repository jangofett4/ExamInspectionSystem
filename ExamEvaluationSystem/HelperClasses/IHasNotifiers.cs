using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExamEvaluationSystem
{
    public interface IHasNotifiers
    {
        public void NotifySuccess(string message);

        public void NotifyInformation(string message);

        public void NotifyWarning(string message);

        public void NotifyError(string message);
    }
}
