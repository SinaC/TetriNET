using System;
using System.Timers;
using TetriNET.Common.Interfaces;

namespace TetriNET.ConsoleWCFClient.GameController
{
    public class FirstBot
    {
        private readonly IClient _client;
        private readonly Func<ITetrimino, ITetrimino> _cloneTetrimino;
        private readonly Timer _timer;

        private readonly Random _random;

        public FirstBot(IClient client, Func<ITetrimino, ITetrimino> cloneTetrimino)
        {
            if (client == null)
                throw new ArgumentNullException("client");
            if (cloneTetrimino == null)
                throw new ArgumentNullException("cloneTetrimino");

            _client = client;
            _cloneTetrimino = cloneTetrimino;
            
            _timer = new Timer(100);
            _timer.Elapsed += TimerOnElapsed;

            _random = new Random();

            client.OnTetriminoPlaced += ClientOnTetriminoPlaced;
            client.OnGameStarted += ClientOnGameStarted;
            client.OnGameOver += ClientOnOnGameOver;
            client.OnGameFinished += ClientOnOnGameFinished;
        }

        #region IClient event handlers

        private void ClientOnTetriminoPlaced()
        {
            _timer.Start();
        }

        private void ClientOnGameStarted()
        {
            _timer.Start();
        }

        private void ClientOnOnGameOver()
        {
            _timer.Stop();
        }

        private void ClientOnOnGameFinished()
        {
            _timer.Stop();
        }

        #endregion

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            // Get own copy of grid
            byte[] grid = new byte[_client.Width * _client.Height];
            Array.Copy(_client.Grid, grid, _client.Width * _client.Height);

            // Simulate all combinations
            ITetrimino current = _cloneTetrimino(_client.CurrentTetrimino);
            ITetrimino next = _cloneTetrimino(_client.NextTetrimino);
            for (int rotation = 0; rotation < 4; rotation++)
            {
                // From left to right
                for (int posX = 0; posX < _client.Width; posX++)
                {
                    // From bottom to top, stop when no conflicts occurs
                    for (int posY = _client.Height - 1; posY >= 0; posY--)
                    {
                        bool partConflict = false;
                        for (int i = 0; i < current.Width * current.Height; i++)
                            if (current.Parts[i] > 0)
                            {
                                int partX = i % current.Width;
                                int partY = i / current.Width;

                                if (posX + partX >= 0 && posX + partX < _client.Width && posY + partY >= 0 && posY + partY < _client.Height)
                                {
                                    int linearPartInGrid = (partY + posY)*_client.Width + partX + posX;
                                    if (grid[linearPartInGrid] == 0)
                                    {
                                        partConflict = true;
                                        break;
                                    }
                                }
                            }
                        if (!partConflict)
                        {
                            // TODO: Compute score, store x, y, rotation if best than previous
                            // Stop when no part are in conflict
                            break;
                        }
                    }
                    // Next rotation
                    current.RotateClockwise(_client.Grid);
                }
            }
            // TODO: 
            //http://luckytoilet.wordpress.com/2011/05/27/coding-a-tetris-ai-using-a-genetic-algorithm/
            //  Simulate all combinations (rotations and positions) for current and next block
            //  Compute score for each combinations
            //  Move current block to position with highest score
            //To give a score for a position, we would use an equation like this:
            //Score = A * Sum of Heights
            //+ B * Number of Clears
            //+ C * Number of Holes
            //+ D * Number of Blockades

            //Where A, B, C, and D are weights that we decide — how important is each of the factors. I initially came up with some pretty arbitrary values:

            //-0.03 for the height multiplier
            //-7.5 per hole
            //-3.5 per blockade
            //+8.0 per clear
            //+3.0 for each edge touching another block
            //+2.5 for each edge touching the wall
            //+5.0 for each edge touching the floor
            _timer.Stop();

            int random = _random.Next(_client.Width) - (_client.Width/2);
            if (random < 0)
            {
                for(int i = 0; i <= -random; i++)
                    _client.CurrentTetrimino.MoveLeft(_client.Grid);
            }
            else if (random > 0)
            {
                for (int i = 0; i <= random; i++)
                    _client.CurrentTetrimino.MoveRight(_client.Grid);
            }

            _client.Drop();
        }
    }
}
