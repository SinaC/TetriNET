using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Interfaces;
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
        private ActionQueueMock _actionQueue;

        private IServer CreateServer()
        {
            _factory = new FactoryMock();
            _actionQueue = _factory.CreateActionQueue() as ActionQueueMock;
            return new TetriNET.Server.Server(_factory.CreatePlayerManager(6), _factory.CreateSpectatorManager(10), _factory.CreatePieceProvider(), _actionQueue);
        }

        private IServer CreateServerWithHost()
        {
            _factory = new FactoryMock();
            IPlayerManager playerManager = _factory.CreatePlayerManager(6);
            ISpectatorManager spectatorManager = _factory.CreateSpectatorManager(10);
            IBanManager banManager = _factory.CreateBanManager();
            IPieceProvider pieceProvider = _factory.CreatePieceProvider();
            _host = new HostBaseMock(playerManager, spectatorManager, banManager, _factory);
            _actionQueue = _factory.CreateActionQueue() as ActionQueueMock;
            IServer server = new TetriNET.Server.Server(playerManager, spectatorManager, pieceProvider, _actionQueue);
            server.AddHost(_host);
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
            IPlayer player2 = _factory.CreatePlayer(1, "player2", new CountCallTetriNETCallback());
            _host.PlayerManager.Add(player1);
            _host.PlayerManager.Add(player2);
            server.StartGame(); // start a game to generate entries in WinList
            _host.GameLost(player1.Callback); // player has lost
            _actionQueue.Process(); // --> stop game because only one alive player left --> generate 2 entries in WinList 3 points for player2 and 2 points for player1
            server.StartGame(); // restart a game

            server.ResetWinList();

            Assert.AreEqual(server.WinList.Count, 2);
        }

        [TestMethod]
        public void TestResetWinListOkIfGameNotStarted()
        {
            IServer server = CreateServerWithHost();
            server.StartServer();
            IPlayer player1 = _factory.CreatePlayer(0, "player1", new CountCallTetriNETCallback());
            IPlayer player2 = _factory.CreatePlayer(1, "player2", new CountCallTetriNETCallback());
            _host.PlayerManager.Add(player1);
            _host.PlayerManager.Add(player2);
            server.StartGame();
            _host.GameLost(player1.Callback); // player has lost
            _actionQueue.Process(); // --> stop game because only one alive player left --> gene

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
            server.StartGame();
            _host.GameLost(player1.Callback); // player has lost
            _actionQueue.Process(); // --> stop game because only one alive player left --> gene

            server.ResetWinList();

            Assert.AreEqual(server.WinList.Count, 0);
            Assert.AreEqual(playerCallback1.GetCallCount("OnPublishServerMessage"), 1);
            Assert.AreEqual(playerCallback2.GetCallCount("OnPublishServerMessage"), 1);
            Assert.AreEqual(playerCallback1.GetCallCount("OnWinListModified"), 2);
            Assert.AreEqual(playerCallback2.GetCallCount("OnWinListModified"), 2);
        }

        [TestMethod]
        public void TestGameActionNotQueuedWhenPauseIsActive()
        {
            IServer server = CreateServerWithHost();
            server.StartServer();
            IPlayer player1 = _factory.CreatePlayer(0, "player1", new CountCallTetriNETCallback());
            _host.PlayerManager.Add(player1);
            server.StartGame();
            server.PauseGame();

            _host.UseSpecial(player1.Callback, 0, Specials.NukeField);

            Assert.AreEqual(_actionQueue.ActionCount, 0);
            Assert.AreEqual(_log.LastLogLevel, LogLevels.Warning);
            Assert.IsTrue(_log.LastLogLine.Contains("while game is not started"));
        }
        
        [TestMethod]
        public void TestGameActionQueuedWhenGameIsStarted()
        {
            IServer server = CreateServerWithHost();
            server.StartServer();
            IPlayer player1 = _factory.CreatePlayer(0, "player1", new CountCallTetriNETCallback());
            _host.PlayerManager.Add(player1);
            server.StartGame();

            _host.UseSpecial(player1.Callback, 0, Specials.NukeField);

            Assert.AreEqual(_actionQueue.ActionCount, 1);
        }

        // TODO: test every host event handlers
    }
}
