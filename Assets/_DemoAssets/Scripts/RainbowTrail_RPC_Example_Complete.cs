using UnityEngine;
using Unity.Netcode;

namespace Platformer.Mechanics
{
    /// <summary>
    /// This script is the exact same thing as the "RainbowTrail_RPC_Example" script.
    /// The only reason this exists is because this is a demo, and there are two versions
    /// of the PlayerController script while you work through it. So for simplicity sake
    /// this version uses the completed version of the PlayerController while 
    /// "RainbowTrail_RPC_Example" uses the Demo version
    /// </summary>
    public class RainbowTrail_RPC_Example_Complete : NetworkBehaviour
    {
        PlayerController_Complete_Locked playerController;
        PlayerController_Complete_Locked.JumpState previousJumpState;
        TrailRenderer trailRenderer;

        void Awake()
        {
            playerController = GetComponentInParent<PlayerController_Complete_Locked>();
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

            if (currentState == PlayerController_Complete_Locked.JumpState.InFlight)
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
