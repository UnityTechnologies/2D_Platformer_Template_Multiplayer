using System;
using System.Runtime.CompilerServices;
using Unity.Properties;
using Unity.Services.Multiplayer;
using UnityEngine.UIElements;

namespace Blocks.Sessions.Common
{
    public class PlayerCountViewModel : INotifyBindablePropertyChanged, IDataSourceViewHashProvider, IDisposable
    {
        const string k_DefaultDisplayText = "- / - Players";

        SessionObserver m_SessionObserver;
        ISession m_Session;
        long m_UpdateVersion;

        /// <summary>
        /// This property is bound to <see cref="PlayerCountLabel.text"/> so that the label displays the number of
        /// players in the session.
        /// It is a property using [CreateProperty] attribute to allow for data binding in UIToolkit
        /// <summary>
        [CreateProperty]
        public string DisplayText
        {
            get => m_DisplayText;
            set
            {
                if (m_DisplayText == value)
                {
                    return;
                }

                m_DisplayText = value;
                ++m_UpdateVersion;
                Notify();
            }
        }
        string m_DisplayText = k_DefaultDisplayText;

        public PlayerCountViewModel(string sessionType)
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
            m_Session.Changed += OnSessionChanged;
            m_Session.RemovedFromSession += OnSessionRemoved;
            m_Session.Deleted += OnSessionRemoved;
            OnSessionChanged();
        }

        void OnSessionRemoved()
        {
            DisplayText = k_DefaultDisplayText;
            CleanUpSession();
        }

        void CleanUpSession()
        {
            m_Session.RemovedFromSession -= OnSessionRemoved;
            m_Session.Deleted -= OnSessionRemoved;
            m_Session.Changed -= OnSessionChanged;
            m_Session = null;
        }

        void OnSessionChanged()
        {
            DisplayText = $"{m_Session.Players.Count} / {m_Session.MaxPlayers} Players";
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
                CleanUpSession();
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
