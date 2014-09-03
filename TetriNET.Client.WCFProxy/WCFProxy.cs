using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.ServiceModel;
using System.ServiceModel.Channels;
using ServiceModelEx;
using TetriNET.Client.Interfaces;
using TetriNET.Common.Contracts;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Helpers;
using TetriNET.Common.Logger;

namespace TetriNET.Client.WCFProxy
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

            // Get WCF endpoint
            EndpointAddress endpointAddress = null;
            if (String.IsNullOrEmpty(address) || address.ToLower() == "auto")
            {
                List<EndpointAddress> addresses = DiscoverEndpoints().ToList();
                if (addresses.Any())
                    endpointAddress = addresses[0];
            }
            else
                endpointAddress = new EndpointAddress(address);

            // Create WCF proxy from endpoint
            if (endpointAddress != null)
            {
                Log.Default.WriteLine(LogLevels.Debug, "Connecting to server:{0}", endpointAddress.Uri);
                Binding binding = new NetTcpBinding(SecurityMode.None);
                InstanceContext instanceContext = new InstanceContext(callback);
                _factory = new DuplexChannelFactory<IWCFTetriNET>(instanceContext, binding, endpointAddress);
                _proxy = _factory.CreateChannel(instanceContext);
            }
            else
                throw new Exception(String.Format("Server {0} not found", address));
        }

        public static List<string> DiscoverHosts() // TODO: endpoint + version
        {
            IEnumerable<EndpointAddress> addresses = DiscoverEndpoints();
            return addresses.Select(x => x.Uri.ToString()).ToList();
        }

        private static IEnumerable<EndpointAddress> DiscoverEndpoints()
        {
            Log.Default.WriteLine(LogLevels.Debug, "Searching IWCFTetriNET server");
            EndpointAddress[] endpointAddresses = DiscoveryHelper.DiscoverAddresses<IWCFTetriNET>();
            if (endpointAddresses != null && endpointAddresses.Any())
            {
                foreach (EndpointAddress endpoint in endpointAddresses)
                    Log.Default.WriteLine(LogLevels.Debug, "{0}:\t{1}", Array.IndexOf(endpointAddresses, endpoint), endpoint.Uri);
                Log.Default.WriteLine(LogLevels.Debug, "Selecting first server");

                return endpointAddresses;
            }
            else
            {
                Log.Default.WriteLine(LogLevels.Debug, "No server found");
                return Enumerable.Empty<EndpointAddress>();
            }
        }

        private void ExceptionFreeAction(Action action, [CallerMemberName]string actionName = null)
        {
            try
            {
                action();
                LastActionToServer = DateTime.Now;
            }
            catch (Exception ex)
            {
                Log.Default.WriteLine(LogLevels.Error, "Exception:{0} {1}", actionName, ex);
                ConnectionLost.Do(x => x());
                _factory.Do(x => x.Abort());
            }
        }

        #region IProxy

        public DateTime LastActionToServer { get; private set; }

        public event ProxyConnectionLostEventHandler ConnectionLost;

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
                Log.Default.WriteLine(LogLevels.Warning, "Exception:{0}", ex);
                _factory.Abort();
            }
            _factory = null;
            return true;
        }

        #endregion

        #region ITetriNET

        public void RegisterPlayer(ITetriNETCallback callback, Versioning clientVersion, string playerName, string team)
        {
            ExceptionFreeAction(() => _proxy.RegisterPlayer(clientVersion, playerName, team));
        }

        public void UnregisterPlayer(ITetriNETCallback callback)
        {
            ExceptionFreeAction(_proxy.UnregisterPlayer);
        }

        public void Heartbeat(ITetriNETCallback callback)
        {
            ExceptionFreeAction(_proxy.Heartbeat);
        }

        public void PlayerTeam(ITetriNETCallback callback, string team)
        {
            ExceptionFreeAction(() => _proxy.PlayerTeam(team));
        }

        public void PublishMessage(ITetriNETCallback callback, string msg)
        {
            ExceptionFreeAction(() => _proxy.PublishMessage(msg));
        }

        public void PlacePiece(ITetriNETCallback callback, int pieceIndex, int highestIndex, Pieces piece, int orientation, int posX, int posY, byte[] grid)
        {
            ExceptionFreeAction(() => _proxy.PlacePiece(pieceIndex, highestIndex, piece, orientation, posX, posY, grid));
        }

        public void ModifyGrid(ITetriNETCallback callback, byte[] grid)
        {
            ExceptionFreeAction(() => _proxy.ModifyGrid(grid));
        }

        public void UseSpecial(ITetriNETCallback callback, int targetId, Specials special)
        {
            ExceptionFreeAction(() => _proxy.UseSpecial(targetId, special));
        }

        public void ClearLines(ITetriNETCallback callback, int count)
        {
            ExceptionFreeAction(() => _proxy.ClearLines(count));
        }

        public void GameLost(ITetriNETCallback callback)
        {
            ExceptionFreeAction(_proxy.GameLost);
        }

        public void FinishContinuousSpecial(ITetriNETCallback callback, Specials special)
        {
            ExceptionFreeAction(() => _proxy.FinishContinuousSpecial(special));
        }

        public void StartGame(ITetriNETCallback callback)
        {
            ExceptionFreeAction(_proxy.StartGame);
        }

        public void StopGame(ITetriNETCallback callback)
        {
            ExceptionFreeAction(_proxy.StopGame);
        }

        public void PauseGame(ITetriNETCallback callback)
        {
            ExceptionFreeAction(_proxy.PauseGame);
        }

        public void ResumeGame(ITetriNETCallback callback)
        {
            ExceptionFreeAction(_proxy.ResumeGame);
        }

        public void ChangeOptions(ITetriNETCallback callback, GameOptions options)
        {
            ExceptionFreeAction(() => _proxy.ChangeOptions(options));
        }

        public void KickPlayer(ITetriNETCallback callback, int playerId)
        {
            ExceptionFreeAction(() => _proxy.KickPlayer(playerId));
        }

        public void BanPlayer(ITetriNETCallback callback, int playerId)
        {
            ExceptionFreeAction(() => _proxy.BanPlayer(playerId));
        }

        public void ResetWinList(ITetriNETCallback callback)
        {
            ExceptionFreeAction(_proxy.ResetWinList);
        }

        public void EarnAchievement(ITetriNETCallback callback, int achievementId, string achievementTitle)
        {
            ExceptionFreeAction(() => _proxy.EarnAchievement(achievementId, achievementTitle));
        }

        #endregion
    }
}
