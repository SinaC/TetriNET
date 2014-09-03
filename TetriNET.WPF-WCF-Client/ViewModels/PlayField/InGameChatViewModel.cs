using System;
using System.Collections.ObjectModel;
using TetriNET.Common.DataContracts;
using TetriNET.Client.Interfaces;
using TetriNET.WPF_WCF_Client.Helpers;
using TetriNET.WPF_WCF_Client.Models;
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

        public bool IsOnPlayer { get; set; }
    }

    public class SpecialEntry : InGameChatEntry
    {
        public int Id { get; set; }
        public Specials Special { get; set; }
        public string Source { get; set; }
        public int SourceId { get; set; }
        public string Target { get; set; }
        public int TargetId { get; set; }

        public bool IsOnPlayer { get; set; }
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

    public class SimpleMessageEntry : InGameChatEntry
    {
        public string Msg { get; set; }
        public ChatColor Color { get; set; }
    }

    public class InGameChatViewModel : ViewModelBase
    {
        private const int MaxEntries = 100;

        private readonly ObservableCollection<InGameChatEntry> _entries = new ObservableCollection<InGameChatEntry>();
        public ObservableCollection<InGameChatEntry> Entries
        {
            get { return _entries; }
        }

        //public InGameChatViewModel()
        //{
        //    AddEntry(new OneLineAddedEntry
        //    {
        //        Id = 1,
        //        Source = "JOEL0123456789012345",
        //        SourceId = 1,
        //    });
        //    AddEntry(new SpecialEntry
        //    {
        //        Id = 2,
        //        Special = Specials.SwitchFields,
        //        Source = "JOEL0123456789012345",
        //        SourceId = 1,
        //        Target = "SOMEONE0123456789012",
        //        TargetId = 4,
        //        IsOnPlayer = false,
        //    });
        //    AddEntry(new MultiLineAddedEntry
        //    {
        //        Id = 3,
        //        Source = "TSEKWA01234567890123",
        //        SourceId = 5,
        //        LinesAdded = 4,
        //    });
        //    AddEntry(new SpecialEntry
        //    {
        //        Id = 4,
        //        Special = Specials.SwitchFields,
        //        Source = "JOEL0123456789012345",
        //        SourceId = 1,
        //        Target = "SOMEONE0123456789012",
        //        TargetId = 4,
        //        IsOnPlayer = false,
        //    });
        //    AddEntry(new PlayerLostEntry
        //    {
        //        PlayerName = "SOMEONE0123456789012"
        //    });
        //    AddEntry(new PlayerLostEntry
        //    {
        //        PlayerName = "JOEL0123456789012345",
        //        IsOnPlayer = true,
        //    });
        //    AddEntry(new AchievementEarnedEntry
        //        {
        //            PlayerId = 1,
        //            PlayerName = "JOEL0123456789012345",
        //            AchievementTitle = "Sniper",
        //        });
        //    AddEntry(new SelfAchievementEarnedEntry
        //    {
        //        AchievementTitle = "Run baby, Run",
        //    });
        //    for (int i = 5; i < 30; i++)
        //    {
        //        AddEntry(new SpecialEntry
        //        {
        //            Id = i,
        //            Special = Specials.SwitchFields,
        //            SourceId = i,
        //            Source = String.Format("JOEL{0}", i),
        //            TargetId = i,
        //            Target = String.Format("JOEL{0}", i),
        //            IsOnPlayer = true,
        //        });
        //    }
        //}

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
            oldClient.RegisteredAsPlayer -= OnClientRegistered;
            oldClient.RegisteredAsSpectator -= OnClientRegistered;
            oldClient.GameStarted -= OnGameStarted;
            oldClient.PlayerAddLines -= OnPlayerAddLines;
            oldClient.SpecialUsed -= OnSpecialUsed;
            oldClient.PlayerLost -= OnPlayerLost;
            oldClient.PlayerAchievementEarned -= OnPlayerAchievementEarned;
            oldClient.AchievementEarned -= OnAchievementEarned;
            oldClient.GameOver -= OnGameOver;
            oldClient.GamePaused -= OnGamePaused;
            oldClient.GameResumed -= OnGameResumed;
        }

        public override void SubscribeToClientEvents(IClient newClient)
        {
            newClient.RegisteredAsPlayer += OnClientRegistered;
            newClient.RegisteredAsSpectator += OnClientRegistered;
            newClient.GameStarted += OnGameStarted;
            newClient.PlayerAddLines += OnPlayerAddLines;
            newClient.SpecialUsed += OnSpecialUsed;
            newClient.PlayerLost += OnPlayerLost;
            newClient.PlayerAchievementEarned += OnPlayerAchievementEarned;
            newClient.AchievementEarned += OnAchievementEarned;
            newClient.GameOver += OnGameOver;
            newClient.GamePaused += OnGamePaused;
            newClient.GameResumed += OnGameResumed;
        }

        #endregion

        #region IClient events handler

        private void OnClientRegistered(RegistrationResults result, Versioning serverVersion, int playerId, bool isServerMaster)
        {
            ClearEntries();
        }

        private void OnClientRegistered(RegistrationResults result, Versioning serverVersion, int spectatorId)
        {
            ClearEntries();
        }

        private void OnGameStarted()
        {
            ClearEntries();
        }

        private void OnPlayerAddLines(int playerId, string playerName, int specialId, int count)
        {
            if (Client.IsPlaying || Client.IsSpectator || ClientOptionsViewModel.Instance.DisplayOpponentsFieldEvenWhenNotPlaying)
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
            if (Client.IsPlaying || Client.IsSpectator || ClientOptionsViewModel.Instance.DisplayOpponentsFieldEvenWhenNotPlaying)
            {
                AddEntry(new SpecialEntry
                    {
                        Id = specialId + 1,
                        Special = special,
                        SourceId = playerId,
                        Source = playerName,
                        TargetId = targetId,
                        Target = targetName,

                        IsOnPlayer = targetId == Client.PlayerId && !Client.IsSpectator,
                    });
            }
        }

        private void OnPlayerLost(int playerId, string playerName)
        {
            if (Client.IsPlaying || Client.IsSpectator || ClientOptionsViewModel.Instance.DisplayOpponentsFieldEvenWhenNotPlaying)
            {
                AddEntry(new PlayerLostEntry
                    {
                        PlayerId = playerId,
                        PlayerName = playerName,

                        IsOnPlayer = playerId == Client.PlayerId && !Client.IsSpectator,
                    });
            }
        }

        private void OnPlayerAchievementEarned(int playerId, string playerName, int achievementId, string achievementTitle)
        {
            if (Client.IsPlaying || Client.IsSpectator || ClientOptionsViewModel.Instance.DisplayOpponentsFieldEvenWhenNotPlaying)
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
            if (achievement != null && firstTime && (Client.IsPlaying || Client.IsSpectator || ClientOptionsViewModel.Instance.DisplayOpponentsFieldEvenWhenNotPlaying))
            {
                AddEntry(new SelfAchievementEarnedEntry
                    {
                        AchievementTitle = achievement.Title
                    });
            }
        }

        private void OnGameOver()
        {
            if (Client.IsPlaying || Client.IsSpectator || ClientOptionsViewModel.Instance.DisplayOpponentsFieldEvenWhenNotPlaying)
            {
                AddEntry(new PlayerLostEntry
                {
                    PlayerId = Client.PlayerId,
                    PlayerName = Client.Name,
                });
            }
        }

        private void OnGameResumed()
        {
            if (Client.IsPlaying || Client.IsSpectator || ClientOptionsViewModel.Instance.DisplayOpponentsFieldEvenWhenNotPlaying)
            {
                AddEntry(new SimpleMessageEntry
                {
                    Msg = "*** GAME RESUMED ***",
                    Color = ChatColor.Magenta,
                });
            }
        }

        private void OnGamePaused()
        {
            if (Client.IsPlaying || Client.IsSpectator || ClientOptionsViewModel.Instance.DisplayOpponentsFieldEvenWhenNotPlaying)
            {
                AddEntry(new SimpleMessageEntry
                {
                    Msg = "*** GAME PAUSED ***",
                    Color = ChatColor.Magenta,
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
                IsOnPlayer = false,
            });
            Entries.Add(new MultiLineAddedEntry
            {
                Id = 3,
                Source = "TSEKWA01234567890123",
                SourceId = 5,
                LinesAdded = 4,
            });
            Entries.Add(new SpecialEntry
            {
                Id = 4,
                Special = Specials.SwitchFields,
                SourceId = 1,
                Source = "TSEKWA01234567890123",
                TargetId = 4,
                Target = "SOMEONE0123456789012",
                IsOnPlayer = false,
            });
            Entries.Add(new PlayerLostEntry
            {
                PlayerName = "SOMEONE0123456789012"
            });
            Entries.Add(new PlayerLostEntry
            {
                PlayerName = "JOEL0123456789012345",
                IsOnPlayer = true,
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
            Entries.Add(new SimpleMessageEntry
                {
                    Msg = "Simple message",
                    Color = ChatColor.DodgerBlue,
                });
            for (int i = 5; i < 30; i++)
            {
                Entries.Add(new SpecialEntry
                {
                    Id = i,
                    Special = Specials.SwitchFields,
                    SourceId = i,
                    Source = String.Format("JOEL{0}", i),
                    TargetId = i,
                    Target = String.Format("JOEL{0}", i),
                    IsOnPlayer = true,
                });
            }
        }
    }
}