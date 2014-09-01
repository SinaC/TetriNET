namespace TetriNET.Common.Logger
{
    public static class Log
    {
        public static ILog Default { get; private set; }

        static Log()
        {
            Default = new NLogger();
        }

        public static void SetLogger(ILog log)
        {
            Default = log;
        }
    }
}
