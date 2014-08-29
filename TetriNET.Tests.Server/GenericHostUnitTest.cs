using System;
using System.Globalization;
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

        // TODO: RegisterSpectator + GenericHost API: check HostXXX callback

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
        public void TestPlayerNotRegisterIfNameToLong()
        {
            IHost host = CreateHost();

            int count = 0;
            host.HostPlayerRegistered += (player, id) => count++;
            host.RegisterPlayer(new CountCallTetriNETCallback(), "012345678901234567890123", "team1");

            Assert.AreEqual(count, 0);
            Assert.AreEqual(host.PlayerManager.PlayerCount, 0);
        }

        [TestMethod]
        public void TestPlayerNotRegisterIfTooManyPlayers()
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
