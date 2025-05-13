using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class WanderMovement : BaseEnemyMovement
{
    [SerializeField] private float changeDirectionTime = 2f;
    
    protected override void Awake()
    {
        base.Awake();
        ChooseNewDirection();
    }
    
    protected override async UniTask StartMovement(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            // Apply continuous movement in FixedUpdate
            rb.linearVelocity = new Vector3(
                moveDirection.x * moveSpeed,
                rb.linearVelocity.y, // Preserve gravity effect
                moveDirection.z * moveSpeed
            );
            
            // Wait for the direction change time
            float randomWaitTime = Random.Range(changeDirectionTime * 0.5f, changeDirectionTime * 1.5f);
            await UniTask.Delay((int)(randomWaitTime * 1000), cancellationToken: cancellationToken);
            
            // Choose a new direction
            if (!cancellationToken.IsCancellationRequested)
            {
                //ChooseNewDirection();
            }
        }
    }
    
    // Fixed update is removed as we're handling movement in the UniTask
} 