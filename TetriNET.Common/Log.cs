using System;

namespace TetriNET.Common
{
    public static class Log
    {
        public static void WriteLine(string line)
        {
            string threadLine = String.Format("[{0:X}]{1}", System.Threading.Thread.CurrentThread.ManagedThreadId, line);
            Console.WriteLine(threadLine);
            System.Diagnostics.Debug.WriteLine(threadLine);
        }

        public static void WriteLine(string format, params object[] args)
        {
            string line = String.Format(format, args);
            WriteLine(line);
        }
    }
}
