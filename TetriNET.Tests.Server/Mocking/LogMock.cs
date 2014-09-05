using System;
using TetriNET.Common.Interfaces;

namespace TetriNET.Tests.Server.Mocking
{
    public class LogMock : ILog
    {
        public LogLevels LastLogLevel { get; private set; }
        public string LastLogLine { get; private set; }

        #region ILog

        public void Initialize(string path, string file, string fileTarget = "logfile")
        {
            // NOP
        }
        
        public void WriteLine(LogLevels level, string format, params object[] args)
        {
            LastLogLevel = level;
            LastLogLine = String.Format(format, args);
        }

        #endregion

        public void Clear()
        {
            LastLogLevel = LogLevels.Debug;
            LastLogLine = null;
        }
    }
}
