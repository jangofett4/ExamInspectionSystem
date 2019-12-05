using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ExamEvaluationSystem
{
    public static class MemoryRenderer
    {
        public static void Render(FrameworkElement e)
        {
            var size = new Size(5, 5);
            var rect = new Rect(size);
            e.Measure(size);
            e.Arrange(rect);
            e.UpdateLayout();
        }

        public static void Render(params FrameworkElement[] es)
        {
            foreach (var e in es)
            {
                var size = new Size(5, 5);
                var rect = new Rect(size);
                e.Measure(size);
                e.Arrange(rect);
                e.UpdateLayout();
            }
        }
    }
}
