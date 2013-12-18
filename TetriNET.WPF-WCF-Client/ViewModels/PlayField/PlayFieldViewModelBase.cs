namespace TetriNET.WPF_WCF_Client.ViewModels.PlayField
{
    public abstract class PlayFieldViewModelBase : ViewModelBase, ITabIndex
    {
        #region ITabIndex

        public int TabIndex
        {
            get { return 4; }
        }

        #endregion
    }
}
