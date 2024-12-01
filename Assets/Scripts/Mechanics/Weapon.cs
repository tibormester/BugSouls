using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float damage = 10f; // Damage amount
    public enum WeaponType {dagger, twoHandSword}; // Type of sword, used for animations
    public WeaponType type;
    public AudioSource audioSource;
    public AudioClip OnSwing;

    private List<Health> hitting = new(); 

    private void Start(){
        audioSource = GetComponent<AudioSource>();
    }
    private void OnTriggerEnter(Collider other){
        // Check if the collided object is in the player layer
        var rb =other.attachedRigidbody;
        if (rb != null && rb.gameObject.layer == LayerMask.NameToLayer("Enemy")){
            // Get the Health component from the collided object
            Health health = rb.GetComponent<Health>();
            if (health != null){
                EnemyHit?.Invoke(health);
                hitting.Add(health);
            }
        }
    }
    private void OnTriggerExit(Collider other){
        // Check if the collided object is in the player layer
        var rb = other.attachedRigidbody;
        if (rb && rb.gameObject.layer == LayerMask.NameToLayer("Enemy")){
            // Get the Health component from the collided object
            Health health = rb.GetComponent<Health>();
            if (health != null && hitting.Contains(health)){
                hitting.Remove(health);
            }
        }
    }
    public void ToggleActive(bool state, float duration = 1f){
        if (state){
            StartCoroutine(Swinging(duration));
        }
    }

    public float knockBack = 1f;
    public GameObject windTrail;
    public IEnumerator Swinging(float duration){
        //Wait through the windup
        yield return new WaitForSeconds(0.15f * duration);
        ToggleActiveSword(true);
        if(audioSource){
            audioSource.clip = OnSwing;
            audioSource?.PlayDelayed(0.1f);
        }
        //Activate collider triggers and wind trail
        yield return new WaitForSeconds(0.65f * duration);
        ToggleActiveSword(false);
        //End before cooldown is over
    }
    public void ToggleActiveSword(bool active = true){
        foreach(var collider in GetComponentsInChildren<Collider>()){   
            collider.enabled = active;
            collider.isTrigger = active;
        }
        windTrail?.SetActive(active);
        damaged.Clear();
        if (active){
            EnemyHit += Attack;
        } else {
            EnemyHit -= Attack;
        }
    }

    public event Action<Health> EnemyHit;
    private void Attack(Health health){ //Wraps the corountine so we can add and subtract it from the event without ambiguity
        if(damaged.Contains(health)){
            return;
        } else {
            damaged.Add(health);
        }
        StartCoroutine(ApplyDamage(health));
    }
    private List<Health> damaged = new();
    private IEnumerator ApplyDamage(Health health){
        var wait = new WaitForSeconds(0.05f);
        var rb = health.GetComponent<Rigidbody>();
        health.ApplyDamage(damage);
        if(rb == null){
            yield break;
        }
        for(int i = 0; i < 15; i++){
            Vector3 direction =  rb.transform.position - transform.position;
            direction = direction.normalized * knockBack;
            rb.AddForce(direction, ForceMode.Impulse);
            yield return wait;
        }
    }
}
