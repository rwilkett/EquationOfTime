using System;

namespace SolarPositionCalculator.Services
{
    /// <summary>
    /// Event arguments for time update notifications
    /// </summary>
    public class TimeUpdateEventArgs : EventArgs
    {
        public DateTime CurrentTime { get; }
        public DateTime UtcTime { get; }

        public TimeUpdateEventArgs(DateTime currentTime, DateTime utcTime)
        {
            CurrentTime = currentTime;
            UtcTime = utcTime;
        }
    }

    /// <summary>
    /// Service interface for real-time tracking functionality
    /// </summary>
    public interface IRealTimeService
    {
        /// <summary>
        /// Event fired when time is updated during real-time mode
        /// </summary>
        event EventHandler<TimeUpdateEventArgs> TimeUpdated;

        /// <summary>
        /// Gets whether real-time updates are currently running
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Starts real-time updates with timer-based notifications every minute
        /// </summary>
        void StartRealTimeUpdates();

        /// <summary>
        /// Stops real-time updates and cleans up timer resources
        /// </summary>
        void StopRealTimeUpdates();
    }
}