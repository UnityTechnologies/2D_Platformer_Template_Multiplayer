using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Properties;
using Unity.Services.Multiplayer;
using UnityEngine.UIElements;

namespace Blocks.Sessions.Common
{
    public class PlayerListViewModel : INotifyBindablePropertyChanged, IDataSourceViewHashProvider, IDisposable
    {
        SessionObserver m_SessionObserver;
        ISession m_Session;
        long m_UpdateVersion;

        List<PlayerViewModel> m_Players;

        [CreateProperty]
        public List<PlayerViewModel> Players
        {
            get => m_Players;
            set
            {
                if (m_Players == value)
                {
                    return;
                }

                m_Players = value;
                ++m_UpdateVersion;
                Notify();
            }
        }

        public PlayerListViewModel(string sessionType)
        {
            Players = new List<PlayerViewModel>();

            m_SessionObserver = new SessionObserver(sessionType);
            m_SessionObserver.SessionAdded += OnSessionAdded;

            if (m_SessionObserver.Session != null)
            {
                OnSessionAdded(m_SessionObserver.Session);
            }
        }

        void UpdatePlayerList()
        {
            if (m_Session == null)
                return;

            var updatedPlayerList = new List<PlayerViewModel>();
            foreach (var player in m_Session.Players)
            {
                updatedPlayerList.Add(new PlayerViewModel(player, m_Session));
            }

            // properly dispose the player view models first
            DisposeAndClearPlayerList();
            Players.AddRange(updatedPlayerList);

            ++m_UpdateVersion;

            Notify(nameof(Players));
        }

        private void DisposeAndClearPlayerList()
        {
            foreach (var player in Players)
            {
                player.Dispose();
            }

            Players.Clear();
        }

        void OnSessionAdded(ISession newSession)
        {
            m_Session = newSession;
            m_Session.RemovedFromSession += OnSessionRemoved;
            m_Session.Deleted += OnSessionRemoved;

            m_Session.PlayerJoined += OnPlayerCountChanged;
            m_Session.PlayerHasLeft += OnPlayerCountChanged;
            m_Session.PlayerPropertiesChanged += OnPlayerPropertiesChanged;

            UpdatePlayerList();
        }

        void OnPlayerCountChanged(string playerId)
        {
            UpdatePlayerList();
        }

        void OnPlayerPropertiesChanged()
        {
            UpdatePlayerList();
        }

        void OnSessionRemoved()
        {
            DisposeAndClearPlayerList();
            CleanupSession();
        }

        void CleanupSession()
        {
            m_Session.RemovedFromSession -= OnSessionRemoved;
            m_Session.Deleted -= OnSessionRemoved;
            m_Session.PlayerHasLeft -= OnPlayerCountChanged;
            m_Session.PlayerJoined -= OnPlayerCountChanged;
            m_Session.PlayerPropertiesChanged -= OnPlayerPropertiesChanged;
            m_Session = null;
        }

        public void Dispose()
        {
            if (m_SessionObserver != null)
            {
                m_SessionObserver.SessionAdded -= OnSessionAdded;
                m_SessionObserver.Dispose();
                m_SessionObserver = null;
            }

            if (m_Session != null)
            {
                CleanupSession();
            }
        }

        /// <summary>
        /// This method is used by UIToolkit to determine if any data bound to the UI has changed.
        /// Instead of hashing the data, an m_UpdateVersion counter is incremented when changes occur.
        /// </summary>
        public long GetViewHashCode() => m_UpdateVersion;

        /// <summary>
        /// Suggested implementation of INotifyBindablePropertyChanged from UIToolkit.
        /// </summary>
        public event EventHandler<BindablePropertyChangedEventArgs> propertyChanged;
        void Notify([CallerMemberName] string property = null)
        {
            propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
        }
    }
}
