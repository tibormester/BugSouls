using System;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public HealthBar health;

    private ParticleSystem pSys;

    public Action DeathEvent;
    public Action Damaged;

    public Transform healthBar; //A red cube where the scale along the z axis represents the fill

    private void Start()
    {
        currentHealth = maxHealth; // Initialize health
        //health = GetComponent<HealthBar>();
        pSys = GetComponent<ParticleSystem>();
        health?.setMaxHealth(maxHealth);
        health?.setHealth(currentHealth);
        if(healthBar)
            healthBar.transform.localScale = new Vector3(healthBar.transform.localScale.x,healthBar.transform.localScale.y, currentHealth/maxHealth);
    }
    

    public void ApplyDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Ensure health doesn't go below 0
        
        health?.setHealth(currentHealth);

        if(healthBar)
            healthBar.transform.localScale = new Vector3(healthBar.transform.localScale.x,healthBar.transform.localScale.y, currentHealth/maxHealth);
        
        if(damageAmount > 0f){
            Damaged?.Invoke();
            StartCoroutine(PlayHitParticles());
        }
        if (currentHealth <= 0){
            StartCoroutine(Die());
        }
        
        
    }
    private IEnumerator PlayHitParticles(){
        pSys?.Play();
        yield return new WaitForSeconds(0.2f);
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

            StartCoroutine(Corpsify(gameObject)); //Removes this script btw
            //gameObject.SetActive(false); // Example action on death
        }
        
    }
    public IEnumerator Corpsify(GameObject enemy){
        Throwable throwable = enemy.AddComponent<Throwable>();
        Rigidbody rb = enemy.GetComponent<Rigidbody>();
        if (healthBar){
            Destroy(healthBar.gameObject); //For somereason on death it doesnt shrink to nothing
        }
        throwable.baseDamage = 1f / rb.mass; //Make sure heavy corpses dont one shot anything
        enemy.layer = LayerMask.NameToLayer("Throwable");
        Vector3 scale = enemy.transform.localScale;
        int max_frames = 50;
        float shrunkSize = 0.7f;
        Destroy(enemy.GetComponent<CharacterMovement>());
        for (int i = max_frames; i > 0; i--){   
            //TODO: Make death shrivel the corpse
            rb.AddTorque(Vector3.right, ForceMode.Impulse);
            enemy.transform.localScale = scale * (shrunkSize + (1f - shrunkSize)*((float)i/(float)max_frames)); //scale * (0.7 + 0.3*Interpolator(from 1 to 0))
            yield return null;
        }
        Destroy(enemy.GetComponent<Health>());
        Destroy(enemy.GetComponent<ParticleSystem>());
        yield return null;


    }
}
