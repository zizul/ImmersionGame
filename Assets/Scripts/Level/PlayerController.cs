using UnityEngine;
using UnityEngine.Events;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private BoxCollider _boxCollider;

    [Header("Movement Parameters")]
    [SerializeField] private float _movementSpeed = 5f;
    [SerializeField] private float _accelerationMultiplier = 1.5f;
    [SerializeField] private float _mouseSensitivity = 100f;
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _groundCheckDistance = 0.3f;
    [SerializeField] private float _edgeDetectionDistance = 0.6f;
    [SerializeField] private Vector3 _resetPosition = new(-9.59000015f, 2.8000005f, -2.88000011f);
    [SerializeField] private string _movingPlatformTag = "MovingPlatform";

    [Header("Look Controls")]
    // Mouse rotation variables
    private float _rotationY = -90f;
    private float _currentMouseXDelta = 0f;
    private float _mouseDeltaXVelocity = 0f;
    private float _smoothTime = 0.03f;

    // Movement state variables
    private Vector3 _movementDirection;
    private bool _triggerJump;
    private bool _isGrounded;
    private bool _isAccelerating;

    // Moving platform
    private Transform _currentPlatform;
    private Vector3 _platformVelocity;
    private MovingPlatformController _currentPlatformController;

    // use field to avoid raycast hit allocation
    private RaycastHit hit;

    public delegate void StateChangedHandler(PlayerState playerState);
    public delegate void SpeedBoostChangedHandler(float speedMultiplier, float jumpMultiplier, float remainingTime);

    public event StateChangedHandler OnPlayerStateChanged;
    public event SpeedBoostChangedHandler OnSpeedBoostChanged;

    [Header("Power-Up Parameters")]
    [SerializeField] private GameObject _speedBoostVFX;
    private float _speedBoostMultiplier = 1f;
    private float _jumpBoostMultiplier = 1f;
    private float _speedBoostEndTime = 0f;
    private GameObject _activeSpeedBoostVFX;

    private void Awake()
    {
        if (_rigidbody == null)
            _rigidbody = GetComponent<Rigidbody>();

        if (_boxCollider == null)
            _boxCollider = GetComponent<BoxCollider>();
    }

    private void Update()
    {
        HandleInput();
        CheckGrounded();
        CheckMovingPlatform();
        UpdatePlayerState();
        UpdateSpeedBoostState();
    }

    private void UpdateSpeedBoostState()
    {
        // Update speed boost UI if active
        if (Time.time < _speedBoostEndTime)
        {
            float remainingTime = _speedBoostEndTime - Time.time;
            OnSpeedBoostChanged?.Invoke(_speedBoostMultiplier, _jumpBoostMultiplier, remainingTime);
        }
        else if (_speedBoostMultiplier > 1f)
        {
            // Reset boost when expired
            _speedBoostMultiplier = 1f;
            _jumpBoostMultiplier = 1f;
            
            // Notify UI that boost has ended
            OnSpeedBoostChanged?.Invoke(1f, 1f, 0f);
            
            // Destroy boost VFX if active
            if (_activeSpeedBoostVFX != null)
            {
                Destroy(_activeSpeedBoostVFX);
                _activeSpeedBoostVFX = null;
            }
        }
    }

    private void FixedUpdate()
    {
        // Apply physics-based movement in FixedUpdate
        ApplyMovement();
    }

    private void HandleInput()
    {
        // Get movement input
        HandleMovementInput();

        // Handle rotation input
        HandleRotationInput();

        // Handle jumping input
        if (Input.GetKeyDown(KeyCode.Space) && _isGrounded)
        {
            _triggerJump = true;
        }

        // Handle reset position input
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetPosition();
        }

        // Handle acceleration input
        _isAccelerating = Input.GetKey(KeyCode.LeftShift);
    }

    private void HandleMovementInput()
    {
        // Get horizontal and vertical input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Calculate movement direction relative to camera
        _movementDirection = transform.forward * vertical + transform.right * horizontal;
        _movementDirection.Normalize();
    }

    private void HandleRotationInput()
    {
        // Get mouse X input for horizontal rotation
        float mouseX = Input.GetAxis("Mouse X");

        // Apply smoothing to mouse input
        _currentMouseXDelta = Mathf.SmoothDamp(
            _currentMouseXDelta,
            -mouseX,
            ref _mouseDeltaXVelocity,
            _smoothTime
        );

        // Calculate rotation
        _rotationY -= _currentMouseXDelta * _mouseSensitivity * Time.deltaTime;

        // Apply rotation to player
        transform.rotation = Quaternion.Euler(0, _rotationY, 0);
    }

    private void ResetPosition()
    {
        _rigidbody.linearVelocity = Vector3.zero;
        transform.position = _resetPosition;
    }

    private void ApplyMovement()
    {
        // Check if there's floor in the movement direction
        bool canMove = true;
        if (_movementDirection.magnitude > 0.1f && _isGrounded)
        {
            canMove = IsFloorAhead(_movementDirection);
        }

        // Calculate movement speed based on acceleration state and power-up
        float currentSpeed = _movementSpeed;
        
        // Apply acceleration if shift is held
        if (_isAccelerating)
        {
            currentSpeed *= _accelerationMultiplier;
        }
        
        // Apply power-up boost
        if (Time.time < _speedBoostEndTime)
        {
            currentSpeed *= _speedBoostMultiplier;
        }
        else if (_speedBoostMultiplier > 1f)
        {
            // Reset boost when expired
            _speedBoostMultiplier = 1f;
            _jumpBoostMultiplier = 1f;
            
            // Destroy boost VFX if active
            if (_activeSpeedBoostVFX != null)
            {
                Destroy(_activeSpeedBoostVFX);
                _activeSpeedBoostVFX = null;
            }
        }

        // Apply movement only if there's floor ahead
        Vector3 moveVelocity = canMove ? _movementDirection * currentSpeed : Vector3.zero;

        // Preserve vertical velocity
        moveVelocity.y = _rigidbody.linearVelocity.y;

        // Add platform velocity if player is on a moving platform
        if (_currentPlatform != null)
        {
            moveVelocity += _platformVelocity;
        }

        // Apply velocity
        _rigidbody.linearVelocity = moveVelocity;

        // Handle jumping
        if (_triggerJump)
        {
            float jumpForceToApply = _jumpForce;
            
            // Apply jump boost if active
            if (Time.time < _speedBoostEndTime)
            {
                jumpForceToApply *= _jumpBoostMultiplier;
            }
            
            _rigidbody.AddForce(Vector3.up * jumpForceToApply, ForceMode.Impulse);
            _triggerJump = false;
        }
    }

    private bool IsFloorAhead(Vector3 direction)
    {
        // Get the normalized direction
        Vector3 normalizedDirection = direction.normalized;

        // Calculate the start position for the raycast (slightly above the ground)
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;

        // Calculate the end position for the raycast
        Vector3 rayEnd = rayStart + normalizedDirection * _edgeDetectionDistance;

        // Draw debug ray
        Debug.DrawLine(rayStart, rayEnd, Color.blue);
        Debug.DrawRay(rayEnd, Vector3.down * (_groundCheckDistance + 0.5f), Color.red);

        // Cast a ray downward from the end position to check for floor
        if (Physics.Raycast(rayEnd, Vector3.down, out hit, _groundCheckDistance + 1.0f, _groundLayer))
        {
            // Floor detected
            return true;
        }

        // No floor detected
        return false;
    }

    private void CheckGrounded()
    {
        // Raycast to check if player is grounded
        _isGrounded = Physics.Raycast(GetCubeFloorPositionWithCollider(), Vector3.down, out hit, _groundCheckDistance, _groundLayer);
    }

    private void CheckMovingPlatform()
    {
        // Check if player is on a moving platform
        if (_isGrounded)
        {
            HandlePlatformDetection(hit);
        }
        else
        {
            DetachFromPlatform();
        }
    }

    private void HandlePlatformDetection(RaycastHit hit)
    {
        if (hit.collider.CompareTag(_movingPlatformTag))
        {
            Transform hitTransform = hit.collider.transform;

            // If we're on a new platform
            if (_currentPlatform != hitTransform)
            {
                // Detach from previous platform if any
                DetachFromPlatform();

                // Attach to new platform
                _currentPlatform = hitTransform;

                // Parent player to platform
                transform.parent = _currentPlatform;

                // Try to get the platform controller
                _currentPlatformController = _currentPlatform.GetComponent<MovingPlatformController>();
                if (_currentPlatformController != null)
                {
                    _currentPlatformController.OnMove += UpdatePlatformVelocity;
                    // Initialize with current velocity
                    UpdatePlatformVelocity(_currentPlatformController.GetCurrentVelocity());
                }
            }
        }
        else if (_currentPlatform != null)
        {
            // We're grounded but not on a platform anymore
            DetachFromPlatform();
        }
    }

    private void DetachFromPlatform()
    {
        if (_currentPlatformController != null)
        {
            _currentPlatformController.OnMove -= UpdatePlatformVelocity;
            _currentPlatformController = null;
        }

        if (gameObject.activeInHierarchy)
        {
            transform.parent = null;
        }

        // Deparent from platform
        _currentPlatform = null;
        _platformVelocity = Vector3.zero;
    }

    private void UpdatePlatformVelocity(Vector3 platformVelocity)
    {
        _platformVelocity = platformVelocity;
    }

    private void UpdatePlayerState()
    {
        // Create a player state object
        PlayerState currentState = new PlayerState
        {
            IsMoving = _movementDirection.magnitude > 0.1f,
            IsJumping = !_isGrounded,
            IsAccelerating = _isAccelerating
        };

        // Trigger event for UI and other systems
        OnPlayerStateChanged?.Invoke(currentState);
    }

    // Get the floor position using collider
    Vector3 GetCubeFloorPositionWithCollider()
    {
        //return Vector3.zero;
        if (_boxCollider != null)
        {
            // Calculate the bottom center position based on the collider
            Vector3 bottomCenter = transform.position + (Vector3.down * GetColliderHalfSize()) + (Vector3.up * 0.1f);

            return bottomCenter;
        }

        // Fallback if no collider is found
        return transform.position;
    }

    public float GetColliderHalfSize()
    {
        return _boxCollider.size.y / 2;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw ground check ray
        Gizmos.color = Color.green;
        Vector3 rayStart = GetCubeFloorPositionWithCollider();

        // Edge detection rays (only if moving)
        if (_movementDirection != null && _movementDirection.magnitude > 0.1f)
        {
            Vector3 normalizedDirection = _movementDirection.normalized;
            Vector3 edgeRayStart = transform.position + Vector3.up * 0.1f;
            Vector3 edgeRayEnd = edgeRayStart + normalizedDirection * _edgeDetectionDistance;

            Gizmos.color = Color.blue;
            Gizmos.DrawLine(edgeRayStart, edgeRayEnd);

            Gizmos.color = Color.red;
            Gizmos.DrawRay(edgeRayEnd, Vector3.down * (_groundCheckDistance + 0.5f));
        }

        // teleport ray
        //Gizmos.DrawRay(transform.position, transform.forward * 100);

        Gizmos.DrawRay(rayStart, Vector3.down * _groundCheckDistance);

        // Draw slope check rays
        Gizmos.color = Color.yellow;
        Vector3[] directions = new Vector3[]
        {
            transform.forward, -transform.forward,
            transform.right, -transform.right
        };

        foreach (Vector3 dir in directions)
        {
            Gizmos.DrawRay(
                rayStart,
                (Vector3.down + dir.normalized * 0.25f).normalized * (_groundCheckDistance * 1.5f)
            );
        }
    }

    public void ApplySpeedJumpBoost(float speedMultiplier, float jumpMultiplier, float duration)
    {
        _speedBoostMultiplier = speedMultiplier;
        _jumpBoostMultiplier = jumpMultiplier;
        _speedBoostEndTime = Time.time + duration;
        
        // Notify UI about the new boost
        OnSpeedBoostChanged?.Invoke(_speedBoostMultiplier, _jumpBoostMultiplier, duration);
        
        // Create visual effect if provided
        if (_speedBoostVFX != null)
        {
            // Remove existing VFX if any
            if (_activeSpeedBoostVFX != null)
            {
                Destroy(_activeSpeedBoostVFX);
            }
            
            // Create new VFX
            _activeSpeedBoostVFX = Instantiate(_speedBoostVFX, transform);
        }
    }

    private void OnDestroy()
    {
        // Clean up event listeners
        DetachFromPlatform();
        
        // Clean up VFX
        if (_activeSpeedBoostVFX != null)
        {
            Destroy(_activeSpeedBoostVFX);
        }
    }
}