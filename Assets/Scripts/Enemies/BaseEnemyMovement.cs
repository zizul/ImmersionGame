using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

[RequireComponent(typeof(Rigidbody))]
public abstract class BaseEnemyMovement : MonoBehaviour
{
    [SerializeField] protected float moveSpeed = 3f;
    
    protected Rigidbody rb;
    protected Vector3 moveDirection;
    protected CancellationTokenSource movementCts;
    
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        // Make sure the rigidbody has proper settings for movement
        if (!rb.isKinematic)
        {
            rb.constraints = RigidbodyConstraints.FreezeRotation;
        }
    }
    
    protected virtual void OnEnable()
    {
        // Create a new cancellation token source when enabled
        movementCts = new CancellationTokenSource();
        
        // Start the movement behavior
        StartMovement(movementCts.Token).Forget();
    }
    
    protected virtual void OnDisable()
    {
        // Cancel any ongoing tasks when disabled
        if (movementCts != null)
        {
            movementCts.Cancel();
            movementCts.Dispose();
            movementCts = null;
        }
    }
    
    // Abstract method that each movement type must implement
    protected abstract UniTask StartMovement(CancellationToken cancellationToken);
    
    // Choose a new random direction (horizontal plane movement)
    protected virtual void ChooseNewDirection()
    {
        // Create movement in the XZ plane (horizontal)
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        moveDirection = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)).normalized;
    }
    
    // Handle wall collisions
    protected virtual void OnCollisionEnter(Collision collision)
    {
        // Get the normal of the collision
        Vector3 normal = collision.contacts[0].normal;
        
        // Only reflect horizontal components (ignore Y component for standard reflection)
        Vector3 reflectionNormal = new Vector3(normal.x, 0f, normal.z).normalized;
        
        // Reflect the movement direction off the normal (keeping any vertical component)
        Vector3 horizontalDirection = new Vector3(moveDirection.x, 0f, moveDirection.z);
        Vector3 reflectedHorizontal = Vector3.Reflect(horizontalDirection, reflectionNormal).normalized;
        
        // Maintain the original y component
        moveDirection = new Vector3(reflectedHorizontal.x, moveDirection.y, reflectedHorizontal.z);
    }
    
    // Clean up resources when destroyed
    protected virtual void OnDestroy()
    {
        if (movementCts != null)
        {
            movementCts.Cancel();
            movementCts.Dispose();
        }
    }
} 