namespace LogzioJaegerSample.Lib.DistributedTracing.Diagnostics
{
    public interface IActivityEventLogger<T>
    {
        void LogError(string message);

        void LogWarning(string message);

        void LogInformation(string message);

        void LogDebug(string message);
    }
}
