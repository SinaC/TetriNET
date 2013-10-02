using System.Windows;
using System.Windows.Controls;

namespace TetriNET.WPF_WCF_Client.UserControls
{
    /// <summary>
    /// Interaction logic for SensibilityControl.xaml
    /// </summary>
    public partial class SensibilityControl : UserControl
    {
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("LabelProperty", typeof(string), typeof(SensibilityControl));
        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public SensibilityControl()
        {
            InitializeComponent();
        }
    }
}
