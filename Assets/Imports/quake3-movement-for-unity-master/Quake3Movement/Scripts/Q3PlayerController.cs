using UnityEngine;
using UnityEngine.InputSystem;

namespace Q3Movement
{
    /// <summary>
    /// This script handles Quake III CPM(A) mod style player movement logic.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class Q3PlayerController : MonoBehaviour
    {
        [System.Serializable]
        public class MovementSettings
        {
            public float MaxSpeed;
            public float Acceleration;
            public float Deceleration;

            public MovementSettings(float maxSpeed, float accel, float decel)
            {
                MaxSpeed = maxSpeed;
                Acceleration = accel;
                Deceleration = decel;
            }
        }

        [Header("Aiming")]
        [SerializeField] private Camera m_Camera;
        [SerializeField] private MouseLook m_MouseLook = new MouseLook();
        [SerializeField] private InputActionAsset m_InputActions;

        [Header("Movement")]
        [SerializeField] private float m_Friction = 6;
        [SerializeField] private float m_Gravity = 20;
        [SerializeField] private float m_JumpForce = 8;
        [Tooltip("Automatically jump when holding jump button")]
        [SerializeField] private bool m_AutoBunnyHop = false;
        [Tooltip("How precise air control is")]
        [SerializeField] private float m_AirControl = 0.3f;
        [SerializeField] private MovementSettings m_GroundSettings = new MovementSettings(7, 14, 10);
        [SerializeField] private MovementSettings m_AirSettings = new MovementSettings(7, 2, 2);
        [SerializeField] private MovementSettings m_StrafeSettings = new MovementSettings(1, 50, 50);

        /// <summary>
        /// Returns player's current speed.
        /// </summary>
        public float Speed { get { return m_Character.velocity.magnitude; } }

        private CharacterController m_Character;
        private Vector3 m_MoveDirectionNorm = Vector3.zero;
        private Vector3 m_PlayerVelocity = Vector3.zero;

        // Used to queue the next jump just before hitting the ground.
        private bool m_JumpQueued = false;

        // Used to display real time friction values.
        private float m_PlayerFriction = 0;

        private Vector3 m_MoveInput;
        private Transform m_Tran;
        private Transform m_CamTran;
        private InputAction m_MoveAction;
        private InputAction m_JumpAction;
        private InputAction m_LookAction;

        private void OnEnable()
        {
            if (!m_InputActions)
                return;

            InputActionMap playerActions = m_InputActions.FindActionMap("Player", true);
            m_MoveAction = playerActions.FindAction("Move", true);
            m_JumpAction = playerActions.FindAction("Jump", true);
            m_LookAction = playerActions.FindAction("Look", true);
            playerActions.Enable();
        }

        private void OnDisable()
        {
            if (m_InputActions)
                m_InputActions.FindActionMap("Player", true).Disable();
        }

        private void Start()
        {
            m_Tran = transform;
            m_Character = GetComponent<CharacterController>();

            if (!m_Camera)
                m_Camera = Camera.main;

            if (!m_InputActions || m_MoveAction == null || m_JumpAction == null || m_LookAction == null)
            {
                Debug.LogError("Q3PlayerController requires the InputSystem_Actions asset with a Player action map.", this);
                enabled = false;
                return;
            }

            m_CamTran = m_Camera.transform;
            m_MouseLook.Init(m_Tran, m_CamTran, m_LookAction);
        }

        private void Update()
        {
            Vector2 moveInput = m_MoveAction.ReadValue<Vector2>();
            m_MoveInput = new Vector3(moveInput.x, 0, moveInput.y);
            m_MouseLook.UpdateCursorLock();    
            QueueJump();

            // Set movement state.
            if (m_Character.isGrounded)
            {
                GroundMove();
            }
            else
            {
                AirMove();
            }

            // Rotate the character and camera.
            m_MouseLook.LookRotation(m_Tran, m_CamTran);

            // Move the character.
            m_Character.Move(m_PlayerVelocity * Time.deltaTime);
        }

        // Queues the next jump.
        private void QueueJump()
        {
            if (m_AutoBunnyHop)
            {
                m_JumpQueued = m_JumpAction.IsPressed();
                return;
            }

            if (m_JumpAction.WasPressedThisFrame() && !m_JumpQueued)
            {
                m_JumpQueued = true;
            }

            if (m_JumpAction.WasReleasedThisFrame())
            {
                m_JumpQueued = false;
            }
        }

        // Handle air movement.
        private void AirMove()
        {
            float accel;

            var wishdir = new Vector3(m_MoveInput.x, 0, m_MoveInput.z);
            wishdir = m_Tran.TransformDirection(wishdir);

            float wishspeed = wishdir.magnitude;
            wishspeed *= m_AirSettings.MaxSpeed;

            wishdir.Normalize();
            m_MoveDirectionNorm = wishdir;

            // CPM Air control.
            float wishspeed2 = wishspeed;
            if (Vector3.Dot(m_PlayerVelocity, wishdir) < 0)
            {
                accel = m_AirSettings.Deceleration;
            }
            else
            {
                accel = m_AirSettings.Acceleration;
            }

            // If the player is ONLY strafing left or right
            if (m_MoveInput.z == 0 && m_MoveInput.x != 0)
            {
                if (wishspeed > m_StrafeSettings.MaxSpeed)
                {
                    wishspeed = m_StrafeSettings.MaxSpeed;
                }

                accel = m_StrafeSettings.Acceleration;
            }

            Accelerate(wishdir, wishspeed, accel);
            if (m_AirControl > 0)
            {
                AirControl(wishdir, wishspeed2);
            }

            // Apply gravity
            m_PlayerVelocity.y -= m_Gravity * Time.deltaTime;
        }

        // Air control occurs when the player is in the air, it allows players to move side 
        // to side much faster rather than being 'sluggish' when it comes to cornering.
        private void AirControl(Vector3 targetDir, float targetSpeed)
        {
            // Only control air movement when moving forward or backward.
            if (Mathf.Abs(m_MoveInput.z) < 0.001f || Mathf.Abs(targetSpeed) < 0.001f)
            {
                return;
            }

            Vector3 horizontalVelocity = m_PlayerVelocity;
            horizontalVelocity.y = 0f;

            float speed = horizontalVelocity.magnitude;
            if (speed < 0.001f)
            {
                return;
            }

            Vector3 horizontalDir = horizontalVelocity / speed;
            float dot = Vector3.Dot(horizontalDir, targetDir);
            if (dot <= 0f)
            {
                return;
            }

            float k = 32f * m_AirControl * dot * dot * Time.deltaTime;
            k = Mathf.Clamp01(k);

            // Smoothly steer the existing horizontal velocity toward the current
            // target direction while keeping the vertical velocity untouched.
            Vector3 correctedDirection = Vector3.Lerp(horizontalDir, targetDir, k);
            correctedDirection.Normalize();

            m_PlayerVelocity.x = correctedDirection.x * speed;
            m_PlayerVelocity.z = correctedDirection.z * speed;
            m_MoveDirectionNorm = correctedDirection;
        }

        // Handle ground movement.
        private void GroundMove()
        {
            // Do not apply friction if the player is queueing up the next jump
            if (!m_JumpQueued)
            {
                ApplyFriction(1.0f);
            }
            else
            {
                ApplyFriction(0);
            }

            var wishdir = new Vector3(m_MoveInput.x, 0, m_MoveInput.z);
            wishdir = m_Tran.TransformDirection(wishdir);
            wishdir.Normalize();
            m_MoveDirectionNorm = wishdir;

            var wishspeed = wishdir.magnitude;
            wishspeed *= m_GroundSettings.MaxSpeed;

            Accelerate(wishdir, wishspeed, m_GroundSettings.Acceleration);

            // Reset the gravity velocity
            m_PlayerVelocity.y = -m_Gravity * Time.deltaTime;

            if (m_JumpQueued)
            {
                m_PlayerVelocity.y = m_JumpForce;
                m_JumpQueued = false;
            }
        }

        private void ApplyFriction(float t)
        {
            // Equivalent to VectorCopy();
            Vector3 vec = m_PlayerVelocity; 
            vec.y = 0;
            float speed = vec.magnitude;
            float drop = 0;

            // Only apply friction when grounded.
            if (m_Character.isGrounded)
            {
                float control = speed < m_GroundSettings.Deceleration ? m_GroundSettings.Deceleration : speed;
                drop = control * m_Friction * Time.deltaTime * t;
            }

            float newSpeed = speed - drop;
            m_PlayerFriction = newSpeed;
            if (newSpeed < 0)
            {
                newSpeed = 0;
            }

            if (speed > 0)
            {
                newSpeed /= speed;
            }

            m_PlayerVelocity.x *= newSpeed;
            // playerVelocity.y *= newSpeed;
            m_PlayerVelocity.z *= newSpeed;
        }

        // Calculates acceleration based on desired speed and direction.
        private void Accelerate(Vector3 targetDir, float targetSpeed, float accel)
        {
            float currentspeed = Vector3.Dot(m_PlayerVelocity, targetDir);
            float addspeed = targetSpeed - currentspeed;
            if (addspeed <= 0)
            {
                return;
            }

            float accelspeed = accel * Time.deltaTime * targetSpeed;
            if (accelspeed > addspeed)
            {
                accelspeed = addspeed;
            }

            m_PlayerVelocity.x += accelspeed * targetDir.x;
            m_PlayerVelocity.z += accelspeed * targetDir.z;
        }
    }
}