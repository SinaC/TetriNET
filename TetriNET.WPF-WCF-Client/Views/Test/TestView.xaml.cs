using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TetriNET.Common.Attributes;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Helpers;
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

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                // Add textures to Canvas
                ITextureManager textures = TextureManager.TextureManager.TexturesSingleton.Instance;

                int i = 0;
                foreach (Specials special in EnumHelper.GetSpecials(available => true))
                {
                    //
                    Brush bigBrush = textures.GetBigSpecial(special);
                    Rectangle bigRectangle = new Rectangle
                    {
                        Width = 16,
                        Height = 16,
                        Fill = bigBrush,
                    };
                    Canvas.Children.Add(bigRectangle);
                    Canvas.SetLeft(bigRectangle, 5 + i*(16 + 5));
                    Canvas.SetTop(bigRectangle, 5);

                    //
                    Brush smallBrush = textures.GetSmallSpecial(special);
                    Rectangle smallRectangle = new Rectangle
                    {
                        Width = 8,
                        Height = 8,
                        Fill = smallBrush,
                    };
                    Canvas.Children.Add(smallRectangle);
                    Canvas.SetLeft(smallRectangle, 5 + i*(8 + 5));
                    Canvas.SetTop(smallRectangle, 30);
                    //
                    i++;
                }

                i = 0;
                foreach (Pieces piece in EnumHelper.GetPieces())
                {
                    //
                    Brush bigBrush = textures.GetBigPiece(piece);
                    Rectangle bigRectangle = new Rectangle
                    {
                        Width = 16,
                        Height = 16,
                        Fill = bigBrush,
                    };
                    Canvas.Children.Add(bigRectangle);
                    Canvas.SetLeft(bigRectangle, 5 + i*(16 + 5));
                    Canvas.SetTop(bigRectangle, 50);

                    //
                    Brush smallBrush = textures.GetSmallPiece(piece);
                    Rectangle smallRectangle = new Rectangle
                    {
                        Width = 8,
                        Height = 8,
                        Fill = smallBrush,
                    };
                    Canvas.Children.Add(smallRectangle);
                    Canvas.SetLeft(smallRectangle, 5 + i*(8 + 5));
                    Canvas.SetTop(smallRectangle, 75);
                    // 
                    i++;
                }
            }
        }
    }
}
