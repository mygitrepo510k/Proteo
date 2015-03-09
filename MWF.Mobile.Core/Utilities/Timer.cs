using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MWF.Mobile.Core.Utilities
{
    public delegate void TimerCallback(object state);

    public sealed class Timer : CancellationTokenSource, IDisposable
    {
        private TimerCallback _callback;
        private object _state;
        private int _dueTime;

        public Timer(TimerCallback callback, object state, int dueTime)
        {
            _callback = callback;
            _state = state;
            _dueTime = dueTime;

            this.Reset();

        }

        public void Reset()
        {
            Task.Delay(_dueTime, Token).ContinueWith((t, s) =>
            {
                var tuple = (Tuple<TimerCallback, object>)s;
                tuple.Item1(tuple.Item2);
            }, Tuple.Create(_callback, _state), CancellationToken.None,
              TaskContinuationOptions.OnlyOnRanToCompletion,
              TaskScheduler.Default);
        }


        public new void Dispose() { base.Cancel(); }
    }
}
