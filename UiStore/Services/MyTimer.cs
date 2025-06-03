
using System;
using System.Threading;

namespace UiStore.Services
{
    internal class MyTimer : IDisposable
    {
        private readonly Timer _timer;
        private bool _isRunning;
        public MyTimer(TimerCallback timerCallback) {
            _timer = new Timer(timerCallback, null, Timeout.Infinite, Timeout.Infinite);
            _isRunning = false;
        }

        public void Start(int dueTime, int period)
        {
            if (!_isRunning)
            {
                _timer.Change(dueTime, period);
                _isRunning = true;
            }
        }

        public void Stop()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            _isRunning = false;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
