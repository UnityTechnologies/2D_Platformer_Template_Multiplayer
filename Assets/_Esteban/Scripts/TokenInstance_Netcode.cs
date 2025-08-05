using Platformer.Gameplay;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using static Platformer.Core.Simulation;


namespace Platformer.Mechanics
{
    /// <summary>
    /// This class contains the data required for implementing token collection mechanics.
    /// It does not perform animation of the token, this is handled in a batch by the 
    /// TokenController in the scene.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class TokenInstance_Netcode : NetworkBehaviour
    {
        public AudioClip tokenCollectAudio;
        
        [SerializeField]
        private Animator animator;
        
        [SerializeField]
        private NetworkAnimator networkAnimator;
        
        private NetworkVariable<bool> isCollected = new(
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Owner,
            value: false);
        
        public override void OnNetworkSpawn()
        {
            if (IsClient)
            {
                isCollected.OnValueChanged += OnIsCollectableCollected;   
            }
            
            base.OnNetworkSpawn();
        }

        private void OnIsCollectableCollected(bool previousValue, bool newValue)
        {
            gameObject.SetActive(false);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if(!IsOwner)
                return;

            Debug.Log($"Triggered with {other.gameObject.name}");
            
            //only exectue OnPlayerEnter if the player collides with this token.
            var player = other.gameObject.GetComponent<PlayerController_Netcode>();
            if (player != null)
                OnPlayerEnter(player);
        }

        void OnPlayerEnter(PlayerController_Netcode player)
        {
            if (isCollected.Value)
                return;
            
            networkAnimator.Animator.SetBool("isCollected", true);
            
            //disable the gameObject and remove it from the controller update list.
            isCollected.Value = true;
            
            PlayCollectedSoundRpc(RpcTarget.Single(player.NetworkObject.OwnerClientId, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.SpecifiedInParams)]
        void PlayCollectedSoundRpc(RpcParams clientRpcParams)
        {
            //send an event into the gameplay system to perform some behaviour.
            AudioSource.PlayClipAtPoint(tokenCollectAudio, transform.position);
        }
    }
}