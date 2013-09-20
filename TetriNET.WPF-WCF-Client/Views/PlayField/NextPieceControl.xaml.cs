using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Interfaces;
using TetriNET.WPF_WCF_Client.Helpers;

namespace TetriNET.WPF_WCF_Client.Views.PlayField
{
    /// <summary>
    /// Interaction logic for NextPieceControl.xaml
    /// </summary>
    public partial class NextPieceControl : UserControl
    {
        private const int CellWidth = 16;
        private const int CellHeight = 16;
        private const int MarginWidth = 0;
        private const int MarginHeight = 0;

        private static readonly SolidColorBrush TransparentColor = new SolidColorBrush(Colors.Transparent);

        public static readonly DependencyProperty ClientProperty = DependencyProperty.Register("NextPieceClientProperty", typeof(IClient), typeof(NextPieceControl), new PropertyMetadata(Client_Changed));
        public IClient Client
        {
            get { return (IClient)GetValue(ClientProperty); }
            set { SetValue(ClientProperty, value); }
        }

        private readonly List<Rectangle> _grid = new List<Rectangle>();

        public NextPieceControl()
        {
            InitializeComponent();

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

        private void DrawNextPiece()
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
            IPiece temp = Client.NextPiece.Clone();
            int minX, minY, maxX, maxY;
            temp.GetAbsoluteBoundingRectangle(out minX, out minY, out maxX, out maxY);
            // Move to top, left
            temp.Translate(-minX, 0);
            if (maxY > board.Height)
                temp.Translate(0, board.Height - maxY);
            Pieces cellPiece = temp.Value;
            for (int i = 1; i <= temp.TotalCells; i++)
            {
                int x, y;
                temp.GetCellAbsolutePosition(i, out x, out y); // 1->Width x 1->Height
                int cellY = board.Height - y;
                int cellX = x;

                Rectangle uiPart = GetControl(cellX, cellY);
                uiPart.Fill = TextureManager.TextureManager.TexturesSingleton.Instance.GetBigPiece(cellPiece);
            }
        }

        private Rectangle GetControl(int cellX, int cellY)
        {
            return _grid[cellX + cellY * 4];
        }

        private static void Client_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            NextPieceControl @this = sender as NextPieceControl;

            if (@this != null)
            {
                // Remove old handlers
                IClient oldClient = args.OldValue as IClient;
                if (oldClient != null)
                {
                    oldClient.OnGameStarted -= @this.OnGameStarted;
                    oldClient.OnRoundStarted -= @this.OnRoundStarted;
                }
                // Set new client
                IClient newClient = args.NewValue as IClient;
                @this.Client = newClient;
                // Add new handlers
                if (newClient != null)
                {
                    newClient.OnGameStarted += @this.OnGameStarted;
                    newClient.OnRoundStarted += @this.OnRoundStarted;
                }
            }
        }

        #region IClient events handler
        private void OnGameStarted()
        {
            ExecuteOnUIThread.Invoke(DrawNextPiece);
        }

        private void OnRoundStarted()
        {
            ExecuteOnUIThread.Invoke(DrawNextPiece);
        }
        #endregion
    }
}
