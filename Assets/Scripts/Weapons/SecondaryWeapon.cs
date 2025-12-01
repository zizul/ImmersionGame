using UnityEngine;
using System.Collections.Generic;

public class SecondaryWeapon : Weapon
{
    [Header("Secondary Weapon Splash Settings")]
    [SerializeField] private float _splashRadius = 3f;
    
    protected override void ApplyDamageToTargets(List<TargetInfo> targets, float damageMultiplier)
    {
        int calculatedDamage = Mathf.RoundToInt(_baseDamage * damageMultiplier);
        HashSet<GameObject> damagedObjects = new HashSet<GameObject>();
        
        // Apply direct damage to targets in crosshair
        foreach (TargetInfo targetInfo in targets)
        {
            if (targetInfo.Target != null)
            {
                targetInfo.Target.TakeDamage(calculatedDamage);
                damagedObjects.Add(targetInfo.GameObject);
                
                // Apply splash damage around each hit target
                ApplySplashDamage(targetInfo.HitPoint, calculatedDamage, damagedObjects);
            }
        }
    }
    
    private void ApplySplashDamage(Vector3 explosionCenter, int baseDamage, HashSet<GameObject> alreadyDamaged)
    {
        Collider[] colliders = Physics.OverlapSphere(explosionCenter, _splashRadius);
        
        foreach (Collider collider in colliders)
        {
            // Skip if already damaged
            if (alreadyDamaged.Contains(collider.gameObject))
                continue;
            
            IDamageable target = collider.GetComponent<IDamageable>();
            if (target != null)
            {
                // Check line of sight from explosion center
                Vector3 directionToTarget = (collider.transform.position - explosionCenter).normalized;
                float distance = Vector3.Distance(explosionCenter, collider.transform.position);
                
                // Simple raycast check for obstacles
                if (!Physics.Raycast(explosionCenter, directionToTarget, distance, LayerMask.GetMask("Default")))
                {
                    // Calculate damage falloff based on distance
                    float damagePercent = 1f - (distance / _splashRadius);
                    int finalDamage = Mathf.RoundToInt(baseDamage * damagePercent * 0.5f); // 50% splash damage
                    
                    target.TakeDamage(finalDamage);
                    alreadyDamaged.Add(collider.gameObject);
                }
            }
        }
    }
}
