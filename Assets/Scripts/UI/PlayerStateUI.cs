using UnityEngine;
using TMPro;
using System.Text;

public class PlayerStateUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _playerStateText;
    [SerializeField] private PlayerController _playerController;
    
    // Create a StringBuilder instance to reuse
    private StringBuilder _stringBuilder = new StringBuilder();

    private void Start()
    {
        if (_playerController != null)
        {
            // Subscribe to player state events
            _playerController.OnPlayerStateChanged += UpdateUI;
            _playerController.OnSpeedBoostChanged += UpdateSpeedBoostUI;

            // Initialize the UI with current state if available
            // UpdateUI(_playerController.GetCurrentState());
        }
    }
    
    private void OnDestroy()
    {
        if (_playerController != null)
        {
            // Unsubscribe from events when destroyed
            _playerController.OnPlayerStateChanged -= UpdateUI;
            _playerController.OnSpeedBoostChanged -= UpdateSpeedBoostUI;
        }
    }

    public void UpdateUI(PlayerState state)
    {
        if (_playerStateText == null)
            return;
            
        // Clear the StringBuilder
        _stringBuilder.Clear();
        _stringBuilder.Append("Player: ");
        
        if (state.IsJumping)
        {
            _stringBuilder.Append("\nJumping");
        }
        if (!state.IsJumping)
        {
            _stringBuilder.Append("\nGrounded");
        }
        if (state.IsMoving)
        {
            _stringBuilder.Append(state.IsAccelerating ? "\nRunning" : "\nWalking");
        }
        if (!state.IsJumping && !state.IsMoving)
        {
            _stringBuilder.Append("\nStanding");
        }
        
        _playerStateText.text = _stringBuilder.ToString();
    }
    
    public void UpdateSpeedBoostUI(float speedMultiplier, float jumpMultiplier, float remainingTime)
    {
        if (_playerStateText == null)
            return;
            
        // Get current text
        string currentText = _playerStateText.text;
        
        // Clear the StringBuilder and start with the current text
        _stringBuilder.Clear();
        _stringBuilder.Append(currentText);
        
        // If there's a speed boost active
        if ((speedMultiplier > 1f || jumpMultiplier > 1f) && remainingTime > 0f)
        {
            // Check if we already have speed boost info
            if (currentText.Contains("SPEED BOOST"))
            {
                // Remove existing speed boost info
                int boostIndex = currentText.IndexOf("\nSPEED BOOST");
                if (boostIndex >= 0)
                {
                    int nextLineIndex = currentText.IndexOf('\n', boostIndex + 1);
                    if (nextLineIndex >= 0)
                    {
                        // Replace the boost line
                        _stringBuilder.Clear();
                        _stringBuilder.Append(currentText.Substring(0, boostIndex));
                        _stringBuilder.AppendFormat("\nSPEED BOOST: x{0:F1} ({1:F1}s)", speedMultiplier, remainingTime);
                        _stringBuilder.Append(currentText.Substring(nextLineIndex));
                    }
                    else
                    {
                        // Replace to the end
                        _stringBuilder.Clear();
                        _stringBuilder.Append(currentText.Substring(0, boostIndex));
                        _stringBuilder.AppendFormat("\nSPEED BOOST: x{0:F1} ({1:F1}s)", speedMultiplier, remainingTime);
                    }
                }
            }
            else
            {
                // Add the boost info
                _stringBuilder.AppendFormat("\nSPEED BOOST: x{0:F1} ({1:F1}s)", speedMultiplier, remainingTime);
            }
            
            // Update the current text for further processing
            currentText = _stringBuilder.ToString();
            
            // Add jump boost info if different from speed boost
            if (jumpMultiplier != speedMultiplier)
            {
                if (currentText.Contains("JUMP BOOST"))
                {
                    // Replace the existing jump boost info
                    int boostIndex = currentText.IndexOf("\nJUMP BOOST");
                    if (boostIndex >= 0)
                    {
                        int nextLineIndex = currentText.IndexOf('\n', boostIndex + 1);
                        if (nextLineIndex >= 0)
                        {
                            // Replace the boost line
                            _stringBuilder.Clear();
                            _stringBuilder.Append(currentText.Substring(0, boostIndex));
                            _stringBuilder.AppendFormat("\nJUMP BOOST: x{0:F1}", jumpMultiplier);
                            _stringBuilder.Append(currentText.Substring(nextLineIndex));
                        }
                        else
                        {
                            // Replace to the end
                            _stringBuilder.Clear();
                            _stringBuilder.Append(currentText.Substring(0, boostIndex));
                            _stringBuilder.AppendFormat("\nJUMP BOOST: x{0:F1}", jumpMultiplier);
                        }
                    }
                }
                else
                {
                    // Add the jump boost info
                    _stringBuilder.AppendFormat("\nJUMP BOOST: x{0:F1}", jumpMultiplier);
                }
            }
        }
        else
        {
            // Remove any speed boost info
            int speedBoostIndex = currentText.IndexOf("\nSPEED BOOST");
            if (speedBoostIndex >= 0)
            {
                int nextLineIndex = currentText.IndexOf('\n', speedBoostIndex + 1);
                if (nextLineIndex >= 0)
                {
                    // Remove the boost line
                    _stringBuilder.Clear();
                    _stringBuilder.Append(currentText.Substring(0, speedBoostIndex));
                    _stringBuilder.Append(currentText.Substring(nextLineIndex));
                }
                else
                {
                    // Remove to the end
                    _stringBuilder.Clear();
                    _stringBuilder.Append(currentText.Substring(0, speedBoostIndex));
                }
            }
            
            // Update the current text for further processing
            currentText = _stringBuilder.ToString();
            
            // Remove any jump boost info
            int jumpBoostIndex = currentText.IndexOf("\nJUMP BOOST");
            if (jumpBoostIndex >= 0)
            {
                int nextLineIndex = currentText.IndexOf('\n', jumpBoostIndex + 1);
                if (nextLineIndex >= 0)
                {
                    // Remove the boost line
                    _stringBuilder.Clear();
                    _stringBuilder.Append(currentText.Substring(0, jumpBoostIndex));
                    _stringBuilder.Append(currentText.Substring(nextLineIndex));
                }
                else
                {
                    // Remove to the end
                    _stringBuilder.Clear();
                    _stringBuilder.Append(currentText.Substring(0, jumpBoostIndex));
                }
            }
        }
        
        _playerStateText.text = _stringBuilder.ToString();
    }
} 