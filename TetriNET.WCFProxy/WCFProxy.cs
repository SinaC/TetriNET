using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using TetriNET.Common.Contracts;
using TetriNET.Common.GameDatas;
using TetriNET.Common.Interfaces;
using TetriNET.Common.WCF;

namespace TetriNET.WCFProxy
{
    public sealed class WCFProxy : IProxy
    {
        private readonly string _address;
        private readonly ITetriNETCallback _callback;

        private DuplexChannelFactory<IWCFTetriNET> _factory;
        private IWCFTetriNET _proxy;

        public DateTime LastActionToServer { get; private set; }

        public WCFProxy(ITetriNETCallback callback, string address)
        {
            LastActionToServer = DateTime.Now;
            _callback = callback;
            _address = address;
        }

        private void ExceptionFreeAction(Action action, string actionName)
        {
            try
            {
                action();
                LastActionToServer = DateTime.Now;
            }
            catch (Exception ex)
            {
                Log.Log.WriteLine(Log.Log.LogLevels.Error, "Exception:{0} {1}", actionName, ex);
                if (OnConnectionLost != null)
                    OnConnectionLost();
                if (_factory != null)
                    _factory.Abort();
            }
        }

        #region IProxy

        public event ProxyConnectionLostHandler OnConnectionLost;

        public bool Connect()
        {
            if (_factory != null)
                return false; // should disconnect first
            // Get WCF endpoint
            EndpointAddress endpointAddress = null;
            if (String.IsNullOrEmpty(_address) || _address.ToLower() == "auto")
            {
                Log.Log.WriteLine(Log.Log.LogLevels.Debug, "Searching IWCFTetriNET server");
                List<EndpointAddress> endpointAddresses = DiscoveryHelper.DiscoverAddresses<IWCFTetriNET>();
                if (endpointAddresses != null && endpointAddresses.Any())
                {
                    foreach (EndpointAddress endpoint in endpointAddresses)
                        Log.Log.WriteLine(Log.Log.LogLevels.Debug, "{0}:\t{1}", endpointAddresses.IndexOf(endpoint), endpoint.Uri);
                    Log.Log.WriteLine(Log.Log.LogLevels.Debug, "Selecting first server");
                    endpointAddress = endpointAddresses[0];
                }
                else
                {
                    Log.Log.WriteLine(Log.Log.LogLevels.Debug, "No server found");
                }
            }
            else
                endpointAddress = new EndpointAddress(_address);

            // Create WCF proxy from endpoint
            if (endpointAddress != null)
            {
                Log.Log.WriteLine(Log.Log.LogLevels.Debug, "Connecting to server:{0}", endpointAddress.Uri);
                Binding binding = new NetTcpBinding(SecurityMode.None);
                InstanceContext instanceContext = new InstanceContext(_callback);
                //_proxy = DuplexChannelFactory<IWCFTetriNET>.CreateChannel(instanceContext, binding, endpointAddress);
                _factory = new DuplexChannelFactory<IWCFTetriNET>(instanceContext, binding, endpointAddress);
                _proxy = _factory.CreateChannel(instanceContext);
                return true;
            }
            return false;
        }

        public bool Disconnect()
        {
            if (_factory == null)
                return false; // should connect first
            try
            {
                _factory.Close();
            }
            catch (Exception ex)
            {
                Log.Log.WriteLine(Log.Log.LogLevels.Error, "Exception:{1}", ex);
                _factory.Abort();
            }
            _factory = null;
            return true;
        }

        #endregion

        #region ITetriNET

        public void RegisterPlayer(ITetriNETCallback callback, string playerName)
        {
            ExceptionFreeAction(() => _proxy.RegisterPlayer(playerName), "RegisterPlayer");
        }

        public void UnregisterPlayer(ITetriNETCallback callback)
        {
            ExceptionFreeAction(_proxy.UnregisterPlayer, "UnregisterPlayer");
        }

        public void Heartbeat(ITetriNETCallback callback)
        {
            ExceptionFreeAction(_proxy.Heartbeat, "Heartbeat");
        }

        public void PublishMessage(ITetriNETCallback callback, string msg)
        {
            ExceptionFreeAction(() => _proxy.PublishMessage(msg), "PublishMessage");
        }

        public void PlaceTetrimino(ITetriNETCallback callback, int index, Tetriminos tetrimino, int orientation, int posX, int posY, byte[] grid)
        {
            ExceptionFreeAction(() => _proxy.PlaceTetrimino(index, tetrimino, orientation, posX, posY, grid), "PlaceTetrimino");
        }

        public void ModifyGrid(ITetriNETCallback callback, byte[] grid)
        {
            ExceptionFreeAction(() => _proxy.ModifyGrid(grid), "ModifyGrid");
        }

        public void UseSpecial(ITetriNETCallback callback, int targetId, Specials special)
        {
            ExceptionFreeAction(() => _proxy.UseSpecial(targetId, special), "UseSpecial");
        }

        public void SendLines(ITetriNETCallback callback, int count)
        {
            ExceptionFreeAction(() => _proxy.SendLines(count), "SendLines");
        }

        public void StartGame(ITetriNETCallback callback)
        {
            ExceptionFreeAction(_proxy.StartGame, "StartGame");
        }

        public void StopGame(ITetriNETCallback callback)
        {
            ExceptionFreeAction(_proxy.StopGame, "StopGame");
        }

        public void PauseGame(ITetriNETCallback callback)
        {
            ExceptionFreeAction(_proxy.PauseGame, "PauseGame");
        }

        public void ResumeGame(ITetriNETCallback callback)
        {
            ExceptionFreeAction(_proxy.ResumeGame, "ResumeGame");
        }

        public void GameLost(ITetriNETCallback callback)
        {
            ExceptionFreeAction(_proxy.GameLost, "ResumeGame");
        }

        public void ChangeOptions(ITetriNETCallback callback, GameOptions options)
        {
            ExceptionFreeAction(() => _proxy.ChangeOptions(options), "ChangeOptions");
        }

        public void KickPlayer(ITetriNETCallback callback, int playerId)
        {
            ExceptionFreeAction(() => _proxy.KickPlayer(playerId), "KickPlayer");
        }
        public void BanPlayer(ITetriNETCallback callback, int playerId)
        {
            ExceptionFreeAction(() => _proxy.BanPlayer(playerId), "BanPlayer");
        }

        public void ResetWinList(ITetriNETCallback callback)
        {
            ExceptionFreeAction(_proxy.ResetWinList, "ResetWinList");
        }

        #endregion
    }
}
