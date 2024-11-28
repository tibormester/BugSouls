using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    [SerializeField] FloatingHealthBar healthBar;

    new private ParticleSystem particleSystem;

    private void Start()
    {
        currentHealth = maxHealth; // Initialize health
        particleSystem = GetComponent<ParticleSystem>();
        healthBar = GetComponentInChildren<FloatingHealthBar>();
    }

    public void ApplyDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Ensure health doesn't go below 0
        healthBar.UpdateHealthBar(currentHealth, maxHealth);

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
