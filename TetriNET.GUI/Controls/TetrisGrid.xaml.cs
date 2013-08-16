using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Tetris.Model;

namespace Tetris.Controls
{
    /// <summary>
    /// Interaktionslogik für TetrisGrid.xaml
    /// </summary>
    public partial class TetrisGrid : UserControl
    {
        #region Fields

        public static readonly DependencyProperty TetrisProperty = DependencyProperty.Register("Tetris", typeof (Model.Tetris), typeof (TetrisGrid), new PropertyMetadata(Tetris_Changed));

        #endregion

        #region Properties

        public Model.Tetris Tetris
        {
            get { return (Model.Tetris) GetValue(TetrisProperty); }
            set { SetValue(TetrisProperty, value); }
        }

        #endregion

        public TetrisGrid()
        {
            InitializeComponent();

            #region Populate the grid with parts

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

            #endregion
        }

        #region Methods

        private void Tetris_NewBlockAdded()
        {
            PaintCurrentBlock();
        }

        private void Tetris_BlockMoving()
        {
            RemoveCurrentBlock();
        }

        private void Tetris_BlockMoved()
        {
            PaintCurrentBlock();
        }

        private void Tetris_RowsCompleting(int[] rows)
        {
            //No Need for this event right now.
            //The DeleteRow method just finishes completly before invoking the CompletedEvent
        }

        private void Tetris_RowsCompleted(int[] rows)
        {
            //Get all parts
            var rowParts = grid.Children.Cast<Control>().Where(e => rows.Contains(Grid.GetRow(e))).ToList();

            //Use a WPF Storyboard to display an effect on the row (Thread.Sleep() after setting the opacity will just not display anything)
            Storyboard storyboard = new Storyboard();

            foreach (var p in rowParts)
            {
                #region Add the animation for every part in the row to the Storyboard

                //This also allows to make a more advanced effect like let the row slowly fade away

                #region Create the animtion

                DoubleAnimation animation = new DoubleAnimation
                    {
                        Duration = TimeSpan.FromMilliseconds(1000),
                        From = 1,
                        To = 0,
                        FillBehavior = FillBehavior.Stop //Necessary, or the programm won't be able to reset the opacity
                    };

                #endregion

                storyboard.Children.Add(animation);
                Storyboard.SetTarget(animation, p);
                Storyboard.SetTargetProperty(animation, new PropertyPath("(Control.Opacity)"));

                #endregion
            }

            #region Start the redraw of all parts after the Storyboard finished

            storyboard.Completed += (obj, args) =>
                {
                    ClearGrid();
                    RedrawGrid();
                };

            #endregion

            storyboard.Begin();

            Settings.Instance.SoundPlayer.PlayResourceFile(new Uri("Tetris;component/Sounds/Effects/row.wav", UriKind.Relative));

            #region Extremly strange method making the whole thread wait but somehow manages not to delay the animation

            //This allows to pause the whole Tetris class without putting UI stuff in it, so no new Block is generated until this finishes
            //The ticks are measured in nanoseconds and have to match the animations Duration property
            long dtEnd = DateTime.Now.AddTicks(10000000).Ticks;

            while (DateTime.Now.Ticks < dtEnd)
            {
                Dispatcher.Invoke(DispatcherPriority.Background, (DispatcherOperationCallback) (unused => null), null);
            }

            #endregion
        }

        private void Tetris_AttackReceived()
        {
            // TODO: storyboard + sound
            ClearGrid();
            RedrawGrid();
        }

        private void Tetris_RoundFinished()
        {
            Settings.Instance.SoundPlayer.PlayResourceFile(new Uri("Tetris;component/Sounds/Effects/drop.mp3", UriKind.Relative));
        }

        /// <summary>
        /// Draw the CurrentBlock on the grid
        /// Not a good method to get the UIElements, because the complete Grid gets iterated through)
        /// </summary>
        private void PaintCurrentBlock()
        {
            foreach (Part p in Tetris.CurrentBlock.Parts)
            {
                var uiPart = grid.Children.Cast<Control>().Single(e => Grid.GetRow(e) == p.PosY && Grid.GetColumn(e) == p.PosX);
                uiPart.Background = new SolidColorBrush(p.Color);
            }
        }

        /// <summary>
        /// Remove the CurrentBlock from the grid
        /// Not a good method to get the UIElements, because the complete Grid gets iterated through)    
        /// </summary>
        private void RemoveCurrentBlock()
        {
            foreach (Part p in Tetris.CurrentBlock.Parts)
            {
                var uiPart = grid.Children.Cast<Control>().Single(e => Grid.GetRow(e) == p.PosY && Grid.GetColumn(e) == p.PosX);
                uiPart.Background = new SolidColorBrush(Colors.Transparent);
            }
        }

        /// <summary>
        /// Clear the complete Grid (reset all colors and the opacity).
        /// (Needed either when the game is restarted or a row is complete).
        /// </summary>
        private void ClearGrid()
        {
            var controls = grid.Children.Cast<Control>().ToList();
            foreach (var c in controls)
            {
                c.Opacity = 1;
                c.Background = new SolidColorBrush(Colors.Transparent);
            }
        }

        /// <summary>
        /// Redraw every part in the Tetris.Grid
        /// </summary>
        private void RedrawGrid()
        {
            #region Redraw every part

            foreach (var p in Tetris.Grid)
            {
                var uiPart = grid.Children.Cast<Control>().Single(e => Grid.GetRow(e) == p.PosY && Grid.GetColumn(e) == p.PosX);
                uiPart.Background = new SolidColorBrush(p.Color);
            }

            #endregion
        }

        private static void Tetris_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var __this = sender as TetrisGrid;

            if (__this != null)
            {
                __this.ClearGrid();

                var tetris = args.NewValue as Model.Tetris;
                if (tetris != null)
                {
                    #region Register the Tetris-Objects events

                    tetris.NewBlockAdded += __this.Tetris_NewBlockAdded;
                    tetris.BlockMoved += __this.Tetris_BlockMoved;
                    tetris.BlockMoving += __this.Tetris_BlockMoving;

                    tetris.RowsCompleting += __this.Tetris_RowsCompleting;
                    tetris.RowsCompleted += __this.Tetris_RowsCompleted;

                    tetris.AttackReceived += __this.Tetris_AttackReceived;

                    tetris.RoundFinished += __this.Tetris_RoundFinished;

                    #endregion
                }
            }
        }

        #endregion
    }
}