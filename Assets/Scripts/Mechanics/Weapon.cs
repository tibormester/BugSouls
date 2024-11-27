using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float damage = 10f; // Damage amount
    public GameObject player; //Could do get component in parent since weapons get reparented to the player
    public enum WeaponType {dagger, twoHandSword}; // Type of sword, used for animations
    public WeaponType type;

    private List<Health> hitting = new(); 
    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collided object is in the player layer
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy")){
            // Get the Health component from the collided object
            Health health = collision.gameObject.GetComponent<Health>();
            if (health != null){
                hitting.Add(health);
            }
        }
    }
    public void ToggleActive(bool state, float duration = 1f){
        foreach(var collider in GetComponentsInChildren<Collider>()){
            
            collider.enabled = state;
        }
        if (state){
            StartCoroutine(Swinging(duration));
        }
    }

    public float knockBack = 1f;
    public GameObject windTrail;
    public IEnumerator Swinging(float duration){
        yield return new WaitForSeconds(0.15f * duration);
        //Start the effects and refresh our list of hits
        windTrail?.SetActive(true);
        hitting.Clear();

        //Wait until the swing is over
        yield return new WaitForSeconds(0.85f * duration);
        
        //Stop the swing and apply all the damage
        foreach(var health in hitting ){
            Debug.LogWarning("Hitting " + health.name);
            // Apply damage
            health.ApplyDamage(damage);
            //Apply some knockback
            var rb = health.GetComponent<Rigidbody>();
            if(rb){
                Vector3 direction =  rb.transform.position - player.transform.position;
                direction = direction.normalized * knockBack;
                rb.AddForce(direction, ForceMode.Impulse);
            }
        }
        windTrail?.SetActive(false);
        hitting.Clear();
        yield return null;
    }
}
