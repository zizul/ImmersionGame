using UnityEngine;
using System;
using System.Collections;

public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private int _maxHealth = 100;
    private int _currentHealth;
    
    public event Action<int, int> OnHealthChanged; // (current, max)
    public event Action OnDeath;
    
    private EnemyController _enemyController;
    
    private void Awake()
    {
        _currentHealth = _maxHealth;
        _enemyController = GetComponent<EnemyController>();

        if (_enemyController != null)
        {
            OnHealthChanged += _enemyController.UpdateColor;
            OnDeath += _enemyController.ShakeAndDestroy;
        }
    }

    public void Update()
    {
        _enemyController.UpdateColor(_currentHealth, _maxHealth);
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0)
            return;
            
        _currentHealth = Mathf.Max(0, _currentHealth - damage);
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        
        if (_currentHealth == 0)
        {
            Die();
        }
    }
    
    public void Heal(int amount)
    {
        if (amount <= 0)
            return;
            
        _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }
    
    private void Die()
    {
        OnDeath?.Invoke();
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }
}