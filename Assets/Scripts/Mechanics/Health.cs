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

    public AudioClip hit;
    public AudioClip death;
    public AudioSource audioSource;
    private void Start()
    {
        if (currentHealth == 0){
            currentHealth = maxHealth; // Initialize health
        }
        audioSource = GetComponent<AudioSource>();
        //health = GetComponent<HealthBar>();
        pSys = GetComponent<ParticleSystem>();
        health?.setMaxHealth(maxHealth);
        health?.setHealth(currentHealth);
        if(healthBar)
            healthBar.transform.localScale = new Vector3(healthBar.transform.localScale.x,healthBar.transform.localScale.y, currentHealth/maxHealth);
    }
    

    public void ApplyDamage(float damageAmount)
    {   
        if (currentHealth <= 0f){
            return; //Prevents dying twice
        }
        currentHealth -= damageAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Ensure health doesn't go below 0
        
        health?.setHealth(currentHealth);

        if(healthBar)
            healthBar.transform.localScale = new Vector3(healthBar.transform.localScale.x,healthBar.transform.localScale.y, currentHealth/maxHealth);
        
        if( damageAmount > 0f){
            Damaged?.Invoke();
            if(audioSource){
                audioSource.clip = hit;
                audioSource.Play();
            }
            StartCoroutine(PlayHitParticles());
        }
        if (currentHealth <= 0){
            StartCoroutine(Die());
            if(audioSource){
                audioSource.clip = death;
                audioSource.Play();
            }
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
        GetComponent<CharacterMovement>().maxSpeed = 0f;

        yield return new WaitForSeconds(0.01f);

        StartCoroutine(Corpsify(gameObject)); //Removes this script btw
        //gameObject.SetActive(false); // Example action on death
    
        
    }
    public IEnumerator Corpsify(GameObject enemy){
        Throwable throwable = enemy.AddComponent<Throwable>();
        Rigidbody rb = enemy.GetComponent<Rigidbody>();
        rb.drag = 0.5f;
        if (healthBar){
            Destroy(healthBar.gameObject); //For somereason on death it doesnt shrink to nothing
        }
        SetLayerRecursively(enemy, LayerMask.NameToLayer("Throwable"));
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

    public static void SetLayerRecursively(GameObject gameObject, int layer)
    {
        if (gameObject == null) return;

        // Change the layer of the current GameObject
        gameObject.layer = layer;

        // Recursively change the layer of all child GameObjects
        foreach (Transform child in gameObject.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}
