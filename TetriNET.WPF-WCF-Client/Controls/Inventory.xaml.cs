using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using TetriNET.Common.GameDatas;
using TetriNET.Common.Interfaces;

namespace TetriNET.WPF_WCF_Client.Controls
{
    /// <summary>
    /// Interaction logic for Inventory.xaml
    /// </summary>
    public partial class Inventory : UserControl
    {
        public static readonly DependencyProperty ClientProperty = DependencyProperty.Register("InventoryClientProperty", typeof(IClient), typeof(Inventory), new PropertyMetadata(Client_Changed));
        public IClient Client
        {
            get { return (IClient)GetValue(ClientProperty); }
            set { SetValue(ClientProperty, value); }
        }

        public Inventory()
        {
            InitializeComponent();

            TxtInventory.Text = "";
            TxtFirstSpecial.Text = "No special";
        }

        private void DrawInventory()
        {
            if (Client == null)
                return;
            List<Specials> specials = Client.Inventory;
            if (specials != null && specials.Any())
            {
                StringBuilder sb = new StringBuilder(specials.Count);
                foreach (Specials special in specials)
                    sb.Append(Mapper.MapSpecialToChar(special));
                TxtInventory.Text = sb.ToString();
                TxtFirstSpecial.Text = Mapper.MapSpecialToString(specials[0]);
            }
            else
            {
                TxtInventory.Text = "";
                TxtFirstSpecial.Text = "No special";
            }
        }

        private static void Client_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            Inventory _this = sender as Inventory;

            if (_this != null)
            {
                // Remove old handlers
                IClient oldClient = args.OldValue as IClient;
                if (oldClient != null)
                {
                    oldClient.OnGameStarted -= _this.OnGameStarted;
                    oldClient.OnInventoryChanged -= _this.OnInventoryChanged;
                }
                // Set new client
                IClient newClient = args.NewValue as IClient;
                _this.Client = newClient;
                // Add new handlers
                if (newClient != null)
                {
                    newClient.OnGameStarted += _this.OnGameStarted;
                    newClient.OnInventoryChanged += _this.OnInventoryChanged;
                }
            }
        }

        private void OnGameStarted()
        {
            ExecuteOnUIThread.Invoke(DrawInventory);
        }

        private void OnInventoryChanged()
        {
            ExecuteOnUIThread.Invoke(DrawInventory);
        }
    }
}
