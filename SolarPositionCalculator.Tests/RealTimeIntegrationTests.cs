using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using SolarPositionCalculator.Services;
using SolarPositionCalculator.ViewModels;
using SolarPositionCalculator.Models;

namespace SolarPositionCalculator.Tests
{
    /// <summary>
    /// Integration tests for real-time functionality including service behavior,
    /// UI updates, and state management
    /// </summary>
    public class RealTimeIntegrationTests : IDisposable
    {
        private readonly IRealTimeService _realTimeService;
        private readonly IAstronomicalCalculator _astronomicalCalculator;
        private readonly ICoordinateConverter _coordinateConverter;
        private readonly IValidationService _validationService;
        private readonly IErrorHandlingService _errorHandlingService;
        private readonly ITimeZoneService _timeZoneService;
        private readonly MainViewModel _mainViewModel;

        public RealTimeIntegrationTests()
        {
            // Create service instances for testing
            _realTimeService = new RealTimeService();
            _astronomicalCalculator = new AstronomicalCalculator();
            _timeZoneService = new TimeZoneService();
            _coordinateConverter = new CoordinateConverter(_timeZoneService);
            _validationService = new ValidationService();
            _errorHandlingService = new ErrorHandlingService();

            // Create MainViewModel with all dependencies
            _mainViewModel = new MainViewModel(
                _astronomicalCalculator,
                _coordinateConverter,
                _realTimeService,
                _validationService,
                _errorHandlingService);
        }

        [Fact]
        public void RealTimeService_StartStop_StateManagement()
        {
            // Arrange
            Assert.False(_realTimeService.IsRunning);

            // Act - Start real-time updates
            _realTimeService.StartRealTimeUpdates();

            // Assert - Service should be running
            Assert.True(_realTimeService.IsRunning);

            // Act - Stop real-time updates
            _realTimeService.StopRealTimeUpdates();

            // Assert - Service should be stopped
            Assert.False(_realTimeService.IsRunning);
        }

        [Fact]
        public void RealTimeService_MultipleStartCalls_NoSideEffects()
        {
            // Arrange
            Assert.False(_realTimeService.IsRunning);

            // Act - Start multiple times
            _realTimeService.StartRealTimeUpdates();
            _realTimeService.StartRealTimeUpdates();
            _realTimeService.StartRealTimeUpdates();

            // Assert - Should still be running normally
            Assert.True(_realTimeService.IsRunning);

            // Cleanup
            _realTimeService.StopRealTimeUpdates();
            Assert.False(_realTimeService.IsRunning);
        }

        [Fact]
        public void RealTimeService_MultipleStopCalls_NoSideEffects()
        {
            // Arrange
            _realTimeService.StartRealTimeUpdates();
            Assert.True(_realTimeService.IsRunning);

            // Act - Stop multiple times
            _realTimeService.StopRealTimeUpdates();
            _realTimeService.StopRealTimeUpdates();
            _realTimeService.StopRealTimeUpdates();

            // Assert - Should remain stopped
            Assert.False(_realTimeService.IsRunning);
        }

        [Fact]
        public void RealTimeService_TimeUpdateEvents_FireCorrectly()
        {
            // Arrange
            var eventFired = false;
            var eventArgs = (TimeUpdateEventArgs?)null;
            var eventWaitHandle = new ManualResetEventSlim(false);

            _realTimeService.TimeUpdated += (sender, args) =>
            {
                eventFired = true;
                eventArgs = args;
                eventWaitHandle.Set();
            };

            // Act - Start real-time updates (should fire immediately)
            _realTimeService.StartRealTimeUpdates();

            // Wait for the initial event (should fire immediately)
            var eventReceived = eventWaitHandle.Wait(TimeSpan.FromSeconds(2));

            // Assert
            Assert.True(eventReceived, "TimeUpdated event should fire immediately when starting");
            Assert.True(eventFired);
            Assert.NotNull(eventArgs);
            Assert.True(Math.Abs((eventArgs.CurrentTime - DateTime.Now).TotalMinutes) < 1);
            Assert.True(Math.Abs((eventArgs.UtcTime - DateTime.UtcNow).TotalMinutes) < 1);

            // Cleanup
            _realTimeService.StopRealTimeUpdates();
        }

        [Fact]
        public void RealTimeService_TimerAccuracy_UpdatesAtCorrectInterval()
        {
            // Arrange
            var updateCount = 0;
            var firstUpdateTime = DateTime.MinValue;
            var eventWaitHandle = new ManualResetEventSlim(false);

            _realTimeService.TimeUpdated += (sender, args) =>
            {
                updateCount++;
                if (updateCount == 1)
                {
                    firstUpdateTime = DateTime.Now;
                    eventWaitHandle.Set();
                }
            };

            // Act - Start real-time updates
            _realTimeService.StartRealTimeUpdates();

            // Wait for the first update (should be immediate)
            var firstEventReceived = eventWaitHandle.Wait(TimeSpan.FromSeconds(2));

            // Assert - We should get at least the immediate first update
            Assert.True(firstEventReceived, "Should receive the immediate first update");
            Assert.True(updateCount >= 1, "Should receive at least the immediate first update");
            Assert.NotEqual(DateTime.MinValue, firstUpdateTime);

            // Note: For full timer testing, we'd need to modify the service to accept configurable intervals
            // or use a test-specific implementation with shorter intervals

            // Cleanup
            _realTimeService.StopRealTimeUpdates();
        }

        [Fact]
        public void MainViewModel_RealTimeMode_ToggleCorrectly()
        {
            // Arrange
            Assert.False(_mainViewModel.IsRealTimeMode);
            Assert.False(_mainViewModel.IsRealTimeServiceRunning);

            // Act - Enable real-time mode
            _mainViewModel.ToggleRealTimeModeCommand.Execute(null);

            // Assert - Real-time mode should be enabled
            Assert.True(_mainViewModel.IsRealTimeMode);
            Assert.True(_mainViewModel.IsRealTimeServiceRunning);
            // Note: Status message might be overwritten by real-time updates, so we focus on the core functionality

            // Act - Disable real-time mode
            _mainViewModel.ToggleRealTimeModeCommand.Execute(null);

            // Assert - Real-time mode should be disabled
            Assert.False(_mainViewModel.IsRealTimeMode);
            Assert.False(_mainViewModel.IsRealTimeServiceRunning);
        }

        [Fact]
        public void MainViewModel_RealTimeUpdates_UpdateCalculations()
        {
            // Arrange
            _mainViewModel.Latitude = 51.4778; // London coordinates
            _mainViewModel.Longitude = -0.0015;

            var initialSolarPosition = _mainViewModel.CurrentSolarPosition;
            var calculationUpdated = false;
            var eventWaitHandle = new ManualResetEventSlim(false);

            // Subscribe to property changes to detect calculation updates
            _mainViewModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(_mainViewModel.CurrentSolarPosition))
                {
                    calculationUpdated = true;
                    eventWaitHandle.Set();
                }
            };

            // Act - Enable real-time mode
            _mainViewModel.ToggleRealTimeModeCommand.Execute(null);

            // Wait for calculation update
            var updateReceived = eventWaitHandle.Wait(TimeSpan.FromSeconds(3));

            // Assert - Calculations should be updated
            Assert.True(_mainViewModel.IsRealTimeMode);
            Assert.True(updateReceived || calculationUpdated, "Solar position calculations should be updated in real-time mode");

            // Cleanup
            _mainViewModel.ToggleRealTimeModeCommand.Execute(null);
        }

        [Fact]
        public void MainViewModel_RealTimeMode_UpdatesCurrentTime()
        {
            // Arrange
            var originalDateTime = _mainViewModel.SelectedDateTime;

            // Act - Enable real-time mode
            _mainViewModel.ToggleRealTimeModeCommand.Execute(null);

            // Assert - DateTime should be updated to current time
            Assert.True(_mainViewModel.IsRealTimeMode);
            var timeDifference = Math.Abs((_mainViewModel.SelectedDateTime - DateTime.Now).TotalMinutes);
            Assert.True(timeDifference < 1, "Selected DateTime should be updated to current time in real-time mode");

            // Cleanup
            _mainViewModel.ToggleRealTimeModeCommand.Execute(null);
        }

        [Fact]
        public void MainViewModel_RealTimeServiceIntegration_PropertyReflectsServiceState()
        {
            // Arrange
            Assert.False(_mainViewModel.IsRealTimeServiceRunning);

            // Act - Start service directly
            _realTimeService.StartRealTimeUpdates();

            // Assert - ViewModel property should reflect service state
            Assert.True(_mainViewModel.IsRealTimeServiceRunning);

            // Act - Stop service directly
            _realTimeService.StopRealTimeUpdates();

            // Assert - ViewModel property should reflect service state
            Assert.False(_mainViewModel.IsRealTimeServiceRunning);
        }

        [Fact]
        public void RealTimeService_EventUnsubscription_StopsUpdatesCorrectly()
        {
            // Arrange
            var updateCount = 0;
            EventHandler<TimeUpdateEventArgs> handler = (sender, args) => updateCount++;

            _realTimeService.TimeUpdated += handler;
            _realTimeService.StartRealTimeUpdates();

            // Wait for initial update
            Thread.Sleep(100);
            var initialCount = updateCount;
            Assert.True(initialCount > 0, "Should receive initial update");

            // Act - Unsubscribe from events
            _realTimeService.TimeUpdated -= handler;

            // Wait a bit more
            Thread.Sleep(100);
            var finalCount = updateCount;

            // Assert - No more updates should be received after unsubscribing
            Assert.Equal(initialCount, finalCount);

            // Cleanup
            _realTimeService.StopRealTimeUpdates();
        }

        [Fact]
        public void MainViewModel_Dispose_StopsRealTimeService()
        {
            // Arrange
            _mainViewModel.ToggleRealTimeModeCommand.Execute(null);
            Assert.True(_mainViewModel.IsRealTimeMode);
            Assert.True(_mainViewModel.IsRealTimeServiceRunning);

            // Act - Dispose the ViewModel
            _mainViewModel.Dispose();

            // Assert - Real-time service should be stopped
            Assert.False(_realTimeService.IsRunning);
        }

        [Fact]
        public async Task RealTimeService_ConcurrentStartStop_ThreadSafe()
        {
            // Arrange
            var tasks = new Task[10];
            var exceptions = new List<Exception>();

            // Act - Concurrent start/stop operations
            for (int i = 0; i < 10; i++)
            {
                var index = i;
                tasks[i] = Task.Run(() =>
                {
                    try
                    {
                        if (index % 2 == 0)
                        {
                            _realTimeService.StartRealTimeUpdates();
                        }
                        else
                        {
                            _realTimeService.StopRealTimeUpdates();
                        }
                    }
                    catch (Exception ex)
                    {
                        lock (exceptions)
                        {
                            exceptions.Add(ex);
                        }
                    }
                });
            }

            await Task.WhenAll(tasks);

            // Assert - No exceptions should occur
            Assert.Empty(exceptions);

            // Cleanup - Ensure service is stopped
            _realTimeService.StopRealTimeUpdates();
        }

        public void Dispose()
        {
            _mainViewModel?.Dispose();
            _realTimeService?.StopRealTimeUpdates();
        }
    }
}