using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using System.ComponentModel;
using Tetris.Model.Blocks;

namespace Tetris.Model
{
    // TODO: use BlockBackground non-moving parts
    // new event to redraw only some part
    public class Tetris : INotifyPropertyChanged
    {
        #region Fields

        private int _level;
        private int _clearedLines;
        private int _score;
        private Block _nextBlock;
        private Random _random;

        #endregion

        #region Properties

        /// <summary>
        /// Grid used to be a [18,10] array of Parts, but since every Block holds its absolute position 
        /// and the parts theirs relative to it, the Grid is now a List<Part/> which makes it much easier to use for certain operations
        /// </summary>
        public List<Part> Grid { get; set; }
        public Block CurrentBlock { get; set; }
        public bool IsPaused { get; private set; }
        private DispatcherTimer GameTimer { get; set; }

        //In the original tetris, the user gets additional points depending on the number of MoveDown steps he triggered manually for each block
        //A user invoked MoveDown() increments this counter, a KeyUp event for the "Down"-Key resets it
        private int SoftDrops { get; set; }

        public int Level
        {
            get { return _level; }
            set
            {
                _level = value;
                OnPropertyChanged("Level");
            }
        }
        public int ClearedLines
        {
            get { return _clearedLines; }
            set
            {
                _clearedLines = value;
                OnPropertyChanged("ClearedLines");
            }
        }
        public int Score
        {
            get { return _score; }
            set
            {
                _score = value;
                OnPropertyChanged("Score");
            }
        }
        public Block NextBlock
        {
            get { return _nextBlock; }
            set
            {
                _nextBlock = value;
                OnPropertyChanged("NextBlock");
            }
        }

        #endregion

        #region Constructor

        public Tetris()
        {
            _random = new Random();

            Grid = new List<Part>();

            #region Initialize the timer

            GameTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(800),
                };
            GameTimer.Tick += TetrisTick;

            #endregion

            IsPaused = true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Start the game
        /// </summary>
        public void StartGame()
        {
            NextBlock = Block.NewBlock(Grid); //Create a "NextBlock" in the beginning
            NewBlock();
            GameTimer.Start();
            IsPaused = false;
        }

        /// <summary>
        /// Pause the game at its current state
        /// </summary>
        public void PauseGame()
        {
            GameTimer.Stop();
            IsPaused = true;
            if (IsPausedChanged != null)
            {
                IsPausedChanged();
            }
        }

        /// <summary>
        /// Resume the game after a previous pause
        /// </summary>
        public void ResumeGame()
        {
            GameTimer.Start();
            IsPaused = false;
            if (IsPausedChanged != null)
            {
                IsPausedChanged();
            }
        }

        /// <summary>
        /// Called by the timer to let the Block fall down
        /// </summary>
        private void TetrisTick(object sender, EventArgs e)
        {
            MoveDown(sender);
        }

        /// <summary>
        /// Generates a random new Block as the CurrentBlock and adds its Parts to the Grid.
        /// </summary>
        /// <returns>False in case of the block couldn't be place at the spawning location (which means its Game Over)</returns>
        private bool NewBlock()
        {
            //The NextBlock is now the CurrentBlock
            CurrentBlock = NextBlock;

            //Create a random new NextBlock
            NextBlock = Block.NewBlock(Grid);

            Grid.AddRange(CurrentBlock.Parts);

            if (NewBlockAdded != null)
            {
                NewBlockAdded();
            }

            #region Check for GameOver

            if (CurrentBlock.HasPositionConflict())
                return false;

            #endregion

            return true;
        }

        /// <summary>
        /// Wraps NewBlock and calls CheckForCompleteRows() (Stops the timer)
        /// </summary>
        private void FinishRound()
        {
            //Stop the timer
            GameTimer.Stop();
            IsPaused = true;

            if (RoundFinished != null)
            {
                RoundFinished();
            }

            var rows = CheckForCompleteRows();

            if (rows.Count() > 0)
                if (RowsCompleting != null)
                {
                    RowsCompleting(rows);
                }

            #region Delete all parts in every completed row

            foreach (var row in rows)
            {
                DeleteRow(row);
            }

            if (rows.Count() > 0)
                if (RowsCompleted != null)
                {
                    RowsCompleted(rows);
                }

            #endregion

            //Increase the counter for the cleared Lines
            ClearedLines += rows.Count();

            #region Add points for deleted rows (level+1)*c (c=40, 100, 300, 1200 based on how many rows were cleared)

            //Dirty code...
            if (rows.Count() == 1) Score += (Level + 1)*40;
            if (rows.Count() == 2) Score += (Level + 1)*100;
            if (rows.Count() == 3) Score += (Level + 1)*300;
            if (rows.Count() == 4) Score += (Level + 1)*1200;

            #endregion

            #region Add points for softdrops

            Score += SoftDrops;
            ResetSoftDrop();

            #endregion

            #region Check if the level has increased

            if (Level < ClearedLines/10)
                NextLevel();

            #endregion

            #region Create a new block, invoke GameOver if it fails or continue be restarting the timer

            if (NewBlock())
            {
                GameTimer.Start();
                IsPaused = false;
            }
            else
                EndGame();

            #endregion
        }

        /// <summary>
        /// Searches the Grid for complete rows every round and deletes it
        /// </summary>
        /// <returns>The y-value of the row to be deleted</returns>
        private int[] CheckForCompleteRows()
        {
            #region Get all full rows, (grouped by PosY == 10)

            var completeRows = Grid.GroupBy(p => p.PosY)
                .Where(r => r.Count() == 10)
                .Select(ps => new
                    {
                        Row = ps.First().PosY
                    })
                .ToList();

            #endregion

            //Turn it into an int array
            return completeRows.Select(i => i.Row).OrderBy(r => r).ToArray();
        }

        /// <summary>
        /// Deletes a complete row. It overrides the Parts in the completed row and rearranges the Grid
        /// </summary>
        /// <param name="row">The row in the complete Grid that will be deleted.</param>
        private void DeleteRow(int row)
        {
            //Get all parts which have to be removed
            var parts = Grid.Where(p => p.PosY == row).ToList();

            //Remove them from the Grid
            Grid.RemoveAll(p => p.PosY == row);

            #region Perform the deletion on every affected block to delete the parts from their blocks and rearrange the remaining ones

            //Get all affected blocks
            parts.GroupBy(p => p.ParentBlock)
                .Select(ps => ps.First().ParentBlock)
                .ToList()
                .ForEach(b => b.DeleteRow(row));

            #endregion

            #region Rearrange the rest

            //Get all blocks above the just deleted row (parts grouped by their block so every block only moves down once)
            var blocks = Grid.Where(p => p.PosY < row)
                .GroupBy(p => p.ParentBlock)
                .Select(ps => ps.First().ParentBlock).ToList();

            //Move the blocks (conflicts dont matter because the blocks are already locked)
            blocks.ForEach(b => b.MoveDown());

            #endregion
        }

        /// <summary>
        /// Next Level every 10 completed rows. (Increases the speed)
        /// </summary>
        private void NextLevel()
        {
            Level = ClearedLines/10;
            //Every Level the Block moves down 50 ms faster
            GameTimer.Interval -= TimeSpan.FromMilliseconds(4*Level + 50.0);
        }

        /// <summary>
        /// End the game regardless of its current state.
        /// This is also called when NewBlock() cant place the new created block due to a conflict.
        /// </summary>
        public void EndGame()
        {
            GameTimer.Stop();
            if (GameOver != null)
            {
                GameOver(Score);
            }
        }

        /// <summary>
        /// In the original tetris, the user gets additional points depending on the number of MoveDown steps he triggered manually for each block
        /// A user invoked MoveDown() increments this counter, a KeyUp event for the "Down"-Key resets it
        /// </summary>
        public void ResetSoftDrop()
        {
            SoftDrops = 0;
        }

        #region Wrap the CurrentBlocks move methods

        /// <summary>
        /// Moves the CurrentBlock down (if it fails, the next round will be started)
        /// </summary>
        public void MoveDown(object sender)
        {
            if (BlockMoving != null)
            {
                BlockMoving();
            }

            var ret = CurrentBlock.MoveDown();

            if (BlockMoved != null)
            {
                BlockMoved();
            }

            #region Handle the moves result

            if (!ret) 
                FinishRound();
            else if (sender as DispatcherTimer == null) 
                SoftDrops++;
            //A user invoked MoveDown() is awarded with points

            #endregion
        }

        /// <summary>
        /// Moves the CurrentBlock left (if it fails nothing happens)
        /// </summary>
        public void MoveLeft()
        {
            if (BlockMoving != null)
            {
                BlockMoving();
            }
            CurrentBlock.MoveLeft();
            if (BlockMoving != null)
            {
                BlockMoved();
            }
        }

        /// <summary>
        /// Moves the CurrentBlock right (if it fails nothing happens)
        /// </summary>
        public void MoveRight()
        {
            if (BlockMoving != null)
            {
                BlockMoving();
            }
            CurrentBlock.MoveRight();
            if (BlockMoving != null)
            {
                BlockMoved();
            }
        }

        /// <summary>
        /// Rotates the CurrentBlock (if it fails nothing happens)
        /// </summary>
        public void Rotate()
        {
            if (BlockMoving != null)
            {
                BlockMoving();
            }
            CurrentBlock.Rotate();
            if (BlockMoving != null)
            {
                BlockMoved();
            }
        }

        #endregion

        /// <summary>
        /// Wraps the PropertyChanged-Event.
        /// </summary>
        /// <param name="property">The name of the property that changed.</param>
        private void OnPropertyChanged(string property)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(property));
        }

        /// <summary>
        /// Output all values and a simple Grid visualization on the console
        /// </summary>
        public void DebugOutput()
        {
            Debug.WriteLine("============== Current Tetris State ==============");
            Debug.WriteLine("Game State:\t\t" + IsPaused.ToString());
            Debug.WriteLine("Level:\t\t\t" + Level.ToString(CultureInfo.InvariantCulture));
            Debug.WriteLine("ClearedLines:\t" + ClearedLines.ToString(CultureInfo.InvariantCulture));
            Debug.WriteLine("Score:\t\t\t" + Score.ToString(CultureInfo.InvariantCulture));
            Debug.WriteLine("Current Block:\t" + CurrentBlock.GetType().Name);
            Debug.WriteLine("\t\t\t\t" + CurrentBlock.PosX.ToString(CultureInfo.InvariantCulture));
            Debug.WriteLine("\t\t\t\t" + CurrentBlock.PosY.ToString(CultureInfo.InvariantCulture));
            Debug.WriteLine("Next Block:\t\t" + NextBlock.GetType().Name);
            Debug.WriteLine("--- Current Grid State ---");

            #region Output the grid

            Debug.WriteLine("+-+-+-+-+-+-+-+-+-+-+");
            for (int i = 0; i < 18; i++)
            {
                StringBuilder line = new StringBuilder(10);
                line.Append("|");
                for (int j = 0; j < 10; j++)
                {
                    int count = Grid.Count(p => p.PosY == i && p.PosX == j);
                    if (count > 1)
                        line.Append(count);
                    else if (count == 1)
                        line.Append("X");
                    else
                        line.Append(" ");
                }
                line.Append("|");
                Debug.WriteLine(line.ToString());
            }
            Debug.WriteLine("+-+-+-+-+-+-+-+-+-+-+");

            #endregion

            Debug.WriteLine("==================================================");
        }

        #region Attacks
        /// <summary>
        /// Add <param name="count"/> junk lines
        /// </summary>
        /// <param name="count">Number of lines to add</param>
        public void AddLines(int count)
        {
            if (count <= 0)
                return;

            // Create block in bottom row
            BlockAdditionalRows blockAdditionalRows = new BlockAdditionalRows(Grid, count)
            {
                PosX = 0,
                PosY = 18 - count
            };

            // Move all parts 'rows' line above (except current block)
            var blocks = Grid
                .Where(p => p.PosY <= 17 && p.ParentBlock != CurrentBlock)
                .GroupBy(p => p.ParentBlock)
                .Select(ps => ps.First().ParentBlock).ToList();
            blocks.ForEach(block =>
            {
                // Remove parts
                block.Parts.ForEach(p => Grid.Remove(p));
                // Move up
                block.PosY -= count;
                // Readd parts
                Grid.AddRange(block.Parts);
            }
            );

            // Add block to grid
            Grid.AddRange(blockAdditionalRows.Parts);

            // Inform View
            if (AttackReceived != null)
                AttackReceived();

            // Check for game over
            if (CurrentBlock.HasPositionConflict())
                EndGame();
        }

        /// <summary>
        /// Clear bottom line
        /// </summary>
        public void ClearLine()
        {
            DeleteRow(18);
            // No need to inform view, DeleteRow already does it
        }

        /// <summary>
        /// Remove all blocks
        /// </summary>
        public void NukeField()
        {
            Grid.Clear();

            // Inform View
            if (AttackReceived != null)
                AttackReceived();
        }

        /// <summary>
        /// Remove <paramref name="count"/> random blocks
        /// </summary>
        /// <param name="count"></param>
        public void RandomBlocksClear(int count)
        {
            // Delete 'count' random block
            for(int i = 0; i < count; i++)
            {
                // Get random position
                int x = _random.Next(10);
                int y = _random.Next(18);

                // Remove parts
                Grid.RemoveAll(p => p.ParentBlock != CurrentBlock && p.PosX == x && p.PosY == y);
            }
            // Inform View
            if (AttackReceived != null)
                AttackReceived();
        }

        //TODO: public void SwitchField()           impossible while part and block are associated
        //TODO: public void ClearSpecialBlocks()    special blocks to yet implemented
        //TODO: public void Gravity()               impossible while part and block are associated
        //TODO: public void Quake()                 impossible while part and block are associated
        //TODO: public void Bomb()                  impossible while part and block are associated

        #endregion
        #endregion

        #region Events

        //Delegates
        public delegate void NewBlockEventHandler();
        public delegate void BlockMovingEventHandler();
        public delegate void BlockMovedEventHandler();
        public delegate void RowsCompletingEventHandler(int[] rows);
        public delegate void RowsCompletedEventHandler(int[] rows);
        public delegate void AttackReceivedEventHandler();
        public delegate void GameOverEventHandler(int score);
        public delegate void IsPausedChangedEventHandler();
        public delegate void RoundFinishedEventHandler();

        //Events
        public event NewBlockEventHandler NewBlockAdded; //The UI basically has to do the same as for BlockMoved, just redraw the CurrentBlock

        public event BlockMovingEventHandler BlockMoving; //BlockMoving (triggers the UI to delete the Parts of CurrentBlock)
        public event BlockMovedEventHandler BlockMoved; //BlockMoved  (display the CurrentBlocks parts again)

        public event RowsCompletingEventHandler RowsCompleting; //RowCompleting (triggers the GUI to display a flash on the Row)
        public event RowsCompletedEventHandler RowsCompleted; //RowCompleted (clear and redraw everything)

        public event AttackReceivedEventHandler AttackReceived;

        public event GameOverEventHandler GameOver; //GameOver() - display the GameOver screen

        public event IsPausedChangedEventHandler IsPausedChanged;

        public event PropertyChangedEventHandler PropertyChanged;

        public event RoundFinishedEventHandler RoundFinished;

        #endregion
    }
}