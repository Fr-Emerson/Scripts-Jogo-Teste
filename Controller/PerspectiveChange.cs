using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

namespace Controller
{
    public class PerspectiveChange : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera firstPersonCamera;
        [SerializeField] private CinemachineCamera thirdPersonCamera;
        [SerializeField] private InputActionAsset actionMap;
        
        public static PerspectiveChange Instance { get; private set; }
        
        private InputAction _perspectiveChangeAction;
        private bool _isFirstPerson = true;

        public bool IsFirstPerson => _isFirstPerson;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            Instance = this;
            
            if (!firstPersonCamera || !thirdPersonCamera)
            {
                AutoAttachCameras();
            }
            
            _perspectiveChangeAction = actionMap.FindAction("SwitchCamera");
            if (_perspectiveChangeAction != null)
            {
                _perspectiveChangeAction.performed += OnPerspectiveChange;
            }
            else
            {
                Debug.LogError("SwitchCamera action not found in Input Action Asset!");
            }
        }

        private void OnEnable()
        {
            actionMap?.FindActionMap("Player")?.Enable();
        }

        private void OnDisable()
        {
            actionMap?.FindActionMap("Player")?.Disable();
            
            if (_perspectiveChangeAction != null)
            {
                _perspectiveChangeAction.performed -= OnPerspectiveChange;
            }
        }

        private void Start()
        {
            SetCameraPriorities();
        }

        private void OnPerspectiveChange(InputAction.CallbackContext obj)
        {
            _isFirstPerson = !_isFirstPerson;
            SetCameraPriorities();
            
            Debug.Log(_isFirstPerson ? "Switching to First Person" : "Switching to Third Person");
        }

        private void SetCameraPriorities()
        {
            if (firstPersonCamera && thirdPersonCamera)
            {
                if (_isFirstPerson)
                {
                    firstPersonCamera.Priority = 10;
                    thirdPersonCamera.Priority = 0;
                }
                else
                {
                    firstPersonCamera.Priority = 0;
                    thirdPersonCamera.Priority = 10;
                }
            }
            else
            {
                Debug.LogError("Cameras not assigned!");
            }
        }

        private void AutoAttachCameras()
        {
            if (!firstPersonCamera)
            {
                GameObject fpCamObject = GameObject.FindWithTag("FirstPersonCamera");
                if (fpCamObject != null)
                {
                    firstPersonCamera = fpCamObject.GetComponent<CinemachineCamera>();
                    if (firstPersonCamera)
                    {
                        Debug.Log("First person camera auto-attached: " + firstPersonCamera.name);
                    }
                    else
                    {
                        Debug.LogError("FirstPersonCamera tag found but no CinemachineCamera component!");
                    }
                }
                else
                {
                    Debug.LogWarning("No GameObject with tag 'FirstPersonCamera' found!");
                }
            }

            if (!thirdPersonCamera)
            {
                GameObject tpCamObject = GameObject.FindWithTag("ThirdPersonCamera");
                if (tpCamObject != null)
                {
                    thirdPersonCamera = tpCamObject.GetComponent<CinemachineCamera>();
                    if (thirdPersonCamera)
                    {
                        Debug.Log("Third person camera auto-attached: " + thirdPersonCamera.name);
                    }
                    else
                    {
                        Debug.LogError("ThirdPersonCamera tag found but no CinemachineCamera component!");
                    }
                }
                else
                {
                    Debug.LogWarning("No GameObject with tag 'ThirdPersonCamera' found!");
                }
            }
        }
    }
}