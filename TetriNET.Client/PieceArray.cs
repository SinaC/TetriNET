using System;
using System.Globalization;
using System.Linq;
using TetriNET.Common.DataContracts;

namespace TetriNET.Client
{
    internal sealed class PieceArray
    {
        private readonly object _lock = new object();
        private Pieces[] _array;

        public int HighestIndex { get; private set; }
        public int Size { get; private set; }

        public PieceArray(int size)
        {
            Size = size;
            _array = new Pieces[Size];
        }

        public void Reset()
        {
            HighestIndex = 0;
            for(int i = 0; i < Size; i++)
                _array[i] = Pieces.Invalid;
        }

        public Pieces this[int index]
        {
            get
            {
                Pieces piece;
                lock (_lock)
                {
                    piece = _array[index];
                }
                return piece;
            }
            set
            {
                lock (_lock)
                {
                    if (index > HighestIndex)
                        HighestIndex = index;
                    if (index >= Size)
                        Grow(64);
                    _array[index] = value;
                }
            }
        }

        private void Grow(int increment)
        {
            int newSize = Size + increment;
            Pieces[] newArray = new Pieces[newSize];
            if (Size > 0)
                Array.Copy(_array, newArray, Size);
            _array = newArray;
            Size = newSize;
        }

        public string Dump(int size)
        {
            return _array.Take(size).Select((t, i) => "[" + i.ToString(CultureInfo.InvariantCulture) + ":" + t.ToString() + "]").Aggregate((s, t) => s + "," + t);
        }
    }
}
