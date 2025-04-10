using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;
using System.Collections;

public class Projectile : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private int _damage;
    private bool _hasSplashDamage;
    private float _splashRadius;
    private ProjectilePool _pool;
    
    [SerializeField] private float _lifetime = 5f;
    [SerializeField] private GameObject _impactEffectPrefab;
    [SerializeField] private float _projectileRotation;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void Initialize(int damage, bool hasSplashDamage, float splashRadius, ProjectilePool pool)
    {
        _damage = damage;
        _hasSplashDamage = hasSplashDamage;
        _splashRadius = splashRadius;
        _pool = pool;

        // Use a coroutine to return to pool after lifetime
        StartCoroutine(ReturnToPoolAfterLifetime());
    }
    
    private System.Collections.IEnumerator ReturnToPoolAfterLifetime()
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
        if (collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        // Handle impact effect
        if (_impactEffectPrefab != null)
        {
            Instantiate(_impactEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // Apply damage
        IDamageable target = collision.gameObject.GetComponent<IDamageable>();
        if (target != null)
        {
            target.TakeDamage(_damage);
        }
        
        // Apply splash damage if applicable
        if (_hasSplashDamage) 
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, _splashRadius).Where(x => x.gameObject != collision.gameObject).ToArray();
            ApplySplashDamage(colliders);
        }
        
        ReturnToPool();
    }
    
    private void ApplySplashDamage(Collider[] colliders)
    {
        foreach (Collider collider in colliders)
        {
            IDamageable target = collider.GetComponent<IDamageable>();
            if (target != null)
            {
                // Calculate distance to determine damage falloff
                float distance = Vector3.Distance(transform.position, collider.transform.position);
                float damagePercent = 1f - (distance / _splashRadius);
                int finalDamage = Mathf.RoundToInt(_damage * damagePercent);

                target.TakeDamage(finalDamage);
            }
        }
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