using UnityEngine;
using UnityEngine.InputSystem;

namespace Controller
{
    [RequireComponent(typeof(CharacterController))]
    public class Move : MonoBehaviour
    {
        [Header("Walk")]
        [SerializeField] private InputActionAsset actionMap;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float acceleration = 3f;
        [SerializeField] private float deceleration = 5f;
        
        [Header("Jump")]
        [SerializeField] private float jumpHeight = 1.5f;
        [SerializeField] private float gravity = -19.62f;
        [SerializeField] private float gravityMultiplier = 2.5f;
        [SerializeField] private float lowJumpMultiplier = 2f;
        
        
        private float _currentSpeed;
        private CharacterController _characterController;
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private Vector3 _velocity;
        private bool _isMoving;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _moveAction = actionMap.FindAction("Move");
            _jumpAction = actionMap.FindAction("Jump");
        }

        private void OnEnable()
        {
            actionMap.FindActionMap("Player").Enable();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnDisable()
        {
            actionMap.FindActionMap("Player").Disable();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        void Update()
        {
            HandleSpeedChange();
            Walk();
            ApplyGravity();
            if (_jumpAction.WasPressedThisFrame() && IsGrounded()) Jump();
        }

        private void HandleSpeedChange()
        {
            Vector2 move = _moveAction.ReadValue<Vector2>();
            bool hasInput = move.magnitude > 0.1f;
    
            if (hasInput)
            {
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, moveSpeed, acceleration * Time.deltaTime);
            }
            else
            {
                _currentSpeed = Mathf.MoveTowards(_currentSpeed, 0f, deceleration * Time.deltaTime);
            }
        }

        private void Walk()
        {
            Vector2 move = _moveAction.ReadValue<Vector2>();
            
            if (move.magnitude > 0.1f)
            {
                Vector3 moveDir = transform.right * move.x + transform.forward * move.y;
                moveDir.y = 0;
                moveDir.Normalize();
                _characterController.Move(moveDir * (_currentSpeed * Time.deltaTime));
            }
        }

        private void Jump()
        {
            _velocity.y = Mathf.Sqrt(-2f * gravity * jumpHeight);
        }

        private void ApplyGravity()
        {
            if (IsGrounded() && _velocity.y < 0)
            {
                _velocity.y = -2f;
            }

            float currentGravity = gravity; 
            if (_velocity.y < 0)
            {
                currentGravity *= gravityMultiplier;
            }
            else if (_velocity.y > 0f && !_jumpAction.IsPressed())
            {
                currentGravity *= lowJumpMultiplier;
            }
            _velocity.y += currentGravity * Time.deltaTime;
            _characterController.Move(_velocity * Time.deltaTime);
        }

        public float Gravity
        {
            get => gravity;
            set => gravity = value;
        }

        public bool IsGrounded() => _characterController.isGrounded;
    }
}