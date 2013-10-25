using System.Collections.Generic;
using System.Linq;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Helpers;
using TetriNET.Common.Logger;

namespace TetriNET.Client.Strategy
{
    public class SinaCSpecials : ISpecialStrategy
    {
        // TODO: helpers should return IOpponent instead of int
        // TODO: when confusion is activated, bot doesn't do anything
        // TODO: immunity

        public bool GetSpecialAdvices(IBoard board, IPiece current, IPiece next, List<Specials> inventory, int inventoryMaxSize, List<IOpponent> opponents, out List<SpecialAdvice> advices)
        {
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
            //  if there is only one player left, and we have enough AddLines to kill him, drop everything except AddLines, Quake, Confusion, Darkness and send them to last opponent
            //  if first special is Switch, destroy own board and switch with strongest opponent (TODO)
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
            //  if first special is Immunity,
            //      send to weakest opponent
            // TODO: zebra, clear column, confusion

            //
            advices = new List<SpecialAdvice>();

            // No inventory
            if (!inventory.Any())
                return false;

            if (!opponents.Any())
            {
                // Solo
                return SoloMode(board, inventory, inventoryMaxSize, advices);
            }
                int maxPile = BoardHelper.GetPileMaxHeight(board);
                if (maxPile >= 14)
                {
                    // Survival
                    return PanicMode(inventory, opponents, advices);
                }
                else
                {
                    // Normal use

                    // Check if we can kill last player
                    int lastOpponent = GetLastOpponent(opponents);
                    if (lastOpponent != -1)
                    {
                        IBoard lastOpponentBoard = opponents.First(x => x.PlayerId == lastOpponent).Board;
                        if (lastOpponentBoard != null)
                        {
                            int pileHeight = BoardHelper.GetPileMaxHeight(lastOpponentBoard);
                            int addLinesCount = inventory.Count(x => x == Specials.AddLines);
                            if (addLinesCount > 0 && pileHeight + addLinesCount >= lastOpponentBoard.Height)
                            {
                                // Finish him
                                return OneOpponentMode(lastOpponent, inventory, advices);
                            }
                        }
                    }

                    return MultiplayerMode(board, inventory, inventoryMaxSize, opponents, advices);
                }
           

            #region Simple strategy

            ////  if negative special,
            ////      if no other player, drop it
            ////      else, use it on random opponent
            ////  else if switch, drop it
            ////  else, use it on ourself
            //Specials firstSpecial = inventory[0];
            //int specialValue = 0;
            //switch (firstSpecial)
            //{
            //    case Specials.AddLines:
            //        specialValue = -1;
            //        break;
            //    case Specials.ClearLines:
            //        specialValue = +1;
            //        break;
            //    case Specials.NukeField:
            //        specialValue = +1;
            //        break;
            //    case Specials.RandomBlocksClear:
            //        specialValue = -1;
            //        break;
            //    case Specials.SwitchFields:
            //        specialValue = 0;
            //        break;
            //    case Specials.ClearSpecialBlocks:
            //        specialValue = -1;
            //        break;
            //    case Specials.BlockGravity:
            //        specialValue = +1;
            //        break;
            //    case Specials.BlockQuake:
            //        specialValue = -1;
            //        break;
            //    case Specials.BlockBomb:
            //        specialValue = -1;
            //        break;
            //    case Specials.ClearColumn:
            //        specialValue = -1;
            //        break;
            //    case Specials.ZebraField:
            //        specialValue = -1;
            //        break;
            //}
            //if (specialValue == 0)
            //    advices.Add(new SpecialAdvice
            //                            {
            //                                SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.Discard,
            //                            });
            //else if (specialValue > 0)
            //    advices.Add(new SpecialAdvice
            //    {
            //        SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.UseSelf,
            //    });
            //else
            //{
            //    if (opponents.Any())
            //    {
            //        // Get strongest opponent
            //        int strongest = GetStrongestOpponent(opponents);
            //        advices.Add(new SpecialAdvice
            //        {
            //            SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.UseOpponent,
            //            OpponentId = strongest,
            //        });
            //    }
            //    else
            //        advices.Add(new SpecialAdvice
            //        {
            //            SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.Discard,
            //        });
            //}

            #endregion
        }

        public bool SoloMode(IBoard board, List<Specials> inventory, int inventoryMaxSize, List<SpecialAdvice> advices)
        {
            //  drop everything except Nuke/Gravity/ClearLines
            //  use clear line if no nuke/gravity on bottom line and board not empty or when reaching top of board or when inventory is full
            //  use nuke when reaching top of board
            //  use gravity when reaching mid-board

            Specials special = inventory[0];

            int maxPile = BoardHelper.GetPileMaxHeight(board);
            switch (special)
            {
                case Specials.NukeField:
                    if (maxPile >= 14)
                        advices.Add(new SpecialAdvice
                        {
                            SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.UseSelf,
                        });
                    break;
                case Specials.BlockGravity:
                    if (maxPile >= 10)
                        advices.Add(new SpecialAdvice
                        {
                            SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.UseSelf,
                        });
                    break;
                case Specials.ClearLines:
                    {
                        bool hasValuableBottomLine = HasNukeGravityOnBottomLine(board);
                        if ((!hasValuableBottomLine && maxPile > 4) || maxPile >= 14)
                            advices.Add(new SpecialAdvice
                            {
                                SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.UseSelf,
                            });
                        else if (maxPile >= 10 || inventory.Count + 2 >= inventoryMaxSize)
                            advices.Add(new SpecialAdvice
                            {
                                SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.UseSelf,
                            });
                        break;
                    }
                default:
                    advices.Add(new SpecialAdvice
                    {
                        SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.Discard,
                    });
                    break;
            }
            return true;
        }

        public bool PanicMode(List<Specials> inventory, List<IOpponent> opponents, List<SpecialAdvice> advices)
        {
            // Survival
            // If Nuke/Gravity/Switch
            if (inventory.Any(x => x == Specials.NukeField || x == Specials.BlockGravity || x == Specials.SwitchFields))
            {
                Log.WriteLine(Log.LogLevels.Debug, "[SURVIVAL] Found N/G/S in inventory");
                bool saved = false;
                while (true)
                {
                    if (!inventory.Any())
                        break; // Stops when inventory is empty
                    // Get strongest opponent
                    int strongest = GetStrongestOpponent(opponents);
                    // Get current special
                    Specials special = inventory[0];
                    inventory.RemoveAt(0);
                    switch (special)
                    {
                        case Specials.NukeField: // Nuke/Gravity -> use it immediately
                        case Specials.BlockGravity:
                            Log.WriteLine(Log.LogLevels.Debug, "[SURVIVAL]Use {0}", special.ToString());
                            advices.Add(new SpecialAdvice
                            {
                                SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.UseSelf,
                            });
                            saved = true; // Saved, stop emptying inventory
                            break;
                        case Specials.SwitchFields: // Switch -> use it only if strongest is really strong, else drop it
                            {
                                IBoard strongestBoard = opponents.First(x => x.PlayerId == strongest).Board;
                                int pileHeight = BoardHelper.GetPileMaxHeight(strongestBoard);
                                if (pileHeight <= 10)
                                {
                                    Log.WriteLine(Log.LogLevels.Debug, "[SURVIVAL]Use S, found a valid opponent {0} with a pile {1}", strongest, pileHeight);
                                    advices.Add(new SpecialAdvice
                                    {
                                        SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.UseOpponent,
                                        OpponentId = strongest
                                    });
                                    saved = true; // Saved, stop emptying inventory
                                }
                                else
                                {
                                    Log.WriteLine(Log.LogLevels.Debug, "[SURVIVAL]Discard S, no valid opponent");
                                    advices.Add(new SpecialAdvice
                                    {
                                        SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.Discard,
                                    });
                                }
                                break;
                            }
                        case Specials.ClearLines: // ClearLine -> use it immediately  TODO ==> this could lead to unwanted behaviour if we are emptying for a switch
                            Log.WriteLine(Log.LogLevels.Debug, "[SURVIVAL]Use C on ourself");
                            advices.Add(new SpecialAdvice
                            {
                                SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.UseSelf,
                            });
                            break;
                        default: // Other -> use it on strongest
                            Log.WriteLine(Log.LogLevels.Debug, "[SURVIVAL]Use {0} on strongest opponent {1}", special, strongest);
                            advices.Add(new SpecialAdvice
                            {
                                SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.UseOpponent,
                                OpponentId = strongest
                            });

                            break;
                    }

                    if (saved) // If saved or something wrong with UseSpecial, stop loop
                        break;

                    System.Threading.Thread.Sleep(10); // delay next special use
                }
                Log.WriteLine(Log.LogLevels.Debug, "[SURVIVAL] Survival mode N/G/S finished. saved:{0}", saved);
            }
            // Nothing could save use, send everything to weakest and pray 
            else
            {
                Log.WriteLine(Log.LogLevels.Debug, "[SURVIVAL] No N/G/S found -> use everything on weakest and try to kill him");
                while (true)
                {
                    if (!inventory.Any())
                        break; // Stops when inventory is empty
                    // Get strongest opponent
                    int weakest = GetWeakestOpponent(opponents);
                    // Get current special
                    Specials special = inventory[0];
                    inventory.RemoveAt(0);
                    // ClearLine -> use it immediately
                    if (special == Specials.ClearLines)
                    {
                        Log.WriteLine(Log.LogLevels.Debug, "[SURVIVAL]Use C on ourself");
                        advices.Add(new SpecialAdvice
                        {
                            SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.UseSelf,
                        });

                    }
                    // Other -> use it on weakest
                    else
                    {
                        Log.WriteLine(Log.LogLevels.Debug, "[SURVIVAL]Use {0} on weakest opponent {1}", special.ToString(), weakest);
                        advices.Add(new SpecialAdvice
                        {
                            SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.UseOpponent,
                            OpponentId = weakest
                        });

                    }
                }
            }
            return true;
        }

        public bool OneOpponentMode(int lastOpponentId, List<Specials> inventory, List<SpecialAdvice> advices)
        {
            Log.WriteLine(Log.LogLevels.Debug, "[KILLING LAST]Trying to kill last opponent with A");
            while (true)
            {
                if (!inventory.Any())
                    break; // Stops when inventory is empty
                // Get current special
                Specials currentSpecial = inventory[0];
                inventory.RemoveAt(0);

                // AddLines -> use it immediately on last opponent
                if (currentSpecial == Specials.AddLines)
                {
                    Log.WriteLine(Log.LogLevels.Debug, "[KILLING LAST]Use A on last opponent {0}", lastOpponentId);
                    advices.Add(new SpecialAdvice
                    {
                        SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.UseOpponent,
                        OpponentId = lastOpponentId
                    });
                }
                else if (currentSpecial == Specials.BlockQuake || currentSpecial == Specials.Confusion || currentSpecial == Specials.Darkness)
                {
                    Log.WriteLine(Log.LogLevels.Debug, "[KILLING LAST]Use {0} on last opponent {1}", currentSpecial, lastOpponentId);
                    advices.Add(new SpecialAdvice
                    {
                        SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.UseOpponent,
                        OpponentId = lastOpponentId
                    });
                }
                // Else, discard
                else
                {
                    Log.WriteLine(Log.LogLevels.Debug, "[KILLING LAST]Discard {0}", currentSpecial.ToString());
                    advices.Add(new SpecialAdvice
                    {
                        SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.Discard,
                    });
                }

                System.Threading.Thread.Sleep(10); // delay next special use
            }
            return true; // stops here
        }

        public bool MultiplayerMode(IBoard board, List<Specials> inventory, int inventoryMaxSize, List<IOpponent> opponents, List<SpecialAdvice> advices)
        {
            // Get strongest opponent
            int strongest = GetStrongestOpponent(opponents);

            // Get current special
            Specials special = inventory[0];
            inventory.RemoveAt(0);

            //
            switch (special)
            {
                // Destroy own board and switch with strongest opponent
                case Specials.SwitchFields: // TODO:
                    Log.WriteLine(Log.LogLevels.Debug, "[NORMAL]Keep S for survival");
                    // NOP
                    break;
                // Wait
                case Specials.BlockGravity:
                case Specials.NukeField:
                    Log.WriteLine(Log.LogLevels.Debug, "[NORMAL]Keep {0} for survival", special.ToString());
                    // NOP
                    break;
                // Send to strongest opponent
                case Specials.AddLines:
                    Log.WriteLine(Log.LogLevels.Debug, "[NORMAL]Use A on strongest {0}", strongest);
                    advices.Add(new SpecialAdvice
                    {
                        SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.UseOpponent,
                        OpponentId = strongest
                    });

                    break;
                //  Send to strongest opponent with a bomb
                //  If none,
                //      if inventory almost full, drop
                //      else NOP
                case Specials.BlockBomb:
                    {
                        int bombTarget = GetStrongestOpponentWithBomb(opponents);
                        if (bombTarget != -1)
                        {
                            Log.WriteLine(Log.LogLevels.Debug, "[NORMAL]use O to {0}", bombTarget);
                            advices.Add(new SpecialAdvice
                            {
                                SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.UseOpponent,
                                OpponentId = bombTarget
                            });

                        }
                        else
                        {
                            if (inventory.Count + 2 >= inventoryMaxSize)
                            {
                                Log.WriteLine(Log.LogLevels.Debug, "[NORMAL]Discard O, inventory almost full");
                                advices.Add(new SpecialAdvice
                                {
                                    SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.Discard,
                                });
                            }
                            else
                            {
                                Log.WriteLine(Log.LogLevels.Debug, "[NORMAL]Keep O for later");
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
                        int targetId = GetStrongestOpponentWithNukeSwitchGravityOnBottomLine(opponents);
                        if (targetId != -1)
                        {
                            Log.WriteLine(Log.LogLevels.Debug, "[NORMAL]Use C on opponent with N/G/S on bottom line {0}", targetId);
                            advices.Add(new SpecialAdvice
                            {
                                SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.UseOpponent,
                                OpponentId = targetId
                            });

                        }
                        else
                        {
                            targetId = GetStrongestOpponentWithBombOnBottomLine(opponents);
                            if (targetId != -1)
                            {
                                Log.WriteLine(Log.LogLevels.Debug, "[NORMAL]Use C on opponent with O on bottom line {0}", targetId);
                                advices.Add(new SpecialAdvice
                                {
                                    SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.UseOpponent,
                                    OpponentId = targetId
                                });
                            }
                            else
                            {
                                targetId = GetOpponentWithMostSpecialsOnBottomLine(opponents);
                                if (targetId != -1)
                                {
                                    Log.WriteLine(Log.LogLevels.Debug, "[NORMAL]Use C on opponent with most specials on bottom line {0}", targetId);
                                    advices.Add(new SpecialAdvice
                                    {
                                        SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.UseOpponent,
                                        OpponentId = targetId
                                    });
                                }
                                else
                                {
                                    bool hasValuableSpecialOnBottomLine = HasNukeGravitySwitchOnBottomLine(board);
                                    if (hasValuableSpecialOnBottomLine)
                                    {
                                        Log.WriteLine(Log.LogLevels.Debug, "[NORMAL]Discard C, we have N/G/S on bottom line");
                                        advices.Add(new SpecialAdvice
                                        {
                                            SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.Discard,
                                        });
                                    }
                                    else
                                    {
                                        Log.WriteLine(Log.LogLevels.Debug, "[NORMAL]Use C on ourself");
                                        advices.Add(new SpecialAdvice
                                        {
                                            SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.UseSelf,
                                        });
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
                        bool hasBomb = HasSpecial(board, Specials.BlockBomb);
                        if (hasBomb && !inventory.Any(x => x == Specials.NukeField || x == Specials.BlockGravity))
                        {
                            Log.WriteLine(Log.LogLevels.Debug, "[NORMAL]Use B on ourself, we have O in board and no N/G in inventory");
                            advices.Add(new SpecialAdvice
                            {
                                SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.UseSelf,
                            });
                        }
                        else
                        {
                            int targetId = GetStrongestOpponentWithNukeSwitchGravity(opponents);
                            if (targetId != -1)
                            {
                                Log.WriteLine(Log.LogLevels.Debug, "[NORMAL]Use B on strongest opponent with N/S/G {0}", targetId);
                                advices.Add(new SpecialAdvice
                                {
                                    SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.UseOpponent,
                                    OpponentId = targetId
                                });
                            }
                            else
                            {
                                if (inventory.Any() && inventory[0] == Specials.BlockBomb)
                                {
                                    bool hasOpponentWithBomb = HasOpponentWithBomb(opponents);
                                    if (hasOpponentWithBomb)
                                    {
                                        targetId = GetOpponentWithMostSpecialsAndNoBomb(opponents);
                                        if (targetId != -1)
                                        {
                                            Log.WriteLine(Log.LogLevels.Debug, "[NORMAL]Use B on opponent with most specials and no O {0}, we have O in inventory", targetId);
                                            advices.Add(new SpecialAdvice
                                            {
                                                SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.UseOpponent,
                                                OpponentId = targetId
                                            });
                                        }
                                        else
                                        {
                                            Log.WriteLine(Log.LogLevels.Debug, "[NORMAL]Discard B, no opponents without O and we have O in inventory", targetId);
                                            advices.Add(new SpecialAdvice
                                            {
                                                SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.Discard,
                                            });
                                        }
                                    }
                                    else
                                    {
                                        targetId = GetOpponentWithMostSpecials(opponents);
                                        if (targetId != -1)
                                        {
                                            Log.WriteLine(Log.LogLevels.Debug, "[NORMAL]Use B on opponent with most specials {0}", targetId);
                                            advices.Add(new SpecialAdvice
                                            {
                                                SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.UseOpponent,
                                                OpponentId = targetId
                                            });
                                        }
                                        else
                                        {
                                            if (inventory.Count + 2 >= inventoryMaxSize)
                                            {
                                                Log.WriteLine(Log.LogLevels.Debug, "[NORMAL]Discard B, inventory is almost full");
                                                advices.Add(new SpecialAdvice
                                                {
                                                    SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.Discard,
                                                });
                                            }
                                            else
                                            {
                                                Log.WriteLine(Log.LogLevels.Debug, "[NORMAL]Keep B for later");
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    targetId = GetStrongestOpponentWithBomb(opponents);
                                    if (targetId != -1)
                                    {
                                        Log.WriteLine(Log.LogLevels.Debug, "[NORMAL]Use B on opponent with O {0}, we don't have any O", targetId);
                                        advices.Add(new SpecialAdvice
                                        {
                                            SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.UseOpponent,
                                            OpponentId = targetId
                                        });
                                    }
                                    else
                                    {
                                        targetId = GetOpponentWithMostSpecials(opponents);
                                        if (targetId != -1)
                                        {
                                            Log.WriteLine(Log.LogLevels.Debug, "[NORMAL]Use B on opponent with most specials {0}", targetId);
                                            advices.Add(new SpecialAdvice
                                            {
                                                SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.UseOpponent,
                                                OpponentId = targetId
                                            });
                                        }
                                        else
                                        {
                                            if (inventory.Count + 2 >= inventoryMaxSize)
                                            {
                                                Log.WriteLine(Log.LogLevels.Debug, "[NORMAL]Discard B, inventory is almost full");
                                                advices.Add(new SpecialAdvice
                                                {
                                                    SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.Discard,
                                                });
                                            }
                                            else
                                            {
                                                Log.WriteLine(Log.LogLevels.Debug, "[NORMAL]Keep B for later");
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
                        int targetId = GetStrongestOpponentWithNukeSwitchGravity(opponents);
                        if (targetId != -1)
                        {
                            Log.WriteLine(Log.LogLevels.Debug, "[NORMAL]Use R on strongest opponent with N/S/G {0}", targetId);
                            advices.Add(new SpecialAdvice
                            {
                                SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.UseOpponent,
                                OpponentId = targetId
                            });
                        }
                        else
                        {
                            targetId = GetStrongestOpponent(opponents);
                            if (targetId != -1)
                            {
                                Log.WriteLine(Log.LogLevels.Debug, "[NORMAL]Use R on strongest opponent {0}", targetId);
                                advices.Add(new SpecialAdvice
                                {
                                    SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.UseOpponent,
                                    OpponentId = targetId
                                });
                            }
                            else
                            {
                                Log.WriteLine(Log.LogLevels.Debug, "[NORMAL]Keep R for later **** SHOULD NEVER HAPPEN");
                            }
                        }
                        break;
                    }
                    // send to weakest opponent
                case Specials.Immunity:
                    {
                        int targetId = GetWeakestOpponent(opponents);
                        if (targetId != -1)
                        {
                            IBoard lastOpponentBoard = opponents.First(x => x.PlayerId == targetId).Board;
                            if (lastOpponentBoard != null)
                            {
                                int pileHeight = BoardHelper.GetPileMaxHeight(lastOpponentBoard);
                                if (pileHeight >= 10)
                                {
                                    Log.WriteLine(Log.LogLevels.Debug, "[NORMAL]Use I on weakest opponent (board {0}) {1}", pileHeight, targetId);
                                    advices.Add(new SpecialAdvice
                                        {
                                            SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.UseOpponent,
                                            OpponentId = targetId
                                        });
                                }
                                else if (inventory.Count + 2 >= inventoryMaxSize)
                                {
                                    Log.WriteLine(Log.LogLevels.Debug, "[NORMAL]Discard I, inventory is almost full");
                                    advices.Add(new SpecialAdvice
                                    {
                                        SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.Discard,
                                    });
                                }
                                else
                                {
                                    Log.WriteLine(Log.LogLevels.Debug, "[NORMAL]Keep I for later");
                                }
                            }
                        }
                        break;
                    }
                    // TODO
                //  if first special is Quake,
                //      send to opponent with most towers or strongest opponent [TO BE DEFINED]
                // ClearColumn
                // Darkness
                // Confusion
                // ZebraField

                // Send to strongest opponent
                default:
                    {
                        int targetId = GetStrongestOpponent(opponents);
                        if (targetId != -1)
                        {
                            Log.WriteLine(Log.LogLevels.Debug, "[NORMAL]Use {0} on strongest opponent {1}", special.ToString(), targetId);
                            advices.Add(new SpecialAdvice
                            {
                                SpecialAdviceAction = SpecialAdvice.SpecialAdviceActions.UseOpponent,
                                OpponentId = targetId
                            });
                        }
                        else
                        {
                            Log.WriteLine(Log.LogLevels.Debug, "[NORMAL]Keep {0} for later **** SHOULD NEVER HAPPEN", special.ToString());
                        }
                        break;
                    }
            }
            return true;
        }

        private static int GetStrongestOpponent(IEnumerable<IOpponent> opponents) // Get opponent with lowest pile height
        {
            int strongest = -1;
            int strongestPileHeight = 1000;
            foreach (IOpponent opponent in opponents)
            {
                int pileHeight = BoardHelper.GetPileMaxHeight(opponent.Board);
                if (pileHeight < strongestPileHeight)
                {
                    strongestPileHeight = pileHeight;
                    strongest = opponent.PlayerId;
                }
            }
            return strongest;
        }

        private static int GetWeakestOpponent(IEnumerable<IOpponent> opponents) // Get opponent with highest pile height
        {
            int weakest = -1;
            int weakestPileHeight = -1000;
            foreach (IOpponent opponent in opponents)
            {
                int pileHeight = BoardHelper.GetPileMaxHeight(opponent.Board);
                if (pileHeight > weakestPileHeight)
                {
                    weakest = opponent.PlayerId;
                    weakestPileHeight = pileHeight;
                }
            }
            return weakest;
        }

        private static int GetLastOpponent(List<IOpponent> opponents) // Return opponent id if there is only one opponent left
        {
            if (opponents.Count() == 1)
                return opponents.First().PlayerId;
            return -1;
        }

        private static int GetStrongestOpponentWithBomb(IEnumerable<IOpponent> opponents) // Search among opponents which one has a bomb and the lowest board
        {
            int id = -1;
            int lowestPileHeight = 1000;
            foreach (IOpponent opponent in opponents)
            {
                if (opponent.Board.Cells.Any(x => CellHelper.IsSpecial(x) && CellHelper.GetSpecial(x) == Specials.BlockBomb))
                {
                    int pileHeight = BoardHelper.GetPileMaxHeight(opponent.Board);
                    if (pileHeight < lowestPileHeight)
                    {
                        id = opponent.PlayerId;
                        lowestPileHeight = pileHeight;
                    }
                }
            }
            return id;
        }

        private static int GetStrongestOpponentWithNukeSwitchGravityOnBottomLine(IEnumerable<IOpponent> opponents) // Search among opponents which one has a nuke/switch/gravity on bottom line and the lowest board
        {
            int id = -1;
            int lowestPileHeight = 1000;
            foreach (IOpponent opponent in opponents)
            {
                for (int x = 1; x <= opponent.Board.Width; x++)
                {
                    Specials cellSpecial = CellHelper.GetSpecial(opponent.Board[x, 0]);
                    if (cellSpecial == Specials.BlockGravity || cellSpecial == Specials.NukeField || cellSpecial == Specials.SwitchFields)
                    {
                        int pileHeight = BoardHelper.GetPileMaxHeight(opponent.Board);
                        if (pileHeight < lowestPileHeight)
                        {
                            id = opponent.PlayerId;
                            lowestPileHeight = pileHeight;
                        }
                        break;
                    }
                }
            }
            return id;
        }

        private static int GetStrongestOpponentWithBombOnBottomLine(IEnumerable<IOpponent> opponents) // Search among opponents which one has a bomb on bottom line and the lowest board
        {
            int id = -1;
            int lowestPileHeight = 1000;
            foreach (IOpponent opponent in opponents)
            {
                for (int x = 1; x <= opponent.Board.Width; x++)
                {
                    Specials cellSpecial = CellHelper.GetSpecial(opponent.Board[x, 0]);
                    if (cellSpecial == Specials.BlockBomb)
                    {
                        int pileHeight = BoardHelper.GetPileMaxHeight(opponent.Board);
                        if (pileHeight < lowestPileHeight)
                        {
                            id = opponent.PlayerId;
                            lowestPileHeight = pileHeight;
                        }
                        break;
                    }
                }
            }
            return id;
        }

        private static int GetOpponentWithMostSpecialsOnBottomLine(IEnumerable<IOpponent> opponents) // Search among opponents which one has most specials on bottom line
        {
            int id = -1;
            int mostSpecial = 0;
            foreach (IOpponent opponent in opponents)
            {
                int countSpecial = 0;
                for (int x = 1; x <= opponent.Board.Width; x++)
                    if (CellHelper.IsSpecial(opponent.Board[x, 0]))
                        countSpecial++;
                if (countSpecial > mostSpecial)
                {
                    id = opponent.PlayerId;
                    mostSpecial = countSpecial;
                }
            }
            return id;
        }

        private static int GetOpponentWithMostSpecials(IEnumerable<IOpponent> opponents) // Search among opponents which one has most specials
        {
            int id = -1;
            int mostSpecials = 0;
            foreach (IOpponent opponent in opponents)
            {
                int countSpecial = opponent.Board.Cells.Count(x => CellHelper.GetSpecial(x) != Specials.Invalid);
                if (countSpecial > mostSpecials)
                {
                    id = opponent.PlayerId;
                    mostSpecials = countSpecial;
                }
            }
            return id;
        }

        private static bool HasNukeGravitySwitchOnBottomLine(IBoard board)
        {
            for (int x = 1; x <= board.Width; x++)
            {
                Specials cellSpecial = CellHelper.GetSpecial(board[x, 0]);
                if (cellSpecial == Specials.BlockGravity || cellSpecial == Specials.NukeField || cellSpecial == Specials.SwitchFields)
                    return true;
            }
            return false;
        }

        private static bool HasSpecial(IBoard board, Specials special)
        {
            return board.Cells.Any(x => CellHelper.GetSpecial(x) == special);
        }

        private static int GetStrongestOpponentWithNukeSwitchGravity(IEnumerable<IOpponent> opponents) // Search among opponents which one has a nuke/switch/gravity and the lowest board
        {
            int id = -1;
            int lowestPileHeight = 1000;
            foreach (IOpponent opponent in opponents)
            {
                if (opponent.Board.Cells.Any(x => CellHelper.GetSpecial(x) == Specials.SwitchFields || CellHelper.GetSpecial(x) == Specials.NukeField || CellHelper.GetSpecial(x) == Specials.BlockGravity))
                {
                    int pileHeight = BoardHelper.GetPileMaxHeight(opponent.Board);
                    if (pileHeight < lowestPileHeight)
                    {
                        id = opponent.PlayerId;
                        lowestPileHeight = pileHeight;
                    }
                }
            }
            return id;
        }

        private static bool HasOpponentWithBomb(IEnumerable<IOpponent> opponents) // Search among opponents, if there is one with a bomb
        {
            return opponents.Any(opponent => opponent.Board.Cells.Any(x => CellHelper.GetSpecial(x) == Specials.BlockBomb));
        }

        private static int GetOpponentWithMostSpecialsAndNoBomb(IEnumerable<IOpponent> opponents) // Search among opponents which one has most specials and no bomb
        {
            int id = -1;
            int mostSpecials = 0;
            foreach (IOpponent opponent in opponents)
            {
                bool hasBomb = opponent.Board.Cells.Any(x => CellHelper.GetSpecial(x) == Specials.BlockBomb);
                if (!hasBomb)
                {
                    int countSpecial = opponent.Board.Cells.Count(x => CellHelper.GetSpecial(x) != Specials.Invalid);
                    if (countSpecial > mostSpecials)
                    {
                        id = opponent.PlayerId;
                        mostSpecials = countSpecial;
                    }
                }
            }
            return id;
        }

        private static bool HasNukeGravityOnBottomLine(IBoard board)
        {
            for (int x = 1; x <= board.Width; x++)
            {
                Specials cellSpecial = CellHelper.GetSpecial(board[x, 0]);
                if (cellSpecial == Specials.BlockGravity || cellSpecial == Specials.NukeField)
                    return true;
            }
            return false;
        }
    }
}
