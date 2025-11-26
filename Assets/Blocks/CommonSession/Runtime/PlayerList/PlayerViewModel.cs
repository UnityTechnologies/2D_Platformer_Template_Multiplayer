using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Properties;
using Unity.Services.Multiplayer;
using UnityEngine.UIElements;

namespace Blocks.Sessions.Common
{
    /// <summary>
    /// Provides a DataBinding compatible representation of a player in a session.
    /// </summary>
    public class PlayerViewModel :
        /* IReadOnlyPlayer, */
        INotifyBindablePropertyChanged, IDataSourceViewHashProvider, IDisposable
    {
        private IReadOnlyPlayer _player;
        private ISession _session;

        private long _updateVersion;

        /// <summary>
        /// The unique identifier for the player. If not provided for a create or join request, it will be set to the ID of the caller.
        /// </summary>
        [CreateProperty]
        public string Id => _player?.Id;

        /// <summary>
        /// The name of the player retrieved via the Authentication service.
        /// </summary>
        [CreateProperty]
        public string Name => _player?.GetPlayerName();

        /// <summary>
        /// Indicates if the given player is the host of the current Session.
        /// </summary>
        [CreateProperty]
        public bool IsHost => _session?.Host == Id;

        /// <summary>
        /// The allocation id
        /// </summary>
        public string AllocationId => _player?.AllocationId;

        /// <summary>
        /// Custom game-specific properties that apply to an individual player (e.g. &#x60;role&#x60; or &#x60;skill&#x60;).
        /// </summary>
        public IReadOnlyDictionary<string, PlayerProperty> Properties => _player?.Properties;

        /// <summary>
        /// The time at which the member joined the Session.
        /// </summary>
        public DateTime Joined => _player?.Joined ?? DateTime.UnixEpoch;

        /// <summary>
        /// The last time the metadata for this member was updated.
        /// </summary>
        public DateTime LastUpdated => _player?.LastUpdated ?? DateTime.UnixEpoch;

        /// <summary>
        /// The Session the player is part of.
        /// </summary>
        public ISession Session => _session;

        public PlayerViewModel(IReadOnlyPlayer player, ISession session)
        {
            _player = player;
            _session = session;

            _session.Changed += OnSessionChanged;
            _session.SessionHostChanged += OnSessionHostChanged;
            _session.PlayerPropertiesChanged += OnSessionPlayerPropertiesChanged;
        }

        private void OnSessionHostChanged(string obj)
        {
            _updateVersion++;
            Notify(nameof(IsHost));
        }

        private void OnSessionPlayerPropertiesChanged()
        {
            _updateVersion++;
            if (!string.IsNullOrEmpty(Name))
            {
                Notify(nameof(Name));
            }

            Notify(nameof(Properties));
        }

        private void OnSessionChanged()
        {
            _updateVersion++;
            Notify(nameof(Session));
        }

        public void Dispose()
        {
            _session.Changed -= OnSessionChanged;
            _session.SessionHostChanged -= OnSessionHostChanged;
            _session.PlayerPropertiesChanged -= OnSessionPlayerPropertiesChanged;

            _player = null;
            _session = null;
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
