using System.Windows;
using Tetris.Model.UI;
using Tetris.Model.UI.DisplayBehaviours;

namespace Tetris.Views
{
    /// <summary>
    /// Interaktionslogik für Settings.xaml
    /// </summary>
    public partial class SettingsView : OverlayUserControl
    {
        #region Dependency Properties
            public static readonly DependencyProperty SettingsProperty = DependencyProperty.Register("Settings", typeof(Model.Settings), typeof(SettingsView), new UIPropertyMetadata(null));

            public Model.Settings Settings
            {
                get { return (Model.Settings)GetValue(SettingsProperty); }
                set { SetValue(SettingsProperty, value); }
            }
        #endregion

        public SettingsView()
        {
            InitializeComponent();
            DisplayBehaviour = new DisplayFlowFromRight(this);
        }

        private void cmdBack_Click(object sender, RoutedEventArgs e)
        {
            Hide();
            Settings.Serialize();
        }

        /// <summary>
        /// Calculates new fontsize values. The solution via Viewbox failed here.
        /// </summary>
        private void Grid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            grpSound.FontSize = ActualHeight / 38;
            grpControls.FontSize = ActualHeight / 38;
        }
    }
}
