using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Optional debug visualizer for the crosshair targeting system.
/// Attach to the same GameObject as CrosshairUI to see detection sphere, target bounds, and screen raycasts in Scene view.
/// 
/// Gizmos are only drawn when this GameObject is SELECTED in the hierarchy.
/// This prevents confusion between Scene and Game camera perspectives.
/// 
/// Automatically reads configuration from CrosshairUI component:
/// - Circle radius
/// - Screen raycast count and radii
/// - Target and obstacle layers
/// - Whether screen raycast fallback is enabled
/// 
/// Shows both fast path (bounds) and slow path (screen rays) detection methods.
/// Disable this component in production builds.
/// </summary>
[RequireComponent(typeof(CrosshairUI))]
public class CrosshairDebugVisualizer : MonoBehaviour
{
    [Header("Debug Visualization")]
    [SerializeField] private bool _showDetectionSphere = true;
    [SerializeField] private bool _showTargetBounds = true;
    [SerializeField] private bool _showScreenCircle = true;
    [SerializeField] private bool _showScreenRaycasts = true;
    [SerializeField] private bool _showLineOfSight = true;
    [SerializeField] private Color _sphereColor = Color.cyan;
    [SerializeField] private Color _validTargetColor = Color.green;
    [SerializeField] private Color _boundsMissColor = Color.yellow;
    [SerializeField] private Color _screenRayColor = Color.magenta;
    [SerializeField] private Color _lineOfSightClearColor = Color.green;
    [SerializeField] private Color _lineOfSightBlockedColor = Color.red;

    [Header("Visualization Range")]
    [Tooltip("Max range for visualization only (does not affect actual targeting)")]
    [SerializeField] private float _visualizationMaxRange = 100f;
    
    private CrosshairUI _crosshairUI;
    private Camera _mainCamera;
    private Transform _playerTransform;
    
    private void Awake()
    {
        _crosshairUI = GetComponent<CrosshairUI>();
        _mainCamera = Camera.main;
        
        // Try to find player transform
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
        }
    }
    
    private void OnDrawGizmos()
    {
        if (_mainCamera == null || _crosshairUI == null)
            return;

        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Vector3 shooterPosition = _playerTransform != null ? _playerTransform.position : _mainCamera.transform.position;

        // Draw detection sphere
        if (_showDetectionSphere)
        {
            Gizmos.color = new Color(_sphereColor.r, _sphereColor.g, _sphereColor.b, 0.2f);
            Gizmos.DrawWireSphere(shooterPosition, _visualizationMaxRange);
        }
        
        // Draw screen circle projection
        if (_showScreenCircle)
        {
            Ray centerRay = _mainCamera.ScreenPointToRay(screenCenter);
            
            Gizmos.color = Color.green;
            float testDistance = 10f;
            Vector3 circleCenter = centerRay.origin + centerRay.direction * testDistance;
            
            // Draw approximate circle area
            float circleRadius = _crosshairUI.GetCircleRadius();
            float worldRadius = (circleRadius / Screen.height) * testDistance * Mathf.Tan(_mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) * 2f;
            DrawGizmosCircle(circleCenter, worldRadius, centerRay.direction);
        }

        // Draw target bounds if in play mode
        if (_showTargetBounds && Application.isPlaying)
        {
            LayerMask targetLayers = _crosshairUI.GetTargetLayers();
            Collider[] targets = Physics.OverlapSphere(shooterPosition, _visualizationMaxRange, targetLayers);
            
            foreach (Collider target in targets)
            {
                IDamageable damageable = target.GetComponent<IDamageable>();
                if (damageable == null)
                    continue;
                
                // Check if visible in circle
                bool isVisible = IsTargetVisible(target, screenCenter);
                
                // Draw bounds
                Gizmos.color = isVisible ? _validTargetColor : _boundsMissColor;
                Gizmos.DrawWireCube(target.bounds.center, target.bounds.size);
                
                // Draw screen position indicators
                DrawTargetCheckPoints(target, screenCenter, isVisible);
            }
        }
        
        // Draw screen raycasts from crosshair
        if (_showScreenRaycasts && Application.isPlaying)
        {
            DrawScreenRaycastVisualization(screenCenter);
        }
        
        // Draw line-of-sight checks (Phase 3)
        if (_showLineOfSight && Application.isPlaying)
        {
            DrawLineOfSightVisualization(screenCenter, shooterPosition);
        }
    }
    
    private bool IsTargetVisible(Collider targetCollider, Vector2 screenCenter)
    {
        Bounds bounds = targetCollider.bounds;
        float circleRadius = _crosshairUI.GetCircleRadius();
        
        Vector3[] checkPoints = new Vector3[]
        {
            bounds.center,
            bounds.center + bounds.extents,
            bounds.center - bounds.extents,
        };
        
        foreach (Vector3 point in checkPoints)
        {
            Vector3 screenPos = _mainCamera.WorldToScreenPoint(point);
            if (screenPos.z < 0)
                continue;
            
            float distanceFromCenter = Vector2.Distance(new Vector2(screenPos.x, screenPos.y), screenCenter);
            if (distanceFromCenter <= circleRadius)
                return true;
        }
        
        return false;
    }
    
    private void DrawTargetCheckPoints(Collider targetCollider, Vector2 screenCenter, bool isVisible)
    {
        Bounds bounds = targetCollider.bounds;
        
        // Draw check points on bounds
        Vector3[] checkPoints = new Vector3[]
        {
            bounds.center,
            bounds.center + bounds.extents,
            bounds.center - bounds.extents,
        };
        
        foreach (Vector3 point in checkPoints)
        {
            Gizmos.color = isVisible ? _validTargetColor : _boundsMissColor;
            Gizmos.DrawSphere(point, 0.1f);
        }
    }
    
    private void DrawScreenRaycastVisualization(Vector2 screenCenter)
    {
        if (_crosshairUI == null || _mainCamera == null)
            return;
        
        // Check if screen raycast fallback is enabled
        if (!_crosshairUI.GetUseScreenRaycastFallback())
        {
            // Don't draw if the feature is disabled
            return;
        }
        
        // Get configuration from CrosshairUI
        float circleRadius = _crosshairUI.GetCircleRadius();
        int screenRaycastCount = _crosshairUI.GetScreenRaycastCount();
        float[] radii = _crosshairUI.GetScreenRaycastRadii();
        
        if (radii == null || radii.Length == 0)
            return;
        
        // Draw screen raycasts (fallback detection)
        for (int i = 0; i < screenRaycastCount; i++)
        {
            float angle = (360f / screenRaycastCount) * i;
            float radians = angle * Mathf.Deg2Rad;
            
            foreach (float radiusPercent in radii)
            {
                Vector2 offset = new Vector2(
                    Mathf.Cos(radians) * circleRadius * radiusPercent,
                    Mathf.Sin(radians) * circleRadius * radiusPercent
                );
                
                Vector2 screenPoint = screenCenter + offset;
                Ray screenRay = _mainCamera.ScreenPointToRay(screenPoint);
                
                // Draw the ray
                Gizmos.color = _screenRayColor;
                RaycastHit hit;
                if (Physics.Raycast(screenRay, out hit, _visualizationMaxRange))
                {
                    Gizmos.DrawLine(screenRay.origin, hit.point);
                    Gizmos.DrawSphere(hit.point, 0.05f);
                }
                else
                {
                    Gizmos.DrawLine(screenRay.origin, screenRay.origin + screenRay.direction * _visualizationMaxRange);
                }
            }
        }
    }
    
    private void DrawGizmosCircle(Vector3 center, float radius, Vector3 normal)
    {
        Vector3 forward = Vector3.Slerp(Vector3.up, -normal, 0.5f);
        Vector3 right = Vector3.Cross(normal, forward).normalized;
        forward = Vector3.Cross(right, normal).normalized;
        
        Vector3 prevPoint = center + right * radius;
        for (int i = 1; i <= 32; i++)
        {
            float angle = (float)i / 32f * Mathf.PI * 2f;
            Vector3 newPoint = center + (right * Mathf.Cos(angle) + forward * Mathf.Sin(angle)) * radius;
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
    
    /// <summary>
    /// Visualizes Phase 3 line-of-sight checks from camera to targets
    /// </summary>
    private void DrawLineOfSightVisualization(Vector2 screenCenter, Vector3 shooterPosition)
    {
        if (_crosshairUI == null || _mainCamera == null)
            return;
        
        Vector3 cameraPosition = _mainCamera.transform.position;
        LayerMask targetLayers = _crosshairUI.GetTargetLayers();
        LayerMask obstacleLayers = _crosshairUI.GetObstacleLayers();
        
        // Get all potential targets (same as Phase 1)
        Collider[] potentialTargets = Physics.OverlapSphere(shooterPosition, _visualizationMaxRange, targetLayers);
        
        foreach (Collider collider in potentialTargets)
        {
            IDamageable damageable = collider.GetComponent<IDamageable>();
            if (damageable == null)
                continue;
            
            // Check if target is visible in crosshair (same logic as Phase 2)
            if (!IsTargetVisibleInCrosshair(collider, screenCenter))
                continue;
            
            // Get hit point (simplified - using center)
            Vector3 hitPoint = collider.bounds.center;
            
            // Phase 3: Check line of sight from camera
            Vector3 directionToTarget = (hitPoint - cameraPosition).normalized;
            float distanceToTarget = Vector3.Distance(cameraPosition, hitPoint);
            
            // Perform the same raycast as the actual system
            if (Physics.Raycast(cameraPosition, directionToTarget, out RaycastHit losCheck, distanceToTarget, obstacleLayers))
            {
                // Line of sight BLOCKED
                Gizmos.color = _lineOfSightBlockedColor;
                Gizmos.DrawLine(cameraPosition, losCheck.point);
                Gizmos.DrawSphere(losCheck.point, 0.15f);
                
                // Draw dashed line to show what's blocked behind
                DrawDashedLine(losCheck.point, hitPoint, _lineOfSightBlockedColor);
            }
            else
            {
                // Line of sight CLEAR
                Gizmos.color = _lineOfSightClearColor;
                Gizmos.DrawLine(cameraPosition, hitPoint);
                Gizmos.DrawSphere(hitPoint, 0.1f);
            }
        }
    }
    
    /// <summary>
    /// Simplified check if target is visible in crosshair (for visualization)
    /// </summary>
    private bool IsTargetVisibleInCrosshair(Collider targetCollider, Vector2 screenCenter)
    {
        Bounds bounds = targetCollider.bounds;
        float circleRadius = _crosshairUI.GetCircleRadius();
        
        // Check center and a few key points
        Vector3[] checkPoints = new Vector3[]
        {
            bounds.center,
            bounds.center + bounds.extents,
            bounds.center - bounds.extents,
        };
        
        foreach (Vector3 point in checkPoints)
        {
            Vector3 screenPos = _mainCamera.WorldToScreenPoint(point);
            if (screenPos.z < 0)
                continue;
            
            float distanceFromCenter = Vector2.Distance(new Vector2(screenPos.x, screenPos.y), screenCenter);
            if (distanceFromCenter <= circleRadius)
                return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Draws a dashed line between two points
    /// </summary>
    private void DrawDashedLine(Vector3 start, Vector3 end, Color color)
    {
        Gizmos.color = new Color(color.r, color.g, color.b, 0.5f);
        
        int segments = 10;
        Vector3 direction = end - start;
        float totalLength = direction.magnitude;
        
        for (int i = 0; i < segments; i++)
        {
            if (i % 2 == 0) // Only draw every other segment
            {
                float t1 = (float)i / segments;
                float t2 = (float)(i + 1) / segments;
                Vector3 p1 = start + direction * t1;
                Vector3 p2 = start + direction * t2;
                Gizmos.DrawLine(p1, p2);
            }
        }
    }
}

