using UnityEngine;
using System;
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

    public Action<GameObject> DroppedItem;
    //Runs every tick and triggers the drop action, by default is when health hits 50% (also heals)
    public Func<Droppable,bool> When = DefaultTrigger;

    public static bool DefaultTrigger(Droppable drop){
        var health = drop.GetComponent<Health>();
        if(health && drop.item && health.currentHealth / health.maxHealth <= 0.5f){
            var ai = drop.GetComponent<GeneralAI>();
            if(ai){
                ai.data[FleeBehaviour.fleeingTag]=true;
            }
            return true;
        }
        return false;
    }

    //Im guessing this is the same efficiecny starting a coroutine with yield return WaitUntil(When)
    public void FixedUpdate(){
        if(When.Invoke(this)){
            var i = item;
            Drop(); //sets item to null
            DroppedItem?.Invoke(i);
        }
    }
    public bool drop = true; // false means throw

    public void Drop(){
        ChangeLayersRecursive(item,LayerMask.NameToLayer("Throwable") );
        var globalScale = item.transform.lossyScale;
        item.transform.SetParent(this.gameObject.transform.parent); //Maybe set to null?
        item.transform.localScale = globalScale * ScaleMultiplier;

        Rigidbody rb = item.GetComponent<Rigidbody>();
        if(rb){
            rb.useGravity = true;
            rb.isKinematic = false;
            rb.excludeLayers = LayerMask.GetMask(new string[]{});
        }else{
            rb = AddRigidBody(); //Adds it with some init stuff
        }
        Throwable throwable = item.GetComponent<Throwable>();
        if(throwable){
            throwable.enabled = true;
        }else{
            throwable = item.AddComponent<Throwable>();
            
        }
        if(drop){
            StartCoroutine(LaunchItem(rb));
        } else{
            throwable.Thrown(popVector * popDuration);
        }
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

    public static void  ChangeLayersRecursive(GameObject obj, int layerID){
        obj.layer = layerID;
        foreach(Transform child in obj.transform){
            ChangeLayersRecursive(child.gameObject, layerID);
        }
    }
}

