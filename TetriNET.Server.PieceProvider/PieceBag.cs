using System;
using System.Collections.Generic;
using TetriNET.Common.DataContracts;
using TetriNET.Server.Interfaces;

namespace TetriNET.Server.PieceProvider
{
    public sealed class PieceBag : IPieceProvider
    {
        private readonly object _lock = new object();
        private readonly Func<IEnumerable<PieceOccurancy>, IEnumerable<Pieces>, Pieces> _randomFunc;
        private readonly int _historySize;
        private int _size;
        private Pieces[] _array;
        private readonly Pieces[] _history;

        public PieceBag(Func<IEnumerable<PieceOccurancy>, IEnumerable<Pieces>, Pieces> randomFunc, int historySize)
        {
            _randomFunc = randomFunc;
            _historySize = historySize;
            _history = new Pieces[_historySize];
            //Grow(64);
        }

        public void Reset()
        {
            lock (_lock)
            {
                Fill(0, _size);
            }
        }

        public Func<IEnumerable<PieceOccurancy>> Occurancies { get; set; }

        public Pieces this[int index]
        {
            get
            {
                Pieces piece;
                lock (_lock)
                {
                    if (index >= _size)
                        Grow(128);
                    piece = _array[index];
                }
                return piece;
            }
        }

        private void Grow(int increment)
        {
            int newSize = _size + increment;
            Pieces[] newArray = new Pieces[newSize];
            if (_size > 0)
                Array.Copy(_array, newArray, _size);
            _array = newArray;
            Fill(_size, increment);
            _size = newSize;
        }

        private void Fill(int from, int count)
        {
            for (int i = from; i < from + count; i++)
            {
                int endIndex = i < _historySize ? i : _historySize;
                for (int j = 0; j < endIndex; j++)
                    _history[j] = _array[i - endIndex + j];
                for (int j = endIndex; j < _historySize; j++ )
                    _history[j] = Pieces.Invalid;
                _array[i] = _randomFunc(Occurancies(), _history);
            }
        }
    }
}
