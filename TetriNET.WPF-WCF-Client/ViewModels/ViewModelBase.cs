using TetriNET.Client.Interfaces;
using TetriNET.Common.Helpers;
using TetriNET.WPF_WCF_Client.MVVM;

namespace TetriNET.WPF_WCF_Client.ViewModels
{
    public abstract class ViewModelBase : ObservableObject
    {
        public delegate void ClientChangedEventHandler(IClient oldClient, IClient newClient);

        public event ClientChangedEventHandler ClientChanged;

        private IClient _client;
        public IClient Client
        {
            get { return _client; }
            set
            {
                if (_client != value)
                {
                    if (_client != null)
                        UnsubscribeFromClientEvents(_client);
                    IClient oldValue = _client;
                    _client = value;
                    ClientChanged.Do(x => x(oldValue, _client));
                    if (_client != null)
                        SubscribeToClientEvents(_client);
                }
            }
        }

        public abstract void UnsubscribeFromClientEvents(IClient oldClient);
        public abstract void SubscribeToClientEvents(IClient newClient);
    }
}