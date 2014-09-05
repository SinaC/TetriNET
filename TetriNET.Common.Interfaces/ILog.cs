namespace TetriNET.Common.Interfaces
{
    public enum LogLevels
    {
        Debug,
        Info,
        Warning,
        Error,
    }

    public interface ILog
    {
        void Initialize(string path, string file, string fileTarget = "logfile");
        void WriteLine(LogLevels level, string format, params object[] args);
    }
}
