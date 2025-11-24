using UnityEngine;
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
    public class PlayerController_Netcode : KinematicObject //KinematicObject_Netcode //<-- multiplayer version
    {
        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Landed
        }
        
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;
        
        public float maxSpeed = 7;
        public float jumpTakeOffSpeed = 7;
        Vector2 move;
        
        /*internal new*/ public Collider2D collider2d;
        /*internal new*/ public AudioSource audioSource;
        public Health health;
        public bool controlEnabled = true;

        [SerializeField]
        private TrailRenderer trailRenderer;
        
        ////////////////////////////////////////////
        // Animator properties
        internal Animator animator;
        [SerializeField]
        private ClientNetworkAnimator clientNetworkAnimator;
        private Animator Animator => animator;
        
        ////////////////////////////////////////////
        // Jumping properties
        bool jump;
        private bool stopJump;
        
        public JumpState playerJumpState = JumpState.Grounded;
        /* uncomment for networked version
        private NetworkVariable<JumpState> playerJumpState = new(
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Owner,
            value: JumpState.Grounded);
        */
        private JumpState PlayerJumpState
        {
            get => playerJumpState;
            set => playerJumpState = value;
        }
        
        private bool isFlipped = false;
        /* uncomment for networked version
        private NetworkVariable<bool> isFlipped = new (
            readPerm: NetworkVariableReadPermission.Everyone, 
            writePerm: NetworkVariableWritePermission.Owner,
            value: false);
        */
        
        private bool IsFlipped
        {
            get => isFlipped;
            set => isFlipped = value;
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

        /*
        public override void OnNetworkSpawn()
        {
            // Tell everyone who's not the owner that they should react to sprite flip
            if (!IsOwner)
            {
                isFlipped.OnValueChanged += OnIsFlippedChanged;
                playerJumpState.OnValueChanged += OnJumpStateChanged;
            }
            
            base.OnNetworkSpawn();
        }
        */

        private void OnJumpStateChanged(JumpState oldValue, JumpState newValue)
        {
            UpdateTrailRendererVisibility(newValue);
        }
        
        private void UpdateTrailRendererVisibility(JumpState jumpState)
        {
            if(jumpState == JumpState.InFlight)
                trailRenderer.emitting = true;
            else
                trailRenderer.emitting = false;
        }
        
        private void OnIsFlippedChanged(bool oldValue, bool newValue)
        {
            UpdateSpriteFlippedStatus(newValue);
        }
        
        private void UpdateSpriteFlippedStatus(bool isFlipped)
        {
            spriteRenderer.flipX = isFlipped;
        }
        
        protected override void Update()
        {
            //if (!IsOwner)
            //    return;
            
            if (controlEnabled)
            {
                move.x = m_MoveAction.ReadValue<Vector2>().x;
                if (PlayerJumpState == JumpState.Grounded && m_JumpAction.WasPressedThisFrame())
                    PlayerJumpState = JumpState.PrepareToJump;
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
            switch (PlayerJumpState)
            {
                case JumpState.PrepareToJump:
                    PlayerJumpState = JumpState.Jumping;
                    jump = true;
                    stopJump = false;
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                    {
                        audioSource.PlayOneShot(jumpAudio);
                        PlayerJumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                    {
                        PlayerJumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.Landed:
                    PlayerJumpState = JumpState.Grounded;
                    break;
            }

            UpdateTrailRendererVisibility(PlayerJumpState);
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
                IsFlipped = false;
            }
                
            else if (move.x < -0.01f)
            {
                IsFlipped = true;
            }

            UpdateSpriteFlippedStatus(IsFlipped);
            
            Animator.SetBool("grounded", IsGrounded);
            Animator.SetFloat("velocityX", Mathf.Abs(velocity.x) / maxSpeed);

            targetVelocity = move * maxSpeed;
        }
    }
}