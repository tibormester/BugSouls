using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    private void Start()
    {
        currentHealth = maxHealth; // Initialize health
    }

    public void ApplyDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Ensure health doesn't go below 0

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Handle the object's death (e.g., disable it, play animation, etc.)
        Debug.Log($"{gameObject.name} has died!");
        gameObject.SetActive(false); // Example action on death
    }
}
