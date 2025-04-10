using UnityEngine;

public class SpeedJumpPowerUp : PowerUp
{
    [SerializeField] private float _speedMultiplier = 1.5f;
    [SerializeField] private float _jumpMultiplier = 1.5f;
    
    protected override void Apply(GameObject player)
    {
        PlayerController controller = player.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.ApplySpeedJumpBoost(_speedMultiplier, _jumpMultiplier, _duration);
        }
    }
} 