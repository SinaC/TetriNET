using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TetriNET.Client.Interfaces;
using TetriNET.Common.Contracts;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Logger;

namespace TetriNET.Client.TCPProxy
{
    public class TCPProxy : IProxy
    {
        private readonly Task _sendTask;
        private readonly Socket _socket;
        private readonly Queue<byte[]> _packetsToSend;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly ManualResetEvent _packetToSendEvent;
        private volatile bool _readyToSend;

        public int Port { get; set; }

        public TCPProxy(ITetriNETCallback callback, string address)
        {
            _readyToSend = false;
            _packetToSendEvent = new ManualResetEvent(false);

            IPAddress ip = IPAddress.Parse(address);
            IPEndPoint endpoint = new IPEndPoint(ip, Port);

            _packetsToSend = new Queue<byte[]>();

            _cancellationTokenSource = new CancellationTokenSource();
            _sendTask = new Task(SendTask, _cancellationTokenSource.Token);

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.BeginConnect(endpoint, OnConnectedToServer, null);
        }

        private void Send(string data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Enqueue data to send
            lock (_packetsToSend)
                _packetsToSend.Enqueue(byteData);

            _packetToSendEvent.Set();
        }
        
        private void OnConnectedToServer(IAsyncResult ar)
        {
            try
            {
                // Complete the connection.
                _socket.EndConnect(ar);

                //
                Log.WriteLine(Log.LogLevels.Info, "Socket connected to {0}", _socket.RemoteEndPoint);

                // Ready to send
                _readyToSend = true;
            }
            catch (Exception e)
            {
                Log.WriteLine(Log.LogLevels.Error, "OnConnectedToServer: {0}", e);
            }
        }

        private void OnSendCompleted(IAsyncResult ar)
        {
            try
            {
                // Complete sending the data to the remote device.
                int bytesSent = _socket.EndSend(ar);

                //
                Log.WriteLine(Log.LogLevels.Info, "Sent {0} bytes to server.", bytesSent);

                // Ready to send
                _readyToSend = false;
            }
            catch (SocketException e)
            {
                // TODO: handle exception
                Log.WriteLine(Log.LogLevels.Error, "OnSendCompleted: SocketException: {0}", e);
            }
            catch (Exception e)
            {
                // TODO: handle exception
                Log.WriteLine(Log.LogLevels.Error, "OnSendCompleted: Exception: {0}", e);
            }
        }

        private void SendTask()
        {
            while (true)
            {
                // Asked to stop ?
                if (_cancellationTokenSource.IsCancellationRequested)
                    break;

                if (_readyToSend)
                {
                    // Has something to send ?
                    bool isEmpty;
                    lock (_packetsToSend)
                        isEmpty = _packetsToSend.Count == 0;
                    if (!isEmpty)
                    {
                        _readyToSend = false;
                        byte[] byteData;
                        lock (_packetsToSend)
                            byteData = _packetsToSend.Dequeue();
                        // Begin sending the data to the remote device.
                        _socket.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, OnSendCompleted, null);
                    }

                    _packetToSendEvent.WaitOne(100);
                    _packetToSendEvent.Reset();
                }
            }
        }

        #region IProxy

        public void RegisterPlayer(ITetriNETCallback callback, string playerName, string team)
        {
            throw new NotImplementedException();
        }
        
        public void UnregisterPlayer(ITetriNETCallback callback)
        {
            throw new NotImplementedException();
        }
        
        public void Heartbeat(ITetriNETCallback callback)
        {
            throw new NotImplementedException();
        }
        
        public void PlayerTeam(ITetriNETCallback callback, string team)
        {
            throw new NotImplementedException();
        }
        
        public void PublishMessage(ITetriNETCallback callback, string msg)
        {
            throw new NotImplementedException();
        }
        
        public void PlacePiece(ITetriNETCallback callback, int pieceIndex, int highestIndex, Pieces piece, int orientation, int posX, int posY, byte[] grid)
        {
            throw new NotImplementedException();
        }
        
        public void ModifyGrid(ITetriNETCallback callback, byte[] grid)
        {
            throw new NotImplementedException();
        }
        
        public void UseSpecial(ITetriNETCallback callback, int targetId, Specials special)
        {
            throw new NotImplementedException();
        }
        
        //public void SendLines(ITetriNETCallback callback, int count)
        //{
        //    throw new NotImplementedException();
        //}

        public void ClearLines(ITetriNETCallback callback, int count)
        {
            throw new NotImplementedException();
        }
        
        public void GameLost(ITetriNETCallback callback)
        {
            throw new NotImplementedException();
        }
        
        public void FinishContinuousSpecial(ITetriNETCallback callback, Specials special)
        {
            throw new NotImplementedException();
        }
        
        public void EarnAchievement(ITetriNETCallback callback, int achievementId, string achievementTitle)
        {
            throw new NotImplementedException();
        }
        
        public void StartGame(ITetriNETCallback callback)
        {
            throw new NotImplementedException();
        }
        
        public void StopGame(ITetriNETCallback callback)
        {
            throw new NotImplementedException();
        }
        
        public void PauseGame(ITetriNETCallback callback)
        {
            throw new NotImplementedException();
        }
        
        public void ResumeGame(ITetriNETCallback callback)
        {
            throw new NotImplementedException();
        }
        
        public void ChangeOptions(ITetriNETCallback callback, GameOptions options)
        {
            throw new NotImplementedException();
        }
        
        public void KickPlayer(ITetriNETCallback callback, int playerId)
        {
            throw new NotImplementedException();
        }
        public void BanPlayer(ITetriNETCallback callback, int playerId)
        {
            throw new NotImplementedException();
        }
        
        public void ResetWinList(ITetriNETCallback callback)
        {
            throw new NotImplementedException();
        }
        
        public DateTime LastActionToServer { get; private set; }
        
        public event ProxyConnectionLostEventHandler ConnectionLost;
        
        public bool Disconnect()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
