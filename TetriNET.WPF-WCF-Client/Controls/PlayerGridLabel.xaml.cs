using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TetriNET.Common.GameDatas;
using TetriNET.Common.Helpers;
using TetriNET.Common.Interfaces;

namespace TetriNET.WPF_WCF_Client.Controls
{
    /// <summary>
    /// Interaction logic for PlayerGrid.xaml
    /// </summary>
    public partial class PlayerGridLabel : UserControl, INotifyPropertyChanged
    {
        // TODO: dynamically get width/height
        private const int ColumnsCount = 12;
        private const int RowsCount = 22;

        private readonly object _lock = new object();

        private static readonly SolidColorBrush TransparentColor = new SolidColorBrush(Colors.Transparent);
        private static readonly SolidColorBrush SpecialColor = new SolidColorBrush(Colors.LightGray);

        public static readonly DependencyProperty ClientProperty = DependencyProperty.Register("PlayerGridLabelClientProperty", typeof(IClient), typeof(PlayerGridLabel), new PropertyMetadata(Client_Changed));
        public IClient Client
        {
            get { return (IClient) GetValue(ClientProperty); }
            set { SetValue(ClientProperty, value); }
        }

        private int _playerId;
        public int PlayerId
        {
            get { return _playerId; }
            set
            {
                if (_playerId != value)
                {
                    _playerId = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _playerName;
        public string PlayerName
        {
            get { return _playerName; }
            set
            {
                if (_playerName != value)
                {
                    _playerName = value;
                    OnPropertyChanged();
                }
            }
        }

        public PlayerGridLabel()
        {
            InitializeComponent();

            PlayerId = -1;
            PlayerName = "not playing";
            
            // Add Grid row definitions
            for(int i = 0; i < RowsCount; i++)
                Grid.RowDefinitions.Add(new RowDefinition
                {
                    Height = new GridLength(24)
                });
            // Add Grid column definitions
            for (int i = 0; i < ColumnsCount; i++)
                Grid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(24)
                });

            // Populate the grid with cells
            for (int i = 0; i < Grid.RowDefinitions.Count(); i++)
                for (int j = 0; j < Grid.ColumnDefinitions.Count(); j++)
                {
                    //  Create a new Label as "cell" and add it to the grid
                    Label txt = new Label
                    {
                        Foreground = new SolidColorBrush(Colors.Black),
                        Background = new SolidColorBrush(Colors.Transparent),
                        FontSize = 11,
                        Margin = new Thickness(1,1,1,1)
                    };
                    Grid.Children.Add(txt);
                    Grid.SetRow(txt, i);
                    Grid.SetColumn(txt, j);
                }
        }

        private void DrawCurrentTetrimino()
        {
            if (Client == null)
                return;
            IBoard board = Client.Board;
            if (board == null)
                return;
            ITetrimino currentTetrimino = Client.CurrentTetrimino;
            if (currentTetrimino == null)
                return;
            Tetriminos cellTetrimino = Client.CurrentTetrimino.Value;
            for (int i = 1; i <= Client.CurrentTetrimino.TotalCells; i++)
            {
                int x, y;
                Client.CurrentTetrimino.GetCellAbsolutePosition(i, out x, out y); // 1->Width x 1->Height
                int cellY = board.Height - y;
                int cellX = x - 1;

                Label uiPart = GetControl<Label>(cellX, cellY);
                uiPart.Background = Mapper.MapTetriminoToColor(cellTetrimino);
            }
        }

        private void HideCurrentTetrimino()
        {
            if (Client == null)
                return;
            IBoard board = Client.Board;
            if (board == null)
                return;
            ITetrimino currentTetrimino = Client.CurrentTetrimino;
            if (currentTetrimino == null)
                return;
            for (int i = 1; i <= Client.CurrentTetrimino.TotalCells; i++)
            {
                int x, y;
                Client.CurrentTetrimino.GetCellAbsolutePosition(i, out x, out y); // 1->Width x 1->Height
                int cellY = board.Height - y;
                int cellX = x - 1;

                Label uiPart = GetControl<Label>(cellX, cellY);
                uiPart.Background = TransparentColor;
            }
        }

        private void DrawGrid()
        {
            if (Client == null)
                return;
            IBoard board = Client.Board;
            if (board == null)
                return;
            lock (_lock)
            {
                for (int y = 1; y <= board.Height; y++)
                    for (int x = 1; x <= board.Width; x++)
                    {
                        int cellY = board.Height - y;
                        int cellX = x - 1;
                        byte cellValue = board[x, y];

                        Label uiPart = GetControl<Label>(cellX, cellY);
                        if (cellValue == CellHelper.EmptyCell)
                        {
                            uiPart.Content = "";
                            uiPart.Background = TransparentColor;
                        }
                        else
                        {
                            Specials special = CellHelper.GetSpecial(cellValue);
                            Tetriminos color = CellHelper.GetColor(cellValue);

                            if (special == Specials.Invalid)
                            {
                                uiPart.Content = "";
                                uiPart.Background = Mapper.MapTetriminoToColor(color);
                            }
                            else
                            {
                                uiPart.Content = Mapper.MapSpecialToChar(special).ToString(CultureInfo.InvariantCulture);
                                uiPart.Background = SpecialColor;
                            }
                        }
                    }
            }
        }

        private void ClearGrid()
        {
            foreach (Label uiPart in Grid.Children.Cast<Label>())
            {
                uiPart.Background = TransparentColor;
                uiPart.Content = "";
            }
        }

        private T GetControl<T>(int cellX, int cellY) where T : FrameworkElement
        {
            return Grid.Children.Cast<T>().Single(e => Grid.GetRow(e) == cellY && Grid.GetColumn(e) == cellX);
        }

        private static void Client_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            PlayerGridLabel _this = sender as PlayerGridLabel;

            if (_this != null)
            {
                // Remove old handlers
                IClient oldClient = args.OldValue as IClient;
                if (oldClient != null)
                {
                    oldClient.OnGameStarted -= _this.OnGameStarted;
                    oldClient.OnRoundStarted -= _this.OnRoundStarted;
                    oldClient.OnRoundFinished -= _this.OnRoundFinished;
                    oldClient.OnTetriminoMoving -= _this.OnTetriminoMoving;
                    oldClient.OnTetriminoMoved -= _this.OnTetriminoMoved;
                    oldClient.OnRedraw -= _this.OnRedraw;
                }
                // Set new client
                IClient newClient = args.NewValue as IClient;
                _this.Client = newClient;
                // Add new handlers
                if (newClient != null)
                {
                    newClient.OnGameStarted += _this.OnGameStarted;
                    newClient.OnRoundStarted += _this.OnRoundStarted;
                    newClient.OnRoundFinished += _this.OnRoundFinished;
                    newClient.OnTetriminoMoving += _this.OnTetriminoMoving;
                    newClient.OnTetriminoMoved += _this.OnTetriminoMoved;
                    newClient.OnRedraw += _this.OnRedraw;
                }
            }
        }

        private void OnRedraw()
        {
            ExecuteOnUIThread.Invoke(() =>
            {
                DrawGrid();
                DrawCurrentTetrimino();
            });
        }

        private void OnTetriminoMoved()
        {
            ExecuteOnUIThread.Invoke(DrawCurrentTetrimino);
        }

        private void OnTetriminoMoving()
        {
            ExecuteOnUIThread.Invoke(HideCurrentTetrimino);
        }

        private void OnRoundFinished()
        {
            ExecuteOnUIThread.Invoke(HideCurrentTetrimino);
        }

        private void OnRoundStarted()
        {
            ExecuteOnUIThread.Invoke(DrawCurrentTetrimino);
        }

        private void OnGameStarted()
        {
            ExecuteOnUIThread.Invoke(() =>
            {
                Visibility = Visibility.Visible;
                ClearGrid();
                DrawCurrentTetrimino();
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
