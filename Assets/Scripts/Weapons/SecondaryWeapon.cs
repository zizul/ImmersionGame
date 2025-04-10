using UnityEngine;

public class SecondaryWeapon : Weapon
{
    [Header("Secondary Weapon Settings")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private float _projectileSpeed = 15f;
    [SerializeField] private int _damage = 125;
    [SerializeField] private float _splashRadius = 3f;
    
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
            projectileComponent.Initialize(calculatedDamage, true, _splashRadius, _projectilePool);
            projectileComponent.Launch(_muzzlePoint.forward * _projectileSpeed);
        }
    }
} 