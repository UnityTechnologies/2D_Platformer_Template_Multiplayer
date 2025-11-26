using System;
using System.Runtime.CompilerServices;
using Unity.Properties;
using Unity.Services.Multiplayer;
using UnityEngine.UIElements;

namespace Blocks.Sessions.Common
{
    public class SessionStatusViewModel : INotifyBindablePropertyChanged, IDataSourceViewHashProvider, IDisposable
    {
        const string k_NoSessionText = "Not in a session";

        SessionObserver m_SessionObserver;
        ISession m_Session;
        long m_UpdateVersion;

        [CreateProperty]
        public string DisplayText
        {
            get => m_DisplayText;
            set
            {
                if (m_DisplayText == value)
                    return;

                m_DisplayText = value;
                m_UpdateVersion++;
                Notify();
            }
        }

        string m_DisplayText = k_NoSessionText;

        public SessionStatusViewModel(string sessionType)
        {
            m_SessionObserver = new SessionObserver(sessionType);
            m_SessionObserver.AddingSessionStarted += OnAddingSessionStarted;
            m_SessionObserver.AddingSessionFailed += OnAddingSessionFailed;
            m_SessionObserver.SessionAdded += OnSessionAdded;
            if (m_SessionObserver.Session != null)
                OnSessionAdded(m_SessionObserver.Session);
        }

        void OnAddingSessionFailed(AddingSessionOptions sessionOptions, SessionException exception)
        {
            DisplayText = "Session operation failed: " + exception.Message;
        }

        void OnAddingSessionStarted(AddingSessionOptions sessionOptions)
        {
            DisplayText = "Awaiting Session information...";
        }

        void OnSessionAdded(ISession session)
        {
            m_Session = session;
            m_Session.RemovedFromSession += OnRemovedFromSession;
            m_Session.Deleted += OnRemovedFromSession;
            DisplayText = "In session: " + m_Session.Name;
        }

        void OnRemovedFromSession()
        {
            CleanupSession();
            DisplayText = k_NoSessionText;
        }

        void CleanupSession()
        {
            m_Session.RemovedFromSession -= OnRemovedFromSession;
            m_Session.Deleted -= OnRemovedFromSession;
            m_Session = null;
        }

        public void Dispose()
        {
            m_SessionObserver?.Dispose();
            m_SessionObserver = null;

            if (m_Session != null)
                CleanupSession();
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
