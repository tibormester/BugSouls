using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throwable : MonoBehaviour
{
    // Start is called before the first frame update
    private CustomPhysicsBody physicsBody;
    private Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        physicsBody = GetComponent<CustomPhysicsBody>();
    }

    // Update is called once per frame
    void Update() {
        if (held){
            transform.rotation = holder.transform.rotation;

            transform.position = holder.transform.position + holder.transform.TransformDirection(relative);
        }
    }
    private bool held = false;
    private GameObject holder;
    private Vector3 relative = Vector3.zero;
    public void PickedUp(GameObject parent, Vector3 localPosition){
        held = true;
        holder = parent;
        relative = localPosition;
    }
    public void Thrown(Vector3 velocity){
        held = false;
        rb.velocity = velocity;
    }
}