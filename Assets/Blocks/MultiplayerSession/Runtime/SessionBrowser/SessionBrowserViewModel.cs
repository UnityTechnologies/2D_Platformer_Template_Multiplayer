using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Unity.Properties;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UIElements;

namespace Blocks.Sessions
{
    public class SessionBrowserViewModel : INotifyBindablePropertyChanged, IDataSourceViewHashProvider, IDisposable
    {
        private SessionObserver m_SessionObserver;
        private ServiceObserver<IMultiplayerService> m_ServiceObserver;

        private long m_UpdateVersion;
        private bool m_SelectedAndAvailable;
        private bool m_CanRefresh;
        private int m_SelectedSessionIndex;

        private ISession m_Session;
        private List<SessionInfoViewModel> m_Sessions;

        [CreateProperty]
        public List<SessionInfoViewModel> Sessions
        {
            get => m_Sessions;
            set
            {
                if (m_Sessions == value)
                {
                    return;
                }

                m_Sessions = value;
                ++m_UpdateVersion;
                Notify();
            }
        }

        [CreateProperty]
        public int SelectedSessionIndex
        {
            get => m_SelectedSessionIndex;
            set
            {
                if (value >= 0 && value < Sessions.Count)
                {
                    m_SelectedSessionIndex = value;
                    SelectedAndAvailable = true;
                }
                else
                {
                    SelectedAndAvailable = false;
                }
            }
        }

        public string GetSelectedSessionId()
        {
            if (SelectedSessionIndex >= 0 && SelectedSessionIndex < Sessions.Count)
            {
                return Sessions[SelectedSessionIndex].Id;
            }
            return null;
        }

        [CreateProperty]
        public bool SelectedAndAvailable
        {
            get => m_SelectedAndAvailable;
            set
            {
                var newValue = value;

                if (value && m_Session != null && m_Session.Id == GetSelectedSessionId())
                {
                    newValue = false;
                }

                if (m_SelectedAndAvailable != newValue)
                {
                    m_SelectedAndAvailable = newValue;
                    ++m_UpdateVersion;
                    Notify();
                }
            }
        }


        [CreateProperty]
        public bool CanRefresh
        {
            get => m_CanRefresh;
            set
            {
                if (m_CanRefresh == value)
                {
                    return;
                }

                m_CanRefresh = value;
                ++m_UpdateVersion;
                Notify();
            }
        }


        public SessionBrowserViewModel(string sessionType)
        {
            Sessions = new List<SessionInfoViewModel>();

            m_SessionObserver = new SessionObserver(sessionType);
            m_SessionObserver.SessionAdded += OnSessionAdded;

            if (m_SessionObserver.Session != null)
            {
                OnSessionAdded(m_SessionObserver.Session);
            }

            // This can be null while in edit mode and needs to be checked before creating the Observer.
            if (UnityServices.Instance != null)
            {
                m_ServiceObserver = new ServiceObserver<IMultiplayerService>();
                if (m_ServiceObserver.Service != null)
                {
                    CanRefresh = true;
                }
                else
                {
                    CanRefresh = false;
                    m_ServiceObserver.Initialized += OnServicesInitialized;
                }
            }
        }

        public async Task JoinSessionAsync(JoinSessionOptions options)
        {
            try
            {
                await MultiplayerService.Instance.JoinSessionByIdAsync(GetSelectedSessionId(), options);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        void OnServicesInitialized(IMultiplayerService service)
        {
            m_ServiceObserver.Initialized -= OnServicesInitialized;
            CanRefresh = true;
        }

        internal async Task UpdateSessionListAsync(int numberOfMaxSessions)
        {
            // if there is no connection to MultiplayerService, do not try to refresh
            if (!CanRefresh)
            {
                Debug.LogWarning("Cannot refresh session list." +
                    "Multiplayer Services are not initialized." +
                    "You can initialize them with default settings by adding a " +
                    "ServicesInitialization and PlayerAuthentication components in your scene.");
                return;
            }

            try
            {
                CanRefresh = false;
                var queryResult = await MultiplayerService.Instance
                    .QuerySessionsAsync(new QuerySessionsOptions
                    {
                        SortOptions = new List<SortOption>
                        {
                            new (SortOrder.Descending,SortField.Name)
                        }
                    });

                // properly dispose the sessionInfo view models first
                foreach (var session in Sessions)
                {
                    session.Dispose();
                }

                Sessions.Clear();
                for (var i = 0; (i < Math.Min(queryResult.Sessions.Count, numberOfMaxSessions)); i++)
                {
                    Sessions.Add(new SessionInfoViewModel(queryResult.Sessions[i]));
                }

                ++m_UpdateVersion;
                CanRefresh = true;
                // reset selection
                SelectedSessionIndex = -1;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to update session list: {ex.Message}");
            }
        }

        private void OnSessionAdded(ISession newSession)
        {
            m_Session = newSession;
            m_Session.RemovedFromSession += OnSessionRemoved;
            m_Session.Deleted += OnSessionRemoved;
            if (m_Session.Id == GetSelectedSessionId())
            {
                SelectedAndAvailable = false;
            }
        }
        private void OnSessionRemoved()
        {
            var lastSessionId = m_Session.Id;
            CleanupSession();
            if (lastSessionId == GetSelectedSessionId())
            {
                SelectedAndAvailable = true;
            }
        }

        private void CleanupSession()
        {
            m_Session.RemovedFromSession -= OnSessionRemoved;
            m_Session.Deleted -= OnSessionRemoved;
            m_Session = null;
        }

        public async Task<ISession> JoinSessionByIdAsync(JoinSessionOptions joinSessionOptions)
        {

            return await MultiplayerService.Instance.JoinSessionByIdAsync(GetSelectedSessionId(), joinSessionOptions);
        }

        public void Dispose()
        {
            if (m_SessionObserver != null)
            {
                m_SessionObserver.SessionAdded -= OnSessionAdded;
                m_SessionObserver.Dispose();
                m_SessionObserver = null;
            }

            if (m_ServiceObserver != null)
            {
                m_ServiceObserver.Dispose();
                m_ServiceObserver = null;
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

        private void Notify([CallerMemberName] string property = null)
        {
            propertyChanged?.Invoke(this, new BindablePropertyChangedEventArgs(property));
        }
    }
}
