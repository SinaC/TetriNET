using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using TetriNET.Common;
using TetriNET.Common.Interfaces;
using TetriNET.Common.WCF;

namespace TetriNET.Client
{
    [CallbackBehavior(UseSynchronizationContext = false)]
    internal class GameClient : ITetriNETCallback
    {
        public enum States
        {
            ApplicationStarted, // -> Connecting to server
            ConnectingToServer, // -> Connected to server
            ConnectedToServer, // -> Registering
            Registering, // -> Registered or ApplicationStarted
            Registered
        }
        private ITetriNET Proxy { get; set; }

        public string PlayerName { get; set; }
        public States State { get; private set; }
        public int PlayerId { get; private set; }

        public GameClient()
        {
            State = States.ApplicationStarted;
        }

        public void ConnectToServer()
        {
            Log.WriteLine("Searching server");
            State = States.ConnectingToServer;

            InstanceContext instanceContext = new InstanceContext(this);
            List<EndpointAddress> addresses = DiscoveryHelper.DiscoverAddresses<ITetriNET>();

            if (addresses != null && addresses.Any())
            {
                foreach (EndpointAddress endpoint in addresses)
                    Log.WriteLine("{0}:\t{1}", addresses.IndexOf(endpoint), endpoint.Uri);

                Log.WriteLine("Connecting to first server");
                Binding binding = new NetTcpBinding();
                //http://tech.pro/tutorial/914/wcf-callbacks-hanging-wpf-applications
                // Create channel in another thread to solve hanging
                Task<ITetriNET> task = Task<ITetriNET>.Factory.StartNew(
                    () => DuplexChannelFactory<ITetriNET>.CreateChannel(instanceContext, binding, addresses[0])
                );
                task.Wait();
                Proxy = task.Result;
                //Proxy = DuplexChannelFactory<ITetriNET>.CreateChannel(instanceContext, binding, addresses[0]);

                State = States.ConnectedToServer;
            }
            else
            {
                Log.WriteLine("No server found");

                State = States.ApplicationStarted;
            }
        }

        public void Register(string playerName)
        {
            Log.WriteLine("Registering");
            State = States.Registering;

            PlayerName = playerName;
            Proxy.RegisterPlayer(PlayerName);
        }

        public void Test()
        {
            switch (State)
            {
                case States.ApplicationStarted:
                    State = States.ConnectingToServer;
                    ConnectToServer();
                    break;
                case States.ConnectingToServer:
                    // Nop
                    break;
                case States.ConnectedToServer:
                    Register(PlayerName);
                    break;
                case States.Registered:
                    Proxy.PublishMessage(PlayerId, "I'll kill you");
                    Proxy.PlaceTetrimino(PlayerId, Tetriminos.TetriminoI, Orientations.Top, new Position
                    {
                        X = 5,
                        Y = 3
                    });
                    Proxy.SendAttack(PlayerId, PlayerId, Attacks.Nuke);
                    break;
            }
        }

        //public void Close()
        //{
        //    (Proxy as ICommunicationObject).Close();
        //}

        #region ITetriNETCallback
        public void OnPlayerRegistered(bool succeeded, int playerId)
        {
            Log.WriteLine("OnPlayerRegistered:" + succeeded + " => " + playerId);
            if (succeeded)
            {
                PlayerId = playerId;
                State = States.Registered;
            }
            else
                State = States.ApplicationStarted;
        }

        public void OnPublishPlayerMessage(int playerId, string playerName, string msg)
        {
            Log.WriteLine("OnPublishPlayerMessage:" + playerName + "[" + playerId + "]:" + msg);
        }

        public void OnPublishServerMessage(string msg)
        {
            Log.WriteLine("OnPublishServerMessage:" + msg);
        }

        public void OnAttackReceived(Attacks attack)
        {
            Log.WriteLine("OnAttackReceived:" + attack);
        }

        public void OnAttackMessageReceived(int attackId, string msg)
        {
            Log.WriteLine("OnAttackMessageReceived:" + attackId + " " + msg);
        }
        #endregion
    }
}
