using System;
using System.Windows.Threading;

namespace SolarPositionCalculator.Services
{
    /// <summary>
    /// Implementation of real-time tracking service using WPF DispatcherTimer
    /// </summary>
    public class RealTimeService : IRealTimeService
    {
        private readonly DispatcherTimer _timer;
        private bool _isRunning;

        public event EventHandler<TimeUpdateEventArgs>? TimeUpdated;

        public bool IsRunning => _isRunning;

        public RealTimeService()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1) // Update every minute
            };
            _timer.Tick += OnTimerTick;
        }

        public void StartRealTimeUpdates()
        {
            if (_isRunning)
                return;

            _isRunning = true;

            // Fire initial update immediately
            FireTimeUpdate();

            // Start the timer for subsequent updates
            _timer.Start();
        }

        public void StopRealTimeUpdates()
        {
            if (!_isRunning)
                return;

            _isRunning = false;
            _timer.Stop();
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            FireTimeUpdate();
        }

        private void FireTimeUpdate()
        {
            var now = DateTime.Now;
            var utcNow = DateTime.UtcNow;

            TimeUpdated?.Invoke(this, new TimeUpdateEventArgs(now, utcNow));
        }

        public void Dispose()
        {
            StopRealTimeUpdates();
        }
    }
}