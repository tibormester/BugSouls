using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float damage = 10f; // Damage amount
    public GameObject player; //Could do get component in parent since weapons get reparented to the player

    private void OnCollisionEnter(Collision collision)
    {
        print(collision.gameObject.name);
        // Check if the collided object is in the player layer
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy")){
            // Get the Health component from the collided object
            Health health = collision.gameObject.GetComponent<Health>();
            if (health != null)
            {
                // Apply damage
                health.ApplyDamage(damage);
                Debug.LogWarning("Dealth SOme DAMGE!");
            }
        }
    }
    public void ToggleActive(bool state){
        foreach(var collider in GetComponentsInChildren<Collider>()){
            
            collider.enabled = state;
        }
    }
}
