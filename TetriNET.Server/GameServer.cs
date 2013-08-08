using System;
using System.Collections.Concurrent;
using System.Linq;
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
            Log.WriteLine("GameServer:ctor");

            _attackId = 0;
            Task.Factory.StartNew(TaskResolveActions);
        }

        public void StartService() {
            Log.WriteLine("Starting service");
            Uri baseAddress = DiscoveryHelper.AvailableTcpBaseAddress;

            Host = new ServiceHost(this, baseAddress);
            Host.AddDefaultEndpoints();
            Host.Open();

            foreach (var endpt in Host.Description.Endpoints)
            {
                Log.WriteLine("Enpoint address:\t{0}", endpt.Address);
                Log.WriteLine("Enpoint binding:\t{0}", endpt.Binding);
                Log.WriteLine("Enpoint contract:\t{0}\n", endpt.Contract.ContractType.Name);
            }
        }

        public void StopService()
        {
            Log.WriteLine("Stopping service");
            // TODO: notify clients

            // Close service host
            Host.Close();
        }

        #region ITetriNET
        public void RegisterPlayer(string playerName)
        {
            Log.WriteLine("RegisterPlayer:"+playerName);

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
                // Send register message to players
                foreach (Player p in _players.Where(p => p != null))
                    p.Callback.OnPublishServerMessage(playerName + "[" + emptySlot + "] is now connected");
                //
                Log.WriteLine("New player:[" + emptySlot + "] " + playerName);
            }
            else
            {
                Log.WriteLine("Register failed for player " + playerName);
                callback.OnPlayerRegistered(false, 0);
            }
        }

        public void PublishMessage(int playerId, string msg)
        {
            Log.WriteLine("PublishMessage:["+playerId+"]:"+msg);

            Player player = GetPlayer(playerId);
            if (player != null)
            {
                foreach (Player p in _players.Where(p => p != null))
                    p.Callback.OnPublishPlayerMessage(playerId, player.Name, msg);
            }
            else
                Log.WriteLine("PublishMessage from unknown player[" + playerId + "]");
        }

        public void PlaceTetrimino(int playerId, Tetriminos tetrimino, Orientations orientation, Position position)
        {
            Log.WriteLine("PlaceTetrimino:[" + playerId + "]" + tetrimino + " " + orientation + " at " + position.X + "," + position.Y);

            Player player = GetPlayer(playerId);
            if (player != null)
                _actionQueue.Enqueue(() => PlaceTetrimino(player, tetrimino, orientation, position));
            else
                Log.WriteLine("PlaceTetrimino from unknown player[" + playerId + "]");
        }

        public void SendAttack(int playerId, int targetId, Attacks attack)
        {
            Log.WriteLine("SendAttack:["+playerId+"] -> ["+targetId+"]:"+attack);

            Player player = GetPlayer(playerId);
            if (player != null)
            {
                Player target = GetPlayer(targetId);
                if (target != null)
                    _actionQueue.Enqueue(() => Attack(player, target, attack));
                else
                    Log.WriteLine("SendAttack to unknown player[" + targetId + "] from [" + playerId + "]");
            }
            else
                Log.WriteLine("SendAttack from unknown player[" + playerId + "]");
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
            if (playerId < MaxPlayerCount)
                player = _players[playerId];
            return player;
        }

        private int GetEmptySlot()
        {
            // get first empty slot
            int emptySlot = -1;
            for (int i = 0; i < MaxPlayerCount; i++)
                if (_players[i] == null)
                {
                    emptySlot = i;
                    break;
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
                // TODO: break
            }
        }
        #endregion

        private void Attack(Player player, Player target, Attacks attack)
        {
            Log.WriteLine("SendAttack[" + player.Name + "][" + target.Name + "]" + attack);

            // Store attack id locally
            int attackId = AttackId;
            // Send attack to target
            target.Callback.OnAttackReceived(attack);
            // Send attack message to players
            string attackString = "Attack " + attack + " from " + player.Name + " to " + target.Name;
            foreach (Player p in _players.Where(p => p != null))
                p.Callback.OnAttackMessageReceived(attackId, attackString);
            // Increment attack id
            IncrementAttackId();
        }

        public void PlaceTetrimino(Player player, Tetriminos tetrimino, Orientations orientation, Position position)
        {
            Log.WriteLine("PlaceTetrimino[" + player.Name + "]" + tetrimino + " " + orientation + " at " + position.X + "," + position.Y);

            // TODO:
        }
    }
}
