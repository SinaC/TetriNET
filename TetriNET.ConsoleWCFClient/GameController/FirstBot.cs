using System;
using System.Linq.Expressions;
using System.Timers;
using TetriNET.Common;
using TetriNET.Common.Interfaces;

namespace TetriNET.ConsoleWCFClient.GameController
{
    public class FirstBot
    {
        private const double HeightMultiplier = -0.03;
        private const double HoleMultiplier = -7.5;
        private const double BlocadeMultiplier = -3.5;
        private const double ClearMultiplier = 8.0;
        private const double EdgeTouchingAnotherBlockMultiplier = 3.0;
        private const double EdgeTouchingWallMultiplier = 2.5;
        private const double EdgeTouchingFloorMultiplier = 5.0;

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
            _timer.Stop();

            // Get own copy of grid
            byte[] grid = new byte[_client.Width*_client.Height];
            Array.Copy(_client.Grid, grid, _client.Width*_client.Height);

            double bestScore = -9999.99;
            int bestRotation = -99;
            int bestPosX = -99;

            //http://luckytoilet.wordpress.com/2011/05/27/coding-a-tetris-ai-using-a-genetic-algorithm/
            // Simulate all combinations
            for (int rotation = 0; rotation < 4; rotation++)
            {
                // Clone
                ITetrimino currentRotation = _cloneTetrimino(_client.CurrentTetrimino);
                // Rotate
                for (int i = 0; i < rotation; i++)
                    currentRotation.RotateClockwise(grid);
                for (int posX = 0; posX < _client.Width; posX++)
                {
                    // Move to posX
                    int originalPosX = currentRotation.PosX;
                    if (originalPosX > posX)
                        while (currentRotation.PosX > posX && currentRotation.MoveLeft(grid))
                        {
                            // NOP
                        }
                    else if (originalPosX < posX)
                        while (currentRotation.PosX < posX && currentRotation.MoveRight(grid))
                        {
                            // NOP
                        }
                    // Check if moving to posX was possible
                    if (currentRotation.PosX == posX)
                    {
                        // Drop
                        while (currentRotation.MoveDown(grid))
                        {
                            // NOP
                        }
                        // Compute score, store x, y, rotation if best than previous
                        int hole = CountHoles(grid, _client.Width, _client.Height, currentRotation, posX, currentRotation.PosY);
                        int clears = CountClears(grid, _client.Width, _client.Height, currentRotation, posX, currentRotation.PosY);
                        int blockades = CountBlockades(grid, _client.Width, _client.Height, currentRotation, posX, currentRotation.PosY);
                        int edgeTouchingBlock = 0;
                        int edgeTouchingWall = 0;
                        int edgeTouchingFloor = 0;
                        //
                        double score =
                            (22 - currentRotation.PosY) * HeightMultiplier +
                            hole * HoleMultiplier +
                            blockades * BlocadeMultiplier +
                            clears * ClearMultiplier +
                            edgeTouchingBlock * EdgeTouchingAnotherBlockMultiplier +
                            edgeTouchingWall * EdgeTouchingWallMultiplier +
                            edgeTouchingFloor * EdgeTouchingFloorMultiplier;
                        if (score > bestScore)
                        {
                            bestScore = score;
                            bestRotation = rotation;
                            bestPosX = posX;
                        }
                    }
                }
            }

            Console.WriteLine("\t\tBest score: {0:0.000000} r:{1} x:{2}", bestScore, bestRotation, bestPosX);

            for (int i = 0; i < bestRotation; i++)
                _client.RotateClockwise();
            if (_client.CurrentTetrimino.PosX != bestPosX)
            {
                // Move to posX
                int moves = bestPosX - _client.CurrentTetrimino.PosX;
                if (moves < 0)
                    for(int i = 0; i < Math.Abs(moves); i++)
                        _client.MoveLeft();
                else                     
                    for(int i = 0; i < Math.Abs(moves); i++)
                        _client.MoveRight();

            }

            System.Threading.Thread.Sleep(100);
            
            _client.Drop();
        }

        private int CountHoles(byte[] grid, int width, int height, ITetrimino current, int posX, int posY)
        {
            int holes = 0;
            for (int x = 0; x < width; x++)
            {
                bool emptyColumn = true;
                // Get highest row with a part in this column
                int minPartY = 22;
                for (int y = height - 1; y >= 0; y--)
                {
                    int linearIndex = y*width + x;
                    if (grid[linearIndex] > 0)
                    {
                        minPartY = y;
                        emptyColumn = false;
                    }
                }
                // Every empty row below this row is a hole
                if (!emptyColumn)
                    for (int y = height - 1; y >= minPartY; y--)
                    {
                        int linearIndex = y*width + x;
                        if (grid[linearIndex] == 0)
                            holes++;
                    }

            }
            return holes;
        }

        private int CountClears(byte[] grid, int width, int height, ITetrimino current, int posX, int posY)
        {
            int rows = 0;
            for (int y = 1; y < height; y++)
            {
                // Count number of part in row
                int countPart = 0;
                for (int x = 0; x < width; x++)
                {
                    int linearIndex = y * width + x;
                    countPart += grid[linearIndex] > 0 ? 1 : 0;
                }
                // Full row, delete it and move part above one step below
                if (countPart == width)
                    rows++;
            }
            return rows;
        }

        private int CountBlockades(byte[] grid, int width, int height, ITetrimino current, int posX, int posY)
        {
            // TODO
            return 0;
        }
    }
}
