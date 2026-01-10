using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Controller
{
    [RequireComponent(typeof(CharacterController))]
    public class Rotate : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActionAsset;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float sensitivity = 10f;
        
        private InputAction _lookAction;

        private void Awake()
        {
            _lookAction = inputActionAsset.FindAction("Look");
        }

      
        private void Update()
        {
            AttachCamera();
            RotateCamera();
        }
        
        private float _rotationX = 0f;

        private void RotateCamera() {
            if (!cameraTransform )
            {
                Debug.LogWarning("Camera Transform is not assigned.");
            }
            Vector2 lookInput = _lookAction.ReadValue<Vector2>();
            float mouseX = lookInput.x * Time.deltaTime;
            float mouseY = lookInput.y * Time.deltaTime;
            transform.Rotate(Vector3.up * mouseX);
            transform.forward = cameraTransform.forward;
    
            if (cameraTransform) {
                _rotationX -= mouseY;
                _rotationX = Mathf.Clamp(_rotationX, -90f, 90f); 
                cameraTransform.localRotation = Quaternion.Euler(_rotationX, 0f, 0f);
            }
        }

        private void OnEnable() {
            _lookAction?.Enable();
        }

        private void OnDisable() {
            _lookAction?.Disable();
        }
        private void AttachCamera()
        {
            if (!cameraTransform)
            {
                if (Camera.main)
                {
                    cameraTransform = Camera.main.transform;
                    
                }
            }
        }
        
    }
}