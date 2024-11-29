using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class MeleeSpiderlingAI : MonoBehaviour{
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
        SceneDescriptor sd = gameObject.scene.GetRootGameObjects().Select(go => go.GetComponent<SceneDescriptor>()).FirstOrDefault(desc => desc != null);
        sd.PlayerEntered += RecievePlayer;
        PlayerCollision += (hit) => Debug.LogError("Player Collision");
    } 
    public void RecievePlayer(Transform player){
        target = player;
    }
    public Transform target;
    public float minEngageDistance = 2f, maxEngageDistance = 3f;
    public float backupAccelMultiplier = 2f;
    public float maxChaseDistance = 100f;
    public float maxStuckTime = 4f;
    public float maxStuckDistance = 1f;

    public float originalAccel = 5f;
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
                            StartCoroutine(LeapAttack());
                            attackTimer = attackCooldown;
                            strafeDirection *= -1f;
                        } else{
                            charMovement.Move(transform.right * strafeDirection );
                        }
                    }
                }
            }            
        }
        attackTimer = attackTimer < 0f ? 0f : attackTimer - Time.fixedDeltaTime;
    }
    private float strafeDirection = 1f;
    public IEnumerator LeapAttack(){
        //Check if we are already colliding
        damaged = false;
        if(hit != null){
            ApplyDamage(hit);
        }
        PlayerCollision += ApplyDamage;
        //Damage target if we end up colliding
        //Tilt head down
        for (int i = 0; i < 10; i++){
            rb.AddTorque(Vector3.right * 5f, ForceMode.Impulse);
            yield return null;
        }
        //Launch forward
        Vector3 direction = target.transform.position - transform.position;
        for (int i = 0; i < 10; i++){
            rb.AddForce(direction.normalized * 1.5f, ForceMode.Impulse);
            yield return null;
        }

        //Stop attempting to damage the target on collision
        PlayerCollision -= ApplyDamage;
        //reset the bool flag so we can damage next cycle
        yield return null;
    }
    private bool damaged = false;
    public Action<Health> PlayerCollision;
    private void ApplyDamage(Health health){
        if (damaged){
            return;
        }
        var direction = health.transform.position - transform.position;
        direction = Vector3.ProjectOnPlane(direction, health.transform.up); //flatten so we dont hop on hit
        health.ApplyDamage(damage);
        health.GetComponent<Rigidbody>()?.AddForce(direction.normalized * 1f, ForceMode.Impulse);
        damaged = true;
    }
    void OnCollisionEnter(Collision other) {
        Rigidbody collision = other.rigidbody;
        if (collision){
            if(collision.gameObject.layer == LayerMask.NameToLayer("Player")){
                var health = collision.GetComponent<Health>();
                if (health != null){
                    PlayerCollision?.Invoke(health);
                    hit = health;
                }   
            }
        }
    }
    void OnCollisionExit(Collision other) {
        Rigidbody collision = other.rigidbody;
        if (collision){
            if(collision.gameObject.layer == LayerMask.NameToLayer("Player")){
                var hitHealth = collision.GetComponent<Health>();
                if(hitHealth != null && hitHealth == hit){
                    hit = null;
                }
            }
        }
    }
    public Health hit;
    private float  attackTimer = 0;
    public float attackCooldown = 1.5f;


}

