using System.Collections.ObjectModel;
using TetriNET.Common.DataContracts;
using TetriNET.Client.Interfaces;
using TetriNET.WPF_WCF_Client.Helpers;
using TetriNET.WPF_WCF_Client.ViewModels.Options;

namespace TetriNET.WPF_WCF_Client.ViewModels.PlayField
{
    public class InGameChatEntry
    {
        public int Id { get; set; }
        public Specials Special { get; set; }
        public string Source { get; set; }
        public int SourceId { get; set; }
        public string Target { get; set; }
        public int TargetId { get; set; }
        public int LinesAdded { get; set; }

        public bool IsColorSchemeUsed
        {
            get { return ClientOptionsViewModel.Instance == null || ClientOptionsViewModel.Instance.IsColorSchemeUsed; } // true if no instance (aka in designer mode)
        }

        public bool IsAddOneLine
        {
            get { return LinesAdded == 1 && !IsColorSchemeUsed; }
        }

        public bool IsMoreThanOneLine
        {
            get { return LinesAdded > 1 && !IsColorSchemeUsed; }
        }

        public bool IsSpecial
        {
            get { return LinesAdded == 0 && !IsColorSchemeUsed; }
        }

        public bool IsColorAddOneLine
        {
            get { return LinesAdded == 1 && IsColorSchemeUsed; }
        }

        public bool IsColorMoreThanOneLine
        {
            get { return LinesAdded > 1 && IsColorSchemeUsed; }
        }

        public bool IsColorSpecial
        {
            get { return LinesAdded == 0 && IsColorSchemeUsed; }
        }
    }

    public class InGameChatViewModel : ViewModelBase
    {
        private const int MaxEntries = 12;

        private readonly ObservableCollection<InGameChatEntry> _entries = new ObservableCollection<InGameChatEntry>();
        public ObservableCollection<InGameChatEntry> Entries
        {
            get { return _entries; }
        }

        public void AddEntry(int id, Specials special, string source, int sourceId, string target, int targetId)
        {
            ExecuteOnUIThread.Invoke(() =>
                {
                    Entries.Add(new InGameChatEntry
                        {
                            Id = id,
                            Special = special,
                            Source = source,
                            SourceId = sourceId,
                            Target = target,
                            TargetId = targetId,
                        });
                    if (Entries.Count > MaxEntries)
                        Entries.RemoveAt(0);
                });
        }

        public void AddEntry(int id, int linesAdded, string source, int sourceId)
        {
            ExecuteOnUIThread.Invoke(() =>
            {
                Entries.Add(new InGameChatEntry
                {
                    Id = id,
                    LinesAdded = linesAdded,
                    Source = source,
                    SourceId = sourceId,
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

        private void OnPlayerAddLines(int playerId, string playerName, int specialId, int count)
        {
            if (Client.IsPlaying || ClientOptionsViewModel.Instance.DisplayOpponentsFieldEvenWhenNotPlaying)
                AddEntry(specialId + 1, count, playerName, playerId);
        }

        private void OnSpecialUsed(int playerId, string playerName, int targetId, string targetName, int specialId, Specials special)
        {
            if (Client.IsPlaying || ClientOptionsViewModel.Instance.DisplayOpponentsFieldEvenWhenNotPlaying)
                AddEntry(specialId + 1, special, playerName, playerId, targetName, targetId);
        }

        #endregion
    }

    public class InGameChatViewModelDesignData : InGameChatViewModel
    {
        public InGameChatViewModelDesignData()
        {
            Entries.Add(new InGameChatEntry
            {
                Id = 1,
                Source = "JOEL0123456789012345",
                SourceId = 1,
                LinesAdded = 1,
            });
            Entries.Add(new InGameChatEntry
            {
                Id = 2,
                Source = "JOEL0123456789012345",
                SourceId = 1,
                Target = "SOMEONE0123456789012",
                TargetId = 4,
                Special = Specials.SwitchFields,
            });
            Entries.Add(new InGameChatEntry
            {
                Id = 3,
                Source = "TSEKWA01234567890123",
                SourceId = 5,
                LinesAdded = 4,
            });
        }
    }
}