using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public HealthBar health;

    private ParticleSystem particleSystem;

    private void Start()
    {
        currentHealth = maxHealth; // Initialize health
        health.setMaxHealth(maxHealth);
        health.setHealth(currentHealth);
        particleSystem = GetComponent<ParticleSystem>();
    }

    public void ApplyDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Ensure health doesn't go below 0
        health.setHealth(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        StartCoroutine(PlayHitParticles());
        
    }
    private IEnumerator PlayHitParticles(){
        particleSystem?.Play();
        yield return new WaitForSeconds(0.3f);
        particleSystem?.Stop();
    }

    private void Die()
    {
        // Handle the object's death (e.g., disable it, play animation, etc.)
        Debug.Log($"{gameObject.name} has died!");
        gameObject.SetActive(false); // Example action on death
    }
}
