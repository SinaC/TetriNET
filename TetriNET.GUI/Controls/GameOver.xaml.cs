using System.Windows;
using Tetris.Model.UI;
using Tetris.Model.UI.DisplayBehaviours;
using Tetris.Model;

namespace Tetris.Controls
{
    /// <summary>
    /// Interaktionslogik für GameOver.xaml
    /// </summary>
    public partial class GameOver : OverlayUserControl
    {
        #region Dependency Properties
        public static readonly DependencyProperty ScoreProperty = DependencyProperty.Register("Score", typeof(int), typeof(GameOver), new PropertyMetadata(Score_Changed));

            public int Score
            {
                get { return (int)GetValue(ScoreProperty); }
                set { SetValue(ScoreProperty, value); }
            }
        #endregion
        
        public GameOver()
        {
            InitializeComponent();
            DisplayBehaviour = new DisplayFadeIn(this);
        }

        private void cmdSubmit_Click(object sender, RoutedEventArgs e)
        {
            Highscores.Instance.Add(Score, txtName.Text);
            grpName.Visibility = Visibility.Collapsed;
        }

        private static void Score_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var __this = (GameOver)sender;
            __this.grpName.Visibility = Highscores.Instance.CheckScore((int)args.NewValue) ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
