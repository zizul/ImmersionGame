using UnityEngine;
using System.Collections.Generic;

public class WeaponManager : MonoBehaviour
{
    [Header("Weapon Settings")]
    [SerializeField] private List<PrimaryWeapon> _primaryWeapons = new List<PrimaryWeapon>();
    [SerializeField] private List<SecondaryWeapon> _secondaryWeapons = new List<SecondaryWeapon>();
    [SerializeField] private Transform _weaponMountPoint;
    
    [Header("Power-Up Settings")]
    [SerializeField] private GameObject _damageBoostVFX;
    
    private PrimaryWeapon _currentPrimaryWeapon;
    private SecondaryWeapon _currentSecondaryWeapon;
    private int _currentPrimaryWeaponIndex = 0;
    private int _currentSecondaryWeaponIndex = 0;
    private float _damageBoostMultiplier = 1f;
    private float _damageBoostEndTime = 0f;
    private GameObject _activeDamageBoostVFX;
    private Weapon _activeWeapon;
    
    public delegate void WeaponChangedHandler(WeaponInfo weaponInfo);
    public delegate void DamageBoostChangedHandler(float multiplier, float remainingTime);
    
    public event WeaponChangedHandler OnWeaponChanged;
    public event DamageBoostChangedHandler OnDamageBoostChanged;
    
    private void Start()
    {
        // Initialize weapons
        if (_primaryWeapons.Count > 0)
        {
            EquipPrimaryWeapon(0);
        }
        
        if (_secondaryWeapons.Count > 0)
        {
            EquipSecondaryWeapon(0);
            // Hide secondary weapon initially
            if (_currentSecondaryWeapon != null)
            {
                _currentSecondaryWeapon.gameObject.SetActive(false);
            }
        }
        
        // Set primary weapon as active by default
        if (_currentPrimaryWeapon != null)
        {
            _activeWeapon = _currentPrimaryWeapon;
        }
    }
    
    private void Update()
    {
        // Handle weapon switching and firing
        if (Input.GetMouseButtonDown(0)) // Left mouse button
        {
            if (_activeWeapon != _currentPrimaryWeapon)
            {
                SwitchToWeapon(_currentPrimaryWeapon);
            }
            FireActiveWeapon(WeaponTriggerType.Primary);
        }
        else if (Input.GetMouseButtonDown(1)) // Right mouse button
        {
            if (_activeWeapon != _currentSecondaryWeapon)
            {
                SwitchToWeapon(_currentSecondaryWeapon);
            }
            FireActiveWeapon(WeaponTriggerType.Primary);
        }
        
        // Check if damage boost has expired
        if (Time.time > _damageBoostEndTime && _damageBoostMultiplier > 1f)
        {
            _damageBoostMultiplier = 1f;
            
            // Destroy boost VFX if active
            if (_activeDamageBoostVFX != null)
            {
                Destroy(_activeDamageBoostVFX);
                _activeDamageBoostVFX = null;
            }
            
            // Notify listeners about damage boost expiration
            OnDamageBoostChanged?.Invoke(1f, 0f);
        }
        
        // Periodically update the UI with remaining boost time
        if (_damageBoostMultiplier > 1f && Time.time < _damageBoostEndTime)
        {
            // Update every 0.5 seconds
            if (Time.frameCount % 30 == 0)
            {
                float remainingTime = _damageBoostEndTime - Time.time;
                OnDamageBoostChanged?.Invoke(_damageBoostMultiplier, remainingTime);
            }
        }
    }
    
    private void SwitchToWeapon(Weapon newWeapon)
    {
        if (newWeapon == null)
            return;
            
        // Hide current active weapon
        if (_activeWeapon != null)
        {
            _activeWeapon.gameObject.SetActive(false);
        }
        
        // Show new weapon
        newWeapon.gameObject.SetActive(true);
        _activeWeapon = newWeapon;
        
        // Notify listeners about weapon change
        NotifyWeaponChanged();
    }
    
    public void EquipPrimaryWeapon(int index)
    {
        if (index < 0 || index >= _primaryWeapons.Count)
            return;
        
        // Equip new primary weapon
        _currentPrimaryWeapon = _primaryWeapons[index];
        _currentPrimaryWeaponIndex = index;
        
        // If primary weapon is active, update it
        if (_activeWeapon == _currentPrimaryWeapon || _activeWeapon == null)
        {
            SwitchToWeapon(_currentPrimaryWeapon);
        }
    }
    
    public void EquipSecondaryWeapon(int index)
    {
        if (index < 0 || index >= _secondaryWeapons.Count)
            return;
        
        // Equip new secondary weapon
        _currentSecondaryWeapon = _secondaryWeapons[index];
        _currentSecondaryWeaponIndex = index;
        
        // If secondary weapon is active, update it
        if (_activeWeapon == _currentSecondaryWeapon)
        {
            SwitchToWeapon(_currentSecondaryWeapon);
        }
    }
    
    private void FireActiveWeapon(WeaponTriggerType triggerType)
    {
        if (_activeWeapon != null)
        {
            _activeWeapon.Fire(triggerType, _damageBoostMultiplier);
        }
    }
    
    // Method to add a new primary weapon
    public void AddPrimaryWeapon(PrimaryWeapon weapon)
    {
        if (weapon != null && !_primaryWeapons.Contains(weapon))
        {
            weapon.transform.SetParent(_weaponMountPoint);
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;
            weapon.gameObject.SetActive(false);
            
            _primaryWeapons.Add(weapon);
        }
    }
    
    // Method to add a new secondary weapon
    public void AddSecondaryWeapon(SecondaryWeapon weapon)
    {
        if (weapon != null && !_secondaryWeapons.Contains(weapon))
        {
            weapon.transform.SetParent(_weaponMountPoint);
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;
            weapon.gameObject.SetActive(false);
            
            _secondaryWeapons.Add(weapon);
        }
    }
    
    public void ApplyDamageBoost(float multiplier, float duration)
    {
        _damageBoostMultiplier = multiplier;
        _damageBoostEndTime = Time.time + duration;
        
        // Create visual effect if provided
        if (_damageBoostVFX != null)
        {
            // Remove existing VFX if any
            if (_activeDamageBoostVFX != null)
            {
                Destroy(_activeDamageBoostVFX);
            }
            
            // Create new VFX
            _activeDamageBoostVFX = Instantiate(_damageBoostVFX, _weaponMountPoint);
        }
        
        // Notify listeners about damage boost change
        OnDamageBoostChanged?.Invoke(_damageBoostMultiplier, duration);
    }
    
    private void OnDestroy()
    {
        // Clean up VFX
        if (_activeDamageBoostVFX != null)
        {
            Destroy(_activeDamageBoostVFX);
        }
    }
    
    public WeaponInfo GetActiveWeaponInfo()
    {
        if (_activeWeapon == null)
            return new WeaponInfo();
        
        int baseDamage = 0;
        string weaponType = "Unknown";
        
        // Determine weapon type and base damage
        if (_activeWeapon == _currentPrimaryWeapon)
        {
            weaponType = "Primary";
            baseDamage = 25; // Default value if not obtainable
        }
        else if (_activeWeapon == _currentSecondaryWeapon)
        {
            weaponType = "Secondary";
            baseDamage = 50; // Default value if not obtainable
        }
        
        return new WeaponInfo(
            _activeWeapon.name,
            weaponType,
            baseDamage,
            true
        );
    }
    
    private void NotifyWeaponChanged()
    {
        OnWeaponChanged?.Invoke(GetActiveWeaponInfo());
    }
}