using UnityEngine;
using System;

[RequireComponent(typeof(Rigidbody))]
public class MovingPlatformController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float moveDistance = 5.0f;
    [SerializeField] private Vector3 moveAxis = Vector3.right;
    [SerializeField] private Vector3 startPosition;

    [Header("Optional Settings")]
    [SerializeField] private bool useLocalPosition = false;
    [SerializeField] public bool smoothMovement = false;
    [SerializeField] private bool isKinematic = false;

    // Event that other objects can subscribe to for platform velocity information
    public event Action<Vector3> OnMove;

    private float direction = 1.0f;
    private float distanceTraveled = 0.0f;
    private Vector3 currentVelocity;
    private Rigidbody rb;
    private float timeOffset;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = isKinematic; // Usually kinematic for platforms
        rb.interpolation = RigidbodyInterpolation.Interpolate; // Smoother movement
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // Better collision detection
        rb.useGravity = false; // Platforms shouldn't fall
    }

    void Start()
    {
        // Store the initial position
        startPosition = useLocalPosition ? transform.localPosition : transform.position;
        
        // Normalize the direction vector
        moveAxis.Normalize();
        
        // Add a random time offset for smooth movement to avoid all platforms moving in sync
        timeOffset = UnityEngine.Random.Range(0f, Mathf.PI * 2);
    }

    void FixedUpdate()
    {
        Vector3 targetVelocity;
        
        if (smoothMovement)
        {
            // Calculate the target position using a sine wave
            float time = Time.time + timeOffset;
            float sinValue = Mathf.Sin(time * moveSpeed);
            float cosValue = Mathf.Cos(time * moveSpeed); // Derivative of sin for velocity
            
            // Calculate the target position
            Vector3 targetPosition = startPosition + moveAxis * sinValue * (moveDistance / 2);
            
            // Calculate velocity based on the derivative of the position function
            targetVelocity = moveAxis * cosValue * moveSpeed * (moveDistance / 2);
            
            if (useLocalPosition)
            {
                // For local position, we need to convert to world space
                Vector3 worldTargetPosition = transform.parent ? 
                    transform.parent.TransformPoint(targetPosition) : 
                    targetPosition;
                
                rb.MovePosition(worldTargetPosition);
            }
            else
            {
                rb.MovePosition(targetPosition);
            }
        }
        else
        {
            // Calculate movement for this frame
            float moveThisFrame = moveSpeed * Time.fixedDeltaTime * direction;

            // Update distance traveled
            distanceTraveled += Mathf.Abs(moveThisFrame);

            // Check if we need to change direction
            if (distanceTraveled >= moveDistance)
            {
                direction *= -1; // Reverse direction
                distanceTraveled = 0; // Reset distance counter
            }

            // Calculate velocity vector
            targetVelocity = moveAxis * moveSpeed * direction;
            
            // Apply movement using Rigidbody
            if (useLocalPosition && transform.parent)
            {
                // For local movement with a parent
                Vector3 worldDirection = transform.parent.TransformDirection(moveAxis);
                Vector3 worldVelocity = worldDirection * moveSpeed * direction;
                rb.linearVelocity = worldVelocity;
                
                // Update target velocity to match world space
                targetVelocity = worldVelocity;
            }
            else
            {
                // Direct world space movement
                rb.linearVelocity = targetVelocity;
            }
        }
        
        // Store current velocity for external use
        currentVelocity = targetVelocity;
        
        // Notify subscribers about the current velocity
        OnMove?.Invoke(currentVelocity);
    }

    // Optional: Visualize the path in the editor
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            Vector3 pos = useLocalPosition ? transform.localPosition : transform.position;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(
                pos - moveAxis * (moveDistance / 2),
                pos + moveAxis * (moveDistance / 2)
            );
        }
    }

    public Vector3 GetCurrentVelocity()
    {
        return currentVelocity;
    }
} 