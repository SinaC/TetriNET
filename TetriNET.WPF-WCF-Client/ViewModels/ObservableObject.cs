using System.ComponentModel;
using System.Runtime.CompilerServices;
using TetriNET.Common.Helpers;

namespace TetriNET.WPF_WCF_Client.ViewModels
{
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler.Do(x => x(this, new PropertyChangedEventArgs(propertyName)));
        }

        #endregion
    }
}
