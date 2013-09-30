﻿using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using TetriNET.Common.DataContracts;
using TetriNET.Common.Interfaces;
using TetriNET.WPF_WCF_Client.Helpers;
using TetriNET.WPF_WCF_Client.ViewModels.Options;

namespace TetriNET.WPF_WCF_Client.Views.PlayField
{
    /// <summary>
    /// Interaction logic for InventoryView.xaml
    /// </summary>
    public partial class InventoryView : UserControl, INotifyPropertyChanged
    {
        private const int MaxInventorySize = 15;

        private static readonly SolidColorBrush TransparentColor = new SolidColorBrush(Colors.Transparent);

        public static readonly DependencyProperty ClientProperty = DependencyProperty.Register("InventoryClientProperty", typeof(IClient), typeof(InventoryView), new PropertyMetadata(Client_Changed));
        public IClient Client
        {
            get { return (IClient)GetValue(ClientProperty); }
            set { SetValue(ClientProperty, value); }
        }

        private readonly List<Rectangle> _inventory = new List<Rectangle>();

        private string _firstSpecial;
        public string FirstSpecial
        {
            get { return _firstSpecial; }
            set
            {
                if (_firstSpecial != value)
                {
                    _firstSpecial = value;
                    OnPropertyChanged();
                }
            }
        }

        public InventoryView()
        {
            InitializeComponent();

            for (int i = 0; i < MaxInventorySize; i++)
            {
                Rectangle rect = new Rectangle
                {
                    Width = 16,
                    Height = 16,
                    Fill = TransparentColor
                };
                _inventory.Add(rect);
                Canvas.Children.Add(rect);
                Canvas.SetLeft(rect, i*16);
                Canvas.SetTop(rect, 0);
            }

            FirstSpecial = "No Special Blocks";
        }

        private void DrawInventory()
        {
            if (Client == null)
                return;
            // Reset
            for (int i = 0; i < MaxInventorySize; i++)
                _inventory[i].Fill = TransparentColor;
            // Draw
            List<Specials> specials = Client.Inventory;
            if (specials != null && specials.Any())
            {
                for (int i = 0; i < specials.Count; i++)
                    _inventory[i].Fill = TextureManager.TextureManager.TexturesSingleton.Instance.GetBigSpecial(specials[i]);
                FirstSpecial = Mapper.MapSpecialToString(specials[0]);
            }
            else
            {
                FirstSpecial = "No Special Blocks";
            }
        }

        private void SetInventoryLength()
        {
            Canvas.Width = ServerOptionsViewModel.Instance.Options.InventorySize*16;
        }

        private static void Client_Changed(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            InventoryView @this = sender as InventoryView;

            if (@this != null)
            {
                // Remove old handlers
                IClient oldClient = args.OldValue as IClient;
                if (oldClient != null)
                {
                    oldClient.OnGameStarted -= @this.OnGameStarted;
                    oldClient.OnInventoryChanged -= @this.OnInventoryChanged;
                }
                // Set new client
                IClient newClient = args.NewValue as IClient;
                @this.Client = newClient;
                // Add new handlers
                if (newClient != null)
                {
                    newClient.OnGameStarted += @this.OnGameStarted;
                    newClient.OnInventoryChanged += @this.OnInventoryChanged;
                }
            }
        }

        #region IClient events handler
        private void OnGameStarted()
        {
            ExecuteOnUIThread.Invoke(() =>
            {
                SetInventoryLength();
                DrawInventory();
            });
        }

        private void OnInventoryChanged()
        {
            ExecuteOnUIThread.Invoke(DrawInventory);
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
