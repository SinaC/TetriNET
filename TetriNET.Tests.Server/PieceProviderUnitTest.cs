using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Interfaces;
using TetriNET.Common.Logger;
using TetriNET.Common.Randomizer;
using TetriNET.Server.Interfaces;
using TetriNET.Server.PieceProvider;
using TetriNET.Tests.Server.Mocking;

namespace TetriNET.Tests.Server
{
    [TestClass]
    public abstract class PieceProviderUnitTest
    {
        protected abstract IPieceProvider CreatePieceProvider();
        
        protected virtual void Reset(IPieceProvider provider)
        {
            provider.Reset();
        }

        [TestInitialize]
        public void Initialize()
        {
            Log.SetLogger(new LogMock());
        }

        [TestMethod]
        public void TestExceptionIfOccuranciesIsNull()
        {
            IPieceProvider pieceProvider = CreatePieceProvider();

            try
            {
                Pieces piece = pieceProvider[0];

                Assert.Fail("No Exception raised");
            }
            catch(Exception ex)
            {
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void TestGetFirstPieceIsValid()
        {
            IPieceProvider pieceProvider = CreatePieceProvider();
            pieceProvider.Occurancies = () => new[] {new PieceOccurancy
                {
                    Occurancy = 100,
                    Value = Pieces.TetriminoI
                }};

            Pieces piece = pieceProvider[0];

            Assert.AreEqual(piece, Pieces.TetriminoI);
        }

        [TestMethod]
        public void TestGetMultiplePiecesAreValid()
        {
            IPieceProvider pieceProvider = CreatePieceProvider();
            pieceProvider.Occurancies = () => new[] {
                new PieceOccurancy
                {
                    Occurancy = 25,
                    Value = Pieces.TetriminoI
                },
            new PieceOccurancy
                {
                    Occurancy = 25,
                    Value = Pieces.TetriminoJ
                },
            new PieceOccurancy
                {
                    Occurancy = 25,
                    Value = Pieces.TetriminoL
                },
            new PieceOccurancy
                {
                    Occurancy = 25,
                    Value = Pieces.TetriminoO
                }};

            Pieces piece1 = pieceProvider[0];
            Pieces piece2 = pieceProvider[1];
            Pieces piece3 = pieceProvider[2];
            Pieces piece4 = pieceProvider[3];

            Assert.AreEqual(piece1, Pieces.TetriminoI);
            Assert.AreEqual(piece2, Pieces.TetriminoJ);
            Assert.AreEqual(piece3, Pieces.TetriminoL);
            Assert.AreEqual(piece4, Pieces.TetriminoO);
        }

        [TestMethod]
        public void TestReset()
        {
            IPieceProvider pieceProvider = CreatePieceProvider();
            pieceProvider.Occurancies = () => new[] {
                new PieceOccurancy
                {
                    Occurancy = 25,
                    Value = Pieces.TetriminoI
                },
            new PieceOccurancy
                {
                    Occurancy = 25,
                    Value = Pieces.TetriminoJ
                },
            new PieceOccurancy
                {
                    Occurancy = 25,
                    Value = Pieces.TetriminoL
                },
            new PieceOccurancy
                {
                    Occurancy = 25,
                    Value = Pieces.TetriminoO
                }};
            Pieces p1 = pieceProvider[0];
            Pieces p2 = pieceProvider[1];
            Pieces p3 = pieceProvider[2];
            Pieces p4 = pieceProvider[3];

            Reset(pieceProvider);
            Pieces piece1 = pieceProvider[0];
            Pieces piece2 = pieceProvider[1];
            Pieces piece3 = pieceProvider[2];
            Pieces piece4 = pieceProvider[3];

            Assert.AreEqual(piece1, Pieces.TetriminoI);
            Assert.AreEqual(piece2, Pieces.TetriminoJ);
            Assert.AreEqual(piece3, Pieces.TetriminoL);
            Assert.AreEqual(piece4, Pieces.TetriminoO);
        }
    }

    [TestClass]
    public class PieceBagUnitTest : PieceProviderUnitTest
    {
        private const int HistorySize = 4;

        // Always get first available
        protected Pieces PseudoRandom(IEnumerable<IOccurancy<Pieces>> occurancies, IEnumerable<Pieces> history)
        {
            var available = (occurancies as IList<IOccurancy<Pieces>> ?? occurancies.ToList()).Where(x => !history.Contains(x.Value)).ToList();
            if (available.Any())
            {
                Pieces piece = available[0].Value;
                return piece;
            }
            return Pieces.Invalid;
        }

        protected override IPieceProvider CreatePieceProvider()
        {
            return new PieceBag(PseudoRandom, HistorySize);
        }
        
        [TestMethod]
        public void TestHistory()
        {
            IPieceProvider pieceProvider = CreatePieceProvider();
            pieceProvider.Occurancies = () => new[] {
                new PieceOccurancy
                {
                    Occurancy = 25,
                    Value = Pieces.TetriminoI
                },
            new PieceOccurancy
                {
                    Occurancy = 25,
                    Value = Pieces.TetriminoJ
                },
            new PieceOccurancy
                {
                    Occurancy = 25,
                    Value = Pieces.TetriminoL
                },
            new PieceOccurancy
                {
                    Occurancy = 25,
                    Value = Pieces.TetriminoO
                }};

            Pieces piece1 = pieceProvider[0];
            Pieces piece2 = pieceProvider[1];
            Pieces piece3 = pieceProvider[2];
            Pieces piece4 = pieceProvider[3];
            Pieces piece5 = pieceProvider[4];

            Assert.AreEqual(piece5, Pieces.Invalid);
        }
    }

    [TestClass]
    public class PieceQueueUnitTest : PieceProviderUnitTest
    {
        private int _index;

        // Always get first available
        protected Pieces PseudoRandom(IEnumerable<PieceOccurancy> occurancies)
        {
            var available = occurancies.ToList();
            if (_index < available.Count)
            {
                Pieces piece = available[_index].Value;
                _index++;
                return piece;
            }
            return Pieces.Invalid;
        }

        protected override IPieceProvider CreatePieceProvider()
        {
            _index = 0;
            return new PieceQueue(PseudoRandom);
        }

        protected override void Reset(IPieceProvider provider)
        {
            _index = 0;
            base.Reset(provider);
        }
    }
}
