using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using TetriNET.Common.Contracts;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Logger;
using TetriNET.Server.Interfaces;

namespace TetriNET.Server.TCPHost
{
    public sealed class TCPHost : GenericHost.GenericHost
    {
        public sealed class TetriNETTCPCallback : ITetriNETCallback
        {
            public const int BufferSize = 1024;

            public Socket Socket { get; set; }
            public byte[] Buffer = new byte[BufferSize];
            public StringBuilder Message = new StringBuilder();

            // TODO: serialize message and send on socket

            #region ITetriNETCallback

            public void OnHeartbeatReceived()
            {
                throw new NotImplementedException();
            }

            public void OnServerStopped()
            {
                throw new NotImplementedException();
            }

            public void OnPlayerRegistered(RegistrationResults result, int playerId, bool gameStarted, bool isServerMaster, GameOptions options)
            {
                throw new NotImplementedException();
            }

            public void OnPlayerJoined(int playerId, string name, string team)
            {
                throw new NotImplementedException();
            }

            public void OnPlayerLeft(int playerId, string name, LeaveReasons reason)
            {
                throw new NotImplementedException();
            }

            public void OnPlayerTeamChanged(int playerId, string team)
            {
                throw new NotImplementedException();
            }

            public void OnPublishPlayerMessage(string playerName, string msg)
            {
                throw new NotImplementedException();
            }

            public void OnPublishServerMessage(string msg)
            {
                throw new NotImplementedException();
            }

            public void OnPlayerLost(int playerId)
            {
                throw new NotImplementedException();
            }

            public void OnPlayerWon(int playerId)
            {
                throw new NotImplementedException();
            }

            public void OnGameStarted(List<Pieces> pieces)
            {
                throw new NotImplementedException();
            }

            public void OnGameFinished(GameStatistics statistics)
            {
                throw new NotImplementedException();
            }

            public void OnGamePaused()
            {
                throw new NotImplementedException();
            }

            public void OnGameResumed()
            {
                throw new NotImplementedException();
            }

            public void OnServerAddLines(int lineCount)
            {
                throw new NotImplementedException();
            }

            public void OnPlayerAddLines(int specialId, int playerId, int lineCount)
            {
                throw new NotImplementedException();
            }

            public void OnSpecialUsed(int specialId, int playerId, int targetId, Specials special)
            {
                throw new NotImplementedException();
            }

            public void OnNextPiece(int firstIndex, List<Pieces> piece)
            {
                throw new NotImplementedException();
            }

            public void OnGridModified(int playerId, byte[] grid)
            {
                throw new NotImplementedException();
            }

            public void OnServerMasterChanged(int playerId)
            {
                throw new NotImplementedException();
            }

            public void OnWinListModified(List<WinEntry> winList)
            {
                throw new NotImplementedException();
            }

            public void OnContinuousSpecialFinished(int playerId, Specials special)
            {
                throw new NotImplementedException();
            }

            public void OnAchievementEarned(int playerId, int achievementId, string achievementTitle)
            {
                throw new NotImplementedException();
            }

            public void OnOptionsChanged(GameOptions options)
            {
                throw new NotImplementedException();
            }

            public void OnSpectatorRegistered(RegistrationResults result, int spectatorId, bool gameStarted, GameOptions options)
            {
                throw new NotImplementedException();
            }

            public void OnSpectatorJoined(int spectatorId, string name)
            {
                throw new NotImplementedException();
            }

            public void OnSpectatorLeft(int spectatorId, string name, LeaveReasons reason)
            {
                throw new NotImplementedException();
            }

            #endregion
        }

        public sealed class SocketServiceHost : ITetriNET, ITetriNETSpectator
        {
            private readonly IHost _host;
            private readonly Dictionary<Socket, TetriNETTCPCallback> _connections = new Dictionary<Socket, TetriNETTCPCallback>();
            private Socket _serverSocket;

            public int Port { get; set; }

            public SocketServiceHost(IHost host)
            {
                if (host == null)
                    throw new ArgumentNullException("host");

                _host = host;
            }

            public void Start()
            {
                // Create listening socket
                //_serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                IPEndPoint ipLocal = new IPEndPoint(IPAddress.Any, Port);
                // Bind to local IP Address
                _serverSocket.Bind(ipLocal);
                // Start Listening
                _serverSocket.Listen(1000);
                // Creat callback to handle client connections
                _serverSocket.BeginAccept(OnClientConnected, null);
            }

            public void Stop()
            {
            }

            private void OnClientConnected(IAsyncResult async)
            {
                // Get client socket
                Socket socket = _serverSocket.EndAccept(async);

                // Create state/callback object
                TetriNETTCPCallback stateObject = new TetriNETTCPCallback
                {
                    Socket = socket,
                };

                // Add state object to active connections
                _connections.Add(socket, stateObject);

                // Release server socket to keep listening if limit is not reached
                _serverSocket.BeginAccept(OnClientConnected, null);

                // Allow connected client to receive data and designate a callback method
                socket.BeginReceive(stateObject.Buffer, 0, TetriNETTCPCallback.BufferSize, 0, OnReceivedClientData, stateObject);
            }

            private void OnReceivedClientData(IAsyncResult async)
            {
                TetriNETTCPCallback stateObject = (TetriNETTCPCallback)async.AsyncState;

                try
                {
                    //DataContractSerializer serializer = new DataContractSerializer();

                    // Complete async receive method and read data length
                    int bytesRead = stateObject.Socket.EndReceive(async);

                    if (stateObject.Socket.Connected)
                    {
                        if (bytesRead > 0)
                        {
                            stateObject.Message.Append(Encoding.ASCII.GetString(stateObject.Buffer, 0, bytesRead)); // TODO: non ascii

                            if (true /*TODO: parse string/buffer and check <EOF>*/)
                            {
                                // All data has been received
                                // TODO: deserialize and call IHost method
                                Console.WriteLine("OnReceivedClientData:[{0}]", stateObject.Message);

                                stateObject.Message.Clear();
                            }

                            stateObject.Socket.BeginReceive(stateObject.Buffer, 0, TetriNETTCPCallback.BufferSize, 0, OnReceivedClientData, stateObject);
                        }
                        else
                            OnClientDisconnected(stateObject.Socket);
                    }
                    else
                        OnClientDisconnected(stateObject.Socket);
                }
                catch (SocketException ex)
                {
                    Log.Default.WriteLine(LogLevels.Error, "OnReceivedClientData: SocketException: {0}", ex);
                    OnClientDisconnected(stateObject.Socket);
                }
                catch(Exception ex)
                {
                    Log.Default.WriteLine(LogLevels.Error, "OnReceivedClientData: Exception: {0}", ex);
                    OnClientDisconnected(stateObject.Socket);
                }
            }

            private void OnClientDisconnected(Socket socket)
            {
                if (socket == null)
                {
                    Log.Default.WriteLine(LogLevels.Error, "OnClientDisconnected: client socket is null, cannot remove it from connected client collection");
                }
                else if (!_connections.ContainsKey(socket))
                {
                    Log.Default.WriteLine(LogLevels.Error, "OnClientDisconnected: client already removed from connected client collection");
                }
                else
                {
                    Log.Default.WriteLine(LogLevels.Info, "Client disconnected");
                    // Remove from collection
                    _connections.Remove(socket);
                    // Shutdown socket
                    socket.Shutdown(SocketShutdown.Both);
                    // Close socket
                    socket.Close();
                }
            }

            #region ITetriNET

            public void RegisterPlayer(ITetriNETCallback callback, string playerName, string team)
            {
                _host.RegisterPlayer(callback, playerName, team);
            }

            public void UnregisterPlayer(ITetriNETCallback callback)
            {
                _host.UnregisterPlayer(callback);
            }

            public void Heartbeat(ITetriNETCallback callback)
            {
                _host.Heartbeat(callback);
            }

            public void PlayerTeam(ITetriNETCallback callback, string team)
            {
                _host.PlayerTeam(callback, team);
            }

            public void PublishMessage(ITetriNETCallback callback, string msg)
            {
                _host.PublishMessage(callback, msg);
            }

            public void PlacePiece(ITetriNETCallback callback, int pieceIndex, int highestIndex, Pieces piece, int orientation, int posX, int posY, byte[] grid)
            {
                _host.PlacePiece(callback, pieceIndex, highestIndex, piece, orientation, posX, posY, grid);
            }

            public void ModifyGrid(ITetriNETCallback callback, byte[] grid)
            {
                _host.ModifyGrid(callback, grid);
            }

            public void UseSpecial(ITetriNETCallback callback, int targetId, Specials special)
            {
                _host.UseSpecial(callback, targetId, special);
            }

            public void ClearLines(ITetriNETCallback callback, int count)
            {
                _host.ClearLines(callback, count);
            }

            public void GameLost(ITetriNETCallback callback)
            {
                _host.GameLost(callback);
            }

            public void FinishContinuousSpecial(ITetriNETCallback callback, Specials special)
            {
                _host.FinishContinuousSpecial(callback, special);
            }

            public void EarnAchievement(ITetriNETCallback callback, int achievementId, string achievementTitle)
            {
                _host.EarnAchievement(callback, achievementId, achievementTitle);
            }

            public void StartGame(ITetriNETCallback callback)
            {
                _host.StartGame(callback);
            }

            public void StopGame(ITetriNETCallback callback)
            {
                _host.StopGame(callback);
            }

            public void PauseGame(ITetriNETCallback callback)
            {
                _host.PauseGame(callback);
            }

            public void ResumeGame(ITetriNETCallback callback)
            {
                _host.ResumeGame(callback);
            }

            public void ChangeOptions(ITetriNETCallback callback, GameOptions options)
            {
                _host.ChangeOptions(callback, options);
            }

            public void KickPlayer(ITetriNETCallback callback, int playerId)
            {
                _host.KickPlayer(callback, playerId);
            }
            public void BanPlayer(ITetriNETCallback callback, int playerId)
            {
                _host.BanPlayer(callback, playerId);
            }

            public void ResetWinList(ITetriNETCallback callback)
            {
                _host.ResetWinList(callback);
            }

            #endregion

            #region ITetriNETSpectator

            public void RegisterSpectator(ITetriNETCallback callback, string spectatorName)
            {
                _host.RegisterSpectator(callback, spectatorName);
            }

            public void UnregisterSpectator(ITetriNETCallback callback)
            {
                _host.UnregisterSpectator(callback);
            }

            public void HeartbeatSpectator(ITetriNETCallback callback)
            {
                _host.HeartbeatSpectator(callback);
            }

            public void PublishSpectatorMessage(ITetriNETCallback callback, string msg)
            {
                _host.PublishSpectatorMessage(callback, msg);
            }

            #endregion
        }

        private bool _started;

        private readonly SocketServiceHost _serviceHost;

        public int Port
        {
            get { return _serviceHost.Port; }
            set { _serviceHost.Port = value; }
        }

        public TCPHost(IPlayerManager playerManager, ISpectatorManager spectatorManager, IBanManager banManager, IFactory factory)
            : base(playerManager, spectatorManager, banManager, factory)
        {
            _serviceHost = new SocketServiceHost(this);
        }

        #region IHost

        public override void Start()
        {
            if (_started)
                return;

            _serviceHost.Start();

            _started = true;
        }

        public override void Stop()
        {
            if (!_started)
                return;

            _serviceHost.Stop();

            _started = false;
        }

        public override void RemovePlayer(IPlayer player)
        {
            // NOP
        }

        public override void RemoveSpectator(ISpectator spectator)
        {
            // NOP
        }

        #endregion
    }
}
