using System;

namespace TetriNET.Common.Helpers
{
    public sealed class ThreadSafeSingleton2<T>
        where T : class
    {
        public T Item { get; set; }

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static ThreadSafeSingleton2()
        {
        }

        private ThreadSafeSingleton2()
        {
        }

        public static ThreadSafeSingleton2<T> Instance { get; } = new ThreadSafeSingleton2<T>();
    }

    //http://csharpindepth.com/articles/general/singleton.aspx
    public sealed class BadSingleton
    {
        private static BadSingleton _instance;

        private BadSingleton()
        {
        }

        public static BadSingleton Instance => _instance ?? (_instance = new BadSingleton());
    }

    public sealed class SimpleThreadSafeSingleton
    {
        private static SimpleThreadSafeSingleton _instance;
        private static readonly object Padlock = new object();

        SimpleThreadSafeSingleton()
        {
        }

        public static SimpleThreadSafeSingleton Instance
        {
            get
            {
                lock (Padlock)
                {
                    _instance = _instance ?? new SimpleThreadSafeSingleton();
                    return _instance;
                }
            }
        }
    }

    public sealed class BadThreadSafeSingleton
    {
        private static BadThreadSafeSingleton _instance;
        private static readonly object Padlock = new object();

        BadThreadSafeSingleton()
        {
        }

        public static BadThreadSafeSingleton Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (Padlock)
                    {
                        if (_instance == null)
                        {
                            _instance = new BadThreadSafeSingleton();
                        }
                    }
                }
                return _instance;
            }
        }
    }

    public sealed class ThreadSafeNoLockSingleton
    {
        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static ThreadSafeNoLockSingleton()
        {
        }

        private ThreadSafeNoLockSingleton()
        {
        }

        public static ThreadSafeNoLockSingleton Instance { get; } = new ThreadSafeNoLockSingleton();
    }

    public sealed class FullLazySingleton
    {
        private FullLazySingleton()
        {
        }

        public static FullLazySingleton Instance => Nested._instance;

        private class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }

            internal static readonly FullLazySingleton _instance = new FullLazySingleton();
        }
    }

    public sealed class LazySingleton
    {
        private static readonly Lazy<LazySingleton> Lazy = new Lazy<LazySingleton>(() => new LazySingleton());

        public static LazySingleton Instance => Lazy.Value;

        private LazySingleton()
        {
        }
    }
}
