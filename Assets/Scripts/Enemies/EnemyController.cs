using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private Color _hp100Color;
    [SerializeField] private Color _hp50Color;
    [SerializeField] private Color _hp0Color;
    [SerializeField] private float shakeDuration = 1f;
    [SerializeField] private float shakeMagnitude = 0.1f;
    private MeshRenderer _meshRenderer;

    public void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    public void UpdateColor(int currentHealth, int maxHealth)
    {
        float healthPercentage = (float)currentHealth / maxHealth;


        Color newColor;
        if (healthPercentage == 1.0f)
        {
            newColor = _hp100Color; // 100% - green
        }
        else if (healthPercentage < 1.0f && healthPercentage > 0)
        {
            newColor = _hp50Color; // 50% - yellow
        }
        else
        {
            newColor = _hp0Color; // 0% - red
        }

        _meshRenderer.material.color = newColor;
    }

    public void ShakeAndDestroy()
    {
        StartCoroutine(ShakeAndDestroyCoroutine());
    }

    private IEnumerator ShakeAndDestroyCoroutine()
    {
        Vector3 originalPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = UnityEngine.Random.Range(-shakeMagnitude, shakeMagnitude);
            float y = UnityEngine.Random.Range(-shakeMagnitude, shakeMagnitude);
            transform.position = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;
        Destroy(gameObject);
    }
}