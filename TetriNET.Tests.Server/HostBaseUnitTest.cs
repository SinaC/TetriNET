using System;
using System.Globalization;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Logger;
using TetriNET.Server.Interfaces;
using TetriNET.Tests.Server.Mocking;

namespace TetriNET.Tests.Server
{
    [TestClass]
    public class HostBaseUnitTest
    {
        private readonly Versioning _clientVersion = new Versioning
        {
            Major = 1,
            Minor = 0
        };

        private IHost CreateHost()
        {
            IFactory factory = new FactoryMock();
            IHost host = new HostBaseMock(factory.CreatePlayerManager(6), factory.CreateSpectatorManager(10), factory.CreateBanManager(), factory);
            host.SetVersion(new Versioning
                {
                    Major = 1,
                    Minor = 0
                });
            return host;
        }

        [TestInitialize]
        public void Initialize()
        {
            Log.SetLogger(new LogMock());
        }

        // TODO: HostBase API: check HostXXX callback

        [TestMethod]
        public void TestHostPlayerRegisteredCalledOnRegisterPlayer()
        {
            IHost host = CreateHost();
            int count = 0;
            host.HostPlayerRegistered += (player, id) => count++;
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();

            host.RegisterPlayer(callback1, _clientVersion, "player1", "team1");
            host.RegisterPlayer(callback2, _clientVersion, "player2", "team1");
            
            Assert.AreEqual(count, 2);
            Assert.AreEqual(callback1.GetCallCount("OnPlayerRegistered"), 0); // OnPlayerRegistered callback is called from HostBase if register has failed
            Assert.AreEqual(callback2.GetCallCount("OnPlayerRegistered"), 0); // OnPlayerRegistered callback is called from HostBase if register has failed
        }

        [TestMethod]
        public void TestHostPlayerRegisteredNotCalledIfErrorOnRegisterPlayer()
        {
            IHost host = CreateHost();
            int count = 0;
            host.HostPlayerRegistered += (player, id) => count++;
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();

            host.RegisterPlayer(callback1, _clientVersion, "player1", "team1");
            host.RegisterPlayer(callback2, _clientVersion, "player1", "team1"); // same name -> register failed

            Assert.AreEqual(count, 1);
            Assert.AreEqual(callback1.GetCallCount("OnPlayerRegistered"), 0); // OnPlayerRegistered callback is called from HostBase if register has failed
            Assert.AreEqual(callback2.GetCallCount("OnPlayerRegistered"), 1); // OnPlayerRegistered callback is called from HostBase if register has failed
        }

        [TestMethod]
        public void TestPlayerNotRegisteredIfNameTooLong()
        {
            IHost host = CreateHost();
            int count = 0;
            host.HostPlayerRegistered += (player, id) => count++;

            host.RegisterPlayer(new CountCallTetriNETCallback(), _clientVersion, "012345678901234567890123", "team1");

            Assert.AreEqual(host.PlayerManager.PlayerCount, 0);
        }

        [TestMethod]
        public void TestPlayerNotRegisteredIfTooManyPlayers()
        {
            IHost host = CreateHost();
            int count = 0;
            host.HostPlayerRegistered += (player, id) => count++;

            for (int i = 0; i < host.PlayerManager.MaxPlayers + 10; i++ )
                host.RegisterPlayer(new CountCallTetriNETCallback(), _clientVersion, i.ToString(CultureInfo.InvariantCulture), "team1");

            Assert.AreEqual(count, host.PlayerManager.MaxPlayers);
            Assert.AreEqual(host.PlayerManager.PlayerCount, host.PlayerManager.MaxPlayers);
        }

        [TestMethod]
        public void TestPlayerAddedInManagerOnRegisterPlayer()
        {
            IHost host = CreateHost();

            host.RegisterPlayer(new CountCallTetriNETCallback(), _clientVersion, "player1", "team1");

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

            host.RegisterSpectator(callback1, _clientVersion, "spectator1");
            host.RegisterSpectator(callback2, _clientVersion, "spectator2");

            Assert.AreEqual(count, 2);
            Assert.AreEqual(callback1.GetCallCount("OnSpectatorRegistered"), 0); // OnSpectatorRegistered callback is called from HostBase if register has failed
            Assert.AreEqual(callback2.GetCallCount("OnSpectatorRegistered"), 0); // OnSpectatorRegistered callback is called from HostBase if register has failed
        }

        [TestMethod]
        public void TestHostSpectatorRegisteredNotCalledIfErrorOnRegisterSpectator()
        {
            IHost host = CreateHost();
            int count = 0;
            host.HostSpectatorRegistered += (player, id) => count++;
            CountCallTetriNETCallback callback1 = new CountCallTetriNETCallback();
            CountCallTetriNETCallback callback2 = new CountCallTetriNETCallback();

            host.RegisterSpectator(callback1, _clientVersion, "spectator1");
            host.RegisterSpectator(callback2, _clientVersion, "spectator1"); // same name -> register failed

            Assert.AreEqual(count, 1);
            Assert.AreEqual(callback1.GetCallCount("OnSpectatorRegistered"), 0); // OnSpectatorRegistered callback is called from HostBase if register has failed
            Assert.AreEqual(callback2.GetCallCount("OnSpectatorRegistered"), 1); // OnSpectatorRegistered callback is called from HostBase if register has failed
        }

        [TestMethod]
        public void TestSpectatorNotRegisteredIfNameTooLong()
        {
            IHost host = CreateHost();
            int count = 0;
            host.HostSpectatorRegistered += (player, id) => count++;

            host.RegisterSpectator(new CountCallTetriNETCallback(), _clientVersion, "012345678901234567890123");

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
                host.RegisterSpectator(new CountCallTetriNETCallback(), _clientVersion, i.ToString(CultureInfo.InvariantCulture));

            Assert.AreEqual(count, host.SpectatorManager.MaxSpectators);
            Assert.AreEqual(host.SpectatorManager.SpectatorCount, host.SpectatorManager.MaxSpectators);
        }

        [TestMethod]
        public void TestSpectatorAddedInManagerOnRegisterSpectator()
        {
            IHost host = CreateHost();

            host.RegisterSpectator(new CountCallTetriNETCallback(), _clientVersion, "player1");

            Assert.AreEqual(host.SpectatorManager.SpectatorCount, 1);
        }

        [TestMethod]
        public void TestHostPlayerLeftCalledOnDisconnection()
        {
            IHost host = CreateHost();
            int count = 0;
            host.HostPlayerLeft += (player1, reason) => count++;
            host.RegisterPlayer(new RaiseExceptionTetriNETCallback(), _clientVersion, "player1", "team1");

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
            host.RegisterSpectator(new RaiseExceptionTetriNETCallback(), _clientVersion, "spectator1");

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
            host.RegisterPlayer(new CountCallTetriNETCallback(), _clientVersion, "player1", "team1");
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
            host.RegisterPlayer(new CountCallTetriNETCallback(), _clientVersion, "player1", "team1");
            IPlayer player = host.PlayerManager["player1"];
            DateTime lastActionFromClient = player.LastActionFromClient;

            Thread.Sleep(1);
            host.PauseGame(player.Callback);

            Assert.AreNotEqual(player.LastActionFromClient, lastActionFromClient);
        }

        [TestMethod]
        public void TestRegisterFailedIfWrongClientVersion()
        {
            IHost host = CreateHost();
            int count = 0;
            host.HostPlayerRegistered += (player, id) => count++;
            CountCallTetriNETCallback callback = new CountCallTetriNETCallback();

            host.RegisterPlayer(
                callback,
                new Versioning
                    {
                        Major = 0,
                        Minor = 0
                    },
                "player1", "team1");

            Assert.AreEqual(count, 0);
            Assert.AreEqual(callback.GetCallCount("OnPlayerRegistered"), 1);
        }
    }
}
