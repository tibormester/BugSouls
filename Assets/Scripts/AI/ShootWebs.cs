using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "ShootWeb", menuName = "Behaviours/Attacks/Shoot", order = 1)]
public class ShootWebs : AIBehaviour {
    public static string coolDownTag = "ShootWebs.cooldown";
    public GameObject webPrototype;
    public float LaunchSpeed = 0.5f;
    public int maxLaunchTicks = 200;
    public float damage = 5f;
    public float WindUp = 0.2f;
    public float CoolDown = 2.5f;

    public override void InitializeLocalData(GeneralAI ai){
        ai.data[coolDownTag] = true;
    }

    public override bool CanStart(GeneralAI ai){
        return (bool)ai.data[coolDownTag];
    }

    public override IEnumerator Behaviour(GeneralAI ai) {
        var wait = new WaitForFixedUpdate();
        float timer = 0;
        while (timer < WindUp){
            var diff = ai.target.transform.position - ai.transform.position;
            ai.movement.look_direction = diff;
            timer += Time.fixedDeltaTime;
            yield return wait;
        }
        ai.StartCoroutine(SpitAttack(ai, ai.target.position));
        ai.StartCoroutine(CoolDownTimer(ai));
    }
    public IEnumerator SpitAttack(GeneralAI ai, Vector3 attackLocation){
        var wait = new WaitForFixedUpdate();
        GameObject newWeb = Instantiate(webPrototype, ai.transform);
        //TODO: initialize local variables
        Ray ray = new Ray(ai.transform.position, attackLocation - ai.transform.position);
        Vector3 targetLocation = ray.origin;
        float difference;
        int launchTicks = 0;
        
        //Initial web orientation
        newWeb.transform.LookAt(targetLocation);
        difference = (targetLocation- ai.transform.position).magnitude;
        newWeb.transform.localScale = new Vector3(1,1, difference);
        RaycastHit hit;
        //Each tick, check the ray cast towards the target location for the object 
        while(launchTicks < maxLaunchTicks){
            // Raycast from the current target Location out by the launch speed to the new target location
            if (Physics.Raycast(targetLocation - ray.direction, ray.direction, out hit, LaunchSpeed + 1f, LayerMask.GetMask(new string[]{"Player", "Terrain", "Default"}))){
                    if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Player")){
                        Destroy(newWeb);
                        yield break;
                    }
                    //Update the web to go to the hit location
                    newWeb.transform.LookAt(hit.point);
                    difference = (hit.point- ai.transform.position).magnitude;
                    newWeb.transform.localScale = new Vector3(1,1, difference);

                    hit.rigidbody.GetComponent<Health>().ApplyDamage(damage);

                    CharacterMovement characterMovement = hit.rigidbody.GetComponent<CharacterMovement>();
                    characterMovement.StartCoroutine(ApplySlowStatus(characterMovement, 1.5f, 2f));
                   
                    Destroy(newWeb);
                    yield break;
            } //If we didnt hit anything keep moving the target location forward
            else{ 
                targetLocation += ray.direction * LaunchSpeed;
                launchTicks++;
            }
            //Reorientate the web from the players new position to the new target location
            newWeb.transform.LookAt(targetLocation);
            difference = (targetLocation- ai.transform.position).magnitude;
            newWeb.transform.localScale = new Vector3(1,1, difference);
            yield return wait;
        }
        //If the launch process times out, destroy the web
        Destroy(newWeb);
    }
    private IEnumerator ApplySlowStatus(CharacterMovement character, float duration = 1.5f, float maxSpeed = 1.5f){
        character.maxSpeed = maxSpeed;
        yield return new  WaitForSeconds(duration);
        character.maxSpeed = 99f; //Use this default value because without a modifier stack system we would introduce more bugs (and not the intended ones)
    }

    private IEnumerator CoolDownTimer(GeneralAI ai){
        ai.data[coolDownTag] = false;
        yield return new WaitForSeconds(CoolDown);
        ai.data[coolDownTag] = true;
    }
}
