using Unity.Services.Multiplayer;
using UnityEngine;

#if !GAMEOBJECTS_NETCODE_2_AVAILABLE
using System;
#endif

namespace Blocks.Sessions.Common
{
    [CreateAssetMenu(fileName = nameof(SessionSettings), menuName = "Services/Blocks/Session/" + nameof(SessionSettings))]
    public class SessionSettings : ScriptableObject
    {
        [Header("Session options")]
        [Tooltip("Maximum number of players allowed in the session.")]
        public int maxPlayers = 5;
        [Tooltip("The name of the created session. It can be used to display the list of existing sessions in the UI.")]
        public string sessionName = "default-session-name";
        [Tooltip("The Session Type is used to uniquely identify a type of session locally. This is used to reference a session through the Kits elements.")]
        public string sessionType = "default-session";
        public bool usePlayerName = true;

        [Header("Network options")]
        [Tooltip("If true, a network session will be created when creating or joining a Session. " +
                 "The Multiplayer Service will try to connect using the NetworkManager or the Entity Drivers depending on which network SDK is installed in your project.")]
        public bool createNetworkSession = true;
        [Tooltip("The type of network connection to use. This is only used if `Create Network Session` is true.")]
        public NetworkType networkType = NetworkType.Relay;

        [Header("DirectConnect options")]
        [Tooltip("The IP address to use for direct connections. This is only used if `Network Type` is set to `Direct`.")]
        public string ipAddress = "127.0.0.1";
        [Tooltip("The port to use for direct connections. This is only used if `Network Type` is set to `Direct`.")]
        public ushort port = 7777;

        public SessionOptions ToSessionOptions()
        {
            var options = BuildSessionOptions();

            if (usePlayerName)
            {
                options.WithPlayerName();
            }

            if (createNetworkSession)
            {
                AddCreateNetworkOptions(options);
            }

            return options;
        }

        void AddCreateNetworkOptions(SessionOptions options)
        {
            switch (networkType)
            {
                case NetworkType.Direct:
                    options.WithDirectNetwork(new DirectNetworkOptions(new ListenIPAddress(ipAddress), new PublishIPAddress(ipAddress), port));
                    break;
                case NetworkType.Relay:
                    options.WithRelayNetwork();
                    break;
                case NetworkType.DistributedAuthority:
#if GAMEOBJECTS_NETCODE_2_AVAILABLE
                    options.WithDistributedAuthorityNetwork();
                    break;
#else
                        throw new InvalidOperationException(
                            "Distributed Authority network is not supported without Netcode for Gameobjects v2.0.");
#endif
            }
        }

        SessionOptions BuildSessionOptions()
        {
            return new SessionOptions
            {
                MaxPlayers = maxPlayers,
                Name = sessionName,
                Type = sessionType,
            };
        }

#if UNITY_SERVER
        public SessionOptions ToServerSessionOptions()
        {
            var options = BuildSessionOptions();

            if (createNetworkSession)
            {
                AddCreateNetworkOptions(options);
            }

            return options;
        }
#endif

        public JoinSessionOptions ToJoinSessionOptions()
        {
            var options = new JoinSessionOptions
            {
                Type = sessionType,
            };

            if (usePlayerName)
            {
                options.WithPlayerName();
            }

            return options;
        }
    }
}
