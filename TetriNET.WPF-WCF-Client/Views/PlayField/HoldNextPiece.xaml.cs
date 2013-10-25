using System.Windows;
using System.Windows.Controls;
using TetriNET.Client.Interfaces;
using TetriNET.WPF_WCF_Client.Helpers;

namespace TetriNET.WPF_WCF_Client.Views.PlayField
{
    /// <summary>
    /// Interaction logic for HoldNextPiece.xaml
    /// </summary>
    public partial class HoldNextPiece : UserControl
    {
        public static readonly DependencyProperty ClientProperty = DependencyProperty.Register(
            "HoldPieceClientProperty",
            typeof(IClient),
            typeof(HoldNextPiece),
            new PropertyMetadata(Client_Changed));
        public IClient Client
        {
            get { return (IClient)GetValue(ClientProperty); }
            set { SetValue(ClientProperty, value); }
        }

        public HoldNextPiece()
        {
            InitializeComponent();
        }

        private void DrawHoldPiece()
        {
            if (Client == null)
                return;
            IBoard board = Client.Board;
            if (board == null)
                return;

            PieceControl.DrawPiece(Client.HoldPiece, board.Height);
        }

        private static void Client_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            HoldNextPiece @this = sender as HoldNextPiece;

            if (@this != null)
            {
                // Remove old handlers
                IClient oldClient = args.OldValue as IClient;
                if (oldClient != null)
                {
                    oldClient.OnGameStarted -= @this.OnGameStarted;
                    oldClient.OnHoldPieceModified -= @this.OnHoldPieceModified;
                }
                // Set new client
                IClient newClient = args.NewValue as IClient;
                @this.Client = newClient;
                // Add new handlers
                if (newClient != null)
                {
                    newClient.OnGameStarted += @this.OnGameStarted;
                    newClient.OnHoldPieceModified += @this.OnHoldPieceModified;
                }
            }
        }

        #region IClient events handler
        private void OnGameStarted()
        {
            ExecuteOnUIThread.Invoke(DrawHoldPiece);
        }

        private void OnHoldPieceModified()
        {
            ExecuteOnUIThread.Invoke(DrawHoldPiece);
        }
        #endregion
    }
}
