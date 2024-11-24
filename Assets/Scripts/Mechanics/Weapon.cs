using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float damage = 10f; // Damage amount
    private static int player_layer = 6;
    public GameObject player;

    private void OnCollisionEnter(Collision collision)
    {
        print(collision.gameObject.name);
        // Check if the collided object is in the player layer
        if (collision.gameObject.layer == player_layer && collision.gameObject != player){
            // Get the Health component from the collided object
            Health health = collision.gameObject.GetComponent<Health>();
            if (health != null)
            {
                // Apply damage
                health.ApplyDamage(damage);
            }
        }
    }
    public GameObject windTrail;
    public void ToggleActive(){
        windTrail.SetActive(!windTrail.activeSelf);
    }
}
