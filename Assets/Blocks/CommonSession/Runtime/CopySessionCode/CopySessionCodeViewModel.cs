using System;
using System.Runtime.CompilerServices;
using Unity.Properties;
using Unity.Services.Multiplayer;
using UnityEngine.UIElements;

namespace Blocks.Sessions.Common
{
    public class CopySessionCodeViewModel : IDisposable, IDataSourceViewHashProvider, INotifyBindablePropertyChanged
    {
        const string k_NoSessionText = "No Session Code";

        SessionObserver m_SessionObserver;
        ISession m_Session;
        long m_UpdateVersion;

        [CreateProperty]
        public bool HasSessionCode
        {
            get => m_HasSessionCode;
            private set
            {
                if (m_HasSessionCode == value)
                    return;

                m_HasSessionCode = value;
                Notify();
            }
        }
        bool m_HasSessionCode;

        [CreateProperty]
        public string SessionCode
        {
            get => m_SessionCode;
            private set
            {
                if (m_SessionCode == value)
                    return;

                m_SessionCode = value;
                HasSessionCode = m_SessionCode != k_NoSessionText;
                ++m_UpdateVersion;

                Notify();
            }
        }
        string m_SessionCode = k_NoSessionText;

        public CopySessionCodeViewModel(string sessionType)
        {
            m_SessionObserver = new SessionObserver(sessionType);

            m_SessionObserver.SessionAdded += OnSessionAdded;

            if (m_SessionObserver.Session != null)
            {
                OnSessionAdded(m_SessionObserver.Session);
            }
        }

        void OnSessionAdded(ISession session)
        {
            m_Session = session;
            SessionCode = session?.Code;
            m_Session.RemovedFromSession += OnSessionRemoved;
            m_Session.Deleted += OnSessionRemoved;
        }

        void OnSessionRemoved()
        {
            m_Session.RemovedFromSession -= OnSessionRemoved;
            m_Session.Deleted -= OnSessionRemoved;
            SessionCode = k_NoSessionText;
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


