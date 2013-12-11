using System;
using System.Collections.ObjectModel;
using TetriNET.Common.DataContracts;
using TetriNET.Client.Interfaces;
using TetriNET.WPF_WCF_Client.Helpers;
using TetriNET.WPF_WCF_Client.ViewModels.Options;

namespace TetriNET.WPF_WCF_Client.ViewModels.PlayField
{
    public abstract class InGameChatEntry
    {
    }

    public class PlayerLostEntry : InGameChatEntry
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
    }

    public class SelfSpecialEntry : InGameChatEntry
    {
        public int Id { get; set; }
        public Specials Special { get; set; }
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
    }

    public class SpecialEntry : InGameChatEntry
    {
        public int Id { get; set; }
        public Specials Special { get; set; }
        public string Source { get; set; }
        public int SourceId { get; set; }
        public string Target { get; set; }
        public int TargetId { get; set; }
    }

    public class OneLineAddedEntry : InGameChatEntry
    {
        public int Id { get; set; }
        public string Source { get; set; }
        public int SourceId { get; set; }
    }

    public class MultiLineAddedEntry : InGameChatEntry
    {
        public int Id { get; set; }
        public string Source { get; set; }
        public int SourceId { get; set; }
        public int LinesAdded { get; set; }
    }

    public class AchievementEarnedEntry : InGameChatEntry
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public string AchievementTitle { get; set; }
    }

    public class SelfAchievementEarnedEntry : InGameChatEntry
    {
        public string AchievementTitle { get; set; }
    }

    public class InGameChatViewModel : ViewModelBase
    {
        private const int MaxEntries = 100;

        private readonly ObservableCollection<InGameChatEntry> _entries = new ObservableCollection<InGameChatEntry>();
        public ObservableCollection<InGameChatEntry> Entries
        {
            get { return _entries; }
        }

        public InGameChatViewModel()
        {
            AddEntry(new OneLineAddedEntry
            {
                Id = 1,
                Source = "JOEL0123456789012345",
                SourceId = 1,
            });
            AddEntry(new SpecialEntry
            {
                Id = 2,
                Special = Specials.SwitchFields,
                Source = "JOEL0123456789012345",
                SourceId = 1,
                Target = "SOMEONE0123456789012",
                TargetId = 4,
            });
            AddEntry(new MultiLineAddedEntry
            {
                Id = 3,
                Source = "TSEKWA01234567890123",
                SourceId = 5,
                LinesAdded = 4,
            });
            AddEntry(new SelfSpecialEntry
            {
                Id = 4,
                Special = Specials.SwitchFields,
                PlayerId = 1,
                PlayerName = "TSEKWA01234567890123"
            });
            AddEntry(new PlayerLostEntry
            {
                PlayerName = "JOEL0123456789012345"
            });
            AddEntry(new AchievementEarnedEntry
                {
                    PlayerId = 1,
                    PlayerName = "JOEL0123456789012345",
                    AchievementTitle = "Sniper",
                });
            AddEntry(new SelfAchievementEarnedEntry
            {
                AchievementTitle = "Run baby, Run",
            });
            for (int i = 5; i < 30; i++)
            {
                AddEntry(new SelfSpecialEntry
                {
                    Id = i,
                    Special = Specials.SwitchFields,
                    PlayerId = i,
                    PlayerName = String.Format("JOEL{0}", i),
                });
            }
        }

        public void AddEntry(InGameChatEntry entry)
        {
            ExecuteOnUIThread.Invoke(() =>
                {
                    Entries.Add(entry);
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
            oldClient.OnPlayerLost -= OnPlayerLost;
            oldClient.OnPlayerAchievementEarned -= OnPlayerAchievementEarned;
            oldClient.OnAchievementEarned -= OnAchievementEarned;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.OnGameStarted += OnGameStarted;
            newClient.OnPlayerAddLines += OnPlayerAddLines;
            newClient.OnSpecialUsed += OnSpecialUsed;
            newClient.OnPlayerLost += OnPlayerLost;
            newClient.OnPlayerAchievementEarned += OnPlayerAchievementEarned;
            newClient.OnAchievementEarned += OnAchievementEarned;
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
            {
                if (count == 1)
                    AddEntry(new OneLineAddedEntry
                        {
                            Id = specialId + 1,
                            SourceId = playerId,
                            Source = playerName,
                        });
                else
                    AddEntry(new MultiLineAddedEntry
                        {
                            Id = specialId + 1,
                            SourceId = playerId,
                            Source = playerName,
                            LinesAdded = count,
                        });
            }
        }

        private void OnSpecialUsed(int playerId, string playerName, int targetId, string targetName, int specialId, Specials special)
        {
            if (Client.IsPlaying || ClientOptionsViewModel.Instance.DisplayOpponentsFieldEvenWhenNotPlaying)
            {
                if (targetId == Client.PlayerId)
                    AddEntry(new SelfSpecialEntry
                        {
                            Id = specialId + 1,
                            Special = special,
                            PlayerId = targetId,
                            PlayerName = targetName,
                        });
                else
                    AddEntry(new SpecialEntry
                        {
                            Id = specialId + 1,
                            Special = special,
                            SourceId = playerId,
                            Source = playerName,
                            TargetId = targetId,
                            Target = targetName,
                        });
            }
        }

        private void OnPlayerLost(int playerId, string playerName)
        {
            if (Client.IsPlaying || ClientOptionsViewModel.Instance.DisplayOpponentsFieldEvenWhenNotPlaying)
            {
                AddEntry(new PlayerLostEntry
                    {
                        PlayerId = playerId,
                        PlayerName = playerName,
                    });
            }
        }

        private void OnPlayerAchievementEarned(int playerId, string playerName, int achievementId, string achievementTitle)
        {
            if (Client.IsPlaying || ClientOptionsViewModel.Instance.DisplayOpponentsFieldEvenWhenNotPlaying)
            {
                AddEntry(new AchievementEarnedEntry
                    {
                        PlayerId = playerId,
                        PlayerName = playerName,
                        AchievementTitle = achievementTitle,
                    });
            }
        }

        private void OnAchievementEarned(IAchievement achievement, bool firstTime)
        {
            if (achievement != null && firstTime && (Client.IsPlaying || ClientOptionsViewModel.Instance.DisplayOpponentsFieldEvenWhenNotPlaying))
            {
                AddEntry(new SelfAchievementEarnedEntry
                    {
                        AchievementTitle = achievement.Title
                    });
            }
        }

        #endregion
    }

    public class InGameChatViewModelDesignData : InGameChatViewModel
    {
        public InGameChatViewModelDesignData()
        {
            Entries.Add(new OneLineAddedEntry
            {
                Id = 1,
                Source = "JOEL0123456789012345",
                SourceId = 1,
            });
            Entries.Add(new SpecialEntry
            {
                Id = 2,
                Special = Specials.SwitchFields,
                Source = "JOEL0123456789012345",
                SourceId = 1,
                Target = "SOMEONE0123456789012",
                TargetId = 4,
            });
            Entries.Add(new MultiLineAddedEntry
            {
                Id = 3,
                Source = "TSEKWA01234567890123",
                SourceId = 5,
                LinesAdded = 4,
            });
            Entries.Add(new SelfSpecialEntry
            {
                Id = 4,
                Special = Specials.SwitchFields,
                PlayerId = 1,
                PlayerName = "TSEKWA01234567890123"
            });
            Entries.Add(new PlayerLostEntry
            {
                PlayerName = "JOEL0123456789012345"
            });
            Entries.Add(new AchievementEarnedEntry
                {
                    PlayerId = 1,
                    PlayerName = "JOEL0123456789012345",
                    AchievementTitle = "Sniper",
                });
            Entries.Add(new SelfAchievementEarnedEntry
            {
                AchievementTitle = "Run baby, Run",
            });
            for (int i = 5; i < 30; i++)
            {
                Entries.Add(new SelfSpecialEntry
                {
                    Id = i,
                    Special = Specials.SwitchFields,
                    PlayerId = i,
                    PlayerName = String.Format("JOEL{0}", i),
                });
            }
        }
    }
}