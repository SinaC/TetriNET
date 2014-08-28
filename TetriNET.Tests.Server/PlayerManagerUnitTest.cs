using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TetriNET.Server.Interfaces;
using TetriNET.Server.PlayerManager;
using TetriNET.Tests.Server.Implementations;

namespace TetriNET.Tests.Server
{
    [TestClass]
    public class PlayerManagerUnitTest
    {
        private IPlayerManager CreatePlayerManager(int maxPlayers)
        {
            return new PlayerManagerArrayBased(maxPlayers); // TODO: perform tests for each implementation of IPlayerManager
        }

        [TestMethod]
        public void TestStrictlyPositiveMaxPlayers()
        {
            try
            {
                IPlayerManager playerManager = CreatePlayerManager(0);
                Assert.Fail("ArgumentOutOfRange exception not raised");
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Assert.AreEqual(ex.ParamName, "maxPlayers");
            }
        }

        [TestMethod]
        public void TestLockObjectNotNull()
        {
            IPlayerManager playerManager = CreatePlayerManager(5);
        
            Assert.IsNotNull(playerManager.LockObject);
        }

        [TestMethod]
        public void TestMaxPlayersSet()
        {
            IPlayerManager playerManager = CreatePlayerManager(5);

            Assert.AreEqual(playerManager.MaxPlayers, 5);
        }

        [TestMethod]
        public void TestFirstAvailableIdEmpty()
        {
            IPlayerManager playerManager = CreatePlayerManager(5);

            int id = playerManager.FirstAvailableId;

            Assert.AreEqual(id, 0);
        }

        [TestMethod]
        public void TestFirstAvailableIdSomePlayers()
        {
            IPlayerManager playerManager = CreatePlayerManager(5);
            playerManager.Add(new Player(0, "player1", new NullLoopTetriNETCallback()));
            playerManager.Add(new Player(1, "player2", new NullLoopTetriNETCallback()));

            int id = playerManager.FirstAvailableId;

            Assert.AreEqual(id, 2);
        }

        [TestMethod]
        public void TestFirstAvailableIdFull()
        {
            IPlayerManager playerManager = CreatePlayerManager(1);
            playerManager.Add(new Player(0, "player1", new NullLoopTetriNETCallback()));

            int id = playerManager.FirstAvailableId;

            Assert.AreEqual(id, -1);
        }

        [TestMethod]
        public void TestPlayersCorrectlyInserted()
        {
            IPlayerManager playerManager = CreatePlayerManager(5);
            playerManager.Add(new Player(0, "player1", new NullLoopTetriNETCallback()));
            playerManager.Add(new Player(1, "player2", new NullLoopTetriNETCallback()));

            List<IPlayer> players = playerManager.Players;
            int count = playerManager.PlayerCount;

            Assert.AreEqual(count, 2);
            Assert.AreEqual(players.Count, 2);
            Assert.IsTrue(players.Any(x => x.Name == "player1") && players.Any(x => x.Name == "player2"));
        }

        // TODO: check add, remove, clear, servermaster, indexers
    }
}
