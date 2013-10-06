using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TetriNET.Client.Interfaces;
using TetriNET.Client.Pieces;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Helpers;
using TetriNET.WPF_WCF_Client.TextureManager;

namespace TetriNET.WPF_WCF_Client.Views.Test
{
    /// <summary>
    /// Interaction logic for TestView.xaml
    /// </summary>
    public partial class TestView : UserControl
    {
        private static readonly SolidColorBrush PieceAnchorColor = new SolidColorBrush(Colors.White);

        public TestView()
        {
            InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                int i;
                // Add textures to Canvas
                ITextureManager textures = TextureManager.TextureManager.TexturesSingleton.Instance;

                // Special textures
                i = 0;
                foreach (Specials special in EnumHelper.GetSpecials(available => available))
                {
                    //
                    Brush bigBrush = textures.GetBigSpecial(special);
                    Rectangle bigRectangle = new Rectangle
                    {
                        Width = 16,
                        Height = 16,
                        Fill = bigBrush,
                    };
                    Canvas.Children.Add(bigRectangle);
                    Canvas.SetLeft(bigRectangle, 5 + i * (16 + 5));
                    Canvas.SetTop(bigRectangle, 5);

                    //
                    Brush smallBrush = textures.GetSmallSpecial(special);
                    Rectangle smallRectangle = new Rectangle
                    {
                        Width = 8,
                        Height = 8,
                        Fill = smallBrush,
                    };
                    Canvas.Children.Add(smallRectangle);
                    Canvas.SetLeft(smallRectangle, 5 + i * (8 + 5));
                    Canvas.SetTop(smallRectangle, 30);
                    //
                    i++;
                }

                // Pieces textures
                i = 0;
                foreach (Pieces piece in EnumHelper.GetPieces(available => available))
                {
                    //
                    Brush bigBrush = textures.GetBigPiece(piece);
                    Rectangle bigRectangle = new Rectangle
                    {
                        Width = 16,
                        Height = 16,
                        Fill = bigBrush,
                    };
                    Canvas.Children.Add(bigRectangle);
                    Canvas.SetLeft(bigRectangle, 5 + i * (16 + 5));
                    Canvas.SetTop(bigRectangle, 50);

                    //
                    Brush smallBrush = textures.GetSmallPiece(piece);
                    Rectangle smallRectangle = new Rectangle
                    {
                        Width = 8,
                        Height = 8,
                        Fill = smallBrush,
                    };
                    Canvas.Children.Add(smallRectangle);
                    Canvas.SetLeft(smallRectangle, 5 + i * (8 + 5));
                    Canvas.SetTop(smallRectangle, 75);
                    // 
                    i++;
                }

                // Draw pieces
                i = 0;
                foreach (IPiece piece in EnumHelper.GetPieces(available => available).Select(piece => Piece.CreatePiece(piece, 0, 0, 1, 0, false)))
                {
                    for (int r = 1; r <= piece.MaxOrientations; r++)
                    {
                        DrawPiece(piece, 8, 5 + r * (8 * 4), 120 + i * (8 * 4));
                        piece.RotateCounterClockwise();
                    }
                    //
                    i++;
                }

                // Draw mutated pieces
                i = 0;
                foreach (IPiece piece in EnumHelper.GetPieces(available => available).Select(piece => Piece.CreatePiece(piece, 0, 0, 1, 0, true)))
                {
                    for (int r = 1; r <= piece.MaxOrientations; r++)
                    {
                        DrawPiece(piece, 8, 200 + r * (8 * 4), 120 + i * (8 * 4));
                        piece.RotateCounterClockwise();
                    }
                    //
                    i++;
                }
            }
        }

        private void DrawPiece(IPiece piece, int size, int topX, int topY)
        {
            IPiece temp = piece.Clone();
            Pieces cellPiece = temp.Value;
            for (int i = 1; i <= temp.TotalCells; i++)
            {
                int x, y;
                temp.GetCellAbsolutePosition(i, out x, out y); // 1->Width x 1->Height

                Rectangle rectangle = new Rectangle
                    {
                        Width = size,
                        Height = size,
                        Fill = TextureManager.TextureManager.TexturesSingleton.Instance.GetBigPiece(cellPiece)
                    };
                Canvas.Children.Add(rectangle);
                Canvas.SetLeft(rectangle, topX + x * size);
                Canvas.SetTop(rectangle, topY + y * size);
            }

            Rectangle anchor = new Rectangle
            {
                Width = size,
                Height = size,
                Fill = PieceAnchorColor
            };

            Canvas.Children.Add(anchor);
            Canvas.SetLeft(anchor, topX);
            Canvas.SetTop(anchor, topY);
        }
    }
}
