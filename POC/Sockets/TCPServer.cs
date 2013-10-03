using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TetriNET.Common.Contracts;
using TetriNET.Common.Logger;

namespace POC.Sockets
{
    public class TCPServer
    {
        private class ClientData
        {
            public ITetriNETCallback Callback;
            public TcpClient TCPClient;
            public Task ReadTask;
        }

        private readonly ManualResetEvent _stopEvent = new ManualResetEvent(false);
        private readonly List<ClientData> _clients = new List<ClientData>();
        private Task _acceptClientTask;
        private TcpListener _serverSocket;

        public int Port { get; set; }

        public void Start()
        {
            // Reset stop event
            _stopEvent.Reset();

            // Create server socket
            _serverSocket = new TcpListener(IPAddress.Any, Port);
            _serverSocket.Start();

            // Start accept task
            _acceptClientTask = Task.Factory.StartNew(
                TaskAcceptClient
            );
        }

        public void Stop()
        {
            // Raise stop event
            _stopEvent.Set();

            // Stop accept task
            _acceptClientTask.Wait();

            // Stop server socket
            _serverSocket.Stop();
        }

        private void TaskAcceptClient()
        {
            while(true)
            {
                try
                {
                    TcpClient client = _serverSocket.AcceptTcpClient();
                    // TODO: event ?
                    ClientData clientData = new ClientData
                        {
                            //Callback =  TODO
                            TCPClient = client,
                            //ReadTask = 
                        };
                    clientData.ReadTask = Task.Factory.StartNew(
                        () => TaskReadClient(clientData)
                    );
                    // Add to clients
                    lock (_clients)
                        _clients.Add(clientData);

                    if (_stopEvent.WaitOne(100))
                        break;
                }
                catch(Exception ex)
                {
                    // TODO
                    Log.WriteLine(Log.LogLevels.Error, "Exception: {0}", ex.ToString());
                    break;
                }
            }
        }

        private void TaskReadClient(ClientData clientData)
        {
            string clientEndPoint = clientData.TCPClient.Client.RemoteEndPoint.ToString();

            byte[] buffer = new byte[1024];

            while(true)
            {
                int bytesRead;
                try
                {
                    NetworkStream clientStream = clientData.TCPClient.GetStream();
                    bytesRead = clientStream.Read(buffer, 0, buffer.Length);
                }
                catch (Exception ex)
                {
                    Log.WriteLine(Log.LogLevels.Error, "[{0}] Exception: {1}", clientEndPoint, ex.ToString());
                    break;
                }

                if (bytesRead == 0)
                {
                    Log.WriteLine(Log.LogLevels.Error, "[{0}] client disconnected", clientEndPoint);
                    // TODO: event ?
                    break;
                }

                // TODO: build message from buffer
                // In following comments, whole message may be read as multiple messages
                // buffer may be a whole message
                // buffer may be the beginning of a message
                // buffer may be the end of a message
                // buffer may be the end of a message + a whole message
                // buffer may be a whole message + the beginning of a message
                // buffer may be the end of a message + a whole message + the beginning of a message
                
                // shortcut: we consider we receive a whole message
                string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                Log.WriteLine(Log.LogLevels.Error, "[{0}]:{1}", clientEndPoint, message);
            }
        }
    }
}
