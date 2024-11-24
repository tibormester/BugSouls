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
        rb.AddForce(velocity, ForceMode.VelocityChange);
    }
    
    //For some reason i was struggling to use LayerMask.name to layer (player)
    private static int player_layer = 6;
    //When thrown deal damage based on the force it collides with
    private void OnCollisionEnter(Collision collision)
    {
        if(held){return;}
        print(collision.gameObject.name);
        // Check if the collided object is in the player layer
        if (collision.gameObject.layer == player_layer && collision.gameObject != holder){
            // Get the Health component from the collided object
            Health health = collision.gameObject.GetComponent<Health>();
            if (health != null)
            {
                float damage = baseDamage * collision.impulse.magnitude;
                health.ApplyDamage(damage);
            }
        }
    }
}
