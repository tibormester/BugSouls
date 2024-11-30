using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throwable : MonoBehaviour
{
    // Start is called before the first frame update
 
    private Rigidbody rb;
    public float baseDamage = 1f;
    public float velocityMultiplier = 1f;
    void Start(){
        rb = GetComponent<Rigidbody>();   
    }

    private bool held = false;

    public void PickedUp(GameObject parent, Vector3 localPosition){
        held = true; //Used for collision detection
        transform.SetParent(parent.transform, true);
        rb.isKinematic = true;
    }
    public void Thrown(Vector3 velocity){
        held = false;
        transform.SetParent(null, true);
        rb.isKinematic = false;
        StartCoroutine(Throwing(velocity));
    }
    private IEnumerator Throwing(Vector3 velocity){
        collided.Clear();
        OnCollision += Collide;
        rb.AddForce(velocity, ForceMode.VelocityChange);
        yield return new WaitForSeconds(0.2f); //Let the force go into effect
        yield return new WaitUntil( () => rb.velocity.sqrMagnitude < 0.2f );
        OnCollision -= Collide;
        collided.Clear();
    }
    
    //For some reason i was struggling to use LayerMask.name to layer (player)
    private List<Health> collided = new();
    private Action<Health> OnCollision;
    public void Collide(Health health){
        StartCoroutine(Collision(health));
    }
    public IEnumerator Collision(Health health){
        health.ApplyDamage(baseDamage);
        collided.Add(health);
        //Do something?
        yield return null;
    }
    private void OnCollisionEnter(Collision collision){
        if(held){return;}
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy") && collision.rigidbody != null){
            Health health = collision.rigidbody.gameObject.GetComponent<Health>();
            if (health != null){
                OnCollision?.Invoke(health);
                
            }
        }
    }
}
