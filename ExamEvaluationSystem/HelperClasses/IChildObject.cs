using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ExamEvaluationSystem
{
    public interface IChildObject<T>
    {
        public T ParentObject { get; set; }
    }
}
