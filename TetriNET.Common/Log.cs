using System;

namespace TetriNET.Common
{
    public static class Log
    {
        public static bool DisplayThreadId { get; set; }

        public static void WriteLine(string line)
        {
            string logLine;
            if (DisplayThreadId)
                logLine = String.Format("[{0:HH:mm:ss.fff}][{1:X8}]{2}", DateTime.Now, System.Threading.Thread.CurrentThread.ManagedThreadId, line);
            else
                logLine = String.Format("[{0:HH:mm:ss.fff}]{1}", DateTime.Now, line);
            Console.WriteLine(logLine);
            System.Diagnostics.Debug.WriteLine(logLine);
        }

        public static void WriteLine(string format, params object[] args)
        {
            string line = String.Format(format, args);
            WriteLine(line);
        }
    }
}
