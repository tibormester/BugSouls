using System;
using UnityEngine;

public class CustomPhysicsBody : MonoBehaviour
{

    
    private Rigidbody body;
    
    public bool IsGrounded() => timer < coyoteTime;
    public void Start(){
        this.body = GetComponent<Rigidbody>();
    }

    //TODO: Implement a way to have inertial objects with unfrozen rotations
    public void FixedUpdate(){
        GroundedRaycast();//Find the physics velocity changes and add it to the unpowered velocity
        ApplySurfaceForces();//If on the surface apply the surfaces effects
        FallingLogic();//Change grounded, orientation, coyote time, etc...
        ApplyGravity();
    }
    [Header("Ground Detection")]
    public LayerMask groundMask;
    public float checkDistance = 150f;
    public float velocityCheckDistance = 2f;
    public float groundedDistance = 0.1f;
    public float feetDistance = 1f;

    //PRIVATE VARIABLES
    private bool isGrounded = true; //If theres a ground within ground distance
    private bool freeFall = false; //If there isnt a ground within check distance
    public Vector3 groundNormal = Vector3.up; //The ground's normal 
    public float groundDistance; //How far from the terrain or an object, not really useful rn tbh

    public void GroundedRaycast()
    {
        bool terrain = TerrainRaycast();//Check for terrain and store the normal
        bool general = GeneralRaycast();//Check for any object without changing the normal
        if(!terrain && !general){//Couldn't detect any terrain at max ranges or objects within groundedDistance thus we are in free fall
            freeFall = true;
        }
    }
    private bool TerrainRaycast(){//Return's true when finding terrain, false if not
        RaycastHit hitInfo;
        //We want to check infront of the player to enable them to climb up walls
        Debug.DrawRay(transform.position, body.velocity.normalized * velocityCheckDistance, Color.red);

        if ((body.velocity.sqrMagnitude > 3f && Physics.Raycast(transform.position, body.velocity.normalized, out hitInfo, velocityCheckDistance, groundMask)) ||//Forward
            Physics.Raycast(transform.position, -groundNormal, out hitInfo, checkDistance, groundMask) ||//Ground Down
            //Physics.Raycast(transform.position, body.velocity.normalized -body.transform.up, out hitInfo, velocityCheckDistance, groundMask) ||//Diaganol down
            Physics.Raycast(transform.position, -body.transform.up, out hitInfo, velocityCheckDistance, groundMask)) //Terrain underneath the player or in their direction
        {
            groundNormal = hitInfo.normal;
            groundDistance = hitInfo.distance - feetDistance;
            isGrounded = groundDistance < groundedDistance;
            freeFall = false;
            return true;
        }
        return false;
    }
    private bool GeneralRaycast(){ //Returns true when finding any object, false if not
        RaycastHit hitInfo;
        //We also want to check without the mask and update ground distance with the closest object
        if (Physics.Raycast(transform.position,  -body.transform.up, out hitInfo, checkDistance) //Something beneath the player, but its not terrain
                    && hitInfo.distance - feetDistance < groundedDistance){
                groundDistance = hitInfo.distance - feetDistance;
                isGrounded = true;
                freeFall = false;
                return true;
        } else { //Nothing beneath the player, so cannot be grounded
            isGrounded = false;
        }
        return false;
    }

    private void ApplySurfaceForces(){
        //check the surface and if grounded, depending on the result, do something...
    }


    [Header("Falling")]
    [Tooltip("How long in the air before the player is considered not grounded. If it is 0f, this is immediately. If it is <0f, then the character is never grounded.")]
    public float coyoteTime = 0.1f;
    public float gravity = -32f;
    [Tooltip("How long in free fall (no ground spotted) until re-orienting to global down, If coyoteOrientationTime == 0, the object is never reorientated")]
    public float coyoteOrientationTime = 0.3f;
    [Tooltip("How long in free fall (no ground spotted) until destroying the object. If aerialTimeLimit == 0, the object is never destroyed")]
    public float aerialTimeLimit = 0f;
    private float timer = 0f;
    private float freeFallTimer = 0f;

    void FallingLogic(){
        if (isGrounded){ //If on the ground, reset timers, NOTE uses isGrounded not IsGrounded() meaning ignores coyoteTime
            timer = 0;
            freeFallTimer = 0;
         }else { //If in the air, increment timers NOTE as above, this increments the timer for CoyoteTime
            timer += Time.fixedDeltaTime;
            if (!freeFall){
                freeFallTimer = 0;
            } else{//If in free fall, increment timer and check if we need to destroy or reorientate
                freeFallTimer += Time.fixedDeltaTime;
                if (aerialTimeLimit != 0f && freeFallTimer > aerialTimeLimit){//if falling for too long, delete the object
                    Destroy(gameObject);
                } else if (coyoteOrientationTime != 0f && freeFallTimer > coyoteOrientationTime){ //After falling without ground beneath the character for some time, reorient towards global down
                    Reorientate();
                }
            }
         }
    }
    private void Reorientate(){
        groundNormal = Vector3.up;
    }
    private void ApplyGravity(){
        if (IsGrounded()){//Apply a small sticking force 
            //body.AddForce(groundNormal *0.1f*gravity*Time.fixedDeltaTime, ForceMode.VelocityChange);
        }else{// Apply gravity normally when in free fall
            body.AddForce(groundNormal *gravity * Time.fixedDeltaTime, ForceMode.VelocityChange);
            //body.velocity += gravity * Time.fixedDeltaTime * groundNormal;
        }
    }
}
