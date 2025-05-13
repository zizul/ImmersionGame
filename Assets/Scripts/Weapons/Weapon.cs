using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] protected Transform _muzzlePoint;
    [SerializeField] protected float _fireRate = 1f;
    
    protected float _nextFireTime;

    [Header("Dependencies")]
    [SerializeField] protected ProjectilePool _projectilePool;
    
    public virtual void Fire(float damageMultiplier)
    {
        if (Time.time < _nextFireTime)
            return;
        
        _nextFireTime = Time.time + (1f / _fireRate);
    }
} 