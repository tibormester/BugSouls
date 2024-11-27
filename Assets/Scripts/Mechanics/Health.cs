using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public HealthBar health;

    private ParticleSystem pSys;

    private void Start()
    {
        currentHealth = maxHealth; // Initialize health
        health = GetComponent<HealthBar>();
        pSys = GetComponent<ParticleSystem>();
        health?.setMaxHealth(maxHealth);
        health?.setHealth(currentHealth);
    }

    public void ApplyDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Ensure health doesn't go below 0
        health?.setHealth(currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        StartCoroutine(PlayHitParticles());
        
    }
    private IEnumerator PlayHitParticles(){
        pSys?.Play();
        yield return new WaitForSeconds(0.3f);
        pSys?.Stop();
    }

    private void Die()
    {
        // Handle the object's death (e.g., disable it, play animation, etc.)
        Debug.Log($"{gameObject.name} has died!");
        gameObject.SetActive(false); // Example action on death
    }
}
