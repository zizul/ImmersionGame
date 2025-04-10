using UnityEngine;
using TMPro;
using System.Text;

public class WeaponStateUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _weaponStateText;
    [SerializeField] private WeaponManager _weaponManager;
    
    // Create a StringBuilder instance to reuse
    private StringBuilder _stringBuilder = new StringBuilder();
    
    private void Start()
    {
        if (_weaponManager != null)
        {
            // Subscribe to weapon manager events
            _weaponManager.OnWeaponChanged += UpdateWeaponUI;
            _weaponManager.OnDamageBoostChanged += UpdateDamageBoostUI;
            
            // Initialize the UI
            UpdateWeaponUI(_weaponManager.GetActiveWeaponInfo());
        }
    }
    
    private void OnDestroy()
    {
        if (_weaponManager != null)
        {
            // Unsubscribe from events when destroyed
            _weaponManager.OnWeaponChanged -= UpdateWeaponUI;
            _weaponManager.OnDamageBoostChanged -= UpdateDamageBoostUI;
        }
    }
    
    public void UpdateWeaponUI(WeaponInfo weaponInfo)
    {
        if (_weaponStateText == null)
            return;
            
        // Clear the StringBuilder
        _stringBuilder.Clear();
        
        // Build the weapon info text
        _stringBuilder.Append("Weapon: ").Append(weaponInfo.WeaponName);
        _stringBuilder.Append("\nType: ").Append(weaponInfo.WeaponType);
        _stringBuilder.Append("\nDamage: ").Append(weaponInfo.BaseDamage);
        
        _weaponStateText.text = _stringBuilder.ToString();
    }
    
    public void UpdateDamageBoostUI(float multiplier, float remainingTime)
    {
        if (_weaponStateText == null)
            return;
            
        // Get current text
        string currentText = _weaponStateText.text;
        
        // Clear the StringBuilder
        _stringBuilder.Clear();
        
        // If there's a damage boost active
        if (multiplier > 1f && remainingTime > 0f)
        {
            // Check if we already have damage boost info
            if (currentText.Contains("DAMAGE BOOST"))
            {
                // Replace the existing boost info
                int boostIndex = currentText.IndexOf("\nDAMAGE BOOST");
                if (boostIndex >= 0)
                {
                    int nextLineIndex = currentText.IndexOf('\n', boostIndex + 1);
                    if (nextLineIndex >= 0)
                    {
                        // Replace the boost line
                        _stringBuilder.Append(currentText.Substring(0, boostIndex));
                        _stringBuilder.AppendFormat("\nDAMAGE BOOST: x{0:F1} ({1:F1}s)", multiplier, remainingTime);
                        _stringBuilder.Append(currentText.Substring(nextLineIndex));
                    }
                    else
                    {
                        // Replace to the end
                        _stringBuilder.Append(currentText.Substring(0, boostIndex));
                        _stringBuilder.AppendFormat("\nDAMAGE BOOST: x{0:F1} ({1:F1}s)", multiplier, remainingTime);
                    }
                }
            }
            else
            {
                // Add the boost info
                _stringBuilder.Append(currentText);
                _stringBuilder.AppendFormat("\nDAMAGE BOOST: x{0:F1} ({1:F1}s)", multiplier, remainingTime);
            }
        }
        else
        {
            // Remove any damage boost info
            int boostIndex = currentText.IndexOf("\nDAMAGE BOOST");
            if (boostIndex >= 0)
            {
                int nextLineIndex = currentText.IndexOf('\n', boostIndex + 1);
                if (nextLineIndex >= 0)
                {
                    // Remove the boost line
                    _stringBuilder.Append(currentText.Substring(0, boostIndex));
                    _stringBuilder.Append(currentText.Substring(nextLineIndex));
                }
                else
                {
                    // Remove to the end
                    _stringBuilder.Append(currentText.Substring(0, boostIndex));
                }
            }
            else
            {
                // No boost info to remove, just use the current text
                _stringBuilder.Append(currentText);
            }
        }
        
        _weaponStateText.text = _stringBuilder.ToString();
    }
} 