using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace TetriNET.WPF_WCF_Client.Views.PartyLine
{
    /// <summary>
    /// Interaction logic for ChatControl.xaml
    /// </summary>
    public partial class ChatControl : UserControl
    {
       
        public ChatControl()
        {
            InitializeComponent();
        }

        #region UI events handler
        private void InputChat_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BindingExpression exp = this.TxtInputChat.GetBindingExpression(TextBox.TextProperty);
                if (exp != null)
                    exp.UpdateSource();
            }
        }
        #endregion
    }
}
