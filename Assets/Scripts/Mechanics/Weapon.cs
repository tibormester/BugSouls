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
    private void OnTriggerEnter(Collider other){

        // Check if the collided object is in the player layer
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy")){
            // Get the Health component from the collided object
            Health health = other.gameObject.GetComponent<Health>();
            if (health != null){
                hitting.Add(health);
            }
        }
    }
    private void OnTriggerExit(Collider other){
        // Check if the collided object is in the player layer
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy")){
            // Get the Health component from the collided object
            Health health = other.gameObject.GetComponent<Health>();
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
        yield return new WaitForSeconds(0.15f * duration);
        //Start the effects and refresh our list of hits
        foreach(var collider in GetComponentsInChildren<Collider>()){   
            collider.enabled = true;
            collider.isTrigger = true;
        }
        windTrail?.SetActive(true);

        //Wait until the swing is over
        yield return new WaitForSeconds(0.65f * duration);
        //Stop the windtrail earlier because the cooldown will be slower than the swing
        windTrail?.SetActive(false);
        foreach(var collider in GetComponentsInChildren<Collider>()){   
            collider.enabled = false;
            collider.isTrigger = false;
        }
        //Some delay for emphasis (totatlly not because this is easier to implement)
        yield return new WaitForSeconds(0.2f * duration);
        
        //Stop the swing and apply all the damage
        foreach(var health in hitting ){
            health.ApplyDamage(damage);
        }

        for(int i = 0; i < 15; i++){
            foreach(var health in hitting ){
                //Apply some knockback
                var rb = health.GetComponent<Rigidbody>();
                if(rb){
                    Vector3 direction =  rb.transform.position - player.transform.position;
                    direction = direction.normalized * knockBack;
                    rb.AddForce(direction, ForceMode.Impulse);
                }
            }
            yield return null;
        }
        yield return null;
    }
}
