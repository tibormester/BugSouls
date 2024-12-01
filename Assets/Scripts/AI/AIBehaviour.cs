using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;
//A class that defines CanStart, Can Finish
[CreateAssetMenu(fileName = "AIBehaviour", order = 0)]
public abstract class AIBehaviour : ScriptableObject {

    public virtual void InitializeLocalData(GeneralAI ai){
        return;
    }

    //Can we perform this behaviour?
    public virtual bool CanStart(GeneralAI ai){
        return true;
    }
    //Returns the coroutine (could be one frame or a full attack animation)
    //This way the general AI can return yield and wait until the current behaviour is over before looking for the next one
    public virtual IEnumerator Behaviour(GeneralAI ai){
        yield return null;
    }

    //Applies damage with knockback flattened by the victim's up (so they dont get launched into the air) 
    private void ApplyDamage(GeneralAI attacker, Health victim, float damage = 1f, float knockback = 0f){
        victim.ApplyDamage(damage);
        if(knockback != 0f){
            var direction = victim.transform.position - attacker.transform.position;
            direction = Vector3.ProjectOnPlane(direction, victim.transform.up); //flatten so we dont hop on hit
            victim.GetComponent<Rigidbody>()?.AddForce(direction.normalized * knockback, ForceMode.Impulse);
        }
    }
        
}
