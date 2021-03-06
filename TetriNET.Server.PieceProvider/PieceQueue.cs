﻿using System;
using System.Collections.Generic;
using TetriNET.Common.DataContracts;
using TetriNET.Server.Interfaces;

namespace TetriNET.Server.PieceProvider
{
    public sealed class PieceQueue : IPieceProvider
    {
        private readonly object _lock = new object();
        private readonly Func<IEnumerable<PieceOccurancy>, Pieces> _randomFunc;
        private int _size;
        private Pieces[] _array;

        public PieceQueue(Func<IEnumerable<PieceOccurancy>, Pieces> randomFunc)
        {
            _randomFunc = randomFunc;
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
                _array[i] = _randomFunc(Occurancies());
            }
        }
    }
}
