using System;
using System.Globalization;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TetriNET.Common.Contracts;
using TetriNET.Server.BanManager;
using TetriNET.Server.GenericHost;
using TetriNET.Server.Interfaces;
using TetriNET.Server.PlayerManager;
using TetriNET.Server.SpectatorManager;
using TetriNET.Tests.Server.Mocking;

namespace TetriNET.Tests.Server
{
    [TestClass]
    public class GenericHostUnitTest
    {
        private IHost CreateHost()
        {
            IFactory factory = new MockFactory();
            return new MockHost(factory.CreatePlayerManager(6), factory.CreateSpectatorManager(10), factory.CreateBanManager(), factory);
        }

        // TODO: GenericHost API: check HostXXX callback

        [TestMethod]
        public void TestHostPlayerRegisteredCalledOnRegisterPlayer()
        {
            IHost host = CreateHost();

            int count = 0;
            host.HostPlayerRegistered += (player, id) => count++;
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            host.RegisterPlayer(callback1, "player1", "team1");
            host.RegisterPlayer(callback2, "player2", "team1");
            
            Assert.AreEqual(count, 2);
            Assert.AreEqual(callback1.GetCallCount("OnPlayerRegistered"), 0); // OnPlayerRegistered callback is called from GenericHost if register has failed
            Assert.AreEqual(callback2.GetCallCount("OnPlayerRegistered"), 0); // OnPlayerRegistered callback is called from GenericHost if register has failed
        }

        [TestMethod]
        public void TestHostPlayerRegisteredNotCalledIfErrorOnRegisterPlayer()
        {
            IHost host = CreateHost();

            int count = 0;
            host.HostPlayerRegistered += (player, id) => count++;
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            host.RegisterPlayer(callback1, "player1", "team1");
            host.RegisterPlayer(callback2, "player1", "team1"); // same name -> register failed

            Assert.AreEqual(count, 1);
            Assert.AreEqual(callback1.GetCallCount("OnPlayerRegistered"), 0); // OnPlayerRegistered callback is called from GenericHost if register has failed
            Assert.AreEqual(callback2.GetCallCount("OnPlayerRegistered"), 1); // OnPlayerRegistered callback is called from GenericHost if register has failed
        }

        [TestMethod]
        public void TestPlayerNotRegisteredIfNameTooLong()
        {
            IHost host = CreateHost();

            int count = 0;
            host.HostPlayerRegistered += (player, id) => count++;
            host.RegisterPlayer(new CountCallTetriNETCallback(), "012345678901234567890123", "team1");

            Assert.AreEqual(host.PlayerManager.PlayerCount, 0);
        }

        [TestMethod]
        public void TestPlayerNotRegisteredIfTooManyPlayers()
        {
            IHost host = CreateHost();

            int count = 0;
            host.HostPlayerRegistered += (player, id) => count++;
            for (int i = 0; i < host.PlayerManager.MaxPlayers + 10; i++ )
                host.RegisterPlayer(new CountCallTetriNETCallback(), i.ToString(CultureInfo.InvariantCulture), "team1");

            Assert.AreEqual(count, host.PlayerManager.MaxPlayers);
            Assert.AreEqual(host.PlayerManager.PlayerCount, host.PlayerManager.MaxPlayers);
        }

        [TestMethod]
        public void TestPlayerAddedInManagerOnRegisterPlayer()
        {
            IHost host = CreateHost();

            host.RegisterPlayer(new CountCallTetriNETCallback(), "player1", "team1");

            Assert.AreEqual(host.PlayerManager.PlayerCount, 1);
            Assert.IsNotNull(host.PlayerManager["player1"]);
        }

        [TestMethod]
        public void TestHostSpectatorRegisteredCalledOnRegisterSpectator()
        {
            IHost host = CreateHost();

            int count = 0;
            host.HostSpectatorRegistered += (player, id) => count++;
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            host.RegisterSpectator(callback1, "spectator1");
            host.RegisterSpectator(callback2, "spectator2");

            Assert.AreEqual(count, 2);
            Assert.AreEqual(callback1.GetCallCount("OnSpectatorRegistered"), 0); // OnSpectatorRegistered callback is called from GenericHost if register has failed
            Assert.AreEqual(callback2.GetCallCount("OnSpectatorRegistered"), 0); // OnSpectatorRegistered callback is called from GenericHost if register has failed
        }

        [TestMethod]
        public void TestHostSpectatorRegisteredNotCalledIfErrorOnRegisterSpectator()
        {
            IHost host = CreateHost();

            int count = 0;
            host.HostSpectatorRegistered += (player, id) => count++;
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();
            host.RegisterSpectator(callback1, "spectator1");
            host.RegisterSpectator(callback2, "spectator1"); // same name -> register failed

            Assert.AreEqual(count, 1);
            Assert.AreEqual(callback1.GetCallCount("OnSpectatorRegistered"), 0); // OnSpectatorRegistered callback is called from GenericHost if register has failed
            Assert.AreEqual(callback2.GetCallCount("OnSpectatorRegistered"), 1); // OnSpectatorRegistered callback is called from GenericHost if register has failed
        }

        [TestMethod]
        public void TestSpectatorNotRegisteredIfNameTooLong()
        {
            IHost host = CreateHost();

            int count = 0;
            host.HostSpectatorRegistered += (player, id) => count++;
            host.RegisterSpectator(new CountCallTetriNETCallback(), "012345678901234567890123");

            Assert.AreEqual(count, 0);
            Assert.AreEqual(host.PlayerManager.PlayerCount, 0);
        }

        [TestMethod]
        public void TestSpectatorNotRegisteredIfTooManySpectators()
        {
            IHost host = CreateHost();

            int count = 0;
            host.HostSpectatorRegistered += (player, id) => count++;
            for (int i = 0; i < host.SpectatorManager.MaxSpectators + 10; i++)
                host.RegisterSpectator(new CountCallTetriNETCallback(), i.ToString(CultureInfo.InvariantCulture));

            Assert.AreEqual(count, host.SpectatorManager.MaxSpectators);
            Assert.AreEqual(host.SpectatorManager.SpectatorCount, host.SpectatorManager.MaxSpectators);
        }

        [TestMethod]
        public void TestSpectatorAddedInManagerOnRegisterSpectator()
        {
            IHost host = CreateHost();

            host.RegisterSpectator(new CountCallTetriNETCallback(), "player1");

            Assert.AreEqual(host.SpectatorManager.SpectatorCount, 1);
        }

        [TestMethod]
        public void TestHostPlayerLeftCalledOnDisconnection()
        {
            IHost host = CreateHost();
            int count = 0;
            host.HostPlayerLeft += (player1, reason) => count++;
            host.RegisterPlayer(new RaiseExceptionTetriNETCallback(), "player1", "team1");

            IPlayer player = host.PlayerManager["player1"];
            player.OnGamePaused(); // -> raise an exception -> disconnecting player

            Assert.AreEqual(count, 1);
        }

        [TestMethod]
        public void TestHostSpectatorLeftCalledOnDisconnection()
        {
            IHost host = CreateHost();
            int count = 0;
            host.HostSpectatorLeft += (spectator1, reason) => count++;
            host.RegisterSpectator(new RaiseExceptionTetriNETCallback(), "spectator1");

            ISpectator spectator = host.SpectatorManager["spectator1"];
            spectator.OnGamePaused(); // -> raise an exception -> disconnecting spectator

            Assert.AreEqual(count, 1);
        }

        [TestMethod]
        public void TestHostUnregisteredNotCalledOnUnknownPlayer() // Unregistered could be any TetriNET callback
        {
            IHost host = CreateHost();
            int count = 0;
            host.HostPlayerUnregistered += player => count++;

            host.UnregisterPlayer(new CountCallTetriNETCallback());

            Assert.AreEqual(count, 0);
        }

        [TestMethod]
        public void TestHostUnregisteredCalledOnKnownPlayer() // Unregistered could be any TetriNET callback
        {
            IHost host = CreateHost();
            host.RegisterPlayer(new RaiseExceptionTetriNETCallback(), "player1", "team1");
            IPlayer player = host.PlayerManager["player1"];
            int count = 0;
            host.HostPlayerUnregistered += player1 => count++;

            host.UnregisterPlayer(player.Callback);

            Assert.AreEqual(count, 1);
        }

        [TestMethod]
        public void TestActionFromClientRefreshedOnClientMethod()
        {
            IHost host = CreateHost();
            host.RegisterPlayer(new RaiseExceptionTetriNETCallback(), "player1", "team1");
            IPlayer player = host.PlayerManager["player1"];
            DateTime lastActionFromClient = player.LastActionFromClient;

            Thread.Sleep(1);
            host.PauseGame(player.Callback);

            Assert.AreNotEqual(player.LastActionFromClient, lastActionFromClient);
        }
    }

    public class MockFactory : IFactory
    {
        public IBanManager CreateBanManager()
        {
            return new BanManager(); // TODO: unit test
        }

        public IPlayerManager CreatePlayerManager(int maxPlayers)
        {
            return new PlayerManagerDictionaryBased(maxPlayers);
        }

        public ISpectatorManager CreateSpectatorManager(int maxSpectators)
        {
            return new SpectatorManagerDictionaryBased(maxSpectators); // TODO: unit test
        }

        public IPieceProvider CreatePieceProvider()
        {
            throw new NotImplementedException();
        }

        public IPlayer CreatePlayer(int id, string name, ITetriNETCallback callback)
        {
            return new Player(id, name, callback);
        }

        public ISpectator CreateSpectator(int id, string name, ITetriNETCallback callback)
        {
            return new Spectator(id, name, callback);// TODO: unit test
        }
    }

    public class MockHost : GenericHost
    {
        public MockHost(IPlayerManager playerManager, ISpectatorManager spectatorManager, IBanManager banManager, IFactory factory) : base(playerManager, spectatorManager, banManager, factory)
        {
        }
        
        public override void Start()
        {
            // NOP
        }
        
        public override void Stop()
        {
            // NOP
        }
        
        public override void RemovePlayer(IPlayer player)
        {
            // NOP
        }

        public override void RemoveSpectator(ISpectator spectator)
        {
            // NOP
        }
    }
}
