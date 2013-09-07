using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using TetriNET.Common.GameDatas;
using TetriNET.Common.Interfaces;
using TetriNET.WPF_WCF_Client.Helpers;

namespace TetriNET.WPF_WCF_Client.Controls
{
    public class InGameChatEntry
    {
        public int Id { get; set; }
        public string Special { get; set; }
        public string Source { get; set; }
        public string Target { get; set; }
        public Visibility TargetVisibility { get; set; }
    }

    /// <summary>
    /// Interaction logic for InGameMessages.xaml
    /// </summary>
    public partial class InGameMessages : UserControl
    {
        private const int MaxEntries = 13;

        public static readonly DependencyProperty ClientProperty = DependencyProperty.Register("InGameMessagesClientProperty", typeof(IClient), typeof(InGameMessages), new PropertyMetadata(Client_Changed));
        public IClient Client
        {
            get { return (IClient)GetValue(ClientProperty); }
            set { SetValue(ClientProperty, value); }
        }

        private readonly ObservableCollection<InGameChatEntry> _entries = new ObservableCollection<InGameChatEntry>();
        public ObservableCollection<InGameChatEntry> Entries { get { return _entries; } }

        public InGameMessages()
        {
            InitializeComponent();
        }

        private void AddEntry(InGameChatEntry entry)
        {
            Entries.Add(entry);
            if (Entries.Count > MaxEntries)
                Entries.RemoveAt(0);
        }

        private void ClearEntries()
        {
            Entries.Clear();
        }

        private static void Client_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            InGameMessages _this = sender as InGameMessages;

            if (_this != null)
            {
                // Remove old handlers
                IClient oldClient = args.OldValue as IClient;
                if (oldClient != null)
                {
                    oldClient.OnGameStarted -= _this.OnGameStarted;
                    oldClient.OnPlayerAddLines -= _this.OnPlayerAddLines;
                    oldClient.OnSpecialUsed -= _this.OnSpecialUsed;
                }
                // Set new client
                IClient newClient = args.NewValue as IClient;
                _this.Client = newClient;
                // Add new handlers
                if (newClient != null)
                {
                    newClient.OnGameStarted += _this.OnGameStarted;
                    newClient.OnPlayerAddLines += _this.OnPlayerAddLines;
                    newClient.OnSpecialUsed += _this.OnSpecialUsed;
                }
            }
        }

        private void OnGameStarted()
        {
            ExecuteOnUIThread.Invoke(ClearEntries);
        }

        private void OnPlayerAddLines(string playerName, int specialId, int count)
        {
            ExecuteOnUIThread.Invoke(() => AddEntry(new InGameChatEntry
            {
                Id = specialId+1,
                Special = String.Format("{0} line{1} added to All", count, (count > 1) ? "s" : ""),
                Source = playerName,
                TargetVisibility = Visibility.Collapsed,
            }));
        }

        private void OnSpecialUsed(string playerName, string targetName, int specialId, Specials special)
        {
            ExecuteOnUIThread.Invoke(() => AddEntry(new InGameChatEntry
            {
                Id = specialId+1,
                Special = Mapper.MapSpecialToString(special),
                Source = playerName,
                Target = targetName,
                TargetVisibility = Visibility.Visible,
            }));
        }
    }
}
