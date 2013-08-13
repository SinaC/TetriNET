using System;
using System.ServiceModel;
using TetriNET.Common;

namespace TetriNET.Client
{
    // TODO: use this instead of ITetriNET
    public class ExceptionFreeProxy : ITetriNET
    {
        private readonly ITetriNET _proxy;
        private readonly IClient _client;

        public ExceptionFreeProxy(ITetriNET proxy, IClient client)
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
                Log.WriteLine("CommunicationObjectAbortedException:" + actionName);
                _client.OnDisconnectedFromServer(this);
            }
            catch (CommunicationObjectFaultedException ex)
            {
                Log.WriteLine("CommunicationObjectFaultedException:" + actionName);
                _client.OnDisconnectedFromServer(this);
            }
            catch (EndpointNotFoundException ex)
            {
                Log.WriteLine("EndpointNotFoundException:" + actionName);
                _client.OnServerUnreachable(this);
            }
        }

        public void RegisterPlayer(string playerName)
        {
            ExceptionFreeAction(() => _proxy.RegisterPlayer(playerName), "RegisterPlayer");
        }

        public void Ping()
        {
            ExceptionFreeAction(_proxy.Ping, "Ping");
        }

        public void PublishMessage(string msg)
        {
            ExceptionFreeAction(() => _proxy.PublishMessage(msg), "PublishMessage");
        }

        public void PlaceTetrimino(Tetriminos tetrimino, Orientations orientation, Position position)
        {
            ExceptionFreeAction(() => _proxy.PlaceTetrimino(tetrimino, orientation, position), "PlaceTetrimino");
        }

        public void SendAttack(int targetId, Attacks attack)
        {
            ExceptionFreeAction(() => _proxy.SendAttack(targetId, attack), "SendAttack");
        }
    }
}
