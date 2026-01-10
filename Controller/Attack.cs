using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Controller
{
    [RequireComponent(typeof(Move))]
    [RequireComponent(typeof(EventSystem))]
    [RequireComponent(typeof(InputSystemUIInputModule))]
    public class Attack : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActionAsset;
        [SerializeField] private int groundComboMax = 3;
        [SerializeField] private int airComboMax = 3;
        [SerializeField] private float comboResetDelay = 1f;
        [SerializeField] private float cooldownTime = 2f;
        
        private Move _move;
        private InputAction _attackAction;
        private CharacterController _characterController;

        private int _groundComboCount = 0;
        private float _lastGroundAttackTime = 0f;
        
        private int _airComboCount = 0;
        private float _lastAirAttackTime = 0f;
        
        private float _nextAttackTime = 0f;
        private bool _canAttack = true;

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            _attackAction = inputActionAsset.FindAction("Attack");
            _move = GetComponent<Move>();
        }

        private void OnEnable()
        {
            _attackAction.performed += AttackAction;
        }

        private void OnDisable()
        {
            _attackAction.performed -= AttackAction;
        }

        private void Update()
        {
            if (Time.time - _lastGroundAttackTime > comboResetDelay)
            {
                _groundComboCount = 0;
            }

            if (Time.time - _lastAirAttackTime > comboResetDelay)
            {
                _airComboCount = 0;
            }

            if (Time.time >= _nextAttackTime)
            {
                _canAttack = true;
            }

            if (_characterController.isGrounded && _airComboCount > 0)
            {
                _airComboCount = 0;
            }
        }

        private void AttackAction(InputAction.CallbackContext obj)
        {
            if (!_canAttack) return;

            if (!_characterController.isGrounded)
            {
                MidAirAttack();
            }
            else
            {
                GroundAttack();
            }
        }

        private void MidAirAttack()
        {
            _airComboCount++;
            _lastAirAttackTime = Time.time;

            _airComboCount = Mathf.Clamp(_airComboCount, 1, airComboMax);

            Debug.Log($"Air Attack - Combo: {_airComboCount}/{airComboMax}");

            _groundComboCount = 0;

            if (_airComboCount >= airComboMax)
            {
                _airComboCount = 0;
                _canAttack = false;
                _nextAttackTime = Time.time + cooldownTime;
                Debug.Log("Combo aéreo finalizado! Cooldown ativo.");
            }
        }

        private void GroundAttack()
        {
            _groundComboCount++;
            _lastGroundAttackTime = Time.time;

            _groundComboCount = Mathf.Clamp(_groundComboCount, 1, groundComboMax);

            Debug.Log($"Ground Attack - Combo: {_groundComboCount}/{groundComboMax}");

            _airComboCount = 0;

            if (_groundComboCount >= groundComboMax)
            {
                _groundComboCount = 0;
                _canAttack = false;
                _nextAttackTime = Time.time + cooldownTime;
                Debug.Log("Combo no chão finalizado! Cooldown ativo.");
            }
        }
    }
}