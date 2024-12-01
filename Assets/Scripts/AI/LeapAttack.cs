using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;

[CreateAssetMenu(fileName = "LeapAttack", menuName = "Behaviours/Attacks/Leap", order = 0)]
public class LeapAttack : AIBehaviour {
    public static string coolDownTag = "LeapAttack.ready";

    public float Damage = 0;
    public float Knockback = 0;
    public float CoolDown = 3f;

    public float FollowThroughDuration = 0.5f;

    public override void InitializeLocalData(GeneralAI ai){
        ai.data[coolDownTag] = true;
    }

    public override bool CanStart(GeneralAI ai){
        return (bool)ai.data[coolDownTag];
    }


    public override IEnumerator Behaviour(GeneralAI ai){
        //Init local variables
        var wait = new WaitForFixedUpdate();
        List<Health> damaged = new List<Health>();
        Action<Health,Collision> DamageAction = (Health victim, Collision collision) => {
            if(!damaged.Contains(victim)) { //If not already damaged, damage it
                ApplyDamage(ai, victim, Damage, Knockback);
                damaged.Add(victim);
            }
        };

        //Subscribe damage to our collision event, checking already existing collisions first
        ai.CollisionEvent += DamageAction;
        foreach(Health victim in ai.colliding){
            DamageAction(victim, null);
        }
        //Start flashing red
        Renderer renderer = ai.gameObject.GetComponent<Renderer>();
        Material material = renderer.material;
        Color originalColor = material.color;
        Coroutine flashing = ai.StartCoroutine(FlashRed(material, new Color[]{Color.red, originalColor}));

        //Tilt head down
        var oStr = ai.movement.orientationSpringStrength; //Not safe, because what if this is called twice without reseting between?
        ai.movement.orientationSpringStrength = 3f;
        var mSpd = ai.movement.maxSpeed;
        ai.movement.maxSpeed = 0f;
        for (int i = 0; i < 20; i++){
            ai.rb.AddTorque(ai.transform.right * 5f, ForceMode.VelocityChange);
            yield return wait;
        }
        ai.movement.maxSpeed = mSpd;
        //Launch forward
        for (int i = 0; i < 10; i++){
            Vector3 direction = ai.target.transform.position - ai.transform.position;
            ai.rb.AddForce(direction.normalized * 3f, ForceMode.VelocityChange);
            ai.rb.AddTorque(ai.transform.right * 4f, ForceMode.VelocityChange);
            yield return wait;
        }
        ai.movement.orientationSpringStrength = oStr;

        yield return new WaitForSeconds(FollowThroughDuration);//Need to wait for the spider to get launched

        //Stop attempting to damage the target on collision
        ai.CollisionEvent -= DamageAction;
        //Stop flashing
        ai.StopCoroutine(flashing);
        material.color = originalColor;
        //Start the cooldown
        ai.StartCoroutine(CoolDownTimer(ai));

    }

    private IEnumerator FlashRed(Material material, Color[] colors, float delay = 0.1f){
        //Ideally I would use a shader but i havent looked into that, this is expensive but the game is simple so its good enough
        var wait = new WaitForSeconds(delay);
        while(true){
            foreach(var color in colors){
                material.color = color;
                yield return wait;
            }
        }
    }

    
    private IEnumerator CoolDownTimer(GeneralAI ai){
        ai.data[coolDownTag] = false;
        yield return new WaitForSeconds(CoolDown);
        ai.data[coolDownTag] = true;
    }
}
