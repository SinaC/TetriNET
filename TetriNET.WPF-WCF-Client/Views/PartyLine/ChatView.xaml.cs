using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using TetriNET.Client.Achievements;
using TetriNET.Client.Interfaces;
using TetriNET.WPF_WCF_Client.Models;
using TetriNET.WPF_WCF_Client.ViewModels;
using TetriNET.WPF_WCF_Client.ViewModels.Achievements;
using TetriNET.WPF_WCF_Client.ViewModels.PartyLine;
using TetriNET.WPF_WCF_Client.Views.Achievements;

namespace TetriNET.WPF_WCF_Client.Views.PartyLine
{
    /// <summary>
    /// Interaction logic for ChatView.xaml
    /// </summary>
    public partial class ChatView : UserControl
    {
       
        public ChatView()
        {
            InitializeComponent();
        }

        #region UI events handler

        private void InputChat_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BindingExpression exp = TxtInputChat.GetBindingExpression(TextBox.TextProperty);
                if (exp != null)
                    exp.UpdateSource();
            }
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow mainWindow = Helpers.VisualTree.FindAncestor<MainWindow>(sender as DependencyObject);
            if (mainWindow != null)
            {
                MainWindowViewModel vm = mainWindow.DataContext as MainWindowViewModel;
                if (vm != null)
                {
                    // Switch to achievements
                    vm.ActiveTabItemIndex = 5;

                    // Select achievement
                    TextBlock txt = sender as TextBlock;
                    if (txt != null)
                    {
                        ChatEntry entry = txt.Tag as ChatEntry;
                        if (entry != null)
                        {
                            IAchievement achievement = entry.Achievement;
                            if (achievement != null)
                                vm.AchievementsViewModel.Select(achievement.Id);
                        }
                    }
                }
            }
        }

        #endregion

        //private void Button_Click_1(object sender, RoutedEventArgs e)
        //{
        //    AchievementManager manager = new AchievementManager();
        //    manager.FindAllAchievements(Assembly.Load("TetriNET.Client.Achievements"));
        //    IAchievement rainbow = manager.Achievements.FirstOrDefault(x => x.Id == 35);
        //    IAchievement need4Speed = manager.Achievements.FirstOrDefault(x => x.Id == 19);

        //    ChatViewModel vm = DataContext as ChatViewModel;
        //    if (vm != null)
        //    {
        //        vm.ChatEntries.Add(new ChatEntry
        //            {
        //                Client = vm.Client,
        //                ChatType = ChatEntry.ChatTypes.OtherAchievement,
        //                Color = ChatColor.Blue,
        //                Achievement = rainbow,
        //                PlayerName = "Dummy2",
        //            });

        //        vm.ChatEntries.Add(new ChatEntry
        //        {
        //            Client = vm.Client,
        //            ChatType = ChatEntry.ChatTypes.OtherAchievement,
        //            Color = ChatColor.Blue,
        //            Achievement = need4Speed,
        //            PlayerName = "Dummy1",
        //        });
        //    }
        //}
    }
}
