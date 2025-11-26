#define MULTIPLAYER
#define MULTIPLAYER_ANIMATION
#define MULTIPLAYER_SPRITE_FLIP

using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;
using UnityEngine.InputSystem;
using Unity.Netcode;

namespace Platformer.Mechanics
{
    /// <summary>
    /// This is the main class used to implement control of the player.
    /// It is a superset of the AnimationController class, but is inlined to allow for any kind of customisation.
    /// </summary>
#if MULTIPLAYER
    public class PlayerController1 : KinematicObject_Netcode
#else
    public class PlayerController1 : KinematicObject
#endif
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

        bool jump;
        Vector2 move;
        SpriteRenderer spriteRenderer;
        internal Animator animator;
        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        private InputAction m_MoveAction;
        private InputAction m_JumpAction;

        public Bounds Bounds => collider2d.bounds;

#if MULTIPLAYER_SPRITE_FLIP
        private NetworkVariable<bool> isFlipped = new (
            readPerm: NetworkVariableReadPermission.Everyone, 
            writePerm: NetworkVariableWritePermission.Owner,
            value: false);
#endif

        void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();

#if MULTIPLAYER_ANIMATION
            animator = GetComponent<ClientNetworkAnimator>().Animator;
#else
            animator = GetComponent<Animator>();
#endif      
            m_MoveAction = InputSystem.actions.FindAction("Player/Move");
            m_JumpAction = InputSystem.actions.FindAction("Player/Jump");
            
            m_MoveAction.Enable();
            m_JumpAction.Enable();
        }

#if MULTIPLAYER_SPRITE_FLIP
        public override void OnNetworkSpawn()
        {
            // Tell everyone who's not the owner that they should react to sprite flip
            if (!IsOwner)
                isFlipped.OnValueChanged += OnIsFlippedChanged; 

            base.OnNetworkSpawn();
        }
#endif

        protected override void Update()
        {
#if MULTIPLAYER
            if (!IsOwner)
                return;
#endif
            if (controlEnabled)
            {
                move.x = m_MoveAction.ReadValue<Vector2>().x;
                if (jumpState == JumpState.Grounded && m_JumpAction.WasPressedThisFrame())
                    jumpState = JumpState.PrepareToJump;
                else if (m_JumpAction.WasReleasedThisFrame())
                {
                    stopJump = true;
                    //Schedule<PlayerStopJump>().player = this; // We don't need this in this demo
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
                        //Schedule<PlayerJumped>().player = this; // We don't need this in this demo
                        jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                    {
                        //Schedule<PlayerLanded>().player = this; // We don't need this in this demo
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
                spriteRenderer.flipX = false;
            else if (move.x < -0.01f)
                spriteRenderer.flipX = true;

#if MULTIPLAYER_SPRITE_FLIP
            // Only sync variable if something has changed
            if (spriteRenderer.flipX != isFlipped.Value)
                isFlipped.Value = spriteRenderer.flipX;
#endif

            animator.SetBool("grounded", IsGrounded);
            animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

            targetVelocity = move * maxSpeed;
        }

#if MULTIPLAYER_SPRITE_FLIP
        private void OnIsFlippedChanged(bool oldValue, bool newValue)
        {
            spriteRenderer.flipX = newValue;
        }
#endif

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