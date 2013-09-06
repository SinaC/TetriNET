using System;
using log4net;

namespace TetriNET.Logger
{
    //public static class Log
    //{
    //    public static bool DisplayThreadId { get; set; }
    //    public static bool DisplayInConsole { get; set; }

    //    public static void WriteLine(string line)
    //    {
    //        string logLine;
    //        if (DisplayThreadId)
    //            logLine = String.Format("[{0:HH:mm:ss.fff}][{1:X8}]{2}", DateTime.Now, System.Threading.Thread.CurrentThread.ManagedThreadId, line);
    //        else
    //            logLine = String.Format("[{0:HH:mm:ss.fff}]{1}", DateTime.Now, line);
    //        if (DisplayInConsole)
    //            Console.WriteLine(logLine);
    //        System.Diagnostics.Debug.WriteLine(logLine);
    //    }

    //    public static void WriteLine(string format, params object[] args)
    //    {
    //        string line = String.Format(format, args);
    //        WriteLine(line);
    //    }
    //}
    public static class Log
    {
        public enum LogLevels
        {
            Debug,
            Info,
            Warning,
            Error,
        }

        private readonly static ILog Log4Net = LogManager.GetLogger(typeof(Log));

        public static void Initialize(string path, string file)
        {
            ThreadContext.Properties["LogFilePath"] = path;
            ThreadContext.Properties["LogFileName"] = file;
            log4net.Config.XmlConfigurator.Configure();
        }

        public static void WriteLine(LogLevels level, string format, params object[] args)
        {
            string line = String.Format(format, args);
            switch (level)
            {
                case LogLevels.Debug:
                    Log4Net.Debug(line);
                    break;
                case LogLevels.Info:
                    Log4Net.Info(line);
                    break;
                case LogLevels.Warning:
                    Log4Net.Warn(line);
                    break;
                case LogLevels.Error:
                    Log4Net.Error(line);
                    break;
            }
        }
    }
}
