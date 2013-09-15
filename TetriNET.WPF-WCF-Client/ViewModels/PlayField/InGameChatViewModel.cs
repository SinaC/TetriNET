using System;
using System.Collections.ObjectModel;
using TetriNET.Common.GameDatas;
using TetriNET.Common.Interfaces;
using TetriNET.WPF_WCF_Client.Helpers;

namespace TetriNET.WPF_WCF_Client.ViewModels.PlayField
{
    public class InGameChatEntry
    {
        public int Id { get; set; }
        public string Special { get; set; }
        public string Source { get; set; }
        public string Target { get; set; }
        public bool IsTargetVisible { get; set; }
    }

    public class InGameChatViewModel : ViewModelBase
    {
        private const int MaxEntries = 15;

        private readonly ObservableCollection<InGameChatEntry> _entries = new ObservableCollection<InGameChatEntry>();
        public ObservableCollection<InGameChatEntry> Entries
        {
            get { return _entries; }
        }


        private void AddEntry(int id, string special, string source, string target = null)
        {
            ExecuteOnUIThread.Invoke(() =>
            {
                Entries.Add(new InGameChatEntry
                {
                    Id = id,
                    Special = special,
                    Source = source,
                    Target = target,
                    IsTargetVisible = !String.IsNullOrEmpty(target)
                });
                if (Entries.Count > MaxEntries)
                    Entries.RemoveAt(0);
            });
        }

        private void ClearEntries()
        {
            ExecuteOnUIThread.Invoke(Entries.Clear);
        }

        #region ViewModelBase
        public override void UnsubscribeFromClientEvents(IClient oldClient)
        {
            oldClient.OnGameStarted -= OnGameStarted;
            oldClient.OnPlayerAddLines -= OnPlayerAddLines;
            oldClient.OnSpecialUsed -= OnSpecialUsed;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.OnGameStarted += OnGameStarted;
            newClient.OnPlayerAddLines += OnPlayerAddLines;
            newClient.OnSpecialUsed += OnSpecialUsed;
        }
        #endregion

        #region IClient events handler
        private void OnGameStarted()
        {
            ClearEntries();
        }

        private void OnPlayerAddLines(string playerName, int specialId, int count)
        {
            AddEntry(specialId + 1, String.Format("{0} line{1} added to All", count, (count > 1) ? "s" : ""), playerName);
        }

        private void OnSpecialUsed(string playerName, string targetName, int specialId, Specials special)
        {
            AddEntry(specialId + 1, Mapper.MapSpecialToString(special), playerName, targetName);
        }
        #endregion
    }
}
