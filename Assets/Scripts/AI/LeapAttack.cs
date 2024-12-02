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
        /** TODO: Fix that this changes the particle's material not the meshes material
        //Start flashing red
        Renderer renderer = ai.gameObject.GetComponent<Renderer>();
        Material material = renderer.material;
        Color originalColor = material.color;
        Coroutine flashing = ai.StartCoroutine(FlashRed(material, new Color[]{Color.red, originalColor}));
        **/
        //Tilt head down
        var oStr = ai.movement.orientationSpringStrength; //Not safe, because what if this is called twice without reseting between?
        ai.movement.orientationSpringStrength = 0.5f; //Necessary otherwise the conflicting char movment script forces it back upright

        ai.movement.maxSpeed = 0.5f;
        float timer = 0f;
        while (timer < WindUpDuration + ChargeDuration){
            Vector3 direction = ai.target.transform.position - ai.transform.position;
            ai.movement.look_direction = direction;

            //Quaternion.FromToRotation(ai.transform.forward, directionToTarget);

            Quaternion flatRotation = Quaternion.LookRotation(direction, Vector3.up);
            Quaternion targetRotation = Quaternion.Euler(angle, flatRotation.eulerAngles.y, 0);

            // Calculate angle difference
            Quaternion currentRotation = ai.transform.rotation;

            Quaternion rotationDelta = targetRotation * Quaternion.Inverse(currentRotation);
            // Step 4: Apply the rotation with a check to ensure proper direction
            if (Quaternion.Dot(ai.transform.rotation, targetRotation) > 0){
                // Flip the quaternion to ensure shortest path
                rotationDelta = new Quaternion(-rotationDelta.x, -rotationDelta.y, -rotationDelta.z, -rotationDelta.w);
            }
            // Get a single angle + the difference
            rotationDelta.ToAngleAxis(out float angleDifference, out Vector3 axis);
            // Apply torque if axis isnt zero
            if (axis != Vector3.zero){
                Vector3 torque = axis * angleDifference * RotationSpeed; //Use some small spring strength constant?
                ai.rb.AddTorque(torque, ForceMode.Acceleration);
            }
            if(timer > WindUpDuration){
                ai.movement.maxSpeed = 99f;
                ai.rb.AddForce(direction.normalized * ChargeSpeed, ForceMode.VelocityChange);
            }
            timer += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }
        ai.movement.maxSpeed = 99f;
        ai.movement.orientationSpringStrength = 1f;

        yield return new WaitForSeconds(FollowThroughDuration);//Need to wait for the spider to get launched
        ai.movement.orientationSpringStrength = oStr;

        //Stop attempting to damage the target on collision
        ai.CollisionEvent -= DamageAction;
        
        //Stop flashing
        //ai.StopCoroutine(flashing);
        //material.color = originalColor;
        
        //Start the cooldown
        ai.StartCoroutine(CoolDownTimer(ai));

    }
    public float angle = 45f; //The angle about the x axis we want to lift the z axis by 
    public float RotationSpeed = 1f;
    public float WindUpDuration = 0.3f; //How long we take to achieve the angle
    public float ChargeDuration = 0.1f; //How long we accelerate for
    public float ChargeSpeed = 1f;
    public float FollowThroughDuration = 0.5f; //How long we coast before its over

    /**
    private IEnumerator FlashRed(Material material, Color[] colors, float delay = 0.1f){
        //Ideally I would use a shader but i havent looked into that, this is expensive but the game is simple so its good enough, except it doesnt work since this isn't the enemy's actual model or material
        var wait = new WaitForSeconds(delay);
        while(true){
            foreach(var color in colors){
                material.color = color;
                yield return wait;
            }
        }
    }
    **/

    
    private IEnumerator CoolDownTimer(GeneralAI ai){
        ai.data[coolDownTag] = false;
        yield return new WaitForSeconds(CoolDown);
        ai.data[coolDownTag] = true;
    }
}
