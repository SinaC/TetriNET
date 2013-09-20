//using System;
//using System.Collections.Generic;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using TetriNET.Common;

//namespace TetriNET.Server
//{
//    public sealed class SocketHost : GenericHost
//    {
//        // store socket and handle send serialized message on socket
//        public class TetriNETSocketCallback : ITetriNETCallback
//        {
//            public const int BufferSize = 1024;

//            public Socket Socket { get; set; }
//            public byte[] Buffer = new byte[BufferSize];
//            public StringBuilder Message = new StringBuilder();

//            // TODO: serialize message and send on socket

//            #region ITetriNETCallback
//            public void OnPingReceived()
//            {
//                throw new NotImplementedException();
//            }

//            public void OnServerStopped()
//            {
//                throw new NotImplementedException();
//            }

//            public void OnPlayerRegistered(bool succeeded, int playerId)
//            {
//                throw new NotImplementedException();
//            }

//            public void OnGameStarted(Tetriminos firstTetrimino, Tetriminos secondTetrimino, List<PlayerData> players)
//            {
//                throw new NotImplementedException();
//            }

//            public void OnGameFinished()
//            {
//                throw new NotImplementedException();
//            }

//            public void OnServerAddLines(int lineCount)
//            {
//                throw new NotImplementedException();
//            }

//            public void OnPlayerAddLines(int lineCount)
//            {
//                throw new NotImplementedException();
//            }

//            public void OnPublishPlayerMessage(string playerName, string msg)
//            {
//                throw new NotImplementedException();
//            }

//            public void OnPublishServerMessage(string msg)
//            {
//                throw new NotImplementedException();
//            }

//            public void OnAttackReceived(Attacks attack)
//            {
//                throw new NotImplementedException();
//            }

//            public void OnAttackMessageReceived(string msg)
//            {
//                throw new NotImplementedException();
//            }

//            public void OnNextPiece(int index, Pieces piece)
//            {
//                throw new NotImplementedException();
//            }
//            #endregion
//        }

//        private class SocketServerHost // will create TetriNETSocketCallback and receive serialized message on socket
//        {
//            private readonly Dictionary<Socket, TetriNETSocketCallback> _connections = new Dictionary<Socket, TetriNETSocketCallback>();
//            private readonly IHost _host;
//            private Socket _serverSocket;

//            public int Port { get; set; }

//            public SocketServerHost(IHost host)
//            {
//                _host = host;
//            }

//            public void Start()
//            {
//                // Create listening socket
//                _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
//                _serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
//                IPEndPoint ipLocal = new IPEndPoint(IPAddress.Any, Port);
//                // Bind to local IP Address
//                _serverSocket.Bind(ipLocal);
//                // Start Listening
//                _serverSocket.Listen(1000);
//                // Creat callback to handle client connections
//                _serverSocket.BeginAccept(OnClientConnected, null);
//            }

//            public void Stop()
//            {
//                // TODO
//            }

//            private void OnClientConnected(IAsyncResult async)
//            {
//                // Get client socket
//                Socket socket = _serverSocket.EndAccept(async);

//                // Create state/callback object
//                TetriNETSocketCallback stateObject = new TetriNETSocketCallback
//                {
//                    Socket = socket,
//                };

//                // Add state object to active connections
//                _connections.Add(socket, stateObject);

//                // Release server socket to keep listening if limit is not reached
//                _serverSocket.BeginAccept(OnClientConnected, null);

//                // Allow connected client to receive data and designate a callback method
//                socket.BeginReceive(stateObject.Buffer, 0, TetriNETSocketCallback.BufferSize, 0, OnReceivedClientData, stateObject);
//            }

//            private void OnReceivedClientData(IAsyncResult async)
//            {
//                TetriNETSocketCallback stateObject = (TetriNETSocketCallback)async.AsyncState;

//                try
//                {
//                    // Complete async receive method and read data length
//                    int bytesRead = stateObject.Socket.EndReceive(async);
//                    if (bytesRead > 0)
//                    {
//                        stateObject.Message.Append(Encoding.ASCII.GetString(stateObject.Buffer, 0, bytesRead)); // TODO: non ascii
//                    }
//                    if (true /*TODO: check <EOF>*/)
//                    {
//                        // All data has been received
//                        // TODO: deserialize and call IHost method
//                        Console.WriteLine("Message received:[{0}]", stateObject.Message.ToString());

//                        stateObject.Message.Clear();
//                    }

//                    stateObject.Socket.BeginReceive(stateObject.Buffer, 0, TetriNETSocketCallback.BufferSize, 0, OnReceivedClientData, stateObject);
//                }
//                catch(SocketException ex)
//                {
//                }
//            }
//        }

//        private bool Started { get; set; }

//        private readonly SocketServerHost _serviceHost;

//        public int Port
//        {
//            get { return _serviceHost.Port; }
//            set { _serviceHost.Port = value; }
//        }

//        public SocketHost(IPlayerManager playerManager, Func<string, ITetriNETCallback, IPlayer> createPlayerFunc) : base(playerManager, createPlayerFunc)
//        {
//            _serviceHost = new SocketServerHost(this);

//            Started = false;
//        }

//        #region IHost
//        public override void Start()
//        {
//            if (Started)
//                return;

//            _serviceHost.Start();

//            Started = true;
//        }

//        public override void Stop()
//        {
//            if (!Started)
//                return;

//            // Inform players
//            foreach (IPlayer p in PlayerManager.Players)
//                p.OnServerStopped();

//            _serviceHost.Stop();

//            Started = false;
//        }
//        #endregion
//    }
//}
