using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TetriNET.Client.Interfaces;
using TetriNET.Client.Pieces;
using TetriNET.Common.DataContracts;

namespace TetriNET.WPF_WCF_Client.UserControls
{
    /// <summary>
    /// Interaction logic for ShadowPieceControl.xaml
    /// </summary>
    public partial class ShadowPieceControl : UserControl
    {
        private static readonly SolidColorBrush PieceColor = new SolidColorBrush(Colors.Gray);
        private const int CellSize = 8;

        public static readonly DependencyProperty PieceValueProperty =
            DependencyProperty.Register(
                "PieceValue",
                typeof(Pieces),
                typeof(ShadowPieceControl),
                new PropertyMetadata(Pieces.Invalid, PieceValuePropertyChangedCallback));

        private static void PieceValuePropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            ShadowPieceControl ctrl = dependencyObject as ShadowPieceControl;
            if (ctrl != null)
            {
                Pieces piece = (Pieces) e.NewValue;
                ctrl.DrawPiece(piece);
            }
        }

        public Pieces PieceValue
        {
            get
            {
                return (Pieces)GetValue(PieceValueProperty);
            }

            set
            {
                SetValue(PieceValueProperty, value);
                DrawPiece(value);
            }
        }

        public ShadowPieceControl()
        {
            InitializeComponent();
        }

        private void DrawPiece(Pieces piece)
        {
            Canvas.Children.Clear();

            IPiece temp = Piece.CreatePiece(piece, 0, 0, 1, 0);
            int minX, minY, maxX, maxY;
            temp.GetAbsoluteBoundingRectangle(out minX, out minY, out maxX, out maxY);
            temp.Translate(-minX, -minY);
            for (int i = 1; i <= temp.TotalCells; i++)
            {
                int x, y;
                temp.GetCellAbsolutePosition(i, out x, out y); // 1->Width x 1->Height

                // TODO: move into screen  posX + x must be == 0  same for y

                Rectangle rectangle = new Rectangle
                {
                    Width = CellSize,
                    Height = CellSize,
                    Fill = PieceColor
                };
                Canvas.Children.Add(rectangle);
                Canvas.SetLeft(rectangle, x * (CellSize+1));
                Canvas.SetTop(rectangle, y * (CellSize+1));
            }

            Canvas.Width = (CellSize + 1) * (maxX - minX + 1);
            Canvas.Height = (CellSize + 1) * (maxY - minY + 1);
        }
    }
}
