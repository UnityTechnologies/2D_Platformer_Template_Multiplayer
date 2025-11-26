using System;
using System.Collections.Generic;
using Unity.Properties;
using System.Runtime.CompilerServices;
using Unity.Services.Multiplayer;
using UnityEngine.UIElements;

namespace Blocks.Sessions.Common
{
    public class SessionProperty
    {
        public string Key { get; set; }
        public string Visibility { get; set; }
        public string Value { get; set; }
    }

    public class Players
    {
        public string Id { get; set; }
        public string Joined { get; set; }
        public string LastUpdated { get; set; }
        public List<PlayerProperties> PlayerPropertiesList { get; set; }
    }

    public class PlayerProperties
    {
        public string Key { get; set; }
        public string Visibility { get; set; }
        public string Value { get; set; }
    }

    public class SessionInfoViewModel : IDisposable, IDataSourceViewHashProvider, INotifyBindablePropertyChanged
    {
        SessionObserver m_SessionObserver;
        ISession m_Session;
        long m_UpdateVersion;

        [CreateProperty]
        public string SessionState
        {
            get => m_SessionState;
            set
            {
                if (m_SessionState == value)
                    return;
                m_SessionState = value;
                Notify();
            }
        }

        string m_SessionState = "No Session";

        [CreateProperty]
        public DisplayStyle SessionInfoDisplayStyle
        {
            get => m_SessionInfoDisplayStyle;
            set
            {
                if (m_SessionInfoDisplayStyle == value)
                    return;
                m_SessionInfoDisplayStyle = value;
                Notify();
            }
        }

        DisplayStyle m_SessionInfoDisplayStyle = DisplayStyle.None;

        [CreateProperty]
        public string SessionId
        {
            get => m_SessionId;
            set
            {
                if (m_SessionId == value)
                    return;
                m_SessionId = value;
                Notify();
            }
        }

        string m_SessionId = "Unknown Session ID";

        [CreateProperty]
        public string Name
        {
            get => m_SessionName;
            set
            {
                if (m_SessionName == value)
                    return;
                m_SessionName = value;
                Notify();
            }
        }

        string m_SessionName = "Unknown Session Name";

        [CreateProperty]
        public string MaxPlayers
        {
            get => m_MaxPlayers;
            set
            {
                if (m_MaxPlayers == value)
                    return;
                m_MaxPlayers = value;
                Notify();
            }
        }

        string m_MaxPlayers = "Max Players: 0";

        [CreateProperty]
        public string IsPrivate
        {
            get => m_IsPrivate;
            set
            {
                if (m_IsPrivate == value)
                    return;
                m_IsPrivate = value;
                Notify();
            }
        }

        string m_IsPrivate = "true";

        [CreateProperty]
        public string IsLocked
        {
            get => m_IsLocked;
            set
            {
                if (m_IsLocked == value)
                    return;
                m_IsLocked = value;
                Notify();
            }
        }

        string m_IsLocked = "false";

        [CreateProperty]
        public string SessionCode
        {
            get => m_SessionCode;
            set
            {
                if (m_SessionCode == value)
                    return;
                m_SessionCode = value;
                Notify();
            }
        }

        string m_SessionCode = "Unknown Session Code";

        [CreateProperty]
        public string HostID
        {
            get => m_HostId;
            set
            {
                if (m_HostId == value)
                    return;
                m_HostId = value;
                Notify();
            }
        }

        string m_HostId = "Unknown Host ID";

        [CreateProperty]
        public string IsHost
        {
            get => m_IsHost;
            set
            {
                if (m_IsHost == value)
                    return;
                m_IsHost = value;
                Notify();
            }
        }

        string m_IsHost = "Unknown";

        [CreateProperty]
        public List<SessionProperty> SessionProperties
        {
            get => m_SessionProperties;
            set
            {
                if (m_SessionProperties == value)
                    return;
                m_SessionProperties = value;
                Notify();
            }
        }

        List<SessionProperty> m_SessionProperties;

        [CreateProperty]
        public List<Players> PlayerStats
        {
            get => m_PlayerStats;
            set
            {
                if (m_PlayerStats == value)
                    return;
                m_PlayerStats = value;
                Notify();
            }
        }

        List<Players> m_PlayerStats;

        public SessionInfoViewModel(string sessionType)
        {
            SessionProperties = new List<SessionProperty>();

            m_SessionObserver = new SessionObserver(sessionType);
            m_SessionObserver.AddingSessionStarted += SessionInfoReceptionStarted;
            m_SessionObserver.SessionAdded += OnSessionAdded;
            m_SessionObserver.AddingSessionFailed += OnAddingSessionFailed;
            if (m_SessionObserver.Session != null)
            {
                OnSessionAdded(m_SessionObserver.Session);
            }
        }

        void SessionInfoReceptionStarted(AddingSessionOptions sessionOptions)
        {
            SessionInfoDisplayStyle = DisplayStyle.None;
            SessionState = "Awaiting Session information.";

            ++m_UpdateVersion;
        }

        void OnSessionAdded(ISession newSession)
        {
            m_Session = newSession;
            m_Session.Changed += OnSessionChanged;
            m_Session.RemovedFromSession += OnSessionRemoved;
            m_Session.Deleted += OnSessionRemoved;

            OnSessionChanged();
        }

        void OnSessionChanged()
        {
            SessionInfoDisplayStyle = DisplayStyle.Flex;
            SessionState = $"Session State: {m_Session.State}";
            SessionId = $"Session ID: {m_Session.Id}";
            Name = $"Session Name: {m_Session.Name}";
            MaxPlayers = $"MaxPlayers: {m_Session.MaxPlayers}";
            IsPrivate = $"IsPrivate: {m_Session.IsPrivate}";
            IsLocked = $"IsLocked: {m_Session.IsLocked}";
            SessionCode = $"Session Code: {m_Session.Code}";
            HostID = $"Host ID: {m_Session.Host}";
            IsHost = $"Is Host: {m_Session.IsHost}";

            var sessionPropertiesList = new List<SessionProperty>();
            foreach (var property in m_Session.Properties)
            {
                sessionPropertiesList.Add(new SessionProperty { Key = property.Key, Visibility = property.Value.Visibility.ToString(), Value = property.Value.Value });
            }

            SessionProperties = sessionPropertiesList;

            var playersList = new List<Players>();
            foreach (var player in m_Session.Players)
            {
                var playerPropertiesList = new List<PlayerProperties>();
                var playerProperties = player.Properties;
                foreach (var property in playerProperties)
                {
                    playerPropertiesList.Add(new PlayerProperties { Key = property.Key, Visibility = property.Value.Visibility.ToString(), Value = property.Value.Value });
                }

                playersList.Add(new Players { Id = player.Id, Joined = player.Joined.ToString(), LastUpdated = player.LastUpdated.ToString(), PlayerPropertiesList = playerPropertiesList });
            }

            PlayerStats = playersList;

            ++m_UpdateVersion;
        }

        void OnAddingSessionFailed(AddingSessionOptions addingSessionOptions, Exception exception)
        {
            SessionInfoDisplayStyle = DisplayStyle.None;
            SessionState = exception.Message;

            ++m_UpdateVersion;
        }

        void OnSessionRemoved()
        {
            CleanupSession();

            SessionInfoDisplayStyle = DisplayStyle.None;
            SessionState = "Session State: No Session";

            ++m_UpdateVersion;
        }

        public void Dispose()
        {
            if (m_SessionObserver != null)
            {
                m_SessionObserver.AddingSessionStarted -= SessionInfoReceptionStarted;
                m_SessionObserver.SessionAdded -= OnSessionAdded;
                m_SessionObserver.AddingSessionFailed -= OnAddingSessionFailed;

                m_SessionObserver.Dispose();
                m_SessionObserver = null;
            }

            if (m_Session != null)
            {
                CleanupSession();
            }
        }

        void CleanupSession()
        {
            m_Session.Changed -= OnSessionChanged;
            m_Session.RemovedFromSession -= OnSessionRemoved;
            m_Session.Deleted -= OnSessionRemoved;
            m_Session = null;
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
