using System;

namespace TetriNET.Common.Helpers
{
    public sealed class SingleInstance<T>
            where T : class
    {
        private T _instance;
        private readonly Func<T> _createHandler;

        public SingleInstance(Func<T> create)
        {
            if (create == null)
            {
                throw new ArgumentNullException("create");
            }
            _createHandler = create;
        }

        public T Instance
        {
            get
            {
                _instance = _instance ?? _createHandler();
                return _instance;
            }
        }
    }

    public sealed class ThreadSafeSingleInstance<T>
        where T : class
    {
        private readonly object _syncObject = new object();
        private volatile T _instance;
        private readonly Func<T> _createHandler;

        public ThreadSafeSingleInstance(Func<T> create)
        {
            if (create == null)
            {
                throw new ArgumentNullException("create");
            }
            _createHandler = create;
        }

        public T Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncObject)
                    {
                        if (_instance == null)
                        {
                            _instance = _createHandler();
                        }
                    }
                }
                return _instance;
            }
        }
    }

    public sealed class ThreadSafeSingleton2<T>
        where T : class
    {
        public T Item { get; set; }
        private static readonly ThreadSafeSingleton2<T> _instance = new ThreadSafeSingleton2<T>();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static ThreadSafeSingleton2()
        {
        }

        private ThreadSafeSingleton2()
        {
        }

        public static ThreadSafeSingleton2<T> Instance
        {
            get
            {
                return _instance;
            }
        }
    }

    //http://csharpindepth.com/articles/general/singleton.aspx
    public sealed class BadSingleton
    {
        private static BadSingleton _instance;

        private BadSingleton()
        {
        }

        public static BadSingleton Instance
        {
            get { return _instance ?? (_instance = new BadSingleton()); }
        }
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
        private static readonly ThreadSafeNoLockSingleton _instance = new ThreadSafeNoLockSingleton();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static ThreadSafeNoLockSingleton()
        {
        }

        private ThreadSafeNoLockSingleton()
        {
        }

        public static ThreadSafeNoLockSingleton Instance
        {
            get
            {
                return _instance;
            }
        }
    }

    public sealed class FullLazySingleton
    {
        private FullLazySingleton()
        {
        }

        public static FullLazySingleton Instance { get { return Nested._instance; } }

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

        public static LazySingleton Instance { get { return Lazy.Value; } }

        private LazySingleton()
        {
        }
    }
}
