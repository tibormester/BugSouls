using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class MushroomGuyScript : MonoBehaviour
{
    public GameObject hat;
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
        if(health.currentHealth <= 0.5f * health.maxHealth && hat){
            DropHat();
        }
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
            rb.AddTorque(Vector3.right * -25f, ForceMode.Impulse);
            yield return null;
        }
        //Launch forward
        hit = null;
        Vector3 direction = target.transform.position - transform.position;
        for (int i = 0; i < 25; i++){
            rb.AddForce(direction.normalized * 10f, ForceMode.Impulse);
            yield return null;
        }
        //Apply damage and knockback
        if(hit){
            hit.ApplyDamage(damage);
            target.GetComponent<Rigidbody>()?.AddForce(direction.normalized * 5f, ForceMode.Impulse);
            hit = null;
        }
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

    public void DropHat(){
        hat.layer = LayerMask.NameToLayer("Throwable");
        hat.transform.SetParent(this.gameObject.transform.parent);
        Rigidbody rb = hat.AddComponent<Rigidbody>();
        hat.AddComponent<Throwable>();
        hat.transform.localScale = transform.localScale;
        rb.useGravity = false;
        rb.drag = 0.5f;
        rb.angularDrag = 1f;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        CustomPhysicsBody pb = hat.GetComponent<CustomPhysicsBody>();
        pb.enabled = true;
        hat = null;
    }

}

