using System.ComponentModel;
using System.Runtime.CompilerServices;
using TetriNET.Common.Interfaces;

namespace TetriNET.WPF_WCF_Client.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        private IClient _client;
        public IClient Client {
            get
            {
                return _client;
            }
            set
            {
                if (_client != value)
                {
                    if (_client != null)
                        UnsubscribeFromClientEvents(_client);
                    _client = value;
                    OnClientAssigned(_client);
                    if (_client != null)
                        SubscribeToClientEvents(_client);
                }
            }
        }

        public abstract void UnsubscribeFromClientEvents(IClient oldClient);
        public abstract void SubscribeToClientEvents(IClient newClient);
        public virtual void OnClientAssigned(IClient newClient)
        {
            // NOP
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
