using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TetriNET.Common.GameDatas;
using TetriNET.Common.Interfaces;
using TetriNET.WPF_WCF_Client.Helpers;

namespace TetriNET.WPF_WCF_Client.Views.PlayField
{
    /// <summary>
    /// Interaction logic for NextTetriminoControl.xaml
    /// </summary>
    public partial class NextTetriminoControl : UserControl
    {
        private const int CellWidth = 16;
        private const int CellHeight = 16;
        private const int MarginWidth = 0;
        private const int MarginHeight = 0;

        private static readonly SolidColorBrush TransparentColor = new SolidColorBrush(Colors.Transparent);

        public static readonly DependencyProperty ClientProperty = DependencyProperty.Register("NextTetriminoClientProperty", typeof(IClient), typeof(NextTetriminoControl), new PropertyMetadata(Client_Changed));
        public IClient Client
        {
            get { return (IClient)GetValue(ClientProperty); }
            set { SetValue(ClientProperty, value); }
        }

        private readonly Textures.Textures _textures;
        private readonly List<Rectangle> _grid = new List<Rectangle>();

        public NextTetriminoControl()
        {
            InitializeComponent();

            _textures = Textures.Textures.TexturesSingleton.Instance;

            for(int y = 0; y < 4; y++)
                for (int x = 0; x < 4; x++)
                {
                    int canvasLeft = x * (CellWidth + MarginWidth);
                    int canvasTop = y * (CellHeight + MarginHeight);
                    // Rectangle
                    Rectangle rect = new Rectangle
                    {
                        Width = CellWidth,
                        Height = CellHeight,
                        Fill = TransparentColor,
                    };
                    _grid.Add(rect);
                    Canvas.Children.Add(rect);
                    Canvas.SetLeft(rect, canvasLeft);
                    Canvas.SetTop(rect, canvasTop);
                }
        }

        private void DrawNextTetrimino()
        {
            if (Client == null)
                return;
            IBoard board = Client.Board;
            if (board == null)
                return;

            // Clear
            foreach (Rectangle rect in _grid)
                rect.Fill = TransparentColor;

            // Draw
            ITetrimino temp = Client.NextTetrimino.Clone();
            int minX, minY, maxX, maxY;
            temp.GetAbsoluteBoundingRectangle(out minX, out minY, out maxX, out maxY);
            // Move to top, left
            temp.Translate(-minX, 0);
            if (maxY > board.Height)
                temp.Translate(0, board.Height - maxY);
            Tetriminos cellTetrimino = temp.Value;
            for (int i = 1; i <= temp.TotalCells; i++)
            {
                int x, y;
                temp.GetCellAbsolutePosition(i, out x, out y); // 1->Width x 1->Height
                int cellY = board.Height - y;
                int cellX = x;

                Rectangle uiPart = GetControl(cellX, cellY);
                uiPart.Fill = _textures.BigTetriminosBrushes[cellTetrimino];
            }
        }

        private Rectangle GetControl(int cellX, int cellY)
        {
            return _grid[cellX + cellY * 4];
        }

        private static void Client_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            NextTetriminoControl _this = sender as NextTetriminoControl;

            if (_this != null)
            {
                // Remove old handlers
                IClient oldClient = args.OldValue as IClient;
                if (oldClient != null)
                {
                    oldClient.OnGameStarted -= _this.OnGameStarted;
                    oldClient.OnRoundStarted -= _this.OnRoundStarted;
                }
                // Set new client
                IClient newClient = args.NewValue as IClient;
                _this.Client = newClient;
                // Add new handlers
                if (newClient != null)
                {
                    newClient.OnGameStarted += _this.OnGameStarted;
                    newClient.OnRoundStarted += _this.OnRoundStarted;
                }
            }
        }

        #region IClient events handler
        private void OnGameStarted()
        {
            ExecuteOnUIThread.Invoke(DrawNextTetrimino);
        }

        private void OnRoundStarted()
        {
            ExecuteOnUIThread.Invoke(DrawNextTetrimino);
        }
        #endregion
    }
}
