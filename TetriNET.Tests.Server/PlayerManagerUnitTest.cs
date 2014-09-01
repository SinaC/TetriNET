using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TetriNET.Common.Contracts;
using TetriNET.Common.Logger;
using TetriNET.Server.Interfaces;
using TetriNET.Server.PlayerManager;
using TetriNET.Tests.Server.Mocking;

namespace TetriNET.Tests.Server
{
    [TestClass]
    public abstract class PlayerManagerUnitTest
    {
        protected abstract IPlayerManager CreatePlayerManager(int maxPlayers);

        [TestInitialize]
        public void Initialize()
        {
            Log.SetLogger(new LogMock());
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
            playerManager.Add(new Player(0, "player1", new CountCallTetriNETCallback()));
            playerManager.Add(new Player(1, "player2", new CountCallTetriNETCallback()));

            int id = playerManager.FirstAvailableId;

            Assert.AreEqual(id, 2);
        }

        [TestMethod]
        public void TestFirstAvailableIdFull()
        {
            IPlayerManager playerManager = CreatePlayerManager(1);
            playerManager.Add(new Player(0, "player1", new CountCallTetriNETCallback()));

            int id = playerManager.FirstAvailableId;

            Assert.AreEqual(id, -1);
        }

        [TestMethod]
        public void TestAddNullPlayer()
        {
            IPlayerManager playerManager = CreatePlayerManager(5);

            bool added = playerManager.Add(null);

            Assert.IsFalse(added);
            Assert.AreEqual(playerManager.Players.Count, 0);
        }

        [TestMethod]
        public void TestAddNoMaxPlayers()
        {
            IPlayerManager playerManager = CreatePlayerManager(5);

            playerManager.Add(new Player(0, "player1", new CountCallTetriNETCallback()));
            playerManager.Add(new Player(1, "player2", new CountCallTetriNETCallback()));

            Assert.AreEqual(playerManager.PlayerCount, 2);
            Assert.AreEqual(playerManager.Players.Count, 2);
            Assert.IsTrue(playerManager.Players.Any(x => x.Name == "player1") && playerManager.Players.Any(x => x.Name == "player2"));
        }

        [TestMethod]
        public void TestAddWithMaxPlayers()
        {
            IPlayerManager playerManager = CreatePlayerManager(1);
            playerManager.Add(new Player(0, "player1", new CountCallTetriNETCallback()));
            
            bool inserted = playerManager.Add(new Player(1, "player2", new CountCallTetriNETCallback()));

            Assert.AreEqual(playerManager.PlayerCount, 1);
            Assert.IsFalse(inserted);
            Assert.IsTrue(playerManager.Players.Any(x => x.Name == "player1"));
        }

        [TestMethod]
        public void TestAddSamePlayerName()
        {
            IPlayerManager playerManager = CreatePlayerManager(5);
            playerManager.Add(new Player(0, "player1", new CountCallTetriNETCallback()));

            bool inserted = playerManager.Add(new Player(1, "player1", new CountCallTetriNETCallback()));

            Assert.AreEqual(playerManager.PlayerCount, 1);
            Assert.IsFalse(inserted);
        }

        [TestMethod]
        public void TestAddSamePlayerId()
        {
            IPlayerManager playerManager = CreatePlayerManager(5);
            playerManager.Add(new Player(0, "player1", new CountCallTetriNETCallback()));

            bool inserted = playerManager.Add(new Player(0, "player2", new CountCallTetriNETCallback()));

            Assert.AreEqual(playerManager.PlayerCount, 1);
            Assert.IsFalse(inserted);
        }

        [TestMethod]
        public void TestAddSamePlayerCallback()
        {
            ITetriNETCallback callback = new CountCallTetriNETCallback();
            IPlayerManager playerManager = CreatePlayerManager(5);
            playerManager.Add(new Player(0, "player1", callback));

            bool inserted = playerManager.Add(new Player(1, "player2", callback));

            Assert.AreEqual(playerManager.PlayerCount, 1);
            Assert.IsFalse(inserted);
        }

        [TestMethod]
        public void TestRemoveExistingPlayer()
        {
            IPlayerManager playerManager = CreatePlayerManager(5);
            IPlayer player = new Player(0, "player1", new CountCallTetriNETCallback());
            playerManager.Add(player);

            bool removed = playerManager.Remove(player);

            Assert.IsTrue(removed);
            Assert.AreEqual(playerManager.PlayerCount, 0);
        }

        [TestMethod]
        public void TestRemoveNonExistingPlayer()
        {
            IPlayerManager playerManager = CreatePlayerManager(5);
            playerManager.Add(new Player(0, "player1", new CountCallTetriNETCallback()));

            bool removed = playerManager.Remove(new Player(0, "player2", new CountCallTetriNETCallback()));

            Assert.IsFalse(removed);
            Assert.AreEqual(playerManager.PlayerCount, 1);
            Assert.AreEqual(playerManager.Players.Count, 1);
        }

        [TestMethod]
        public void TestRemoveNullPlayer()
        {
            IPlayerManager playerManager = CreatePlayerManager(5);
            playerManager.Add(new Player(0, "player1", new CountCallTetriNETCallback()));

            bool removed = playerManager.Remove(null);

            Assert.IsFalse(removed);
            Assert.AreEqual(playerManager.PlayerCount, 1);
        }

        [TestMethod]
        public void TestClearNoPlayers()
        {
            IPlayerManager playerManager = CreatePlayerManager(5);

            playerManager.Clear();

            Assert.AreEqual(playerManager.PlayerCount, 0);
        }

        [TestMethod]
        public void TestClearSomePlayers()
        {
            IPlayerManager playerManager = CreatePlayerManager(5);
            playerManager.Add(new Player(0, "player1", new CountCallTetriNETCallback()));
            playerManager.Add(new Player(1, "player2", new CountCallTetriNETCallback()));
            playerManager.Add(new Player(2, "player3", new CountCallTetriNETCallback()));

            playerManager.Clear();

            Assert.AreEqual(playerManager.PlayerCount, 0);
        }

        [TestMethod]
        public void TestServerMasterNullAtCreation()
        {
            IPlayerManager playerManager = CreatePlayerManager(5);

            Assert.IsNull(playerManager.ServerMaster);
        }

        [TestMethod]
        public void TestServerMasterModifiedAfterAddPlayer()
        {
            IPlayer player = new Player(0, "player1", new CountCallTetriNETCallback());
            IPlayerManager playerManager = CreatePlayerManager(5);

            playerManager.Add(player);

            Assert.AreEqual(playerManager.ServerMaster, player);
        }

        [TestMethod]
        public void TestServerMasterSameAsFirstPlayerAdded()
        {
            IPlayer player1 = new Player(0, "player1", new CountCallTetriNETCallback());
            IPlayer player2 = new Player(1, "player2", new CountCallTetriNETCallback());
            IPlayerManager playerManager = CreatePlayerManager(5);

            playerManager.Add(player1);
            playerManager.Add(player2);

            Assert.AreEqual(playerManager.ServerMaster, player1);
        }

        [TestMethod]
        public void TestServerMasterIsLowestIdAfterRemove()
        {
            IPlayer player1 = new Player(0, "player1", new CountCallTetriNETCallback());
            IPlayer player2 = new Player(1, "player2", new CountCallTetriNETCallback());
            IPlayer player3 = new Player(2, "player3", new CountCallTetriNETCallback());
            IPlayerManager playerManager = CreatePlayerManager(5);
            playerManager.Add(player1);
            playerManager.Add(player2);
            playerManager.Add(player3);

            playerManager.Remove(player1);

            Assert.AreEqual(playerManager.ServerMaster, player2);
        }

        [TestMethod]
        public void TestServerMasterNullOnClear()
        {
            IPlayer player1 = new Player(0, "player1", new CountCallTetriNETCallback());
            IPlayer player2 = new Player(1, "player2", new CountCallTetriNETCallback());
            IPlayer player3 = new Player(2, "player3", new CountCallTetriNETCallback());
            IPlayerManager playerManager = CreatePlayerManager(5);
            playerManager.Add(player1);
            playerManager.Add(player2);
            playerManager.Add(player3);

            playerManager.Clear();

            Assert.IsNull(playerManager.ServerMaster);
        }

        [TestMethod]
        public void TestServerMasterModifiedAfterDeletionAndInsertion()
        {
            IPlayer player1 = new Player(0, "player1", new CountCallTetriNETCallback());
            IPlayer player2 = new Player(1, "player2", new CountCallTetriNETCallback());
            IPlayer player3 = new Player(2, "player3", new CountCallTetriNETCallback());
            IPlayerManager playerManager = CreatePlayerManager(5);
            playerManager.Add(player1);
            playerManager.Add(player2);
            playerManager.Add(player3);

            playerManager.Remove(player1);
            playerManager.Add(player1);

            Assert.AreEqual(playerManager.ServerMaster, player1);
        }

        [TestMethod]
        public void TestNameIndexerFindExistingPlayer()
        {
            IPlayer player1 = new Player(0, "player1", new CountCallTetriNETCallback());
            IPlayer player2 = new Player(1, "player2", new CountCallTetriNETCallback());
            IPlayer player3 = new Player(2, "player3", new CountCallTetriNETCallback());
            IPlayerManager playerManager = CreatePlayerManager(5);
            playerManager.Add(player1);
            playerManager.Add(player2);
            playerManager.Add(player3);

            IPlayer searched = playerManager["player2"];

            Assert.AreEqual(searched, player2);
        }

        [TestMethod]
        public void TestNameIndexerFindNonExistingPlayer()
        {
            IPlayer player1 = new Player(0, "player1", new CountCallTetriNETCallback());
            IPlayer player2 = new Player(1, "player2", new CountCallTetriNETCallback());
            IPlayer player3 = new Player(2, "player3", new CountCallTetriNETCallback());
            IPlayerManager playerManager = CreatePlayerManager(5);
            playerManager.Add(player1);
            playerManager.Add(player2);
            playerManager.Add(player3);

            IPlayer searched = playerManager["player4"];

            Assert.IsNull(searched);
        }

        [TestMethod]
        public void TestIdIndexerFindExistingPlayer()
        {
            IPlayer player1 = new Player(0, "player1", new CountCallTetriNETCallback());
            IPlayer player2 = new Player(1, "player2", new CountCallTetriNETCallback());
            IPlayer player3 = new Player(2, "player3", new CountCallTetriNETCallback());
            IPlayerManager playerManager = CreatePlayerManager(5);
            playerManager.Add(player1);
            playerManager.Add(player2);
            playerManager.Add(player3);

            IPlayer searched = playerManager[1];

            Assert.AreEqual(searched, player2);
        }

        [TestMethod]
        public void TestIdIndexerFindNonExistingPlayer()
        {
            IPlayer player1 = new Player(0, "player1", new CountCallTetriNETCallback());
            IPlayer player2 = new Player(1, "player2", new CountCallTetriNETCallback());
            IPlayer player3 = new Player(2, "player3", new CountCallTetriNETCallback());
            IPlayerManager playerManager = CreatePlayerManager(5);
            playerManager.Add(player1);
            playerManager.Add(player2);
            playerManager.Add(player3);

            IPlayer searched = playerManager[3];

            Assert.IsNull(searched);
        }

        [TestMethod]
        public void TestCallbackIndexerFindExistingPlayer()
        {
            ITetriNETCallback callback1 = new CountCallTetriNETCallback();
            ITetriNETCallback callback2 = new CountCallTetriNETCallback();
            ITetriNETCallback callback3 = new CountCallTetriNETCallback();
            IPlayer player1 = new Player(0, "player1", callback1);
            IPlayer player2 = new Player(1, "player2", callback2);
            IPlayer player3 = new Player(2, "player3", callback3);
            IPlayerManager playerManager = CreatePlayerManager(5);
            playerManager.Add(player1);
            playerManager.Add(player2);
            playerManager.Add(player3);

            IPlayer searched = playerManager[callback2];

            Assert.AreEqual(searched, player2);
        }

        [TestMethod]
        public void TestCallbackIndexerFindNonExistingPlayer()
        {
            ITetriNETCallback callback1 = new CountCallTetriNETCallback();
            ITetriNETCallback callback2 = new CountCallTetriNETCallback();
            ITetriNETCallback callback3 = new CountCallTetriNETCallback();
            ITetriNETCallback callback4 = new CountCallTetriNETCallback();
            IPlayer player1 = new Player(0, "player1", callback1);
            IPlayer player2 = new Player(1, "player2", callback2);
            IPlayer player3 = new Player(2, "player3", callback3);
            IPlayer player4 = new Player(3, "player4", callback4); // NOT ADDED IN PLAYERMANAGER
            IPlayerManager playerManager = CreatePlayerManager(5);
            playerManager.Add(player1);
            playerManager.Add(player2);
            playerManager.Add(player3);

            IPlayer searched = playerManager[3];

            Assert.IsNull(searched);
        }
    }

    [TestClass]
    public class PlayerManagerArrayBasedUnitTest : PlayerManagerUnitTest
    {
        [TestInitialize]
        protected override IPlayerManager CreatePlayerManager(int maxPlayers)
        {
            return new PlayerManagerArrayBased(maxPlayers);
        } 
    }

    [TestClass]
    public class PlayerManagerDictionaryBasedUnitTest : PlayerManagerUnitTest
    {
        [TestInitialize]
        protected override IPlayerManager CreatePlayerManager(int maxPlayers)
        {
            return new PlayerManagerDictionaryBased(maxPlayers);
        }
    }
}
