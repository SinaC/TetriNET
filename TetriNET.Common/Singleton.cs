using System;

namespace TetriNET.Common
{
    public class ThreadSafeSingleton<T>
            where T : class
    {
        private T _value;
        private readonly object _syncObject = new object();
        private readonly Func<T> _createHandler;

        public ThreadSafeSingleton(Func<T> create)
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
                if (_value == null)
                {
                    lock (_syncObject)
                    {
                        _value = _value ?? _createHandler();
                    }
                }
                return _value;
            }
        }

    }

    public class Singleton<T>
        where T : class
    {
        private T _value;
        private readonly Func<T> _createHandler;

        public Singleton(Func<T> create)
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
                _value = _value ?? _createHandler();
                return _value;
            }
        }

    }
}
