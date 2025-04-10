using UnityEngine;

public class LoopingXMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float moveDistance = 5.0f;

    [Header("Optional Settings")]
    [SerializeField] private bool useLocalPosition = false;
    [SerializeField] private bool smoothMovement = true;

    private Vector3 startPosition;
    private float direction = 1.0f;
    private float distanceTraveled = 0.0f;

    void Start()
    {
        // Store the initial position
        startPosition = useLocalPosition ? transform.localPosition : transform.position;
    }

    void Update()
    {
        // Calculate movement for this frame
        float moveThisFrame = moveSpeed * Time.deltaTime * direction;

        // Update distance traveled
        distanceTraveled += Mathf.Abs(moveThisFrame);

        // Check if we need to change direction
        if (distanceTraveled >= moveDistance)
        {
            direction *= -1; // Reverse direction
            distanceTraveled = 0; // Reset distance counter
        }

        // Apply movement
        if (useLocalPosition)
        {
            if (smoothMovement)
            {
                // Smooth sine-based movement
                float xPos = startPosition.x + Mathf.Sin(Time.time * moveSpeed) * (moveDistance / 2);
                transform.localPosition = new Vector3(xPos, transform.localPosition.y, transform.localPosition.z);
            }
            else
            {
                // Linear movement
                transform.localPosition += new Vector3(moveThisFrame, 0, 0);
            }
        }
        else
        {
            if (smoothMovement)
            {
                // Smooth sine-based movement
                float xPos = startPosition.x + Mathf.Sin(Time.time * moveSpeed) * (moveDistance / 2);
                transform.position = new Vector3(xPos, transform.position.y, transform.position.z);
            }
            else
            {
                // Linear movement
                transform.position += new Vector3(moveThisFrame, 0, 0);
            }
        }
    }

    // Optional: Visualize the path in the editor
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
        {
            Vector3 pos = useLocalPosition ? transform.localPosition : transform.position;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(
                new Vector3(pos.x - moveDistance / 2, pos.y, pos.z),
                new Vector3(pos.x + moveDistance / 2, pos.y, pos.z)
            );
        }
    }
}