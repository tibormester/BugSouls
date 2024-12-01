using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
/**
    Handles dropping the mushrom caps and matriarch crown on a certain health condition
**/
public class Droppable : MonoBehaviour{
    public GameObject item;

    public float ScaleMultiplier = 0.7f;
    public Vector3 popVector= Vector3.up;
    public int popDuration = 5;

    public void Drop(){
        item.layer = LayerMask.NameToLayer("Throwable");
        item.transform.SetParent(this.gameObject.transform.parent); //Maybe set to null?
        item.transform.localScale = transform.localScale * ScaleMultiplier;

        Rigidbody rb = AddRigidBody(); //Adds it with some init stuff
        Throwable throwable = item.AddComponent<Throwable>();

        StartCoroutine(LaunchItem(rb));

        item = null;
    }
    public IEnumerator LaunchItem(Rigidbody rb){
        var wait = new WaitForFixedUpdate();
        for(int i= 0; i < popDuration; i++){
            rb.AddForce(popVector, ForceMode.Impulse);
            yield return wait;
        }
    }

    public Rigidbody AddRigidBody(){
        Rigidbody rb = item.AddComponent<Rigidbody>();
        rb.useGravity = true;
        rb.drag = 0.5f;
        rb.angularDrag = 1f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        return rb;
    }
}

