using System.Windows;
using System.Windows.Controls;
using TetriNET.Client.Interfaces;
using TetriNET.WPF_WCF_Client.Helpers;

namespace TetriNET.WPF_WCF_Client.Views.PlayField
{
    /// <summary>
    /// Interaction logic for NextPieceView.xaml
    /// </summary>
    public partial class NextPieceView : UserControl
    {
        public static readonly DependencyProperty ClientProperty = DependencyProperty.Register(
            "NextPieceClientProperty", 
            typeof(IClient), 
            typeof(NextPieceView), 
            new PropertyMetadata(Client_Changed));
        public IClient Client
        {
            get { return (IClient)GetValue(ClientProperty); }
            set { SetValue(ClientProperty, value); }
        }

        public NextPieceView()
        {
            InitializeComponent();
        }

        private void DrawNextPiece()
        {
            if (Client == null)
                return;
            IBoard board = Client.Board;
            if (board == null)
                return;

            PieceControl.DrawPiece(Client.NextPiece, board.Height);
        }

        private static void Client_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            NextPieceView @this = sender as NextPieceView;

            if (@this != null)
            {
                // Remove old handlers
                IClient oldClient = args.OldValue as IClient;
                if (oldClient != null)
                {
                    oldClient.GameStarted -= @this.OnGameStarted;
                    oldClient.NextPieceModified -= @this.OnNextPieceModified;
                }
                // Set new client
                IClient newClient = args.NewValue as IClient;
                @this.Client = newClient;
                // Add new handlers
                if (newClient != null)
                {
                    newClient.GameStarted += @this.OnGameStarted;
                    newClient.NextPieceModified += @this.OnNextPieceModified;
                }
            }
        }

        #region IClient events handler
        private void OnGameStarted()
        {
            ExecuteOnUIThread.Invoke(DrawNextPiece);
        }

        private void OnNextPieceModified()
        {
            ExecuteOnUIThread.Invoke(DrawNextPiece);
        }
        #endregion
    }
}
