using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class MushroomGuyScript : MonoBehaviour
{
    public GameObject hat;
    public Health health;
    // Start is called before the first frame update
    public void Start(){
        health = GetComponent<Health>();
    }

    public void FixedUpdate(){
        if(health.currentHealth <= 0.5f * health.maxHealth && hat){
            DropHat();
        }
    }
    public void DropHat(){
        hat.layer = LayerMask.NameToLayer("Throwable");
        hat.transform.SetParent(this.gameObject.transform.parent);
        Rigidbody rb = hat.AddComponent<Rigidbody>();
        hat.AddComponent<Throwable>();
        hat.transform.localScale = Vector3.one * 0.7f;
        rb.useGravity = false;
        rb.drag = 0.5f;
        rb.angularDrag = 1f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        CustomPhysicsBody pb = hat.GetComponent<CustomPhysicsBody>();
        pb.enabled = true;
        pb.groundMask = LayerMask.GetMask("Terrain");
        hat = null;
    }
}
