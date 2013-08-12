using System;
using System.Configuration;
using System.Linq;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using TetriNET.Common;
using TetriNET.Common.WCF;

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

        private const int InactivityTimeoutBeforePing = 500; // in ms

        private ITetriNET Proxy { get; set; }

        private DateTime LastServerAction { get; set; }
        public string PlayerName { get; set; }
        public States State { get; private set; }
        public int PlayerId { get; private set; }

        public GameClient()
        {
            State = States.ApplicationStarted;
        }

        public void ConnectToServer()
        {
            Log.WriteLine("Connecting to server");
            State = States.ConnectingToServer;

            //string baseAddress = "net.tcp://localhost:8765/TetriNET";
            string baseAddress = ConfigurationManager.AppSettings["address"];

            EndpointAddress address = null;
            if (String.IsNullOrEmpty(baseAddress) || baseAddress.ToLower() == "auto")
            {
                Log.WriteLine("Searching ITetriNET server");
                List<EndpointAddress> addresses = DiscoveryHelper.DiscoverAddresses<ITetriNET>();
                if (addresses != null && addresses.Any())
                {
                    foreach (EndpointAddress endpoint in addresses)
                        Log.WriteLine("{0}:\t{1}", addresses.IndexOf(endpoint), endpoint.Uri);
                    Log.WriteLine("Connecting to first server");
                    address = addresses[0];

                    LastServerAction = DateTime.Now;
                }
                else
                {
                    Log.WriteLine("No server found");
                }
            }
            else
                address = new EndpointAddress(baseAddress);


            if (address != null)
            {
                Binding binding = new NetTcpBinding(SecurityMode.None);
                //http://tech.pro/tutorial/914/wcf-callbacks-hanging-wpf-applications
                //// Create channel in another thread to solve hanging
                //Task<ITetriNET> task = Task<ITetriNET>.Factory.StartNew(
                //    () => DuplexChannelFactory<ITetriNET>.CreateChannel(instanceContext, binding, addresses[0])
                //);
                //task.Wait();
                //Proxy = task.Result;
                InstanceContext instanceContext = new InstanceContext(this);
                Proxy = DuplexChannelFactory<ITetriNET>.CreateChannel(instanceContext, binding, address); // Should be in fact a ExceptionFreeTetriNETProxy stored as ITetriNET  IHostManager

                State = States.ConnectedToServer;

                Log.WriteLine("Connected to server");
            }
            else
            {
                State = States.ApplicationStarted;
            }
        }

        public void Register(string playerName)
        {
            Log.WriteLine("Registering");
            State = States.Registering;

            PlayerName = playerName;
            try
            {
                Proxy.RegisterPlayer(PlayerName);
            }
            catch (EndpointNotFoundException ex)
            {
                State = States.ApplicationStarted;
                Log.WriteLine("EndpointNotFound -> ApplicationStarted");
            }
            catch (Exception ex)
            {
                Log.WriteLine("Register failed. Exception:"+ex.ToString());
            }
        }

        public void Test()
        {
            switch (State)
            {
                case States.ApplicationStarted:
                    ConnectToServer();
                    break;
                case States.ConnectingToServer:
                    // NOP: wait connection resolution
                    break;
                case States.ConnectedToServer:
                    Register(PlayerName);
                    break;
                case States.Registering:
                    // NOP: waiting callback OnPlayerRegistered
                    break;
                case States.WaitingStartGame:
                    // NOP: waiting callback OnGameStarted
                    break;
                case States.GameStarted:
                    try
                    {
                        int rnd = new Random().Next(3);
                        switch (rnd)
                        {
                            case 0:
                                Proxy.PublishMessage("I'll kill you");
                                break;
                            case 1:
                                Proxy.PlaceTetrimino(Tetriminos.TetriminoI, Orientations.Top, new Position
                                {
                                    X = 5,
                                    Y = 3
                                });
                                break;
                            case 2:
                                Proxy.SendAttack(PlayerId, Attacks.Nuke);
                                break;
                        }
                        Thread.Sleep(60);
                    }
                    catch (CommunicationObjectFaultedException ex)
                    {
                        State = States.ApplicationStarted;
                        Log.WriteLine("CommunicationObjectFaultedException -> ApplicationStarted");
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine("Register failed. Exception:" + ex.ToString());
                    }
                    break;
                case States.GameFinished:
                    State = States.WaitingStartGame;
                    break;
            }
            if (State >= States.ConnectedToServer)
            {
                TimeSpan timespan = DateTime.Now - LastServerAction;
                if (timespan.TotalMilliseconds > InactivityTimeoutBeforePing)
                    Proxy.Ping();
            }
        }

        //public void Close()
        //{
        //    (Proxy as ICommunicationObject).Close();
        //}

        #region ITetriNETCallback

        public void OnPingReceived()
        {
            Log.WriteLine("OnPingReceived");
            LastServerAction = DateTime.Now;
        }

        public void OnServerStopped()
        {
            Log.WriteLine("OnServerStopped");
            State = States.ApplicationStarted;
            LastServerAction = DateTime.Now;
        }

        public void OnPlayerRegistered(bool succeeded, int playerId)
        {
            Log.WriteLine("OnPlayerRegistered:" + succeeded + " => " + playerId);
            LastServerAction = DateTime.Now;
            if (succeeded)
            {
                PlayerId = playerId;
                State = States.WaitingStartGame;
            }
            else
                State = States.ApplicationStarted;
        }

        public void OnGameStarted(Tetriminos firstTetrimino, Tetriminos secondTetrimino, List<PlayerData> players)
        {
            Log.WriteLine("OnGameStarted:" + firstTetrimino+" "+secondTetrimino);
            LastServerAction = DateTime.Now;
            if (State == States.WaitingStartGame)
            {
                Log.WriteLine("Game started with players:" + players.Select(x => x.Name + "[" + x.Id + "]").Aggregate((n, i) => n + "," + i));
                State = States.GameStarted;
            }
            else
                Log.WriteLine("Was not waiting start game");
        }

        public void OnGameFinished()
        {
            Log.WriteLine("OnGameFinished");
            LastServerAction = DateTime.Now;
            if (State == States.GameStarted)
            {
                State = States.GameFinished;
            }
            else
                Log.WriteLine("Game was not started");
        }

        public void OnServerAddLines(int lineCount)
        {
            Log.WriteLine("OnServerAddLines");
            LastServerAction = DateTime.Now;
        }

        public void OnPlayerAddLines(int lineCount)
        {
            Log.WriteLine("OnPlayerAddLines");
            LastServerAction = DateTime.Now;
        }

        public void OnPublishPlayerMessage(string playerName, string msg)
        {
            Log.WriteLine("OnPublishPlayerMessage:" + playerName + ":" + msg);
            LastServerAction = DateTime.Now;
        }

        public void OnPublishServerMessage(string msg)
        {
            Log.WriteLine("OnPublishServerMessage:" + msg);
            LastServerAction = DateTime.Now;
        }

        public void OnAttackReceived(Attacks attack)
        {
            Log.WriteLine("OnAttackReceived:" + attack);
            LastServerAction = DateTime.Now;
        }

        public void OnAttackMessageReceived(string msg)
        {
            Log.WriteLine("OnAttackMessageReceived:" + msg);
            LastServerAction = DateTime.Now;
        }

        public void OnNextTetrimino(int index, Tetriminos tetrimino)
        {
            Log.WriteLine("OnNextTetrimino:" + index + " " + tetrimino);
            LastServerAction = DateTime.Now;
        }

        #endregion
    }
}
