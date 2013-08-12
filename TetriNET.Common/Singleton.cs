using System;

namespace TetriNET.Common
{
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
