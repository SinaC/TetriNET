using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPFiltering
{
    public class ThreadSafeSingleton<T>
        where T : class
    {
        private object _syncObject = new object();
        private T _value;
        private Func<T> _createHandler;


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
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        public T Value
        {
            get
            {
                if (_value == null)
                {
                    lock (_syncObject)
                    {
                        if (_value == null)
                        {
                            _value = _createHandler();
                        }
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
        private Func<T> _createHandler;

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
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        public T Value
        {
            get
            {
                if (_value == null)
                {
                    _value = _createHandler();
                }
                return _value;
            }
        }

    }
}
