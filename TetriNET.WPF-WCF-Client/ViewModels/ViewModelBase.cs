using System.Diagnostics.CodeAnalysis;
using TetriNET.Client.Interfaces;
using TetriNET.WPF_WCF_Client.MVVM;

namespace TetriNET.WPF_WCF_Client.ViewModels
{
    public abstract class ViewModelBase : ObservableObject
    {
        // DesignMode detection, thanks to Galasoft
        [SuppressMessage(
            "Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "Non static member needed for data binding")]
        public bool IsInDesignMode => Helpers.DesignMode.IsInDesignModeStatic;

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
                    ClientChanged?.Invoke(oldValue, _client);
                    if (_client != null)
                        SubscribeToClientEvents(_client);
                }
            }
        }

        public abstract void UnsubscribeFromClientEvents(IClient oldClient);
        public abstract void SubscribeToClientEvents(IClient newClient);
    }
}