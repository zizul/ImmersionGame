using UnityEngine;

public abstract class PowerUp : MonoBehaviour
{
    [SerializeField] protected float _duration = 10f;
    [SerializeField] protected GameObject _visualEffect;
    [SerializeField] protected AudioClip _pickupSound;
    
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Apply(other.gameObject);
            
            // Play effects
            if (_pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(_pickupSound, transform.position);
            }
            
            // Disable the power-up object
            gameObject.SetActive(false);
        }
    }
    
    protected abstract void Apply(GameObject player);
} 