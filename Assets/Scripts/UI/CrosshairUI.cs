using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CrosshairUI : MonoBehaviour
{
    [Header("Crosshair Settings")]
    [SerializeField] private Image _centerDot;
    [SerializeField] private Image _circle;
    [SerializeField] private float _circleRadius = 50f; // In screen pixels
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _targetColor = Color.red;
    
    [Header("Targeting Settings")]
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private LayerMask _targetLayers; // What can be damaged
    [SerializeField] private LayerMask _obstacleLayers; // What blocks shots (walls, etc.)
    
    [Header("Advanced Detection Settings")]
    [SerializeField] private bool _useScreenRaycastFallback = true;
    [Tooltip("Number of rays cast from screen for edge case detection (windows, gaps)")]
    [SerializeField] private int _screenRaycastCount = 8;
    [Tooltip("Test at multiple radii within circle: 0=center, 0.5=middle, 1.0=edge")]
    [SerializeField] private float[] _screenRaycastRadii = new float[] { 0f, 0.5f, 1f };
    
    private void Awake()
    {
        if (_mainCamera == null)
        {
            _mainCamera = Camera.main;
        }
        
        // Set up the circle size
        if (_circle != null)
        {
            RectTransform circleRect = _circle.GetComponent<RectTransform>();
            circleRect.sizeDelta = new Vector2(_circleRadius * 2, _circleRadius * 2);
        }
    }
    
    public void SetCircleRadius(float radius)
    {
        _circleRadius = radius;
        if (_circle != null)
        {
            RectTransform circleRect = _circle.GetComponent<RectTransform>();
            circleRect.sizeDelta = new Vector2(_circleRadius * 2, _circleRadius * 2);
        }
    }
    
    /// <summary>
    /// Gets all valid targets within the crosshair circle that have line of sight.
    /// Uses Physics.OverlapSphere to guarantee no targets are missed.
    /// </summary>
    public List<TargetInfo> GetTargetsInCrosshair(Vector3 shooterPosition, float maxRange)
    {
        List<TargetInfo> validTargets = new List<TargetInfo>();
        HashSet<GameObject> processedTargets = new HashSet<GameObject>();
        
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        
        // Phase 1: Get ALL potential targets in a sphere around shooter
        // This guarantees we don't miss any targets between rays
        Collider[] potentialTargets = Physics.OverlapSphere(shooterPosition, maxRange, _targetLayers);
        
        foreach (Collider collider in potentialTargets)
        {
            if (processedTargets.Contains(collider.gameObject))
                continue;
            
            IDamageable damageable = collider.GetComponent<IDamageable>();
            if (damageable == null)
                continue;
            
            // Phase 2: Check if target is visible within crosshair circle
            // Uses hybrid approach: bounds check first, then screen raycasts for edge cases
            // Line-of-sight check is now integrated into the detection methods
            if (IsTargetInCrosshairCircle(collider, screenCenter, shooterPosition, out Vector3 hitPoint))
            {
                // Valid target found (visibility and line-of-sight already verified)
                processedTargets.Add(collider.gameObject);
                validTargets.Add(new TargetInfo
                {
                    Target = damageable,
                    HitPoint = hitPoint,
                    Distance = Vector3.Distance(shooterPosition, hitPoint), // Distance from shooter for damage falloff
                    GameObject = collider.gameObject
                });
            }
        }
        
        // Update crosshair color based on targets
        UpdateCrosshairColor(validTargets.Count > 0);
        
        return validTargets;
    }
    
    /// <summary>
    /// Hybrid detection: First checks bounds points (fast), then screen raycasts (edge cases).
    /// This ensures targets visible through windows or gaps are not missed.
    /// </summary>
    private bool IsTargetInCrosshairCircle(Collider targetCollider, Vector2 screenCenter, 
        Vector3 shooterPosition, out Vector3 hitPoint)
    {
        hitPoint = targetCollider.transform.position;

        // Fast path: Check bounds points (catches 90%+ of cases quickly)
        if (CheckBoundsPoints(targetCollider, screenCenter, out hitPoint))
        {
            return true;
        }

        // Slow path: Screen-to-world raycasts for edge cases (windows, narrow gaps, etc.)
        if (_useScreenRaycastFallback && CheckScreenRaycasts(targetCollider, screenCenter, out hitPoint))
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Fast detection method: Tests multiple points on the target's bounding box.
    /// Works for most scenarios where target is directly visible.
    /// Now includes line-of-sight check for each point within the circle.
    /// </summary>
    private bool CheckBoundsPoints(Collider targetCollider, Vector2 screenCenter, out Vector3 hitPoint)
    {
        hitPoint = targetCollider.transform.position;
        Vector3 cameraPosition = _mainCamera.transform.position;
        
        // Check multiple points on the target's bounds
        Bounds bounds = targetCollider.bounds;
        Vector3[] checkPoints = new Vector3[]
        {
            bounds.center,                          // Center
            bounds.center + bounds.extents,         // Far corner
            bounds.center - bounds.extents,         // Near corner
            bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, -bounds.extents.z),
            bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, bounds.extents.z),
            bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, bounds.extents.z),
            bounds.center + new Vector3(-bounds.extents.x, -bounds.extents.y, bounds.extents.z),
            bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, -bounds.extents.z),
            bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, -bounds.extents.z)
        };
        
        // Check if ANY point of the target is within the circle AND has line of sight
        foreach (Vector3 point in checkPoints)
        {
            // Convert world position to screen position
            Vector3 screenPos = _mainCamera.WorldToScreenPoint(point);
            
            // Check if behind camera
            if (screenPos.z < 0)
                continue;
            
            // Check if within circle radius
            float distanceFromCenter = Vector2.Distance(new Vector2(screenPos.x, screenPos.y), screenCenter);
            
            if (distanceFromCenter <= _circleRadius)
            {
                // This point is visible in the circle - now check line of sight from camera
                Vector3 directionToTarget = (point - cameraPosition).normalized;
                float distanceToTarget = Vector3.Distance(cameraPosition, point);
                
                if (!Physics.Raycast(cameraPosition, directionToTarget, out RaycastHit losCheck, distanceToTarget, _obstacleLayers))
                {
                    // Line of sight is clear!
                    hitPoint = point;
                    return true;
                }
                // If line of sight blocked, continue checking other points
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Edge case detection: Casts rays from screen pixels within the crosshair circle into the 3D world.
    /// This catches scenarios like targets visible through windows where bounds points hit walls.
    /// Line-of-sight is inherently checked by this method (obstacles block the ray before reaching target).
    /// </summary>
    private bool CheckScreenRaycasts(Collider targetCollider, Vector2 screenCenter, out Vector3 hitPoint)
    {
        hitPoint = targetCollider.transform.position;
        
        // Sample points within the crosshair circle and cast into world
        for (int i = 0; i < _screenRaycastCount; i++)
        {
            float angle = (360f / _screenRaycastCount) * i;
            float radians = angle * Mathf.Deg2Rad;
            
            // Test at different radii within the circle (center, middle, edge)
            foreach (float radiusPercent in _screenRaycastRadii)
            {
                Vector2 offset = new Vector2(
                    Mathf.Cos(radians) * _circleRadius * radiusPercent,
                    Mathf.Sin(radians) * _circleRadius * radiusPercent
                );
                
                Vector2 screenPoint = screenCenter + offset;
                Ray screenRay = _mainCamera.ScreenPointToRay(screenPoint);
                
                // Cast ray into world - check ALL hits along the ray
                RaycastHit[] hits = Physics.RaycastAll(screenRay, Mathf.Infinity, _targetLayers | _obstacleLayers);
                
                // Sort by distance to find what's visible first
                System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
                
                foreach (RaycastHit hit in hits)
                {
                    // Did we hit our target?
                    if (hit.collider == targetCollider)
                    {
                        // Found it! This part of the target is visible (and not blocked)
                        // Line-of-sight is inherently verified - ray hit target before any obstacle
                        hitPoint = hit.point;
                        return true;
                    }
                    
                    // Did we hit an obstacle first?
                    if (IsObstacle(hit.collider.gameObject))
                    {
                        // Obstacle blocks this ray - stop checking this ray
                        break;
                    }
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Checks if a GameObject is considered an obstacle that blocks shots.
    /// </summary>
    private bool IsObstacle(GameObject obj)
    {
        return (_obstacleLayers & (1 << obj.layer)) != 0;
    }
    
    private void UpdateCrosshairColor(bool hasTarget)
    {
        Color targetColor = hasTarget ? _targetColor : _normalColor;
        
        if (_centerDot != null)
        {
            _centerDot.color = targetColor;
        }
        
        if (_circle != null)
        {
            _circle.color = targetColor;
        }
    }
    
    public float GetCircleRadius()
    {
        return _circleRadius;
    }
    
    public bool GetUseScreenRaycastFallback()
    {
        return _useScreenRaycastFallback;
    }
    
    public int GetScreenRaycastCount()
    {
        return _screenRaycastCount;
    }
    
    public float[] GetScreenRaycastRadii()
    {
        return _screenRaycastRadii;
    }
    
    public LayerMask GetTargetLayers()
    {
        return _targetLayers;
    }
    
    public LayerMask GetObstacleLayers()
    {
        return _obstacleLayers;
    }
}

public struct TargetInfo
{
    public IDamageable Target;
    public Vector3 HitPoint;
    public float Distance;
    public GameObject GameObject;
}
