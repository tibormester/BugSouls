using System;
using System.Collections;
using UnityEngine;

public class RangedSpiderlingAI : MonoBehaviour{
    public Health health;
    public CharacterMovement charMovement;
    public CustomPhysicsBody pb;
    public Rigidbody rb;
    // Start is called before the first frame update
    public void Start(){
        health = GetComponent<Health>();
        pb = GetComponent<CustomPhysicsBody>();
        charMovement = GetComponent<CharacterMovement>();
        rb = GetComponent<Rigidbody>();
        health.DeathEvent += () => Destroy(this);
    } 
    public Transform target;
    public float minEngageDistance = 15f, maxEngageDistance = 50f;
    public float backupAccelMultiplier = 0.8f;
    public float maxChaseDistance = 100f;
    public float maxStuckTime = 5f;
    public float maxStuckDistance = 0.2f;

    public float originalAccel = 4f;
    public float stuckTime = 0f;                       //Elapsed time "stuck"
    private Vector3 stuckPos;  

    public float damage = 5f;

    public void FixedUpdate(){
        /** Maybe have them be suicide bombers?
        if(health.currentHealth <= 0.5f * health.maxHealth && ){
        }
        **/
        if (target != null){
            //The target is close enoguh to chase
            if (Vector3.Distance(target.position, transform.position) < maxChaseDistance){
                charMovement.acceleration = originalAccel;
                //We are too far away too attack, so get closer
                if (Vector3.Distance(target.position, transform.position) > maxEngageDistance){
                    charMovement.Move((target.position - transform.position).normalized);
                    charMovement.look_direction = target.position - transform.position;
                    //If we dont move much after enough time, we might be stuck, if so try jumping
                    if (Vector3.Distance(stuckPos, transform.position) < maxStuckDistance){
                        stuckTime += Time.deltaTime;
                        if (stuckTime > maxStuckTime){
                            charMovement.Jump();
                        }
                    }
                    else{
                        stuckPos = transform.position;
                        stuckTime = 0f;
                    }
                }
                else{
                    stuckTime = 0f;
                    charMovement.look_direction = target.position - transform.position;
                    //If we are too close back up
                    if (Vector3.Distance(target.position, transform.position) < minEngageDistance){
                        charMovement.acceleration = originalAccel * backupAccelMultiplier;
                        charMovement.Move((transform.position - target.position).normalized);
                    }
                    //We are just right lets attack 
                    else {
                        if(attackTimer <= 0f){
                            StartCoroutine(SpitAttack(target.position));
                            attackTimer = attackCooldown;
                        }
                    }
                }
            }            
        }
        attackTimer = attackTimer < 0f ? 0f : attackTimer - Time.fixedDeltaTime;
    }
    public IEnumerator LeapAttack(){
        //Tilt head down
        for (int i = 0; i < 10; i++){
            rb.AddTorque(Vector3.right * 5f, ForceMode.Impulse);
            yield return null;
        }
        //Launch forward
        hit = null;
        Vector3 direction = target.transform.position - transform.position;
        for (int i = 0; i < 25; i++){
            rb.AddForce(direction.normalized * 1f, ForceMode.Impulse);
            yield return null;
        }
        //Apply damage and knockback
        if(hit){
            hit.ApplyDamage(damage);
            target.GetComponent<Rigidbody>()?.AddForce(direction.normalized * 1f, ForceMode.Impulse);
            hit = null;
        }
        yield return null;
    }
    public GameObject webPrototype;
    public float LaunchSpeed = 0.5f;
    public int maxLaunchTicks = 200;

    public IEnumerator SpitAttack(Vector3 attackLocation){
        GameObject newWeb = Instantiate(webPrototype, transform);
        //TODO: initialize local variables
        Ray ray = new Ray(transform.position, attackLocation - transform.position);
        Vector3 targetLocation = ray.origin;
        float difference;
        int launchTicks = 0;
        
        //Initial web orientation
        newWeb.transform.LookAt(targetLocation);
        difference = (targetLocation- transform.position).magnitude;
        newWeb.transform.localScale = new Vector3(1,1, difference);
        RaycastHit hit;
        //Each tick, check the ray cast towards the target location for the object 
        while(launchTicks < maxLaunchTicks){
            // Raycast from the current target Location out by the launch speed to the new target location
            if (Physics.Raycast(targetLocation - ray.direction, ray.direction, out hit, LaunchSpeed + 1f, LayerMask.GetMask(new string[]{"Player"}))){
                    
                    //Update the web to go to the hit location
                    newWeb.transform.LookAt(hit.point);
                    difference = (hit.point- transform.position).magnitude;
                    newWeb.transform.localScale = new Vector3(1,1, difference);

                    hit.rigidbody.GetComponent<Health>().ApplyDamage(damage);

                    var prev = hit.rigidbody.GetComponent<CharacterMovement>().acceleration;
                    if (prev > 1f){ //If we are already slowed, slowing again might loose the base acceleration
                    //Ideally I would construct a stat object that would hold a stack of modifiers
                        hit.rigidbody.GetComponent<CharacterMovement>().maxSpeed = 3f;
                    }
                    yield return new  WaitForSeconds(1.25f);
                    if (prev > 1f){
                        hit.rigidbody.GetComponent<CharacterMovement>().maxSpeed = prev;
                    }
                    Destroy(newWeb);
                    yield break;
            } //If we didnt hit anything keep moving the target location forward
            else{ 
                targetLocation += ray.direction * LaunchSpeed;
                launchTicks++;
            }
            //Reorientate the web from the players new position to the new target location
            newWeb.transform.LookAt(targetLocation);
            difference = (targetLocation- transform.position).magnitude;
            newWeb.transform.localScale = new Vector3(1,1, difference);
            yield return null;
        }
        //If the launch process times out, destroy the web
        Destroy(newWeb);
        yield return null;
    }
    
    private void OnCollisionEnter(Collision other) {
        Rigidbody collision = other.rigidbody;
        if (collision){
            if(collision.gameObject.layer == LayerMask.NameToLayer("Player")){
                hit = collision.GetComponent<Health>();
            }
        }
    }
    public Health hit;
    private float  attackTimer = 0;
    public float attackCooldown = 1.5f;


}

