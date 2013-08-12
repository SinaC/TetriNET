using System;
using System.ServiceModel;
using TetriNET.Common;

namespace TetriNET.Client
{
    // TODO: use this instead of ITetriNET
    public class ExceptionFreeTetriNETProxy : ITetriNET
    {
        private readonly ITetriNET _proxy;

        public ExceptionFreeTetriNETProxy(ITetriNET proxy)
        {
            _proxy = proxy;
        }

        public ITetriNET Proxy { get { return _proxy; }}

        private void ExceptionFreeAction(Action action, string actionName)
        {
            try
            {
                action();
                // TODO: update ServerActionTime -> IProxyManager + IProxy
            }
            catch (CommunicationObjectAbortedException ex)
            {
                Log.WriteLine("CommunicationObjectAbortedException");
                // TODO: raise 'disconnected from server' event -> IProxyManager + IProxy
                throw new ApplicationException("Should be \"disconnected from server\" event");
            }
        }

        public void RegisterPlayer(string playerName)
        {
            ExceptionFreeAction(() => RegisterPlayer(playerName), "RegisterPlayer");
        }

        public void Ping()
        {
            ExceptionFreeAction(Ping, "Ping");
        }

        public void PublishMessage(string msg)
        {
            ExceptionFreeAction(() => PublishMessage(msg), "PublishMessage");
        }

        public void PlaceTetrimino(Tetriminos tetrimino, Orientations orientation, Position position)
        {
            ExceptionFreeAction(() => PlaceTetrimino(tetrimino, orientation, position), "PlaceTetrimino");
        }

        public void SendAttack(int targetId, Attacks attack)
        {
            ExceptionFreeAction(() => SendAttack(targetId, attack), "SendAttack");
        }
    }
}
