using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class LeapAttack : AIBehaviour {
    public float Damage = 0;
    public float Knockback = 0;
    public float CoolDown = 3f;

    public float FollowThroughDuration = 0.5f;

    public override void InitializeLocalData(GeneralAI ai){
        ai.data["LeapAttack.CoolingDown"] = false;
    }


    public override IEnumerator Behaviour(GeneralAI ai){
        //Init local variables
        var wait = new WaitForFixedUpdate();
        List<Health> damaged = new List<Health>();
        delegate DamageAction = (Health victim, Collision collision) => {
            if(!damaged.Contains(victim)) { //If not already damaged, damage it
                ApplyDamage(ai, victim, Damage, Knockback);
                damaged.Add(victim)
            }
        };

        //Subscribe damage to our collision event, checking already existing collisions first
        ai.CollisionEvent += DamageAction;
        foreach(Health victim in ai.colliding){
            Damage(victim, null);
        }
        //Start flashing red
        var originalColor = ai.gameObject.material.color;
        Coroutine flashing = ai.StartCoroutine(FlashRed);

        //Tilt head down
        var oStr = charMovement.orientationSpringStrength; //Not safe, because what if this is called twice without reseting between?
        charMovement.orientationSpringStrength = 3f;
        var mSpd = charMovement.maxSpeed;
        charMovement.maxSpeed = 0f;
        for (int i = 0; i < 20; i++){
            rb.AddTorque(transform.right * 5f, ForceMode.VelocityChange);
            yield return wait;
        }
        charMovement.maxSpeed = mSpd;
        //Launch forward
        for (int i = 0; i < 10; i++){
            Vector3 direction = target.transform.position - transform.position;
            rb.AddForce(direction.normalized * 3f, ForceMode.VelocityChange);
            rb.AddTorque(transform.right * 4f, ForceMode.VelocityChange);
            yield return wait;
        }
        charMovement.orientationSpringStrength = oStr;

        yield return new WaitForSeconds(FollowThroughDuration);//Need to wait for the spider to get launched

        //Stop attempting to damage the target on collision and stop the red indicator
        CollisionEvent -= DamageAction;
        ai.StopCoroutine(flashing);
        ai.gameObject.material.color = originalColor;
        ai.StartCoroutine(CoolDown)

    }

    private IEnumerator FlashRed(GeneralAI ai){
        //Add some code to tint the material forever, then stop the coroutine later
        //Ideally I would use a shader but i havent looked into that
        var wait = new WaitForFixedUpdate();
        while(true){
            ai.gameObject.material.SetColor("", Color.red);
            yield return null;
        }
    }

    
    private IEnumerator CoolDown(GeneralAI ai){
        ai.data["LeapAttack.CoolingDown"] = true;
        yield return new WaitForSeconds(CoolDown);
        ai.data["LeapAttack.CoolingDown"] = false;
    }
}
