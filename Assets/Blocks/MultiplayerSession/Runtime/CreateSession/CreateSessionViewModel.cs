using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.Properties;
using Unity.Services.Multiplayer;
using UnityEngine.UIElements;

namespace Blocks.Sessions
{
    public class CreateSessionViewModel : IDisposable, IDataSourceViewHashProvider, INotifyBindablePropertyChanged
    {
        SessionObserver m_SessionObserver;
        ISession m_Session;
        long m_UpdateVersion;

        [CreateProperty]
        public bool CanRegisterSession
        {
            get => m_CanRegisterSession;
            private set
            {
                if (m_CanRegisterSession == value)
                    return;

                m_CanRegisterSession = value;
                ++m_UpdateVersion;
                Notify();
            }
        }
        bool m_CanRegisterSession = true;

        [CreateProperty]
        public bool HasSessionName
        {
            get => m_HasSessionName;
            private set
            {
                if (m_HasSessionName == value)
                    return;

                m_HasSessionName = value;
                Notify();
            }
        }
        bool m_HasSessionName;

        [CreateProperty]
        public string SessionName
        {
            get => m_SessionName;
            private set
            {
                if (m_SessionName == value)
                    return;

                m_SessionName = value;

                HasSessionName = m_SessionName != "";

                ++m_UpdateVersion;
                Notify();
            }
        }
        string m_SessionName;

        public CreateSessionViewModel(string sessionType)
        {
            m_SessionObserver = new SessionObserver(sessionType);

            m_SessionObserver.AddingSessionStarted += OnAddingSessionStarted;
            m_SessionObserver.SessionAdded += OnSessionAdded;
            m_SessionObserver.AddingSessionFailed += OnAddingSessionFailed;

            if (m_SessionObserver.Session != null)
            {
                OnSessionAdded(m_SessionObserver.Session);
            }
        }
        void OnAddingSessionFailed(AddingSessionOptions session, SessionException exception) => CanRegisterSession = true;
        void OnAddingSessionStarted(AddingSessionOptions session) => CanRegisterSession = false;

        void OnSessionAdded(ISession session)
        {
            m_Session = session;
            m_Session.RemovedFromSession += OnSessionRemoved;
            m_Session.Deleted += OnSessionRemoved;
            CanRegisterSession = false;
        }

        void OnSessionRemoved()
        {
            m_Session.RemovedFromSession -= OnSessionRemoved;
            m_Session.Deleted -= OnSessionRemoved;
            m_Session = null;
            CanRegisterSession = true;
        }

        public bool AreMultiplayerServicesInitialized()
        {
            return MultiplayerService.Instance != null;
        }

        public async Task<IHostSession> CreateSessionAsync(SessionOptions sessionOptions)
        {
            sessionOptions.Name = SessionName;
            return await MultiplayerService.Instance.CreateSessionAsync(sessionOptions);
        }

        public void Dispose()
        {
            if (m_SessionObserver != null)
            {
                m_SessionObserver.AddingSessionStarted -= OnAddingSessionStarted;
                m_SessionObserver.SessionAdded -= OnSessionAdded;
                m_SessionObserver.AddingSessionFailed -= OnAddingSessionFailed;
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
