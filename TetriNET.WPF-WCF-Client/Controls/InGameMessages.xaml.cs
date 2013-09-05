using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TetriNET.Common.GameDatas;
using TetriNET.Common.Interfaces;

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
        private const int MaxEntries = 10;

        public static readonly DependencyProperty ClientProperty = DependencyProperty.Register("InGameMessagesClientProperty", typeof(IClient), typeof(InGameMessages), new PropertyMetadata(Client_Changed));
        public IClient Client
        {
            get { return (IClient)GetValue(ClientProperty); }
            set { SetValue(ClientProperty, value); }
        }

        public ObservableCollection<InGameChatEntry> Entries;

        public InGameMessages()
        {
            InitializeComponent();

        }

        private void AddEntry(InGameChatEntry entry)
        {
            if (Entries == null)
            {
                Entries = new ObservableCollection<InGameChatEntry>();
                ListboxEntries.ItemsSource = Entries;
            }
            Entries.Add(entry);
            if (Entries.Count > MaxEntries)
                Entries.RemoveAt(0);
        }

        private static void Client_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            InGameMessages _this = sender as InGameMessages;

            if (_this != null)
            {
                IClient client = args.NewValue as IClient;
                if (client != null)
                {
                    _this.Client = client;
                    // Register the Client UI events
                    _this.Client.OnPlayerAddLines += _this.OnPlayerAddLines;
                    _this.Client.OnSpecialUsed += _this.OnSpecialUsed;
                }
            }
        }

        private void OnPlayerAddLines(string playerName, int specialId, int count)
        {
            ExecuteOnUIThread.Invoke(() => AddEntry(new InGameChatEntry
            {
                Id = specialId,
                Special = String.Format("{0} line{1} added to All", count, (count > 1) ? "s" : ""),
                Source = playerName,
                TargetVisibility = Visibility.Collapsed,
            }));
        }

        private void OnSpecialUsed(string playerName, string targetName, int specialId, Specials special)
        {
            //Console.Write("{0}. {1} on {2} from {3}", specialId, GetSpecialString(special), targetName, playerName);
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
