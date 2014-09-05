using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Logger;
using TetriNET.Server.Interfaces;
using TetriNET.Tests.Server.Mocking;

namespace TetriNET.Tests.Server
{
    [TestClass]
    public class ServerTest
    {
        private LogMock _log;
        private IHost _host;
        private IFactory _factory;

        private IServer CreateServer()
        {
            IFactory factory = new FactoryMock();
            _factory = factory;
            return new TetriNET.Server.Server(factory.CreatePlayerManager(6), factory.CreateSpectatorManager(10), factory.CreatePieceProvider(), 1, 1);
        }

        private IServer CreateServerWithHost()
        {
            IFactory factory = new FactoryMock();
            IPlayerManager playerManager = factory.CreatePlayerManager(6);
            ISpectatorManager spectatorManager = factory.CreateSpectatorManager(10);
            IBanManager banManager = factory.CreateBanManager();
            IPieceProvider pieceProvider = factory.CreatePieceProvider();
            IHost host = new HostMock(playerManager, spectatorManager, banManager, factory);
            IServer server = new TetriNET.Server.Server(playerManager, spectatorManager, pieceProvider, 1, 1);
            server.AddHost(host);
            _host = host;
            _factory = factory;
            return server;
        }

        [TestInitialize]
        public void Initialize()
        {
            _host = null;
            _factory = null;
            _log = new LogMock();
            Log.SetLogger(_log);
        }

        [TestMethod]
        public void TestCreationState()
        {
            IServer server = CreateServer();

            Assert.AreEqual(server.State, ServerStates.WaitingStartServer);
        }

        [TestMethod]
        public void TestStartServerWithoutHost()
        {
            IServer server = CreateServer();

            server.StartServer();

            Assert.AreEqual(server.State, ServerStates.WaitingStartServer);
        }

        [TestMethod]
        public void TestStartServerOkWithHost()
        {
            IServer server = CreateServerWithHost();

            server.StartServer();

            Assert.AreEqual(server.State, ServerStates.WaitingStartGame);
            Assert.AreEqual(server.SpecialId, 0);
            Assert.IsTrue(server.WinList == null || server.WinList.Count == 0);
        }

        [TestMethod]
        public void TestStopServerWithoutStartServer()
        {
            IServer server = CreateServerWithHost();
            
            server.StopServer();

            Assert.AreEqual(server.State, ServerStates.WaitingStartServer);
        }

        [TestMethod]
        public void TestStopServerWithStartServer()
        {
            IServer server = CreateServerWithHost();
            server.StartServer();

            server.StopServer();

            Assert.AreEqual(server.State, ServerStates.WaitingStartServer);
        }

        [TestMethod]
        public void TestStopServerCallsOnServerStopped()
        {
            IServer server = CreateServerWithHost();
            server.StartServer();
            CountCallTetriNETCallback callback = new CountCallTetriNETCallback();
            _host.PlayerManager.Add(_factory.CreatePlayer(0, "player1", callback));

            server.StopServer();

            Assert.AreEqual(callback.GetCallCount("OnServerStopped"), 1);
        }

        [TestMethod]
        public void TestStopServerClearsPlayerManager()
        {
            IServer server = CreateServerWithHost();
            server.StartServer();
            _host.PlayerManager.Add(_factory.CreatePlayer(0, "player1", new CountCallTetriNETCallback()));

            server.StopServer();

            Assert.AreEqual(_host.PlayerManager.PlayerCount, 0);
        }

        [TestMethod]
        public void TestStartGameFailsIfServerNotStarted()
        {
            IServer server = CreateServerWithHost();

            server.StartGame();

            Assert.AreEqual(server.State, ServerStates.WaitingStartServer);
        }

        [TestMethod]
        public void TestStartGameFailsIfNoPlayers()
        {
            IServer server = CreateServerWithHost();
            server.StartServer();

            server.StartGame();

            Assert.AreEqual(server.State, ServerStates.WaitingStartGame);
        }

        [TestMethod]
        public void TestStartGameModificationOnServerAndPlayersAndSpectators()
        {
            IServer server = CreateServerWithHost();
            server.StartServer();
            IPlayer player1 = _factory.CreatePlayer(0, "player1", new CountCallTetriNETCallback());
            IPlayer player2 = _factory.CreatePlayer(1, "player2", new CountCallTetriNETCallback());
            ISpectator spectator1 = _factory.CreateSpectator(0, "spectator1", new CountCallTetriNETCallback());
            ISpectator spectator2 = _factory.CreateSpectator(1, "spectator2", new CountCallTetriNETCallback());
            _host.PlayerManager.Add(player1);
            _host.PlayerManager.Add(player2);
            _host.SpectatorManager.Add(spectator1);
            _host.SpectatorManager.Add(spectator2);

            server.StartGame();

            Assert.AreEqual(server.State, ServerStates.GameStarted);
            Assert.AreEqual(server.SpecialId, 0);
            Assert.IsTrue(_host.PlayerManager.Players.All(x => x.State == PlayerStates.Playing));
            Assert.IsTrue(_host.PlayerManager.Players.All(x => x.PieceIndex == 0));
            Assert.AreEqual((player1.Callback as CountCallTetriNETCallback).GetCallCount("OnGameStarted"), 1);
            Assert.AreEqual((player2.Callback as CountCallTetriNETCallback).GetCallCount("OnGameStarted"), 1);
            Assert.AreEqual((spectator1.Callback as CountCallTetriNETCallback).GetCallCount("OnGameStarted"), 1);
            Assert.AreEqual((spectator2.Callback as CountCallTetriNETCallback).GetCallCount("OnGameStarted"), 1);
        }

        [TestMethod]
        public void TestStopGameModificationOnServerAndPlayersAndSpectators()
        {
            IServer server = CreateServerWithHost();
            server.StartServer();
            IPlayer player1 = _factory.CreatePlayer(0, "player1", new CountCallTetriNETCallback());
            IPlayer player2 = _factory.CreatePlayer(1, "player2", new CountCallTetriNETCallback());
            ISpectator spectator1 = _factory.CreateSpectator(0, "spectator1", new CountCallTetriNETCallback());
            ISpectator spectator2 = _factory.CreateSpectator(1, "spectator2", new CountCallTetriNETCallback());
            _host.PlayerManager.Add(player1);
            _host.PlayerManager.Add(player2);
            _host.SpectatorManager.Add(spectator1);
            _host.SpectatorManager.Add(spectator2);

            server.StartGame();
            server.StopGame();

            Assert.AreEqual(server.State, ServerStates.WaitingStartGame);
            Assert.IsTrue(_host.PlayerManager.Players.All(x => x.State == PlayerStates.Registered));
            Assert.AreEqual((player1.Callback as CountCallTetriNETCallback).GetCallCount("OnGameFinished"), 1);
            Assert.AreEqual((player2.Callback as CountCallTetriNETCallback).GetCallCount("OnGameFinished"), 1);
            Assert.AreEqual((spectator1.Callback as CountCallTetriNETCallback).GetCallCount("OnGameFinished"), 1);
            Assert.AreEqual((spectator2.Callback as CountCallTetriNETCallback).GetCallCount("OnGameFinished"), 1);
        }

        [TestMethod]
        public void TestPauseGameIsOnGamePausedNotCalledIfGameNotStarted()
        {
            IServer server = CreateServerWithHost();
            server.StartServer();
            IPlayer player1 = _factory.CreatePlayer(0, "player1", new CountCallTetriNETCallback());
            IPlayer player2 = _factory.CreatePlayer(1, "player2", new CountCallTetriNETCallback());
            ISpectator spectator1 = _factory.CreateSpectator(0, "spectator1", new CountCallTetriNETCallback());
            ISpectator spectator2 = _factory.CreateSpectator(1, "spectator2", new CountCallTetriNETCallback());
            _host.PlayerManager.Add(player1);
            _host.PlayerManager.Add(player2);
            _host.SpectatorManager.Add(spectator1);
            _host.SpectatorManager.Add(spectator2);

            server.PauseGame();

            Assert.AreEqual(server.State, ServerStates.WaitingStartGame);
            Assert.AreEqual((player1.Callback as CountCallTetriNETCallback).GetCallCount("OnGamePaused"), 0);
            Assert.AreEqual((player2.Callback as CountCallTetriNETCallback).GetCallCount("OnGamePaused"), 0);
            Assert.AreEqual((spectator1.Callback as CountCallTetriNETCallback).GetCallCount("OnGamePaused"), 0);
            Assert.AreEqual((spectator2.Callback as CountCallTetriNETCallback).GetCallCount("OnGamePaused"), 0);
        }

        [TestMethod]
        public void TestPauseGameIsOnGamePausedCalled()
        {
            IServer server = CreateServerWithHost();
            server.StartServer();
            IPlayer player1 = _factory.CreatePlayer(0, "player1", new CountCallTetriNETCallback());
            IPlayer player2 = _factory.CreatePlayer(1, "player2", new CountCallTetriNETCallback());
            ISpectator spectator1 = _factory.CreateSpectator(0, "spectator1", new CountCallTetriNETCallback());
            ISpectator spectator2 = _factory.CreateSpectator(1, "spectator2", new CountCallTetriNETCallback());
            _host.PlayerManager.Add(player1);
            _host.PlayerManager.Add(player2);
            _host.SpectatorManager.Add(spectator1);
            _host.SpectatorManager.Add(spectator2);
            server.StartGame();

            server.PauseGame();

            Assert.AreEqual(server.State, ServerStates.GamePaused);
            Assert.AreEqual((player1.Callback as CountCallTetriNETCallback).GetCallCount("OnGamePaused"), 1);
            Assert.AreEqual((player2.Callback as CountCallTetriNETCallback).GetCallCount("OnGamePaused"), 1);
            Assert.AreEqual((spectator1.Callback as CountCallTetriNETCallback).GetCallCount("OnGamePaused"), 1);
            Assert.AreEqual((spectator2.Callback as CountCallTetriNETCallback).GetCallCount("OnGamePaused"), 1);
        }

        [TestMethod]
        public void TestResumeGameFailIfNotInPause()
        {
            IServer server = CreateServerWithHost();
            server.StartServer();
            IPlayer player1 = _factory.CreatePlayer(0, "player1", new CountCallTetriNETCallback());
            IPlayer player2 = _factory.CreatePlayer(1, "player2", new CountCallTetriNETCallback());
            ISpectator spectator1 = _factory.CreateSpectator(0, "spectator1", new CountCallTetriNETCallback());
            ISpectator spectator2 = _factory.CreateSpectator(1, "spectator2", new CountCallTetriNETCallback());
            _host.PlayerManager.Add(player1);
            _host.PlayerManager.Add(player2);
            _host.SpectatorManager.Add(spectator1);
            _host.SpectatorManager.Add(spectator2);
            server.StartGame();

            server.ResumeGame();

            Assert.AreEqual(server.State, ServerStates.GameStarted);
            Assert.AreEqual((player1.Callback as CountCallTetriNETCallback).GetCallCount("OnGameResumed"), 0);
            Assert.AreEqual((player2.Callback as CountCallTetriNETCallback).GetCallCount("OnGameResumed"), 0);
            Assert.AreEqual((spectator1.Callback as CountCallTetriNETCallback).GetCallCount("OnGameResumed"), 0);
            Assert.AreEqual((spectator2.Callback as CountCallTetriNETCallback).GetCallCount("OnGameResumed"), 0);
        }

        [TestMethod]
        public void TestResumeGameIsOnGameResumedCalled()
        {
            IServer server = CreateServerWithHost();
            server.StartServer();
            IPlayer player1 = _factory.CreatePlayer(0, "player1", new CountCallTetriNETCallback());
            IPlayer player2 = _factory.CreatePlayer(1, "player2", new CountCallTetriNETCallback());
            ISpectator spectator1 = _factory.CreateSpectator(0, "spectator1", new CountCallTetriNETCallback());
            ISpectator spectator2 = _factory.CreateSpectator(1, "spectator2", new CountCallTetriNETCallback());
            _host.PlayerManager.Add(player1);
            _host.PlayerManager.Add(player2);
            _host.SpectatorManager.Add(spectator1);
            _host.SpectatorManager.Add(spectator2);
            server.StartGame();
            server.PauseGame();

            server.ResumeGame();

            Assert.AreEqual(server.State, ServerStates.GameStarted);
            Assert.AreEqual((player1.Callback as CountCallTetriNETCallback).GetCallCount("OnGameResumed"), 1);
            Assert.AreEqual((player2.Callback as CountCallTetriNETCallback).GetCallCount("OnGameResumed"), 1);
            Assert.AreEqual((spectator1.Callback as CountCallTetriNETCallback).GetCallCount("OnGameResumed"), 1);
            Assert.AreEqual((spectator2.Callback as CountCallTetriNETCallback).GetCallCount("OnGameResumed"), 1);
        }

        [TestMethod]
        public void TestResetWinListFailIfNotWaitingStartGame()
        {
            IServer server = CreateServerWithHost();
            server.StartServer();
            IPlayer player1 = _factory.CreatePlayer(0, "player1", new CountCallTetriNETCallback());
            _host.PlayerManager.Add(player1);
            server.WinList.Add(new WinEntry
                {
                    PlayerName = "player1",
                    Team = "team1",
                    Score = 10
                });
            server.StartGame();
            
            server.ResetWinList();

            Assert.AreEqual(server.WinList.Count, 1);
        }

        [TestMethod]
        public void TestResetWinListOkIfGameNotStarted()
        {
            IServer server = CreateServerWithHost();
            server.StartServer();
            server.WinList.Add(new WinEntry
            {
                PlayerName = "player1",
                Team = "team1",
                Score = 10
            });

            server.ResetWinList();

            Assert.AreEqual(server.WinList.Count, 0);
        }

        [TestMethod]
        public void TestResetWinListAreOnPublishServerMessageAndOnWinListModifiedCalled()
        {
            IServer server = CreateServerWithHost();
            server.StartServer();
            CountCallTetriNETCallback playerCallback1 = new CountCallTetriNETCallback();
            CountCallTetriNETCallback playerCallback2 = new CountCallTetriNETCallback();
            IPlayer player1 = _factory.CreatePlayer(0, "player1", playerCallback1);
            IPlayer player2 = _factory.CreatePlayer(1, "player2", playerCallback2);
            CountCallTetriNETCallback spectatorCallback1 = new CountCallTetriNETCallback();
            CountCallTetriNETCallback spectatorCallback2 = new CountCallTetriNETCallback();
            ISpectator spectator1 = _factory.CreateSpectator(0, "spectator1", spectatorCallback1);
            ISpectator spectator2 = _factory.CreateSpectator(1, "spectator2", spectatorCallback2);
            _host.PlayerManager.Add(player1);
            _host.PlayerManager.Add(player2);
            _host.SpectatorManager.Add(spectator1);
            _host.SpectatorManager.Add(spectator2);
            server.WinList.Add(new WinEntry
            {
                PlayerName = "player1",
                Team = "team1",
                Score = 10
            });

            server.ResetWinList();

            Assert.AreEqual(server.WinList.Count, 0);
            Assert.AreEqual(playerCallback1.GetCallCount("OnPublishServerMessage"), 1);
            Assert.AreEqual(playerCallback2.GetCallCount("OnPublishServerMessage"), 1);
            Assert.AreEqual(playerCallback1.GetCallCount("OnWinListModified"), 1);
            Assert.AreEqual(playerCallback2.GetCallCount("OnWinListModified"), 1);
        }

        [TestMethod]
        public void TestGameActionNotQueuedWhenPauseIsActive()
        {
            Assert.Inconclusive("Irrelevant until game action queue can be stopped");

            IServer server = CreateServerWithHost();
            server.StartServer();
            IPlayer player1 = _factory.CreatePlayer(0, "player1", new CountCallTetriNETCallback());
            _host.PlayerManager.Add(player1);
            server.StartGame();
            server.PauseGame();

            _host.UseSpecial(player1.Callback, 0, Specials.NukeField);

            Assert.AreEqual(server.GameActionCount, 0); // TODO: disable game action queue before checking this
            Assert.AreEqual(_log.LastLogLevel, LogLevels.Warning);
            Assert.IsTrue(_log.LastLogLine.Contains("while game is not started"));
        }

        // TODO: every IHost event handlers  (how game actions and timeout could be mocked???)
    }
}
