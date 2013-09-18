using System.Windows.Controls;
using TetriNET.WPF_WCF_Client.TextureManager;

namespace TetriNET.WPF_WCF_Client.Views.Test
{
    /// <summary>
    /// Interaction logic for TestView.xaml
    /// </summary>
    public partial class TestView : UserControl
    {
        public TestView()
        {
            InitializeComponent();

            // Add textures to Canvas
            ITextureManager textures = TextureManager.TextureManager.TexturesSingleton.Instance;

            foreach()
        }
    }
}
