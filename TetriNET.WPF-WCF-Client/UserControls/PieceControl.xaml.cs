using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TetriNET.Client.Interfaces;
using TetriNET.Common.DataContracts;

namespace TetriNET.WPF_WCF_Client.UserControls
{
    /// <summary>
    /// Interaction logic for PieceControl.xaml
    /// </summary>
    public partial class PieceControl : UserControl
    {
        private const int CellWidth = 16;
        private const int CellHeight = 16;
        private const int MarginWidth = 0;
        private const int MarginHeight = 0;

        private static readonly SolidColorBrush TransparentColor = new SolidColorBrush(Colors.Transparent);

        private readonly List<Rectangle> _grid = new List<Rectangle>();

        public PieceControl()
        {
            InitializeComponent();

            for (int y = 0; y < 4; y++)
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

        public void DrawPiece(IPiece piece, int boardHeight)
        {
            // Clear
            foreach (Rectangle rect in _grid)
                rect.Fill = TransparentColor;

            if (piece == null)
                return;
            // Draw
            IPiece temp = piece.Clone();
            int minX, minY, maxX, maxY;
            temp.GetAbsoluteBoundingRectangle(out minX, out minY, out maxX, out maxY);
            // Move to top, left
            temp.Translate(-minX, 0);
            temp.Translate(0, boardHeight - maxY);
            Pieces cellPiece = temp.Value;
            for (int i = 1; i <= temp.TotalCells; i++)
            {
                int x, y;
                temp.GetCellAbsolutePosition(i, out x, out y); // 1->Width x 1->Height
                int cellY = boardHeight - y;
                int cellX = x;

                Rectangle uiPart = GetControl(cellX, cellY);
                uiPart.Fill = TextureManager.TextureManager.TexturesSingleton.Instance.GetBigPiece(cellPiece);
            }
        }

        private Rectangle GetControl(int cellX, int cellY)
        {
            return _grid[cellX + cellY * 4];
        }
    }
}
