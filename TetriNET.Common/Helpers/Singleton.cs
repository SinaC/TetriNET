using System;

namespace TetriNET.Common.Helpers
{
    public class Singleton<T>
            where T : class
    {
        private T _instance;
        private readonly Func<T> _createHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="Singleton&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="create">The create.</param>
        public Singleton(Func<T> create)
        {
            if (create == null)
            {
                throw new ArgumentNullException("create");
            }
            _createHandler = create;
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        public T Instance
        {
            get
            {
                _instance = _instance ?? _createHandler();
                return _instance;
            }
        }

    }

    public class ThreadSafeSingleton<T>
        where T : class
    {
        private readonly object _syncObject = new object();
        private volatile T _instance;
        private readonly Func<T> _createHandler;


        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadSafeSingleton&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="create">The create.</param>
        public ThreadSafeSingleton(Func<T> create)
        {
            if (create == null)
            {
                throw new ArgumentNullException("create");
            }
            _createHandler = create;
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
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
}
