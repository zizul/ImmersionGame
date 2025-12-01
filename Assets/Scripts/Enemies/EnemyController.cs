using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer),typeof(Health))]
public class EnemyController : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private Color _hp100Color;
    [SerializeField] private Color _hp50Color;
    [SerializeField] private Color _hp0Color;
    [SerializeField] private float shakeDuration = 1f;
    [SerializeField] private ParticleSystem particleSystem;

    private float shakeMagnitude = 0.1f;
    private Health _healthController;
    private MeshRenderer _meshRenderer;

    public void Awake()
    {
        _meshRenderer = GetComponent<MeshRenderer>();

        _healthController = GetComponent<Health>();

        if (_healthController != null)
        {
            _healthController.OnHealthChanged += UpdateColor;
            _healthController.OnHealthChanged += Bleed;
            _healthController.OnDeath += ShakeAndDestroy;
        }
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

    public void Bleed(int currentHealth, int maxHealth)
    {
        if (particleSystem != null)
        {
            particleSystem.Stop();
            particleSystem.Play();
        }
    }

    private IEnumerator ShakeAndDestroyCoroutine()
    {
        Vector3 originalPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-shakeMagnitude, shakeMagnitude);
            float y = Random.Range(-shakeMagnitude, shakeMagnitude);
            transform.position = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;
        Destroy(gameObject);
    }
}