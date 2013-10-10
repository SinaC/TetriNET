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
        //public string Special { get; set; }
        public Specials Special { get; set; }
        public string Source { get; set; }
        public string Target { get; set; }
        //public bool IsTargetVisible { get { return !String.IsNullOrWhiteSpace(Target); } }

        public bool IsAddLines { get; set; }
        public int LinesAdded { get; set; }

        public bool MoreThanOneLine { get { return LinesAdded > 1; } }
    }

    public class InGameChatViewModel : ViewModelBase
    {
        private const int MaxEntries = 12;

        private readonly ObservableCollection<InGameChatEntry> _entries = new ObservableCollection<InGameChatEntry>();
        public ObservableCollection<InGameChatEntry> Entries
        {
            get { return _entries; }
        }

        public void AddEntry(int id, Specials special, string source, string target)
        {
            ExecuteOnUIThread.Invoke(() =>
                {
                    Entries.Add(new InGameChatEntry
                        {
                            Id = id,
                            Special = special,
                            Source = source,
                            Target = target,
                            IsAddLines = false
                        });
                    if (Entries.Count > MaxEntries)
                        Entries.RemoveAt(0);
                });
        }

        public void AddEntry(int id, int linesAdded, string source)
        {
            ExecuteOnUIThread.Invoke(() =>
            {
                Entries.Add(new InGameChatEntry
                {
                    Id = id,
                    LinesAdded = linesAdded,
                    Source = source,
                    IsAddLines = true
                });
                if (Entries.Count > MaxEntries)
                    Entries.RemoveAt(0);
            });
        }

        //public void AddEntry(int id, string special, string source, string target)
        //{
        //    ExecuteOnUIThread.Invoke(() =>
        //        {
        //            Entries.Add(new InGameChatEntry
        //                {
        //                    Id = id,
        //                    Special = special,
        //                    Source = source,
        //                    Target = target,
        //                });
        //            if (Entries.Count > MaxEntries)
        //                Entries.RemoveAt(0);
        //        });
        //}

        //public void AddEntry(int id, int linesAdded, string source)
        //{
        //    ExecuteOnUIThread.Invoke(() =>
        //    {
        //        Entries.Add(new InGameChatEntry
        //        {
        //            Id = id,
        //            Source = source,
        //            Special = String.Format("{0} line{1} added to All", linesAdded, (linesAdded > 1) ? "s" : "")
        //        });
        //        if (Entries.Count > MaxEntries)
        //            Entries.RemoveAt(0);
        //    });
        //}

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
            if (Client.IsPlaying || ClientOptionsViewModel.Instance.DisplayOpponentsFieldEvenWhenNotPlaying)
                AddEntry(specialId + 1, count, playerName);
        }

        private void OnSpecialUsed(int playerId, string playerName, int targetId, string targetName, int specialId, Specials special)
        {
            if (Client.IsPlaying || ClientOptionsViewModel.Instance.DisplayOpponentsFieldEvenWhenNotPlaying)
                AddEntry(specialId + 1, special, playerName, targetName);
                //AddEntry(specialId + 1, Mapper.MapSpecialToString(special), playerName, targetName);
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
                IsAddLines = true,
                Source = "JOEL",
                LinesAdded = 1,
            });
            Entries.Add(new InGameChatEntry
                {
                    Id = 2,
                    IsAddLines = false,
                    Source = "JOEL",
                    Target = "SOMEONE",
                    Special = Specials.SwitchFields,
                });
            Entries.Add(new InGameChatEntry
            {
                Id = 3,
                IsAddLines = true,
                Source = "JOEL",
                LinesAdded = 4,
            });
            //Entries.Add(new InGameChatEntry
            //{
            //    Id = 1,
            //    Source = "JOEL",
            //    Special = "1 line added to All"
            //});
            //Entries.Add(new InGameChatEntry
            //    {
            //        Id = 2,
            //        Source = "JOEL",
            //        Target = "SOMEONE",
            //        Special = Mapper.MapSpecialToString(Specials.SwitchFields),
            //    });
            //Entries.Add(new InGameChatEntry
            //    {
            //        Id = 3,
            //        Source = "JOEL",
            //        Special = "4 lines added to All"
            //    });
        }
    }
}