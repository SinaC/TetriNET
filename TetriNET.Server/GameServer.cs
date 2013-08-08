using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.ServiceModel;
using TetriNET.Common;
using TetriNET.Common.Interfaces;
using TetriNET.Common.WCF;

namespace TetriNET.Server
{
    //[ServiceBehavior(InstanceContextMode=InstanceContextMode.Single, ConcurrencyMode=ConcurrencyMode.Single)]
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant, InstanceContextMode = InstanceContextMode.Single)]
    internal class GameServer : ITetriNET
    {
        private const int MaxPlayerCount = 6;

        private ServiceHost Host { get; set; }

        public GameServer() { 
            Console.WriteLine("GameServer:ctor");

            _attackId = 0;
            Task.Factory.StartNew(TaskResolveActions);
        }

        public void StartService() {
            Console.WriteLine("Starting service");
            Uri baseAddress = DiscoveryHelper.AvailableTcpBaseAddress;

            Host = new ServiceHost(this, baseAddress);
            Host.AddDefaultEndpoints();
            Host.Open();

            foreach (var endpt in Host.Description.Endpoints)
            {
                Console.WriteLine("Enpoint address:\t{0}", endpt.Address);
                Console.WriteLine("Enpoint binding:\t{0}", endpt.Binding);
                Console.WriteLine("Enpoint contract:\t{0}\n", endpt.Contract.ContractType.Name);
            }
        }

        public void StopService()
        {
            Console.WriteLine("Stopping service");
            // TODO: notify clients

            // Close service host
            Host.Close();
        }

        #region ITetriNET
        public void RegisterPlayer(string playerName)
        {
            Console.WriteLine("RegisterPlayer:"+playerName);

            ITetriNETCallback callback = OperationContext.Current.GetCallbackChannel<ITetriNETCallback>();

            // Get first empty slot
            int emptySlot = GetEmptySlot();
            if (emptySlot >= 0)
            {
                // Save player
                Player player = SetPlayer(emptySlot, playerName, callback);
                // Send playerId to player
                if (player != null)
                    player.Callback.OnPlayerRegistered(true, emptySlot);
                // Inform players
                // Send attack message to players
                foreach (Player p in _players.Where(p => p != null))
                    p.Callback.OnPublishServerMessage(playerName + "[" + emptySlot + "] is now connected");
                //
                Console.WriteLine("New player:[" + emptySlot + "] " + playerName);
            }
            else
            {
                Console.WriteLine("Register failed for player " + playerName);
                callback.OnPlayerRegistered(false, 0);
            }
        }

        public void PublishMessage(int playerId, string msg)
        {
            Console.WriteLine("PublishMessage:["+playerId+"]:"+msg);

            Player player = GetPlayer(playerId);
            if (player != null)
            {
                //lock (_players)
                //{
                //    foreach (Player p in _players.Where(p => p != null))
                //        p.Callback.OnPublishPlayerMessage(playerId, player.Name, msg);
                //}
            }
            else
                Console.WriteLine("PublishMessage from unknown player[" + playerId + "]");
        }

        public void PlaceTetrimino(int playerId, Tetriminos tetrimino, Orientations orientation, Position position)
        {
            Console.WriteLine("PlaceTetrimino:[" + playerId + "]" + tetrimino + " " + orientation + " at " + position.X + "," + position.Y);

            Player player = GetPlayer(playerId);
            if (player != null)
                _actionQueue.Enqueue(() => PlaceTetrimino(player, tetrimino, orientation, position));
            else
                Console.WriteLine("PlaceTetrimino from unknown player[" + playerId + "]");
        }

        public void SendAttack(int playerId, int targetId, Attacks attack)
        {
            Console.WriteLine("SendAttack:["+playerId+"] -> ["+targetId+"]:"+attack);

            Player player = GetPlayer(playerId);
            if (player != null)
            {
                Player target = GetPlayer(targetId);
                if (target != null)
                    _actionQueue.Enqueue(() => Attack(player, target, attack));
                else
                    Console.WriteLine("SendAttack to unknown player[" + targetId + "] from [" + playerId + "]");
            }
            else
                Console.WriteLine("SendAttack from unknown player[" + playerId + "]");
        }
        #endregion

        #region Attack Id
        private int _attackId;
        public int AttackId
        {
            get { return _attackId; }
        }
        public int IncrementAttackId()
        {
            return Interlocked.Increment(ref _attackId);
        }

        public int DecrementAttackId()
        {
            return Interlocked.Decrement(ref _attackId);
        }
        #endregion

        #region Player
        private readonly Player[] _players = new Player[MaxPlayerCount]; // TODO: replace with an array or dictionary or free list

        public class Player
        {
            public string Name { get; set; }
            public ITetriNETCallback Callback { get; set; }
        }

        private Player SetPlayer(int playerId, string playerName, ITetriNETCallback callback)
        {
            Player player = new Player
            {
                Name = playerName,
                Callback = callback
            };
            _players[playerId] = player;
            return player;
        }

        private Player GetPlayer(int playerId)
        {
            Player player = null;
            lock (_players)
            {
                if (playerId < MaxPlayerCount)
                    player = _players[playerId];
            }
            return player;
        }

        private int GetEmptySlot()
        {
            // get first empty slot
            int emptySlot = -1;
            lock (_players)
            {
                for (int i = 0; i < MaxPlayerCount; i++)
                    if (_players[i] == null)
                    {
                        emptySlot = i;
                        break;
                    }
            }
            return emptySlot;
        }

        #endregion

        #region Action queue
        private readonly ConcurrentQueue<Action> _actionQueue = new ConcurrentQueue<Action>();

        private void TaskResolveActions()
        {
            while (true)
            {
                if (!_actionQueue.IsEmpty)
                {
                    Action action;
                    bool dequeue = _actionQueue.TryDequeue(out action);
                    if (dequeue)
                        action();
                }
                Thread.Sleep(0);
            }
        }
        #endregion

        private void Attack(Player player, Player target, Attacks attack)
        {
            Console.WriteLine("SendAttack[" + player.Name + "][" + target.Name + "]" + attack);

            // Store attack id locally
            int attackId = AttackId;
            // Send attack to target
            target.Callback.OnAttackReceived(attack);
            // Send attack message to players
            string attackString = "Attack " + attack + " from " + player.Name + " to " + target.Name;
            lock (_players)
            {
                foreach (Player p in _players.Where(p => p != null))
                    p.Callback.OnAttackMessageReceived(attackId, attackString);
            }
            // Increment attack id
            IncrementAttackId();
        }

        public void PlaceTetrimino(Player player, Tetriminos tetrimino, Orientations orientation, Position position)
        {
            Console.WriteLine("PlaceTetrimino[" + player.Name + "]" + tetrimino + " " + orientation + " at " + position.X + "," + position.Y);

            // TODO:
        }
    }
}
