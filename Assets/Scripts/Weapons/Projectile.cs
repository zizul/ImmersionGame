using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private ProjectilePool _pool;
    
    [SerializeField] private float _lifetime = 5f;
    [SerializeField] private GameObject _impactEffectPrefab;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        
        // Disable gravity for straight-line travel
        if (_rigidbody != null)
        {
            _rigidbody.useGravity = false;
        }
    }

    public void Initialize(ProjectilePool pool)
    {
        _pool = pool;

        // Use a coroutine to return to pool after lifetime
        StartCoroutine(ReturnToPoolAfterLifetime());
    }
    
    private IEnumerator ReturnToPoolAfterLifetime()
    {
        yield return new WaitForSeconds(_lifetime);
        if (gameObject.activeInHierarchy)
        {
            ReturnToPool();
        }
    }
    
    public void Launch(Vector3 velocity)
    {
        _rigidbody.linearVelocity = velocity;
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        // Visual effect only - no damage
        // Handle impact effect
        if (_impactEffectPrefab != null)
        {
            Instantiate(_impactEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // Return to pool on impact
        ReturnToPool();
    }

    private void OnDisable()
    {
        // Reset any necessary properties when returning to pool
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
    }

    public void ReturnToPool()
    {
        if (_pool != null)
        {
            _pool.ReturnToPool(gameObject);
        }
        else
        {
            // Fallback if pool reference is missing
            gameObject.SetActive(false);
        }
    }
}
