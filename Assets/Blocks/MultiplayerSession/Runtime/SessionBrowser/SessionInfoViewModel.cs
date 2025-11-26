using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Properties;
using Unity.Services.Multiplayer;
using UnityEngine.UIElements;

namespace Blocks.Sessions
{
    /// <summary>
    /// Provides a DataBinding compatible representation of a <c>Session</c> associated information.
    /// </summary>
    public class SessionInfoViewModel : ISessionInfo,
        INotifyBindablePropertyChanged, IDataSourceViewHashProvider, IDisposable
    {
        private const string k_Unavailalble = "N/A";

        private ISession _session;
        private ISessionInfo _sessionInfo;

        private long _updateVersion;

        public SessionInfoViewModel(ISessionInfo sessionInfo)
        {
            _sessionInfo = sessionInfo;
        }

        public SessionInfoViewModel(ISession session)
        {
            _session = session;

            _session.Changed += OnSessionChanged;
            _session.SessionHostChanged += OnSessionHostChanged;
            _session.SessionPropertiesChanged += OnSessionPropertiesChanged;

            // should we attempt to query the associated info?
        }

        /// <inheritdoc/>
        [CreateProperty]
        public string Name
            => _sessionInfo?.Name ?? _session?.Name;

        /// <inheritdoc/>
        [CreateProperty]
        public string Id
            => _sessionInfo?.Id ?? _session?.Id;

        /// <inheritdoc/>
        [CreateProperty]
        public string Upid
            => _sessionInfo?.Upid ?? k_Unavailalble;

        /// <inheritdoc/>
        [CreateProperty]
        public string HostId
            => _sessionInfo?.HostId ?? _session?.Host;

        /// <inheritdoc/>
        [CreateProperty]
        public int AvailableSlots
            => _sessionInfo?.AvailableSlots ?? _session?.AvailableSlots ?? 0;

        /// <inheritdoc/>
        [CreateProperty]
        public int MaxPlayers
            => _sessionInfo?.MaxPlayers ?? _session?.MaxPlayers ?? 0;

        /// <inheritdoc/>
        [CreateProperty]
        public bool IsLocked
            => _sessionInfo?.IsLocked ?? _session?.IsLocked ?? true;

        /// <inheritdoc/>
        [CreateProperty]
        public bool HasPassword
            => _sessionInfo?.HasPassword ?? _session?.HasPassword ?? true;

        /// <inheritdoc/>
        [CreateProperty]
        public DateTime LastUpdated
            => _sessionInfo?.LastUpdated ?? DateTime.UnixEpoch;

        /// <inheritdoc/>
        [CreateProperty]
        public DateTime Created
            => _sessionInfo?.Created ?? DateTime.UnixEpoch;

        /// <inheritdoc/>
        [CreateProperty]
        public IReadOnlyDictionary<string, SessionProperty> Properties
            => _sessionInfo?.Properties ?? _session?.Properties;

        private void OnSessionHostChanged(string obj)
        {
            _updateVersion++;
            Notify(nameof(HostId));
        }

        private void OnSessionPropertiesChanged()
        {
            _updateVersion++;
            Notify(nameof(Properties));
        }

        private void OnSessionChanged()
        {
            _updateVersion++;
            Notify(nameof(Name));
            Notify(nameof(LastUpdated));
            Notify(nameof(HasPassword));
            Notify(nameof(IsLocked));
            Notify(nameof(MaxPlayers));
            Notify(nameof(AvailableSlots));
        }

        public void Dispose()
        {
            if (_session != null)
            {
                _session.Changed -= OnSessionChanged;
                _session.SessionHostChanged -= OnSessionHostChanged;
                _session.SessionPropertiesChanged -= OnSessionPropertiesChanged;
            }

            _session = null;
            _sessionInfo = null;
        }

        /// <summary>
        /// This method is used by UIToolkit to determine if any data bound to the UI has changed.
        /// Instead of hashing the data, an m_UpdateVersion counter is incremented when changes occur.
        /// </summary>
        public long GetViewHashCode() => _updateVersion;

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
