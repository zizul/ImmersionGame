/// <summary>
/// Represents the current state of the player character.
/// Used for UI updates and gameplay logic.
/// </summary>
public struct PlayerState
{
    /// <summary>Whether the player is currently moving</summary>
    public bool IsMoving;
    
    /// <summary>Whether the player is currently in the air (jumping or falling)</summary>
    public bool IsJumping;
    
    /// <summary>Whether the player is currently accelerating (running)</summary>
    public bool IsAccelerating;
} 