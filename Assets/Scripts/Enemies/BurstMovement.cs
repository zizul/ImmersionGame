using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;

public class BurstMovement : BaseEnemyMovement
{
    [SerializeField] private float slowBurstForce = 10f;
    [SerializeField] private float fastBurstForce = 25f;
    [SerializeField] private float slowBurstDuration = 1.0f;
    [SerializeField] private float fastBurstDuration = 0.5f;
    [SerializeField] private float restDuration = 0.7f;
    [SerializeField] private ForceMode forceMode = ForceMode.Impulse;
    
    protected override void Awake()
    {
        base.Awake();
        ChooseNewDirection();
    }
    
    protected override async UniTask StartMovement(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            // First slow burst
            ApplyForce(slowBurstForce);
            await UniTask.Delay((int)(slowBurstDuration * 1000), cancellationToken: cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            
            // Rest
            await UniTask.Delay((int)(restDuration * 1000), cancellationToken: cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            
            // Second slow burst
            ApplyForce(slowBurstForce);
            await UniTask.Delay((int)(slowBurstDuration * 1000), cancellationToken: cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            
            // Rest
            await UniTask.Delay((int)(restDuration * 1000), cancellationToken: cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            
            // Fast burst
            ApplyForce(fastBurstForce);
            await UniTask.Delay((int)(fastBurstDuration * 1000), cancellationToken: cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            
            // Rest
            await UniTask.Delay((int)(restDuration * 1000), cancellationToken: cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;
            
            // Choose a new direction occasionally
            if (Random.value < 0.3f)
            {
                ChooseNewDirection();
            }
        }
    }
    
    private void ApplyForce(float force)
    {
        // Apply force in the movement direction
        Vector3 forceVector = new Vector3(
            moveDirection.x * force,
            0f,  // No vertical force
            moveDirection.z * force
        );
        
        // Reset velocity before applying new force for more consistent bursts
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        
        // Apply the force
        rb.AddForce(forceVector, forceMode);
    }
} 