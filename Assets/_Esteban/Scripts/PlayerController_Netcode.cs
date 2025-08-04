using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;
using Unity.Netcode;
using UnityEngine.InputSystem;

namespace Platformer.Mechanics
{
    /// <summary>
    /// This is the main class used to implement control of the player.
    /// It is a superset of the AnimationController class, but is inlined to allow for any kind of customisation.
    /// </summary>
    public class PlayerController_Netcode : KinematicObject_Netcode
    {
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        /// <summary>
        /// Max horizontal speed of the player.
        /// </summary>
        public float maxSpeed = 7;
        /// <summary>
        /// Initial jump velocity at the start of a jump.
        /// </summary>
        public float jumpTakeOffSpeed = 7;

        public JumpState jumpState = JumpState.Grounded;
        private bool stopJump;
        /*internal new*/ public Collider2D collider2d;
        /*internal new*/ public AudioSource audioSource;
        public Health health;
        public bool controlEnabled = true;

        internal Animator animator;
        
        [SerializeField]
        private ClientNetworkAnimator clientNetworkAnimator;

        private Animator Animator => clientNetworkAnimator.Animator;
        
        bool jump;
        Vector2 move;

        //private bool isFlipped = false;
        private NetworkVariable<bool> isFlipped = new (
            readPerm: NetworkVariableReadPermission.Everyone, 
            writePerm: NetworkVariableWritePermission.Owner,
            value: false);

        private bool IsFlipped
        {
            get => isFlipped.Value;
            set => isFlipped.Value = value;
            
        }
        
        SpriteRenderer spriteRenderer;
        
        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        private InputAction m_MoveAction;
        private InputAction m_JumpAction;

        public Bounds Bounds => collider2d.bounds;

        void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();

            m_MoveAction = InputSystem.actions.FindAction("Player/Move");
            m_JumpAction = InputSystem.actions.FindAction("Player/Jump");
            
            m_MoveAction.Enable();
            m_JumpAction.Enable();
        }

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
                return;

            isFlipped.OnValueChanged += OnIsFlippedChanged;
            
            base.OnNetworkSpawn();
        }

        private void OnIsFlippedChanged(bool oldValue, bool newValue)
        {
            UpdateSpriteFlippedStatus(newValue);
        }
        
        protected override void Update()
        {
            if (!IsOwner)
                return;
            
            if (controlEnabled)
            {
                move.x = m_MoveAction.ReadValue<Vector2>().x;
                if (jumpState == JumpState.Grounded && m_JumpAction.WasPressedThisFrame())
                    jumpState = JumpState.PrepareToJump;
                else if (m_JumpAction.WasReleasedThisFrame())
                {
                    stopJump = true;
                }
            }
            else
            {
                move.x = 0;
            }
            UpdateJumpState();
            base.Update();
        }

        void UpdateJumpState()
        {
            jump = false;
            switch (jumpState)
            {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    jump = true;
                    stopJump = false;
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                    {
                        audioSource.PlayOneShot(jumpAudio);
                        jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                    {
                        jumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.Landed:
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        protected override void ComputeVelocity()
        {
            if (jump && IsGrounded)
            {
                velocity.y = jumpTakeOffSpeed * model.jumpModifier;
                jump = false;
            }
            else if (stopJump)
            {
                stopJump = false;
                if (velocity.y > 0)
                {
                    velocity.y = velocity.y * model.jumpDeceleration;
                }
            }

            if (move.x > 0.01f)
            {
                isFlipped.Value = false;
            }
                
            else if (move.x < -0.01f)
            {
                isFlipped.Value = true;
            }

            UpdateSpriteFlippedStatus(isFlipped.Value);
            
            Animator.SetBool("grounded", IsGrounded);
            Animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

            targetVelocity = move * maxSpeed;
        }

        private void UpdateSpriteFlippedStatus(bool isFlipped)
        {
            spriteRenderer.flipX = isFlipped;
        }
        
        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Landed
        }
    }
}