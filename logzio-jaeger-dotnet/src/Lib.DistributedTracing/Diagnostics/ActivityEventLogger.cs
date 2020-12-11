using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace LogzioJaegerSample.Lib.DistributedTracing.Diagnostics
{
    public class ActivityEventLogger<T> : IActivityEventLogger<T>
    {
        private readonly ILogger _logger;

        public ActivityEventLogger(ILogger<T> logger)
        {
            _logger = logger;
        }

        public void LogError(string message)
        {
            AddEvent(message, LogLevel.Error);
        }

        public void LogWarning(string message)
        {
            AddEvent(message, LogLevel.Warning);
        }

        public void LogInformation(string message)
        {
            AddEvent(message, LogLevel.Information);
        }

        public void LogDebug(string message)
        {
            AddEvent(message, LogLevel.Debug);
        }

        private void AddEvent(string message, LogLevel category)
        {
            _logger.Log(category, message);

            if ((int)category >= 2)
            {
                var activity = Activity.Current;
                activity?.AddEvent(new ActivityEvent(message));
            }
        }
    }
}
