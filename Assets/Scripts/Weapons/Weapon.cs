using UnityEngine;
using System.Collections.Generic;

public abstract class Weapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] protected Transform _muzzlePoint;
    [SerializeField] protected float _fireRate = 1f;
    [SerializeField] protected float _maxRange = 100f;
    [SerializeField] protected int _baseDamage = 25;
    
    protected float _nextFireTime;

    [Header("Dependencies")]
    [SerializeField] protected ProjectilePool _projectilePool;
    [SerializeField] protected CrosshairUI _crosshairUI;

    [Header("Visual Projectile Settings")]
    [SerializeField] protected GameObject _projectilePrefab;
    [SerializeField] protected float _projectileSpeed = 30f;
    
    protected Camera _mainCamera;
    
    protected virtual void Awake()
    {
        if (_crosshairUI == null)
        {
            _crosshairUI = FindFirstObjectByType<CrosshairUI>();
        }
        
        _mainCamera = Camera.main;
    }
    
    public virtual void Fire(float damageMultiplier)
    {
        if (Time.time < _nextFireTime)
            return;
        
        _nextFireTime = Time.time + (1f / _fireRate);

        // Get targets from crosshair
        if (_crosshairUI != null)
        {
            List<TargetInfo> targets = _crosshairUI.GetTargetsInCrosshair(_muzzlePoint.position, _maxRange);
            ApplyDamageToTargets(targets, damageMultiplier);
        }

        // Spawn visual projectile
        SpawnVisualProjectile();
    }
    
    protected virtual void ApplyDamageToTargets(List<TargetInfo> targets, float damageMultiplier)
    {
        int calculatedDamage = Mathf.RoundToInt(_baseDamage * damageMultiplier);
        
        foreach (TargetInfo targetInfo in targets)
        {
            if (targetInfo.Target != null)
            {
                targetInfo.Target.TakeDamage(calculatedDamage);
            }
        }
    }
    
    protected virtual void SpawnVisualProjectile()
    {
        if (_projectilePrefab == null || _projectilePool == null || _mainCamera == null)
            return;
        
        // Get target point from crosshair
        Vector3 targetPoint = GetCrosshairTargetPoint();
        
        // Calculate direction from muzzle to target point
        Vector3 projectileDirection = (targetPoint - _muzzlePoint.position).normalized;
        
        // Calculate rotation to face the target direction
        Quaternion projectileRotation = Quaternion.LookRotation(projectileDirection) * Quaternion.Euler(0f, 90f, 0f);
        
        GameObject projectile = _projectilePool.GetProjectile(_projectilePrefab, _muzzlePoint.position, projectileRotation);
        Projectile projectileComponent = projectile.GetComponent<Projectile>();
        
        if (projectileComponent != null)
        {
            projectileComponent.Initialize(_projectilePool);
            projectileComponent.Launch(projectileDirection * _projectileSpeed);
        }
    }
    
    /// <summary>
    /// Gets the 3D world point where the crosshair is aiming
    /// </summary>
    protected Vector3 GetCrosshairTargetPoint()
    {
        if (_mainCamera == null)
            return transform.position + transform.forward * _maxRange;
        
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray centerRay = _mainCamera.ScreenPointToRay(screenCenter);
        
        if (Physics.Raycast(centerRay, out RaycastHit hit, _maxRange))
        {
            return hit.point;
        }
        
        return centerRay.origin + centerRay.direction * _maxRange;
    }
    
    public int GetBaseDamage()
    {
        return _baseDamage;
    }
}