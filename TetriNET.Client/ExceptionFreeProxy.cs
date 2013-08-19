using System;
using System.ServiceModel;
using TetriNET.Common;
using TetriNET.Common.Contracts;

namespace TetriNET.Client
{
    public class ExceptionFreeProxy : IWCFTetriNET
    {
        private readonly IWCFTetriNET _proxy;
        private readonly IClient _client;

        public ExceptionFreeProxy(IWCFTetriNET proxy, IClient client)
        {
            _proxy = proxy;
            _client = client;
        }

        private void ExceptionFreeAction(Action action, string actionName)
        {
            try
            {
                action();
                _client.LastAction = DateTime.Now;
            }
            catch (CommunicationObjectAbortedException ex)
            {
                Log.WriteLine("CommunicationObjectAbortedException:{0}", actionName);
                _client.OnDisconnectedFromServer(this);
            }
            catch (CommunicationObjectFaultedException ex)
            {
                Log.WriteLine("CommunicationObjectFaultedException:{0}", actionName);
                _client.OnDisconnectedFromServer(this);
            }
            catch (EndpointNotFoundException ex)
            {
                Log.WriteLine("EndpointNotFoundException:{0}", actionName);
                _client.OnServerUnreachable(this);
            }
        }

        public void RegisterPlayer(string playerName)
        {
            ExceptionFreeAction(() => _proxy.RegisterPlayer(playerName), "RegisterPlayer");
        }

        public void UnregisterPlayer()
        {
            ExceptionFreeAction(_proxy.UnregisterPlayer, "UnregisterPlayer");
        }

        public void Heartbeat()
        {
            ExceptionFreeAction(_proxy.Heartbeat, "Heartbeat");
        }

        public void PublishMessage(string msg)
        {
            ExceptionFreeAction(() => _proxy.PublishMessage(msg), "PublishMessage");
        }

        public void PlaceTetrimino(int index, Tetriminos tetrimino, Orientations orientation, Position position, byte[] grid)
        {
            ExceptionFreeAction(() => _proxy.PlaceTetrimino(index, tetrimino, orientation, position, grid), "PlaceTetrimino");
        }

        public void ModifyGrid(byte[] grid)
        {
            ExceptionFreeAction(() => _proxy.ModifyGrid(grid), "ModifyGrid");
        }

        public void UseSpecial(int targetId, Specials special)
        {
            ExceptionFreeAction(() => _proxy.UseSpecial(targetId, special), "UseSpecial");
        }

        public void SendLines(int count)
        {
            ExceptionFreeAction(() => _proxy.SendLines(count), "SendLines");
        }
        
        public void StartGame()
        {
            ExceptionFreeAction(_proxy.StartGame, "StartGame");
        }

        public void StopGame()
        {
            ExceptionFreeAction(_proxy.StopGame, "StopGame");
        }

        public void PauseGame()
        {
            ExceptionFreeAction(_proxy.PauseGame, "PauseGame");
        }

        public void ResumeGame()
        {
            ExceptionFreeAction(_proxy.ResumeGame, "ResumeGame");
        }

        public void GameLost()
        {
            ExceptionFreeAction(_proxy.GameLost, "ResumeGame");
        }

        public void ChangeOptions(GameOptions options)
        {
            ExceptionFreeAction(() => _proxy.ChangeOptions(options), "ChangeOptions");
        }

        public void KickPlayer(int playerId)
        {
            ExceptionFreeAction(() => _proxy.KickPlayer(playerId), "KickPlayer");
        }

        public void BanPlayer(int playerId)
        {
            ExceptionFreeAction(() => _proxy.BanPlayer(playerId), "BanPlayer");
        }

        public void ResetWinList()
        {
            ExceptionFreeAction(_proxy.ResetWinList, "ResetWinList");
        }
    }
}
