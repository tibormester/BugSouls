using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class GeneralAI : MonoBehaviour{
    
    public Health health;
    public CharacterMovement movement;
    public CustomPhysicsBody pb;
    public Rigidbody rb;

    public void Start(){
        //Cache local components
        health = GetComponent<Health>();
        pb = GetComponent<CustomPhysicsBody>();
        movement = GetComponent<CharacterMovement>();
        rb = GetComponent<Rigidbody>();
        //Find the scene descriptor and listen for the new player entering
        SceneDescriptor sd = gameObject.scene.GetRootGameObjects().Select(go => go.GetComponent<SceneDescriptor>()).FirstOrDefault(desc => desc != null);
        sd.PlayerEntered += RecievePlayer;

        //Setup default event listeners
        health.DeathEvent += () => OnDeath;

        //Let behaviours get setup, (also lets them register to events (like PlayerEntered))
        foreach(var behaviour in AIBehaviours){
            behaviour.InitializeLocalData(this);
        }

        //Starts the Enemy's AI
        StartCoroutine(ProcessBehaviours);
        
    }
    //Place to add listeners for player events
    public virtual void RecievePlayer(Transform player){
        target = player;
        target.health.DeathEvent += OnPlayerKilled;
    }
    //When we die
    public virtual void OnDeath(){
        Destroy(this);
    }
    //When the player dies, stop attacking the corpse
    public virtual void OnPlayerKilled(){
        target = null;
    }

    public Action<AIBehaviour> BehaviourStarted;
    public Action<AIBehaviour> BehaviourFinished;

    public List<AIBehaviour> AIBehaviours;

    public Dictionary<string, object> data; //Store local runtime behaviour data, like cooldown timers

    public IEnumerator ProcessBehaviours(){
        while(true){ //Each update find the first valid behaviour, do it to completion, then start searching again
            foreach( AIBehaviour behaviour in AIBehaviours){
                if (behaviour.CanStart()){
                    BehaviourStarted.Invoke(behaviour);
                    yield return StartCoroutine(behaviour.Start());
                    BehaviourFinished.Invoke(behaviour);
                    break;
                }
            }
        }
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
        //Should go through the list in order
        

        if(health.currentHealth <= 0.5f * health.maxHealth && hat){
            DropHat();
            health.ApplyDamage(-0.5f * health.maxHealth);
        }
        if (target != null ){
            if (hat == null){
                charMovement.acceleration = originalAccel * backupAccelMultiplier * 1.3f;
                charMovement.Move((transform.position - target.position).normalized);
                charMovement.look_direction = -1 *(target.position - transform.position);
            }
            //The target is close enoguh to chase
            else if (Vector3.Distance(target.position, transform.position) < maxChaseDistance){
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
                        } else{
                            charMovement.acceleration = originalAccel * backupAccelMultiplier;
                            charMovement.Move(transform.right);
                        }
                    }
                }
            }            
        }
        attackTimer = attackTimer < 0f ? 0f : attackTimer - Time.fixedDeltaTime;
    }
    
    
    //Collisions are used to detect attacks, We only register a single collision
    public Action<Health, Collision> CollisionEvent;
    public int CollisionEventLayer = LayerMask.NameToLayer("Player"); //TODO (Once have wifi, change to accept Other layers too)
    private List<Health> colliding = new(); //Stores currently colliding objects
    void OnCollisionEnter(Collision collision) {
        Rigidbody rb = collision.rigidbody;
        if (rb){
            if( rb.gameObject.layer == CollisionEventLayer){
                var health = rb.GetComponent<Health>();
                if (health != null){
                    CollisionEvent?.Invoke(health, collision);
                    colliding.Add(health);
                }   
            }
        }
    }
    void OnCollisionExit(Collision collision) {
        Rigidbody rb = collision.rigidbody;
        if (rb){
            if(rb.gameObject.layer == CollisionEventLayer){
                var health = rb.GetComponent<Health>();
                if(health != null && colliding.Contains(health)){
                    colliding.remove(health);
                }
            }
        }
    }
}

