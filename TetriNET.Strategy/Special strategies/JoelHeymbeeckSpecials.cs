using System;
using System.Collections.Generic;
using System.Linq;
using TetriNET.Common.GameDatas;
using TetriNET.Common.Helpers;
using TetriNET.Common.Interfaces;

namespace TetriNET.Strategy
{
    public class JoelHeymbeeckSpecials : SpecialStrategyBase
    {
        private IClient _client;

        public override string StrategyName
        {
            get { return "Joël Heymbeeck Specials Strategy"; }
        }

        public override bool GetSpecialAdvice(IBoard board, ITetrimino current, ITetrimino next, List<Specials> specials, out List<SpecialAdvices> advices)
        {
            throw new NotImplementedException();
        }

        private void UseBestSpecial()
        {
            #region Complex strategy (may send more than one special)
            // if solo, 
            //  drop everything except Nuke/Gravity/ClearLines
            //  use clear line if no nuke/gravity on bottom line or when reaching top of board or when inventory is full
            //  use nuke when reaching top of board
            //  use gravity when reaching mid-board
            // Survival first:
            //  if we are to high (above 14), 
            //      if we have Nuke or Gravity or Switch in inventory, send offensive specials to strongest opponent (lowest board) + send ClearLine to ourself until getting Nuke/Gravity/Switch
            //          use Nuke or Gravity on ourself -> send to ourself ==> saved
            //          use Switch
            //              if strongest opponent board is below 10 -> send to opponent => saved
            //              else drop it and continue to search for a Nuke or Gravity
            //      else, 
            //          empty inventory by sending offensive specials to weakest opponent (highest board) + sending ClearLine to ourself -> not saved
            // If we are saved,
            //  if there is only one player left, and we have enough AddLines to kill him, drop everything except AddLines and send them to last opponent
            //  if first special is Switch, destroy own board and switch with strongest opponent
            //  if first special is Nuke or Gravity, NOP
            //  if first special is AddLines, send to strongest opponent
            //  if first special is Bomb, 
            //      send to strongest opponent with a bomb
            //      if none, NOP
            //  if first special is ClearLine,
            //      send to opponent with Nuke/Gravity/Switch on bottom line
            //      if none, send to opponent with Bomb on bottom line
            //      if none, send to opponent with most specials on bottom line
            //      if none,
            //          if we have Nuke/Gravity/Switch/Bomb on bottom line, drop
            //          else, send to ourself
            //  if first special is ClearSpecialBlocks,
            //      if we have a Bomb in our board and no Gravity/Nuke in inventory, send to ourself
            //      send to opponent with Nuke/Gravity/Switch
            //      if none,
            //          if second special is Bomb
            //              if an opponent has a bomb,
            //                  send to opponent with most specials and no bomb
            //                  if none, drop
            //              else
            //                  send to opponent with most specials
            //                  if none, NOP
            //          else
            //              send to opponent with Bomb
            //              if none, send to opponent with most specials
            //              if none, NOP
            //  if first special is RandomBlocksClear,
            //      send to opponent with Nuke/Gravity/Switch
            //      if none, send to strongest opponent
            //  if first special is Quake,
            //      send to opponent with most towers or strongest opponent [TO BE DEFINED]
            // TODO: zebra and clear column
            List<Specials> inventory = _client.Inventory.Select(x => x).ToList(); // use this local copy
            if (!inventory.Any())
                return;

            bool isSolo = IsSolo();
            if (isSolo)
            {
                //  drop everything except Nuke/Gravity/ClearLines
                //  use clear line if no nuke/gravity on bottom line or when reaching top of board or when inventory is full
                //  use nuke when reaching top of board
                //  use gravity when reaching mid-board

                Specials special = inventory[0];
                inventory.RemoveAt(0);

                int maxPile = BoardHelper.GetPileMaxHeight(_client.Board);
                switch (special)
                {
                    case Specials.NukeField:
                        if (maxPile >= 14)
                            _client.UseSpecial(_client.PlayerId);
                        break;
                    case Specials.BlockGravity:
                        if (maxPile >= 10)
                            _client.UseSpecial(_client.PlayerId);
                        break;
                    case Specials.ClearLines:
                        {
                            bool hasValuableBottomLine = HasNukeGravityOnBottomLine();
                            if (!hasValuableBottomLine || maxPile >= 14)
                                _client.UseSpecial(_client.PlayerId);
                            else if (maxPile >= 10 || inventory.Count + 2 >= _client.InventorySize)
                                _client.UseSpecial(_client.PlayerId);
                            break;
                        }
                    default:
                        _client.DiscardFirstSpecial();
                        break;
                }
            }
            else
            {
                int maxPile = BoardHelper.GetPileMaxHeight(_client.Board);
                if (maxPile >= 14)
                {
                    // Survival
                    // If Nuke/Gravity/Switch
                    if (inventory.Any(x => x == Specials.NukeField || x == Specials.BlockGravity || x == Specials.SwitchFields))
                    {
                        Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[SURVIVAL] Found N/G/S in inventory");
                        bool saved = false;
                        while (true)
                        {
                            bool succeeded;
                            if (!inventory.Any())
                                break; // Stops when inventory is empty
                            // Get strongest opponent
                            int strongest = GetStrongestOpponent();
                            // Get current special
                            Specials special = inventory[0];
                            inventory.RemoveAt(0);
                            switch (special)
                            {
                                case Specials.NukeField: // Nuke/Gravity -> use it immediately
                                case Specials.BlockGravity:
                                    Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[SURVIVAL]Use {0}", special.ToString());
                                    succeeded = _client.UseSpecial(_client.PlayerId);
                                    saved = true; // Saved, stop emptying inventory
                                    break;
                                case Specials.SwitchFields: // Switch -> use it only if strongest is really strong, else drop it
                                    {
                                        IBoard strongestBoard = _client.GetBoard(strongest);
                                        int pileHeight = BoardHelper.GetPileMaxHeight(strongestBoard);
                                        if (pileHeight <= 10)
                                        {
                                            Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[SURVIVAL]Use S, found a valid opponent {0} with a pile {1}", strongest, pileHeight);
                                            succeeded = _client.UseSpecial(strongest);
                                            saved = true; // Saved, stop emptying inventory
                                        }
                                        else
                                        {
                                            Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[SURVIVAL]Discard S, no valid opponent");
                                            _client.DiscardFirstSpecial();
                                            succeeded = true;
                                        }
                                        break;
                                    }
                                case Specials.ClearLines: // ClearLine -> use it immediately  TODO ==> this could lead to unwanted behaviour if we are emptying for a switch
                                    Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[SURVIVAL]Use C on ourself");
                                    succeeded = _client.UseSpecial(_client.PlayerId);
                                    break;
                                default: // Other -> use it on strongest
                                    Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[SURVIVAL]Use {0} on strongest opponent {1}", special, strongest);
                                    succeeded = _client.UseSpecial(strongest);
                                    break;
                            }

                            if (saved || !succeeded) // If saved or something wrong with UseSpecial, stop loop
                                break;

                            System.Threading.Thread.Sleep(10); // delay next special use
                        }
                        Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[SURVIVAL] Survival mode N/G/S finished. saved:{0}", saved);
                    }
                    // Nothing could save use, send everything to weakest and pray 
                    else
                    {
                        Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[SURVIVAL] No N/G/S found -> use everything on weakest and try to kill him");
                        while (true)
                        {
                            bool succeeded;
                            if (!inventory.Any())
                                break; // Stops when inventory is empty
                            // Get strongest opponent
                            int weakest = GetWeakestOpponent();
                            // Get current special
                            Specials special = inventory[0];
                            inventory.RemoveAt(0);
                            // ClearLine -> use it immediately
                            if (special == Specials.ClearLines)
                            {
                                Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[SURVIVAL]Use C on ourself");
                                succeeded = _client.UseSpecial(_client.PlayerId);
                            }
                            // Other -> use it on weakest
                            else
                            {
                                Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[SURVIVAL]Use {0} on weakest opponent {1}", special.ToString(), weakest);
                                succeeded = _client.UseSpecial(weakest);
                            }
                            if (!succeeded) // If something wrong with UseSpecial, stop loop
                                break;
                            System.Threading.Thread.Sleep(10); // delay next special use
                        }
                    }
                    return;
                }
                else
                {
                    // Normal use

                    // Check if we can kill last player
                    int lastOpponent = GetLastOpponent();
                    if (lastOpponent != -1)
                    {
                        IBoard lastOpponentBoard = _client.GetBoard(lastOpponent);
                        if (lastOpponentBoard != null)
                        {
                            int pileHeight = BoardHelper.GetPileMaxHeight(lastOpponentBoard);
                            int addLinesCount = inventory.Count(x => x == Specials.AddLines);
                            if (addLinesCount > 0 && pileHeight + addLinesCount >= lastOpponentBoard.Height)
                            {
                                Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[KILLING LAST]Trying to kill last opponent with A");
                                while (true)
                                {
                                    if (!inventory.Any())
                                        break; // Stops when inventory is empty
                                    bool succeeded;
                                    // Get current special
                                    Specials currentSpecial = inventory[0];
                                    inventory.RemoveAt(0);

                                    // AddLines -> use it immediately on last opponent
                                    if (currentSpecial == Specials.AddLines)
                                    {
                                        Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[KILLING LAST]Use A on last opponent {0}", lastOpponent);
                                        succeeded = _client.UseSpecial(lastOpponent);
                                    }
                                    // Else, discard
                                    else
                                    {
                                        Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[KILLING LAST]Discard {0}", currentSpecial.ToString());
                                        _client.DiscardFirstSpecial();
                                        succeeded = true;
                                    }

                                    if (!succeeded)
                                        break;

                                    System.Threading.Thread.Sleep(10); // delay next special use
                                }
                                return; // stops here
                            }
                        }
                    }

                    // Get strongest opponent
                    int strongest = GetStrongestOpponent();

                    // Get current special
                    Specials special = inventory[0];
                    inventory.RemoveAt(0);

                    //
                    switch (special)
                    {
                        // Destroy own board and switch with strongest opponent
                        case Specials.SwitchFields: // TODO:
                            Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[NORMAL]Keep S for survival");
                            // NOP
                            break;
                        // Wait
                        case Specials.BlockGravity:
                        case Specials.NukeField:
                            Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[NORMAL]Keep {0} for survival", special.ToString());
                            // NOP
                            break;
                        // Send to strongest opponent
                        case Specials.AddLines:
                            Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[NORMAL]Use A on strongest {0}", strongest);
                            _client.UseSpecial(strongest);
                            break;
                        //  Send to strongest opponent with a bomb
                        //  If none,
                        //      if inventory almost full, drop
                        //      else NOP
                        case Specials.BlockBomb:
                            {
                                int bombTarget = GetStrongestOpponentWithBomb();
                                if (bombTarget != -1)
                                {
                                    Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[NORMAL]use O to {0}", bombTarget);
                                    _client.UseSpecial(bombTarget);
                                }
                                else
                                {
                                    if (inventory.Count + 2 >= _client.InventorySize)
                                    {
                                        Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[NORMAL]Discard O, inventory almost full");
                                        _client.DiscardFirstSpecial();
                                    }
                                    else
                                    {
                                        Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[NORMAL]Keep O for later");
                                    }
                                }
                                break;
                            }
                        //  Send to opponent with Nuke/Gravity/Switch on bottom line
                        //  If none, send to opponent with Bomb on bottom line
                        //  If none, send to opponent with most specials on bottom line
                        //  If none,
                        //      if we have Nuke/Gravity/Switch/Bomb on bottom line, drop
                        //      else, send to ourself
                        case Specials.ClearLines:
                            {
                                int targetId = GetStrongestOpponentWithNukeSwitchGravityOnBottomLine();
                                if (targetId != -1)
                                {
                                    Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[NORMAL]Use C on opponent with N/G/S on bottom line {0}", targetId);
                                    _client.UseSpecial(targetId);
                                }
                                else
                                {
                                    targetId = GetStrongestOpponentWithBombOnBottomLine();
                                    if (targetId != -1)
                                    {
                                        Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[NORMAL]Use C on opponent with O on bottom line {0}", targetId);
                                        _client.UseSpecial(targetId);
                                    }
                                    else
                                    {
                                        targetId = GetOpponentWithMostSpecialsOnBottomLine();
                                        if (targetId != -1)
                                        {
                                            Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[NORMAL]Use C on opponent with most specials on bottom line {0}", targetId);
                                            _client.UseSpecial(targetId);
                                        }
                                        else
                                        {
                                            bool hasValuableSpecialOnBottomLine = HasNukeGravitySwitchOnBottomLine();
                                            if (hasValuableSpecialOnBottomLine)
                                            {
                                                Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[NORMAL]Discard C, we have N/G/S on bottom line");
                                                _client.DiscardFirstSpecial();
                                            }
                                            else
                                            {
                                                Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[NORMAL]Use C on ourself");
                                                _client.UseSpecial(_client.PlayerId);
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        //  if we have a Bomb in our board and no Gravity/Nuke in inventory, send to ourself
                        //  send to opponent with Nuke/Gravity/Switch
                        //  if none,
                        //      if second special is Bomb
                        //          if an opponent has a bomb,
                        //              send to opponent with most specials and no bomb
                        //              if none, drop
                        //          else
                        //              send to opponent with most specials
                        //              if none,
                        //                  if inventory almost full, drop
                        //                  else NOP
                        //      else
                        //          send to opponent with Bomb
                        //          if none, send to opponent with most specials
                        //          if none,
                        //              if inventory almost full, drop
                        //              else NOP
                        case Specials.ClearSpecialBlocks:
                            {
                                bool hasBomb = HasSpecial(Specials.BlockBomb);
                                if (hasBomb && !inventory.Any(x => x == Specials.NukeField || x == Specials.BlockGravity))
                                {
                                    Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[NORMAL]Use B on ourself, we have O in board and no N/G in inventory");
                                    _client.UseSpecial(_client.PlayerId);
                                }
                                else
                                {
                                    int targetId = GetStrongestOpponentWithNukeSwitchGravity();
                                    if (targetId != -1)
                                    {
                                        Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[NORMAL]Use B on strongest opponent with N/S/G {0}", targetId);
                                        _client.UseSpecial(targetId);
                                    }
                                    else
                                    {
                                        if (inventory.Any() && inventory[0] == Specials.BlockBomb)
                                        {
                                            bool hasOpponentWithBomb = HasOpponentWithBomb();
                                            if (hasOpponentWithBomb)
                                            {
                                                targetId = GetOpponentWithMostSpecialsAndNoBomb();
                                                if (targetId != -1)
                                                {
                                                    Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[NORMAL]Use B on opponent with most specials and no O {0}, we have O in inventory", targetId);
                                                    _client.UseSpecial(targetId);
                                                }
                                                else
                                                {
                                                    Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[NORMAL]Discard B, no opponents without O and we have O in inventory", targetId);
                                                    _client.DiscardFirstSpecial();
                                                }
                                            }
                                            else
                                            {
                                                targetId = GetOpponentWithMostSpecials();
                                                if (targetId != -1)
                                                {
                                                    Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[NORMAL]Use B on opponent with most specials {0}", targetId);
                                                    _client.UseSpecial(targetId);
                                                }
                                                else
                                                {
                                                    if (inventory.Count + 2 >= _client.InventorySize)
                                                    {
                                                        Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[NORMAL]Discard B, inventory is almost full");
                                                        _client.DiscardFirstSpecial();
                                                    }
                                                    else
                                                    {
                                                        Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[NORMAL]Keep B for later");
                                                    }
                                                }
                                            }
                                        }
                                        else
                                        {
                                            targetId = GetStrongestOpponentWithBomb();
                                            if (targetId != -1)
                                            {
                                                Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[NORMAL]Use B on opponent with O {0}, we don't have any O", targetId);
                                                _client.UseSpecial(targetId);
                                            }
                                            else
                                            {
                                                targetId = GetOpponentWithMostSpecials();
                                                if (targetId != -1)
                                                {
                                                    Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[NORMAL]Use B on opponent with most specials {0}", targetId);
                                                    _client.UseSpecial(targetId);
                                                }
                                                else
                                                {
                                                    if (inventory.Count + 2 >= _client.InventorySize)
                                                    {
                                                        Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[NORMAL]Discard B, inventory is almost full");
                                                        _client.DiscardFirstSpecial();
                                                    }
                                                    else
                                                    {
                                                        Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[NORMAL]Keep B for later");
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        //  send to opponent with Nuke/Gravity/Switch
                        //  if none, send to strongest opponent
                        case Specials.RandomBlocksClear:
                            {
                                int targetId = GetStrongestOpponentWithNukeSwitchGravity();
                                if (targetId != -1)
                                {
                                    Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[NORMAL]Use R on strongest opponent with N/S/G {0}", targetId);
                                    _client.UseSpecial(targetId);
                                }
                                else
                                {
                                    targetId = GetStrongestOpponent();
                                    if (targetId != -1)
                                    {
                                        Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[NORMAL]Use R on strongest opponent {0}", targetId);
                                        _client.UseSpecial(targetId);
                                    }
                                    else
                                    {
                                        Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[NORMAL]Keep R for later **** SHOULD NEVER HAPPEN");
                                    }
                                }
                                break;
                            }
                        // TODO
                        //  if first special is Quake,
                        //      send to opponent with most towers or strongest opponent [TO BE DEFINED]
                        // ClearColumn
                        // ZebraField

                        // Send to strongest opponent
                        default:
                            {
                                int targetId = GetStrongestOpponent();
                                if (targetId != -1)
                                {
                                    Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[NORMAL]Use {0} on strongest opponent {1}", special.ToString(), targetId);
                                    _client.UseSpecial(targetId);
                                }
                                else
                                {
                                    Log.Log.WriteLine(Log.Log.LogLevels.Debug, "[NORMAL]Keep {0} for later **** SHOULD NEVER HAPPEN", special.ToString());
                                }
                                break;
                            }
                    }
                }
            }

            #endregion

            #region Simple strategy
            ////  if negative special,
            ////      if no other player, drop it
            ////      else, use it on random opponent
            ////  else if switch, drop it
            ////  else, use it on ourself
            //List<Specials> inventory = _client.Inventory;
            //if (inventory != null && inventory.Any())
            //{
            //    Specials firstSpecial = inventory[0];
            //    int specialValue = 0;
            //    switch (firstSpecial)
            //    {
            //        case Specials.AddLines:
            //            specialValue = -1;
            //            break;
            //        case Specials.ClearLines:
            //            specialValue = +1;
            //            break;
            //        case Specials.NukeField:
            //            specialValue = +1;
            //            break;
            //        case Specials.RandomBlocksClear:
            //            specialValue = -1;
            //            break;
            //        case Specials.SwitchFields:
            //            specialValue = 0;
            //            break;
            //        case Specials.ClearSpecialBlocks:
            //            specialValue = -1;
            //            break;
            //        case Specials.BlockGravity:
            //            specialValue = +1;
            //            break;
            //        case Specials.BlockQuake:
            //            specialValue = -1;
            //            break;
            //        case Specials.BlockBomb:
            //            specialValue = -1;
            //            break;
            //        case Specials.ClearColumn:
            //            specialValue = -1;
            //            break;
            //        case Specials.ZebraField:
            //            specialValue = -1;
            //            break;
            //    }
            //    if (specialValue == 0)
            //        _client.DiscardFirstSpecial();
            //    else if (specialValue > 0)
            //        _client.UseSpecial(_client.PlayerId);
            //    else
            //    {
            //        List<int> opponents = new List<int>();
            //        for (int i = 0; i < _client.MaxPlayersCount; i++)
            //            if (i != _client.PlayerId)
            //            {
            //                IBoard board = _client.GetBoard(i);
            //                if (board != null)
            //                    opponents.Add(i);
            //            }
            //        if (opponents.Any())
            //        {
            //            int index = _random.Next(opponents.Count);
            //            int specialTarget = opponents[index];
            //            _client.UseSpecial(specialTarget);
            //        }
            //        else
            //            _client.DiscardFirstSpecial();
            //    }
            //}
            #endregion
        }

        private bool IsSolo()
        {
            for (int i = 0; i < _client.MaxPlayersCount; i++)
                if (i != _client.PlayerId)
                {
                    IBoard board = _client.GetBoard(i);
                    if (board != null && _client.IsPlaying(i))
                        return false;
                }
            return true;
        }

        private int GetStrongestOpponent() // Get opponent with lowest pile height
        {
            int strongest = -1;
            int strongestPileHeight = 1000;
            for (int i = 0; i < _client.MaxPlayersCount; i++)
                if (i != _client.PlayerId)
                {
                    IBoard board = _client.GetBoard(i);
                    if (board != null && _client.IsPlaying(i))
                    {
                        int pileHeight = BoardHelper.GetPileMaxHeight(board);
                        if (pileHeight < strongestPileHeight)
                        {
                            strongestPileHeight = pileHeight;
                            strongest = i;
                        }
                    }
                }
            return strongest;
        }

        private int GetWeakestOpponent() // Get opponent with highest pile height
        {
            int weakest = -1;
            int weakestPileHeight = -1000;
            for (int i = 0; i < _client.MaxPlayersCount; i++)
                if (i != _client.PlayerId)
                {
                    IBoard board = _client.GetBoard(i);
                    if (board != null && _client.IsPlaying(i))
                    {
                        int pileHeight = BoardHelper.GetPileMaxHeight(board);
                        if (pileHeight > weakestPileHeight)
                        {
                            weakestPileHeight = pileHeight;
                            weakest = i;
                        }
                    }
                }
            return weakest;
        }

        private int GetLastOpponent() // Return opponent id if there is only one opponent left
        {
            int id = -1;
            for (int i = 0; i < _client.MaxPlayersCount; i++)
                if (i != _client.PlayerId && _client.IsPlaying(i))
                {
                    if (id == -1) // First opponent found
                        id = i;
                    else // More than one opponent -> failed
                        return -1;
                }
            return id;
        }

        private int GetStrongestOpponentWithBomb() // Search among opponents which one has a bomb and the lowest board
        {
            int id = -1;
            int lowestPileHeight = 1000;
            for (int i = 0; i < _client.MaxPlayersCount; i++)
                if (i != _client.PlayerId && _client.IsPlaying(i))
                {
                    IBoard board = _client.GetBoard(i);
                    if (board != null && board.Cells.Any(x => CellHelper.IsSpecial(x) && CellHelper.GetSpecial(x) == Specials.BlockBomb))
                    {
                        int pileHeight = BoardHelper.GetPileMaxHeight(board);
                        if (pileHeight < lowestPileHeight)
                        {
                            id = i;
                            lowestPileHeight = pileHeight;
                        }
                    }
                }
            return id;
        }

        private int GetStrongestOpponentWithNukeSwitchGravityOnBottomLine() // Search among opponents which one has a nuke/switch/gravity on bottom line and the lowest board
        {
            int id = -1;
            int lowestPileHeight = 1000;
            for (int i = 0; i < _client.MaxPlayersCount; i++)
                if (i != _client.PlayerId && _client.IsPlaying(i))
                {
                    IBoard board = _client.GetBoard(i);
                    if (board != null)
                    {
                        for (int x = 1; x <= board.Width; x++)
                        {
                            Specials cellSpecial = CellHelper.GetSpecial(board[x, 0]);
                            if (cellSpecial == Specials.BlockGravity || cellSpecial == Specials.NukeField || cellSpecial == Specials.SwitchFields)
                            {
                                int pileHeight = BoardHelper.GetPileMaxHeight(board);
                                if (pileHeight < lowestPileHeight)
                                {
                                    id = i;
                                    lowestPileHeight = pileHeight;
                                }
                                break;
                            }
                        }
                    }
                }
            return id;
        }

        private int GetStrongestOpponentWithBombOnBottomLine() // Search among opponents which one has a bomb on bottom line and the lowest board
        {
            int id = -1;
            int lowestPileHeight = 1000;
            for (int i = 0; i < _client.MaxPlayersCount; i++)
                if (i != _client.PlayerId && _client.IsPlaying(i))
                {
                    IBoard board = _client.GetBoard(i);
                    if (board != null)
                    {
                        for (int x = 1; x <= board.Width; x++)
                        {
                            Specials cellSpecial = CellHelper.GetSpecial(board[x, 0]);
                            if (cellSpecial == Specials.BlockBomb)
                            {
                                int pileHeight = BoardHelper.GetPileMaxHeight(board);
                                if (pileHeight < lowestPileHeight)
                                {
                                    id = i;
                                    lowestPileHeight = pileHeight;
                                }
                                break;
                            }
                        }
                    }
                }
            return id;
        }

        private int GetOpponentWithMostSpecialsOnBottomLine() // Search among opponents which one has most specials on bottom line
        {
            int id = -1;
            int mostSpecial = 0;
            for (int i = 0; i < _client.MaxPlayersCount; i++)
                if (i != _client.PlayerId && _client.IsPlaying(i))
                {
                    IBoard board = _client.GetBoard(i);
                    if (board != null)
                    {
                        int countSpecial = 0;
                        for (int x = 1; x <= board.Width; x++)
                            if (CellHelper.IsSpecial(board[x, 0]))
                                countSpecial++;
                        if (countSpecial > mostSpecial)
                        {
                            id = i;
                            mostSpecial = countSpecial;
                        }
                    }
                }
            return id;
        }

        private int GetOpponentWithMostSpecials() // Search among opponents which one has most specials
        {
            int id = -1;
            int mostSpecials = 0;
            for (int i = 0; i < _client.MaxPlayersCount; i++)
                if (i != _client.PlayerId && _client.IsPlaying(i))
                {
                    IBoard board = _client.GetBoard(i);
                    int countSpecial = board == null ? 0 : board.Cells.Count(x => CellHelper.GetSpecial(x) != Specials.Invalid);
                    if (countSpecial > mostSpecials)
                    {
                        id = i;
                        mostSpecials = countSpecial;
                    }
                }
            return id;
        }

        private bool HasNukeGravitySwitchOnBottomLine()
        {
            for (int x = 1; x <= _client.Board.Width; x++)
            {
                Specials cellSpecial = CellHelper.GetSpecial(_client.Board[x, 0]);
                if (cellSpecial == Specials.BlockGravity || cellSpecial == Specials.NukeField || cellSpecial == Specials.SwitchFields)
                    return true;
            }
            return false;
        }

        private bool HasSpecial(Specials special)
        {
            return _client.Board.Cells.Any(x => CellHelper.GetSpecial(x) == special);
        }

        private int GetStrongestOpponentWithNukeSwitchGravity() // Search among opponents which one has a nuke/switch/gravity and the lowest board
        {
            int id = -1;
            int lowestPileHeight = 1000;
            for (int i = 0; i < _client.MaxPlayersCount; i++)
                if (i != _client.PlayerId && _client.IsPlaying(i))
                {
                    IBoard board = _client.GetBoard(i);
                    if (board != null && board.Cells.Any(x => CellHelper.GetSpecial(x) == Specials.SwitchFields || CellHelper.GetSpecial(x) == Specials.NukeField || CellHelper.GetSpecial(x) == Specials.BlockGravity))
                    {
                        int pileHeight = BoardHelper.GetPileMaxHeight(board);
                        if (pileHeight < lowestPileHeight)
                        {
                            id = i;
                            lowestPileHeight = pileHeight;
                        }
                    }
                }
            return id;
        }

        private bool HasOpponentWithBomb() // Search among opponents, if there is one with a bomb
        {
            for (int i = 0; i < _client.MaxPlayersCount; i++)
                if (i != _client.PlayerId && _client.IsPlaying(i))
                {
                    IBoard board = _client.GetBoard(i);
                    if (board != null && board.Cells.Any(x => CellHelper.GetSpecial(x) == Specials.BlockBomb))
                        return true;
                }
            return false;
        }

        private int GetOpponentWithMostSpecialsAndNoBomb() // Search among opponents which one has most specials and no bomb
        {
            int id = -1;
            int mostSpecials = 0;
            for (int i = 0; i < _client.MaxPlayersCount; i++)
                if (i != _client.PlayerId && _client.IsPlaying(i))
                {
                    IBoard board = _client.GetBoard(i);
                    if (board != null)
                    {
                        bool hasBomb = board.Cells.Any(x => CellHelper.GetSpecial(x) == Specials.BlockBomb);
                        if (!hasBomb)
                        {
                            int countSpecial = board.Cells.Count(x => CellHelper.GetSpecial(x) != Specials.Invalid);
                            if (countSpecial > mostSpecials)
                            {
                                id = i;
                                mostSpecials = countSpecial;
                            }
                        }
                    }
                }
            return id;
        }

        private bool HasNukeGravityOnBottomLine()
        {
            for (int x = 1; x <= _client.Board.Width; x++)
            {
                Specials cellSpecial = CellHelper.GetSpecial(_client.Board[x, 0]);
                if (cellSpecial == Specials.BlockGravity || cellSpecial == Specials.NukeField)
                    return true;
            }
            return false;
        }
    }
}
