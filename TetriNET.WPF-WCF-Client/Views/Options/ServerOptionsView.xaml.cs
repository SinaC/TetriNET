using System.Windows.Controls;
using TetriNET.WPF_WCF_Client.ViewModels.Options;

namespace TetriNET.WPF_WCF_Client.Views.Options
{
    /// <summary>
    /// Interaction logic for ServerOptionsView.xaml
    /// </summary>
    public partial class ServerOptionsView : UserControl
    {
        public ServerOptionsView()
        {
            InitializeComponent();
        }

        private void PieceOccurancy_TextChanged(object sender, TextChangedEventArgs e)
        {
            ServerOptionsViewModel vm = DataContext as ServerOptionsViewModel;
            if (vm != null)
                vm.PieceOccurancyChangedCommand.Execute(null);
        }

        private void SpecialOccurancy_TextChanged(object sender, TextChangedEventArgs e)
        {
            ServerOptionsViewModel vm = DataContext as ServerOptionsViewModel;
            if (vm != null)
                vm.SpecialOccurancyChangedCommand.Execute(null);
        }

        //private void DrawPiece(Pieces piece, int size, int topX, int topY)
        //{
        //    IPiece temp = DefaultBoardAndPieces.Piece.CreatePiece(piece, 0, 0, 1, 0);
        //    Pieces cellPiece = temp.Value;
        //    for (int i = 1; i <= temp.TotalCells; i++)
        //    {
        //        int x, y;
        //        temp.GetCellAbsolutePosition(i, out x, out y); // 1->Width x 1->Height

        //        Rectangle rectangle = new Rectangle
        //        {
        //            Width = size,
        //            Height = size,
        //            Fill = TextureManager.TextureManager.TexturesSingleton.Instance.GetBigPiece(cellPiece)
        //        };
        //        Canvas.Children.Add(rectangle);
        //        Canvas.SetLeft(rectangle, topX + x * size);
        //        Canvas.SetTop(rectangle, topY + y * size);
        //    }
        //}
    }
}
