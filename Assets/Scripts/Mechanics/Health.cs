using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public HealthBar health;

    private ParticleSystem pSys;

    public Action DeathEvent;

    private void Start()
    {
        currentHealth = maxHealth; // Initialize health
        //health = GetComponent<HealthBar>();
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
            StartCoroutine(Die());
        }
        if(damageAmount > 0f){
            StartCoroutine(PlayHitParticles());
        }
        
    }
    private IEnumerator PlayHitParticles(){
        pSys?.Play();
        yield return new WaitForSeconds(0.3f);
        pSys?.Stop();
    }

    private IEnumerator Die()
    {
        // Handle the object's death (e.g., disable it, play animation, etc.)
        Debug.Log($"{gameObject.name} has died!");
        DeathEvent?.Invoke();
        GetComponent<CharacterMovement>().acceleration = 0f;
        if (gameObject.layer == LayerMask.NameToLayer("Player")){
            yield return null;
        } else{
            yield return new WaitForSeconds(0.01f);

            Corpsify(gameObject); //Removes this script btw
            //gameObject.SetActive(false); // Example action on death
        }
        
    }
    public void Corpsify(GameObject enemy){
        enemy.AddComponent<Throwable>();
        enemy.layer = LayerMask.NameToLayer("Throwable");

        Destroy(enemy.GetComponent<CharacterMovement>());
        Destroy(enemy.GetComponent<Health>());
        Destroy(enemy.GetComponent<ParticleSystem>());


    }
}
