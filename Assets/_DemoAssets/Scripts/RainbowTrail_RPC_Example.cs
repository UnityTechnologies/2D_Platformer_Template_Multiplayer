using UnityEngine;
using Unity.Netcode;

namespace Platformer.Mechanics
{
    public class RainbowTrail_RPC_Example : NetworkBehaviour
    {
        PlayerController_DemoVersion playerController;
        PlayerController_DemoVersion.JumpState previousJumpState;
        TrailRenderer trailRenderer;

        void Awake()
        {
            playerController = GetComponentInParent<PlayerController_DemoVersion>();
            trailRenderer = GetComponent<TrailRenderer>();

            if (playerController == null || trailRenderer == null)
                Debug.LogError("Trail renderer is not set up correctly!");

            if(IsOwner)
                previousJumpState = playerController.jumpState;
        }

        void Update()
        {
            if (!IsOwner)
                return;

            var currentState = playerController.jumpState;
            if (previousJumpState == currentState)
                return;

            if (currentState == PlayerController_DemoVersion.JumpState.InFlight)
                ServerChangeTrailRpc(true);
            else
                ServerChangeTrailRpc(false);

            previousJumpState = currentState;
        }

        // Send up to the server
        [Rpc(SendTo.Server)]
        public void ServerChangeTrailRpc(bool isTrailOn)
        {
            ClientChangeTrailRpc(isTrailOn);
        }

        // Send back down to everyone
        [Rpc(SendTo.Everyone)]
        void ClientChangeTrailRpc(bool isTrailOn)
        {
            trailRenderer.emitting = isTrailOn;
        }
    }
}
