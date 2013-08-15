using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace POC.Sockets
{

    public class StateObject
    {
        public Socket socket;
        public int id;
        public byte[] buffer = new byte[BufferSize];
        public const int BufferSize = 1024;
        public StringBuilder sb = new StringBuilder(1024);
    }

    public class SocketConnectArgs
    {
        public IPAddress ConnectedIP;
        public int clientID;
    }

    public class SocketMessageReceivedArgs
    {
        public string MessageContent;
        public int clientID;
    }

    public class SocketEventArgs
    {
        public int clientID;
    }

    public class TCPSocketServer : IDisposable
    {
        // Or whatever the name.
        private const int MaxLengthOfPendingConnectionsQueue = 1000;

        private int portNumber;
        private int connectionsLimit;
        private Socket connectionSocket;
        private Dictionary<int, StateObject> connectedClients = new Dictionary<int, StateObject>();

        public event SocketConnectedHandler ClientConnected;

        public delegate void SocketConnectedHandler(TCPSocketServer socketServer, SocketConnectArgs e);

        public event SocketMessageReceivedHandler MessageReceived;

        public delegate void SocketMessageReceivedHandler(TCPSocketServer socketServer, SocketMessageReceivedArgs e);

        public event SocketClosedHandler ClientDisconnected;

        public delegate void SocketClosedHandler(TCPSocketServer socketServer, SocketEventArgs e);

        #region Constructors

        public TCPSocketServer(int portNumber, int connectionsLimit = 0)
        {
            this.portNumber = portNumber;
            this.connectionsLimit = connectionsLimit;
            startListening();
        }

        #endregion

        private StateObject GetClient(int clientId)
        {
            StateObject client;
            if (!connectedClients.TryGetValue(clientId, out client))
            {
                return null;
            }
            return client;
        }

        #region Send Messages

        public void SendMessage(string messageToSend, int clientID)
        {
            byte[] data = System.Text.Encoding.UTF8.GetBytes(messageToSend + "\0");
            SendMessage(data, clientID);
        }

        public void SendMessage(byte[] messageToSend, int clientID)
        {
            StateObject client = GetClient(clientID);
            if (client != null)
            {
                try
                {
                    if (client.socket.Connected)
                    {
                        client.socket.Send(messageToSend);
                    }
                }
                catch (SocketException)
                {
                    // TODO: sending failed; disconnect from client, or?
                }
            }
        }

        #endregion

        #region Connection and Listening

        private void startListening()
        {
            try
            {
                // Create listening socket
                connectionSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                connectionSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                IPEndPoint ipLocal = new IPEndPoint(IPAddress.Any, this.portNumber);
                // Bind to local IP Address
                connectionSocket.Bind(ipLocal);
                // Start Listening
                connectionSocket.Listen(MaxLengthOfPendingConnectionsQueue);
                // Creat callback to handle client connections
                connectionSocket.BeginAccept(new AsyncCallback(onClientConnect), null);
            }
            catch (SocketException)
            {
                // TODO: if we fail to start listening, is it even ok to continue?
                // Consider that some of the bootstrapping actions might not even have been done.
                // Thus execution will likely crash on next step.
            }
        }

        private void onClientConnect(IAsyncResult asyn)
        {
            try
            {
                Socket socket = connectionSocket.EndAccept(asyn);

                // TODO: consider if we can instead do this at the beginning of the method.
                // Check against limit
                if (connectedClients.Count > connectionsLimit)
                {
                    // No connection event is sent so close socket silently
                    socket.Close();
                    socket.Dispose();
                    return;
                }

                int id = !connectedClients.Any() ? 1 : connectedClients.Keys.Max() + 1;
                // Create a new StateObject to hold the connected client
                StateObject connectedClient = new StateObject()
                {
                    socket = socket,
                    id = id
                };

                connectedClients.Add(connectedClient.id, connectedClient);

                // Dispatch Event
                if (ClientConnected != null)
                {
                    SocketConnectArgs args = new SocketConnectArgs()
                    {
                        ConnectedIP = IPAddress.Parse(((IPEndPoint) connectedClient.socket.RemoteEndPoint).Address.ToString()),
                        clientID = connectedClient.id
                    };
                    ClientConnected(this, args);
                }

                // Release connectionSocket to keep listening if limit is not reached
                connectionSocket.BeginAccept(new AsyncCallback(onClientConnect), null);

                // Allow connected client to receive data and designate a callback method
                connectedClient.socket.BeginReceive(connectedClient.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(onReceivedClientData), connectedClient);
            }
            catch (SocketException)
            {
                // TODO: should we closeSocketSilent()? Or?
            }
            catch (ObjectDisposedException)
            {
                // TODO: should we closeSocketSilent()? Or?
            }
        }

        private void onReceivedClientData(IAsyncResult asyn)
        {
            // Receive stateobject of the client that sent data
            StateObject dataSender = (StateObject) asyn.AsyncState;

            try
            {
                // Complete aysnc receive method and read data length
                int bytesRead = dataSender.socket.EndReceive(asyn);

                if (bytesRead > 0)
                {
                    // More data could be sent so append data received so far
                    dataSender.sb.Append(Encoding.UTF8.GetString(dataSender.buffer, 0, bytesRead));
                    if (dataSender.sb.Length != 0
                        && MessageReceived != null
                        )
                    {
                        // TODO: is it possible that multiple messages are in the sb?
                        // Consider whether it's necessary to replace with newline.
                        dataSender.sb.Replace("\0", null); // Removes them.

                        // Dispatch Event
                        SocketMessageReceivedArgs args = new SocketMessageReceivedArgs();
                        args.MessageContent = dataSender.sb.ToString();
                        args.clientID = dataSender.id;
                        MessageReceived(this, args);

                        dataSender.sb.Clear();
                    }
                    try
                    {
                        dataSender.socket.BeginReceive(dataSender.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(this.onReceivedClientData), dataSender);
                    }
                    catch (SocketException)
                    {
                    }
                }
                else
                {
                    closeSocket(dataSender.id);
                }
            }
            catch (SocketException)
            {
                // TODO: should we closeSocketSilent()? Or?
            }
            catch (ObjectDisposedException)
            {
                // TODO: should we closeSocketSilent()? Or?
            }
        }

        #endregion

        #region Socket Closing

        public void closeSocket(int socketID)
        {
            closeSocket(socketID, false);
        }

        /// <param name="silent">Whether to skip dispatching the disconnection event. Used to cancel the bootstrapping of the client-server connection.</param>
        private void closeSocket(int socketID, bool silent)
        {
            StateObject client = GetClient(socketID);
            if (client == null)
            {
                return;
            }
            try
            {
                client.socket.Close();
                client.socket.Dispose();

                if (!silent)
                {
                    // Dispatch Event
                    if (ClientDisconnected != null)
                    {
                        SocketEventArgs args = new SocketEventArgs();
                        args.clientID = client.id;
                        ClientDisconnected(this, args);
                    }
                }
                // Moved to finnaly block: connectedClients.Remove(client.id);
            }
            catch (SocketException)
            {
                // Don't care. Or?
            }
            finally
            {
                connectedClients.Remove(client.id);
            }
        }

        public void closeAllSockets()
        {
            var keys = connectedClients.Keys;
            foreach (int key in keys)
            {
                var client = connectedClients[key];
                closeSocket(client.id);
            }
        }

        #endregion

        public void Dispose()
        {
            ClientConnected = null;
            ClientDisconnected = null;
            MessageReceived = null;

            connectionSocket.Close();
        }
    }
}