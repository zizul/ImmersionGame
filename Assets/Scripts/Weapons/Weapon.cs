using UnityEngine;

public enum WeaponTriggerType
{
    Primary,
    Secondary
}

public abstract class Weapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] protected Transform _muzzlePoint;
    [SerializeField] protected float _fireRate = 1f;
    
    protected float _nextFireTime;
    
    [SerializeField] protected ProjectilePool _projectilePool;
    
    public virtual void Fire(WeaponTriggerType triggerType)
    {
        Fire(triggerType, 1.0f);
    }
    
    public virtual void Fire(WeaponTriggerType triggerType, float damageMultiplier)
    {
        if (Time.time < _nextFireTime)
            return;
        
        _nextFireTime = Time.time + (1f / _fireRate);
    }
} 