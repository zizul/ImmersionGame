using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class JumpMovement : BaseEnemyMovement
{
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float timeBetweenJumps = 2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.2f;
    
    private bool isGrounded = false;
    
    protected override void Awake()
    {
        base.Awake();
        ChooseNewDirection();
    }
    
    protected override void ChooseNewDirection()
    {
        // For jumping, we want horizontal movement on the XZ plane
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        moveDirection = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
    }
    
    private void Update()
    {
        CheckGrounded();
    }
    
    private void FixedUpdate()
    {
        // Apply horizontal movement while preserving vertical velocity
        rb.linearVelocity = new Vector3(
            moveDirection.x * moveSpeed,
            rb.linearVelocity.y,
            moveDirection.z * moveSpeed
        );
    }
    
    protected override async UniTask StartMovement(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            // Wait for the jump interval
            await UniTask.Delay((int)(timeBetweenJumps * 1000), cancellationToken: cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            
            if (isGrounded)
            {
                // Jump!
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); // Reset vertical velocity
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }
    }
    
    private void CheckGrounded()
    {
        // Cast a ray downward to check if we're on the ground
        Ray ray = new Ray(transform.position, Vector3.down);
        isGrounded = Physics.Raycast(ray, groundCheckDistance, groundLayer);
    }
    
    protected override void OnCollisionEnter(Collision collision)
    {
        // Get the normal of the collision
        Vector3 normal = collision.contacts[0].normal;
        
        // If we hit a wall (mostly horizontal normal - normal.y close to 0)
        if (Mathf.Abs(normal.y) < 0.3f)
        {
            // Reflect only the horizontal direction
            Vector3 horizontal = new Vector3(moveDirection.x, 0f, moveDirection.z);
            Vector3 reflectionNormal = new Vector3(normal.x, 0f, normal.z).normalized;
            Vector3 reflected = Vector3.Reflect(horizontal, reflectionNormal).normalized;
            
            moveDirection = new Vector3(reflected.x, moveDirection.y, reflected.z);
        }
    }
    
    // Visualization for ground check
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
} 