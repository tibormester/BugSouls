using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throwable : MonoBehaviour
{
    // Start is called before the first frame update
    private CustomPhysicsBody physicsBody;
    private Rigidbody rb;
    public float baseDamage = 1f;
    public float velocityMultiplier = 1f;
    void Start(){
        rb = GetComponent<Rigidbody>();
        physicsBody = GetComponent<CustomPhysicsBody>();
        
    }

    // This probably needs to be fixed update since its a rigidbody
    void Update() {
        if (held){
            //rb.rotation = holderrb.rotation;

            rb.position = holderrb.position + holderrb.transform.TransformDirection(relative);
            rb.velocity = holderrb.velocity;
            rb.angularVelocity = holderrb.angularVelocity;
        }
    }
    private bool held = false;
    private GameObject holder;
    private Vector3 relative = Vector3.zero;
    private Rigidbody holderrb;

    public void PickedUp(GameObject parent, Vector3 localPosition){
        held = true;
        holder = parent;
        relative = localPosition;
        holderrb = holder.GetComponent<Rigidbody>();
    }
    public void Thrown(Vector3 velocity){
        held = false;
        collided.Clear();
        rb.AddForce(velocity, ForceMode.VelocityChange);
    }
    
    //For some reason i was struggling to use LayerMask.name to layer (player)
    private List<Health> collided = new();
    private void OnCollisionEnter(Collision collision)
    {
        if(held){return;}
        Debug.LogWarning(collision.gameObject.name);
        // Check if the collided object is in the player layer
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy")){
            // Get the Health component from the collided object
            Health health = collision.gameObject.GetComponent<Health>();
            if (health != null)
            {
                if(!collided.Contains(health) && rb.velocity.sqrMagnitude > 1f){

                    float damage = Mathf.Clamp(baseDamage * collision.impulse.magnitude, 0f, 65f);
                    health.ApplyDamage(damage);
                    collided.Add(health); //So we dont double collide or the enemy takes damage walking over it
                }
                
            }

        }
    }
}
