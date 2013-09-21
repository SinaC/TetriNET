using System.Collections.Generic;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client
{
    internal sealed class Inventory
    {
        private readonly object _lock = new object();
        private readonly List<Specials> _queue;
        private int _size;

        public Inventory(int size)
        {
            _lock = new object();
            _size = size;
            _queue = new List<Specials>();
        }

        public void Reset(int size)
        {
            lock (_lock)
            {
                _size = size;
                _queue.Clear();
            }
        }

        public bool Enqueue(Specials special)
        {
            bool result = false;
            lock (_lock)
            {
                if (_queue.Count < _size)
                {
                    _queue.Add(special);
                    result = true;
                }
            }
            return result;
        }

        public void Enqueue(List<Specials> specials)
        {
            lock (_lock)
            {
                foreach (Specials special in specials)
                {
                    bool enqueued = Enqueue(special);
                    if (!enqueued)
                        break;
                }
            }
        }

        public bool Dequeue(out Specials special)
        {
            special = 0;
            bool result = false;
            lock (_lock)
            {
                if (_queue.Count > 0)
                {
                    special = _queue[0];
                    _queue.RemoveAt(0);
                    result = true;
                }
            }
            return result;
        }

        public List<Specials> Specials()
        {
            List<Specials> specials;
            lock (_lock)
            {
                specials = new List<Specials>(_queue);
            }
            return specials;
        }
    }
}
