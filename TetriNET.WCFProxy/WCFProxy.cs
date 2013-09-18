using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using TetriNET.Common.Contracts;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Interfaces;
using TetriNET.Common.WCF;

namespace TetriNET.WCFProxy
{
    public sealed class WCFProxy : IProxy
    {
        private DuplexChannelFactory<IWCFTetriNET> _factory;
        private readonly IWCFTetriNET _proxy;

        public WCFProxy(ITetriNETCallback callback, string address)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");
            if (address == null)
                throw new ArgumentNullException("address");

            LastActionToServer = DateTime.Now;
            string address1 = address;

            // Get WCF endpoint
            EndpointAddress endpointAddress = null;
            if (String.IsNullOrEmpty(address1) || address1.ToLower() == "auto")
            {
                Logger.Log.WriteLine(Logger.Log.LogLevels.Debug, "Searching IWCFTetriNET server");
                List<EndpointAddress> endpointAddresses = DiscoveryHelper.DiscoverAddresses<IWCFTetriNET>();
                if (endpointAddresses != null && endpointAddresses.Any())
                {
                    foreach (EndpointAddress endpoint in endpointAddresses)
                        Logger.Log.WriteLine(Logger.Log.LogLevels.Debug, "{0}:\t{1}", endpointAddresses.IndexOf(endpoint), endpoint.Uri);
                    Logger.Log.WriteLine(Logger.Log.LogLevels.Debug, "Selecting first server");
                    endpointAddress = endpointAddresses[0];
                }
                else
                {
                    Logger.Log.WriteLine(Logger.Log.LogLevels.Debug, "No server found");
                }
            }
            else
                endpointAddress = new EndpointAddress(address1);

            // Create WCF proxy from endpoint
            if (endpointAddress != null)
            {
                Logger.Log.WriteLine(Logger.Log.LogLevels.Debug, "Connecting to server:{0}", endpointAddress.Uri);
                Binding binding = new NetTcpBinding(SecurityMode.None);
                InstanceContext instanceContext = new InstanceContext(callback);
                //_proxy = DuplexChannelFactory<IWCFTetriNET>.CreateChannel(instanceContext, binding, endpointAddress);
                _factory = new DuplexChannelFactory<IWCFTetriNET>(instanceContext, binding, endpointAddress);
                _proxy = _factory.CreateChannel(instanceContext);
            }
            else
                throw new Exception(String.Format("Server {0} not found", address));
        }

        public static List<string> DiscoverHosts()
        {
            Logger.Log.WriteLine(Logger.Log.LogLevels.Debug, "Searching IWCFTetriNET server");
            List<EndpointAddress> endpointAddresses = DiscoveryHelper.DiscoverAddresses<IWCFTetriNET>();
            if (endpointAddresses != null && endpointAddresses.Any())
            {
                foreach (EndpointAddress endpoint in endpointAddresses)
                    Logger.Log.WriteLine(Logger.Log.LogLevels.Debug, "{0}:\t{1}", endpointAddresses.IndexOf(endpoint), endpoint.Uri);
                Logger.Log.WriteLine(Logger.Log.LogLevels.Debug, "Selecting first server");

                return endpointAddresses.Select(x => x.Uri.ToString()).ToList();
            }
            else
            {
                Logger.Log.WriteLine(Logger.Log.LogLevels.Debug, "No server found");
                return null;
            }
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
                Logger.Log.WriteLine(Logger.Log.LogLevels.Error, "Exception:{0} {1}", actionName, ex);
                if (OnConnectionLost != null)
                    OnConnectionLost();
                if (_factory != null)
                    _factory.Abort();
            }
        }

        #region IProxy

        public DateTime LastActionToServer { get; private set; }

        public event ProxyConnectionLostHandler OnConnectionLost;

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
                Logger.Log.WriteLine(Logger.Log.LogLevels.Warning, "Exception:{0}", ex);
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
