using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.Properties;
using Unity.Services.Multiplayer;
using UnityEngine.UIElements;

namespace Blocks.Sessions
{
    public class JoinSessionByCodeViewModel : IDisposable, IDataSourceViewHashProvider, INotifyBindablePropertyChanged
    {
        const string k_ValidSessionCodeCharacters = "6789BCDFGHJKLMNPQRTWbcdfghjklmnpqrtw";

        SessionObserver m_SessionObserver;
        ISession m_Session;
        long m_UpdateVersion;

        [CreateProperty]
        public bool CanJoinSession
        {
            get => m_CanJoinSession;
            private set
            {
                var canJoin = value;
                if(canJoin && m_Session != null)
                    canJoin = false;

                if (m_CanJoinSession == canJoin)
                    return;

                m_CanJoinSession = canJoin;
                ++m_UpdateVersion;
                Notify();
            }
        }
        bool m_CanJoinSession;

        [CreateProperty]
        public string SessionCode
        {
            get => m_SessionCode;
            private set
            {
                if (m_SessionCode == value)
                    return;

                m_SessionCode = value;
                CanJoinSession = CheckIsSessionCodeFormatValid(m_SessionCode);

                ++m_UpdateVersion;
                Notify();
            }
        }
        string m_SessionCode;

        public JoinSessionByCodeViewModel(string sessionType)
        {
            m_SessionObserver = new SessionObserver(sessionType);

            m_SessionObserver.SessionAdded += OnSessionAdded;

            if (m_SessionObserver.Session != null)
            {
                OnSessionAdded(m_SessionObserver.Session);
            }
        }

        static bool CheckIsSessionCodeFormatValid(string str)
        {
            if (string.IsNullOrEmpty(str) || str.Length is < 6 or > 8)
                return false;

            foreach (var c in str)
            {
                if (!k_ValidSessionCodeCharacters.Contains(c))
                    return false;
            }
            return true;
        }

        void OnSessionAdded(ISession session)
        {
            m_Session = session;
            m_Session.RemovedFromSession += OnSessionRemoved;
            m_Session.Deleted += OnSessionRemoved;
            CanJoinSession = false;
        }

        void OnSessionRemoved()
        {
            m_Session.RemovedFromSession -= OnSessionRemoved;
            m_Session.Deleted -= OnSessionRemoved;
            m_Session = null;
            CanJoinSession = CheckIsSessionCodeFormatValid(SessionCode);
        }

        public bool AreMultiplayerServicesInitialized()
        {
            return MultiplayerService.Instance != null;
        }

        public async Task<ISession> JoinSessionByCodeAsync(JoinSessionOptions joinSessionOptions)
        {
            return await MultiplayerService.Instance.JoinSessionByCodeAsync(SessionCode, joinSessionOptions);
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
                m_Session.RemovedFromSession -= OnSessionRemoved;
                m_Session.Deleted -= OnSessionRemoved;
                m_Session = null;
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
        void Notify([CallerMemberName] string property = null) =>
            propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
    }
}
