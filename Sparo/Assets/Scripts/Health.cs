using UnityEngine;

public class Health : MonoBehaviour
{
    private float _health;
    private float _maxHealth;

    public void TakeDamage(float dmg)
    {
        _health -= dmg;

        if (_health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        
    }
}
