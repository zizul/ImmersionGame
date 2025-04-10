using UnityEngine;

public class PrimaryWeapon : Weapon
{
    [Header("Primary Weapon Settings")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private float _projectileSpeed = 30f;
    [SerializeField] private int _damage = 25;
    
    public override void Fire(WeaponTriggerType triggerType, float damageMultiplier)
    {
        base.Fire(triggerType, damageMultiplier);
        
        Quaternion projectileRotation = _muzzlePoint.rotation * Quaternion.Euler(0f, 90f, 0f);
        
        // Use the pool instead of Instantiate
        GameObject projectile = _projectilePool.GetProjectile(_projectilePrefab, _muzzlePoint.position, projectileRotation);
        Projectile projectileComponent = projectile.GetComponent<Projectile>();
        
        if (projectileComponent != null)
        {
            int calculatedDamage = Mathf.RoundToInt(_damage * damageMultiplier);
            projectileComponent.Initialize(calculatedDamage, false, 0f, _projectilePool);
            projectileComponent.Launch(_muzzlePoint.forward * _projectileSpeed);
        }
    }
} 