using UnityEngine;

public class WeaponInfo
{
    public string WeaponName { get; set; }
    public string WeaponType { get; set; }
    public int BaseDamage { get; set; }
    public bool IsActive { get; set; }
    
    // Constructor with default values
    public WeaponInfo(string name = "Unknown", string type = "None", int damage = 0, bool active = false)
    {
        WeaponName = name;
        WeaponType = type;
        BaseDamage = damage;
        IsActive = active;
    }
} 