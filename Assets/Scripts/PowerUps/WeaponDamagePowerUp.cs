using UnityEngine;

public class WeaponDamagePowerUp : PowerUp
{
    [SerializeField] private float _damageMultiplier = 2.0f;
    
    protected override void Apply(GameObject player)
    {
        WeaponManager weaponManager = player.GetComponent<WeaponManager>();
        if (weaponManager != null)
        {
            weaponManager.ApplyDamageBoost(_damageMultiplier, _duration);
        }
    }
} 