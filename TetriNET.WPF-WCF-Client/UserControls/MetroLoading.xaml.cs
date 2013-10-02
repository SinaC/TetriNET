using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TetriNET.WPF_WCF_Client.UserControls
{
    /// <summary>
    /// Interaction logic for MetroLoading.xaml
    /// </summary>
    public partial class MetroLoading : UserControl
    {
        public static readonly DependencyProperty ParticleColorProperty =
            DependencyProperty.Register(
                "ParticleColor",
                typeof(Brush),
                typeof(MetroLoading),
                new PropertyMetadata(new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0X66, 0x99))));
        public Brush ParticleColor
        {
            get { return (Brush) GetValue(ParticleColorProperty); }
            set { SetValue(ParticleColorProperty, value); }
        }

        public MetroLoading()
        {
            InitializeComponent();
            DataContext = this; // easier binding
        }
    }
}
