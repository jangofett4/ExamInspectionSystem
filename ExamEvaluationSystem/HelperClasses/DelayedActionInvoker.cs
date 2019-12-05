using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ExamEvaluationSystem
{
    /// <summary>
    /// Invokes an action after specified amount of milliseconds is passed.
    /// Action will be executed once, but if <see cref="DelayedActionInvoker.Reset"/> is called timer resets and action can be executed again.
    /// Also if timer is reset before action starts, previous action will not start. This particulary useful for delayed inputs.
    /// </summary>
    public class DelayedActionInvoker : IDisposable
    {
        /// <summary>
        /// Action to invoke
        /// </summary>
        public Action Action;

        /// <summary>
        /// Delay, in milliseconds
        /// </summary>
        public int Delay;

        private Timer Timer;

        /// <summary>
        /// Initializes a new delayed action with pre-defined action and delay
        /// </summary>
        /// <param name="action"></param>
        /// <param name="delay"></param>
        public DelayedActionInvoker(Action action, int delay)
        {
            Action = action;
            Delay = delay;
            Timer = new Timer((o) => { });
        }

        /// <summary>
        /// Initializes a new delayed action with delay
        /// </summary>
        /// <param name="delay"></param>
        public DelayedActionInvoker(int delay)
        {
            Delay = delay;
        }

        /// <summary>
        /// Starts the delayed action
        /// </summary>
        public void Start()
        {
            Timer = new Timer((obj) =>
            {
                Action?.Invoke();
                Timer.Dispose();
            }, null, Delay, Timeout.Infinite);
        }
        
        /// <summary>
        /// Resets the delayed action
        /// </summary>
        public void Reset()
        {
            Timer.Dispose();
            Start();
        }

        /// <summary>
        /// Disposes this object
        /// </summary>
        public void Dispose()
        {
            Timer.Dispose();
        }
    }
}
