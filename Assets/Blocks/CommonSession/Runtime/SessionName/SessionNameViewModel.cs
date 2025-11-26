using System;
using System.Runtime.CompilerServices;
using Unity.Properties;
using Unity.Services.Multiplayer;
using UnityEngine.UIElements;

namespace Blocks.Sessions.Common
{
    public class SessionNameViewModel : INotifyBindablePropertyChanged, IDataSourceViewHashProvider, IDisposable
    {
        const string k_NoSession = "No Session joined";

        SessionObserver m_SessionObserver;
        ISession m_Session;
        long m_UpdateVersion;

        /// <summary>
        /// This property is bound to <see cref="SessionNameLabel.text"/> so that the label displays the current
        /// session name.
        /// <summary>
        [CreateProperty]
        public string SessionName
        {
            get => m_SessionName;
            set
            {
                if (m_SessionName == value)
                {
                    return;
                }

                m_SessionName = value;
                ++m_UpdateVersion;
                Notify();
            }
        }
        string m_SessionName = k_NoSession;

        public SessionNameViewModel(string sessionType)
        {
            m_SessionObserver = new SessionObserver(sessionType);
            m_SessionObserver.SessionAdded += OnSessionAdded;

            if (m_SessionObserver.Session != null)
            {
                OnSessionAdded(m_SessionObserver.Session);
            }
        }

        void OnSessionAdded(ISession newSession)
        {
            m_Session = newSession;
            m_Session.RemovedFromSession += OnSessionRemoved;
            m_Session.Deleted += OnSessionRemoved;
            m_Session.Changed += OnSessionChanged;
            SessionName = newSession.Name;
        }

        void OnSessionChanged()
        {
            SessionName = m_Session.Name;
        }

        void OnSessionRemoved()
        {
            SessionName = k_NoSession;
            CleanupSession();
        }

        void CleanupSession()
        {
            m_Session.RemovedFromSession -= OnSessionRemoved;
            m_Session.Deleted -= OnSessionRemoved;
            m_Session.Changed -= OnSessionChanged;
            m_Session = null;
        }

        public void Dispose()
        {
            if (m_SessionObserver != null)
            {
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
