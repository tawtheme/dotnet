namespace BlueCollarEngine.API.Repositories.LoggerRepository
{
    public interface ILoggerRepository
    {
        void LogInfo(string message);
        void LogWarn(string message);
        void LogDebug(string message);
        void LogError(string message);
    }
}
