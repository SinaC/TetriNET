using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using System.Threading.Tasks;
using TetriNET.Common;
using TetriNET.Common.Interfaces;

namespace TetriNET.Client
{
    [CallbackBehavior(UseSynchronizationContext = false)]
    internal class GameClient : ITetriNETCallback
    {
        public enum States
        {
            ApplicationStarted, // -> ConnectingToServer
            ConnectingToServer, // -> ConnectedToServer
            ConnectedToServer, // -> Registering
            Registering, // -> WaitingStartGame | ApplicationStarted
            WaitingStartGame, // -> GameStarted
            GameStarted, // -> GameFinished
            GameFinished, // -> WaitingStartGame
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
            //List<EndpointAddress> addresses = DiscoveryHelper.DiscoverAddresses<ITetriNET>();
            List<EndpointAddress> addresses = new List<EndpointAddress>
            {
                new EndpointAddress("net.tcp://localhost:8765/")
            };

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
                    ConnectToServer();
                    break;
                case States.ConnectingToServer:
                    // NOP
                    break;
                case States.ConnectedToServer:
                    Register(PlayerName);
                    break;
                case States.Registering:
                    // NOP
                    break;
                case States.WaitingStartGame:
                    State = States.GameStarted;
                    break;
                case States.GameStarted:
                    Thread.Sleep(60);
                    Proxy.PublishMessage(PlayerId, "I'll kill you");
                    Thread.Sleep(60);
                    Proxy.PlaceTetrimino(PlayerId, Tetriminos.TetriminoI, Orientations.Top, new Position
                    {
                        X = 5,
                        Y = 3
                    });
                    Thread.Sleep(60);
                    Proxy.SendAttack(PlayerId, PlayerId, Attacks.Nuke);
                    break;
                case States.GameFinished:
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
                State = States.WaitingStartGame;
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
