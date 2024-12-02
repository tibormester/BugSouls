using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throwable : MonoBehaviour
{
    // Start is called before the first frame update
 
    private Rigidbody rb;
    public float baseDamage = 1f;
    public bool orientated = false;
    void Start(){
        rb = GetComponent<Rigidbody>();   
    }

    private bool held = false;

    public void PickedUp(GameObject parent, Vector3 localPosition){
        held = true; //Used for collision detection
        transform.SetParent(parent.transform, true);
        transform.localPosition = localPosition;
        if(orientated) transform.localRotation = Quaternion.identity;
        rb.excludeLayers = LayerMask.GetMask(new string[]{"Player"});
        rb.isKinematic = true;
    }
    public void Thrown(Vector3 velocity){
        held = false;
        transform.SetParent(null, true);
        rb.excludeLayers = LayerMask.GetMask(new string[]{});
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
    public Action<Health,Vector3> OnCollision;
    public void Collide(Health health, Vector3 impulse){
        if(!collided.Contains(health))
            StartCoroutine(Collision(health, impulse));
    }
    public float MaxMultiplier = 5f;
    public float ExpectedForce = 1f;
    public float MinMultiplier = 0.1f;
    public float forceMultiplier = 0.02f; //impulses are around 10s to 50s to 150s depending on charge time becomes 0.2x,  1x, 3x 
    public IEnumerator Collision(Health health, Vector3 impulse){
        Debug.LogWarning("collided with impulse: " + impulse.magnitude);
        health.ApplyDamage(baseDamage * Mathf.Clamp(impulse.magnitude * forceMultiplier, MinMultiplier, MaxMultiplier));
        collided.Add(health);
        //Do something like knockback? But the physics simulation already does enough knockback...
        yield return null;
    }
    private void OnCollisionEnter(Collision collision){
        if(held){return;}
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy") && collision.rigidbody != null){
            Health health = collision.rigidbody.gameObject.GetComponent<Health>();
            if (health != null){
                OnCollision?.Invoke(health, collision.impulse);
                
            }
        }
    }
}
