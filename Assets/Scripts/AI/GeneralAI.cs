using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class GeneralAI : MonoBehaviour{
    
    //Local Components
    public Health health;
    public CharacterMovement movement;
    public CustomPhysicsBody pb;
    public Rigidbody rb;
    public Droppable droppable;

    //Player, probably turn into an array for multiplayer
    public Transform target;

    public bool processing = true;

    public void Start(){
        //Cache local components
        health = GetComponent<Health>();
        pb = GetComponent<CustomPhysicsBody>();
        movement = GetComponent<CharacterMovement>();
        rb = GetComponent<Rigidbody>();
        droppable = GetComponent<Droppable>();
        //Find the scene descriptor and listen for the new player entering
        SceneDescriptor sd = gameObject.scene.GetRootGameObjects().Select(go => go.GetComponent<SceneDescriptor>()).FirstOrDefault(desc => desc != null);
        sd.PlayerEntered += RecievePlayer;

        //Setup default event listeners
        health.DeathEvent += () => OnDeath();

        //Let behaviours get setup, (also lets them register to events (like PlayerEntered))
        foreach(var behaviour in AIBehaviours){
            behaviour.InitializeLocalData(this);
        }

        //Starts the Enemy's AI
        StartCoroutine(ProcessBehaviours());
        
    }
    //Place to add listeners for player events
    public virtual void RecievePlayer(Transform player){
        target = player;
        target.GetComponent<Health>().DeathEvent += OnPlayerKilled;
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

    public Dictionary<string, object> data = new(); //Store local runtime behaviour data, like cooldown timers

    public IEnumerator ProcessBehaviours(){
        while(true){ //Each update find the first valid behaviour, do it to completion, then start searching again
            foreach( AIBehaviour behaviour in AIBehaviours){
                if (behaviour.CanStart(this)){
                    BehaviourStarted?.Invoke(behaviour);
                    yield return StartCoroutine(behaviour.Behaviour(this));
                    BehaviourFinished?.Invoke(behaviour);
                    break;
                }
            }
            yield return new WaitUntil(() => processing); //Because disabling the monobehaviour doesn't pause the coroutines
        }
    }

    //Collisions are used to detect attacks, We only register a single collision
    public Action<Health, Collision> CollisionEvent;

    public int CollisionEventLayer = -1; //TODO (Once have wifi, change to accept Other layers too)
    
    public List<Health> colliding = new(); //Stores currently colliding objects //Should be public get protected set but i dont have wifi
    void OnCollisionEnter(Collision collision) {
        Rigidbody rb = collision.rigidbody;
        if (rb){
            if( rb.gameObject.layer ==  LayerMask.NameToLayer("Player") ){
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
            var health = rb.GetComponent<Health>();
            if(health != null && colliding.Contains(health)){
                colliding.Remove(health);
            }
        }
    }
}

