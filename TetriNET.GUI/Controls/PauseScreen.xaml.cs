using Tetris.Model.UI;
using Tetris.Model.UI.DisplayBehaviours;

namespace Tetris.Controls
{
    /// <summary>
    /// Interaktionslogik für PauseScreen.xaml
    /// </summary>
    public partial class PauseScreen : OverlayUserControl
    {
        public PauseScreen()
        {
            InitializeComponent();
            DisplayBehaviour = new DisplayFadeIn(this);
        }
    }
}
