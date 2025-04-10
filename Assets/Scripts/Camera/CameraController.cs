using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform _target;
    [SerializeField] private Vector3 _firstPersonOffset = new Vector3(0f, 0.7f, 0f);
    
    [Header("Camera Controls")]
    [SerializeField] private float _mouseSensitivity = 100f;
    [SerializeField] private float _smoothTime = 0.03f;
    [SerializeField] private float _minVerticalAngle = -30f;
    [SerializeField] private float _maxVerticalAngle = 60f;
    
    // Camera rotation variables
    private float _currentRotationX = 0f;
    private float _currentMouseYDelta = 0f;
    private float _mouseDeltaYVelocity = 0f;
    
    private void Start()
    {
        FindPlayerTarget();
        LockCursor();
    }
    
    private void FindPlayerTarget()
    {
        if (_target == null)
        {
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                _target = player.transform;
            }
            else
            {
                Debug.LogError("CameraController: No target assigned and no PlayerController found in scene.");
            }
        }
    }
    
    private void LockCursor()
    {
        // Lock and hide cursor for first person view
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    private void LateUpdate()
    {
        // Handle mouse input for rotation
        HandleRotationInput();
        
        // Update camera position
        //UpdateCameraPosition();
    }
    
    private void HandleRotationInput()
    {
        // Get mouse Y input for vertical rotation
        float mouseY = Input.GetAxis("Mouse Y");
        
        // Apply smoothing to mouse input
        _currentMouseYDelta = Mathf.SmoothDamp(
            _currentMouseYDelta,
            mouseY,
            ref _mouseDeltaYVelocity,
            _smoothTime
        );

        // Calculate and clamp vertical rotation
        _currentRotationX -= _currentMouseYDelta * _mouseSensitivity * Time.deltaTime;
        _currentRotationX = Mathf.Clamp(_currentRotationX, _minVerticalAngle, _maxVerticalAngle);

        // Apply rotation to camera
        transform.localRotation = Quaternion.Euler(_currentRotationX, _target.rotation.y, 0);
    }
    
    private void UpdateCameraPosition()
    {
        if (_target != null)
        {
            // Position camera at eye level
            transform.position = _target.position + _firstPersonOffset;
        }
    }
} 