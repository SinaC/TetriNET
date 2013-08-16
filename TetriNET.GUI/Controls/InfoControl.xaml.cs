using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Tetris.Model;

namespace Tetris.Controls
{
    /// <summary>
    /// Interaktionslogik für InfoControl.xaml
    /// </summary>
    public partial class InfoControl : UserControl
    {
        #region Fields

        public static readonly DependencyProperty ScoreProperty = DependencyProperty.Register("Score", typeof (int), typeof (InfoControl), new UIPropertyMetadata(0));
        public static readonly DependencyProperty LevelProperty = DependencyProperty.Register("Level", typeof (int), typeof (InfoControl), new UIPropertyMetadata(0));
        public static readonly DependencyProperty ClearedLinesProperty = DependencyProperty.Register("ClearedLines", typeof (int), typeof (InfoControl), new UIPropertyMetadata(0));
        public static readonly DependencyProperty NextBlockProperty = DependencyProperty.Register("NextBlock", typeof (Block), typeof (InfoControl), new PropertyMetadata(NextBlockChanged));

        #endregion

        #region Properties

        public int Score
        {
            get { return (int) GetValue(ScoreProperty); }
            set { SetValue(ScoreProperty, value); }
        }

        public int Level
        {
            get { return (int) GetValue(LevelProperty); }
            set { SetValue(LevelProperty, value); }
        }

        public int ClearedLines
        {
            get { return (int) GetValue(ClearedLinesProperty); }
            set { SetValue(ClearedLinesProperty, value); }
        }

        public Block NextBlock
        {
            get { return (Block) GetValue(NextBlockProperty); }
            set { SetValue(NextBlockProperty, value); }
        }

        #endregion

        public InfoControl()
        {
            InitializeComponent();

            for (int i = 0; i < grid.RowDefinitions.Count(); i++)
            {
                for (int j = 0; j < grid.ColumnDefinitions.Count(); j++)
                {
                    #region Create a new label as "part" and add it to the grid

                    Label lbl = new Label
                        {
                            Background = new SolidColorBrush(Colors.Transparent),
                            BorderBrush = new SolidColorBrush(Colors.Transparent),
                            BorderThickness = new Thickness(1.0)
                        };
                    grid.Children.Add(lbl);
                    Grid.SetRow(lbl, i);
                    Grid.SetColumn(lbl, j);

                    #endregion
                }
            }
        }

        #region Methods/Events

        private static void NextBlockChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var __this = sender as InfoControl;

            if (__this != null && __this.NextBlock != null)
            {
                __this.ClearGrid();

                #region Visualize the NextBlock in the "mini grid"

                foreach (Part p in __this.NextBlock.Parts)
                {
                    var uiPart = __this.grid.Children.Cast<Control>().Single(e => Grid.GetRow(e) == p.PosYInBlock && Grid.GetColumn(e) == p.PosXInBlock);
                    uiPart.Background = new SolidColorBrush(p.Color);
                }

                #endregion
            }
        }

        private void ClearGrid()
        {
            var controls = grid.Children.Cast<Control>().ToList();
            foreach (var c in controls)
            {
                c.Background = new SolidColorBrush(Colors.Transparent);
            }
        }

        /// <summary>
        /// Make sure the mini grid displaying the next block maintains a 1:1 ratio
        /// </summary>
        private void GroupBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var height = e.NewSize.Height - e.NewSize.Height*0.2;
            var width = e.NewSize.Width - e.NewSize.Width*0.2;

            #region The smaller value determines the size

            if (height > width)
            {
                grid.Height = width;
                grid.Width = width;
            }
            else
            {
                grid.Height = height;
                grid.Width = height;
            }

            #endregion
        }

        /// <summary>
        /// Set the font size of the GroupBox headers dynamically
        /// </summary>
        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            grpScore.FontSize = ActualHeight/20;
            grpLevel.FontSize = ActualHeight/20;
            grpLines.FontSize = ActualHeight/20;
            grpNext.FontSize = ActualHeight/20;
        }

        #endregion
    }
}