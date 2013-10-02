using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TetriNET.WPF_WCF_Client.UserControls
{
    /// <summary>
    /// Interaction logic for MetroLoadingControl.xaml
    /// </summary>
    public partial class MetroLoadingControl : UserControl
    {
        public static readonly DependencyProperty ParticleColorProperty =
            DependencyProperty.Register(
                "ParticleColor",
                typeof(Brush),
                typeof(MetroLoadingControl),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0X66, 0x99))));
        public Brush ParticleColor
        {
            get { return (Brush) GetValue(ParticleColorProperty); }
            set { SetValue(ParticleColorProperty, value); }
        }

        public MetroLoadingControl()
        {
            InitializeComponent();
            DataContext = this; // easier binding
        }
    }
}
