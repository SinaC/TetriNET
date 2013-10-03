using System;

namespace TetriNET.Common.Logger
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
    //public static class Log
    //{
    //    public enum LogLevels
    //    {
    //        Debug,
    //        Info,
    //        Warning,
    //        Error,
    //    }

    //    private readonly static ILog Log4Net = LogManager.GetLogger(typeof(Log));

    //    public static void Initialize(string path, string file)
    //    {
    //        ThreadContext.Properties["LogFilePath"] = path;
    //        ThreadContext.Properties["LogFileName"] = file;
    //        log4net.Config.XmlConfigurator.Configure();
    //    }

    //    public static void WriteLine(LogLevels level, string format, params object[] args)
    //    {
    //        switch (level)
    //        {
    //            case LogLevels.Debug:
    //                Log4Net.DebugFormat(format, args);
    //                break;
    //            case LogLevels.Info:
    //                Log4Net.InfoFormat(format, args);
    //                break;
    //            case LogLevels.Warning:
    //                Log4Net.WarnFormat(format, args);
    //                break;
    //            case LogLevels.Error:
    //                Log4Net.ErrorFormat(format, args);
    //                break;
    //        }
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

        private static readonly NLog.Logger Logger = NLog.LogManager.GetLogger("TetriNET");

        public static void Initialize(string path, string file, string fileTarget = "logfile")
        {
            string logfile = System.IO.Path.Combine(path, file);
            NLog.Targets.FileTarget target = NLog.LogManager.Configuration.FindTargetByName(fileTarget) as NLog.Targets.FileTarget;
            if (target == null)
                throw new ApplicationException(String.Format("Couldn't find target {0} in NLog config", fileTarget));
            target.FileName = logfile; 
        }

        public static void WriteLine(LogLevels level, string format, params object[] args)
        {
            switch (level)
            {
                case LogLevels.Debug:
                    Logger.Debug(format, args);
                    break;
                case LogLevels.Info:
                    Logger.Info(format, args);
                    break;
                case LogLevels.Warning:
                    Logger.Warn(format, args);
                    break;
                case LogLevels.Error:
                    Logger.Error(format, args);
                    break;
            }
        }
    }
}
