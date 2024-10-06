using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throwable : MonoBehaviour
{
    // Start is called before the first frame update
    private CustomPhysicsBody physicsBody;
    private Rigidbody rb;
    void Start(){
        rb = GetComponent<Rigidbody>();
        physicsBody = GetComponent<CustomPhysicsBody>();
    }

    // Update is called once per frame
    void Update() {
        if (held){
            rb.rotation = holderrb.rotation;

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
}
