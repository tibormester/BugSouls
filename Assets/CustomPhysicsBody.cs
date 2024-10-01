using System;
using UnityEngine;

public class CustomPhysicsBody : MonoBehaviour
{

    
    private Rigidbody body;
    
    public bool IsGrounded() => timer < coyoteTime;
    public void Start(){
        this.body = GetComponent<Rigidbody>();
        body.freezeRotation = true;
    }

    [Header("Global Velocity")]
    private Vector3 prevPoweredVelocity = Vector3.zero;//Store between fixed updates
    [Tooltip("This velocity is applied to the object each frame but in a non cumulative fashion. That is, at the start of a frame, the previous frames powered velocity is set to zero")]
    public Vector3 poweredVelocity = Vector3.zero; //Velocity to apply each frame in world coordinates
    [Tooltip("Do not change this, it is public only for debuging with the editor")]
    public Vector3 unpoweredVelocity = Vector3.zero; //Rigidbody velocity - poweredVelocity, so we can 
    private Vector3 prevVelocity = Vector3.zero;
    //TODO: Implement a way to have inertial objects with unfrozen rotations
    public void FixedUpdate(){//PoweredVelocity may change between frames due to player/AI inputs, while unpoweredVelocity changes due to the physics simulation (collisions)
        AlignSurface();
        GroundedRaycast();
        //Find the physics velocity changes and add it to the unpowered velocity
        ApplyDrag();
        //Do stuff that might change the unpowered velocity (like gravity or drag)
        Fall();
        //Sets the rigid bodies velocity to the sum of powered + unpowered velocity
        body.velocity = body.transform.TransformDirection(unpoweredVelocity) + poweredVelocity;
        prevVelocity = body.velocity;
        
        prevPoweredVelocity = poweredVelocity;
    }
    [Header("Ground Detection")]
    public LayerMask groundMask;
    public float checkDistance = 10f;
    public float velocityCheckDistance = 2f;
    public float groundedDistance = 0.1f;
    public float feetDistance = 1f;

    //PRIVATE VARIABLES
    private bool isGrounded = true; //If theres a ground within ground distance
    private bool freeFall = false; //If there isnt a ground within check distance
    private Vector3 groundNormal; //The ground's normal 
    private float groundDistance; //How far from the terrain or an object, not really useful rn tbh

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
        if (Physics.Raycast(transform.position, body.velocity.normalized, out hitInfo, velocityCheckDistance, groundMask) ||
            Physics.Raycast(transform.position, -body.transform.up, out hitInfo, checkDistance, groundMask)) //Terrain underneath the player or in their direction
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
    [Header("Drag")]
    [Tooltip("If inertial, velocities from collisions are maintained in the next frame. WARNING inertial objects are a bit buggy when under powered movement... maybe adding drag will mitigate this issue?")]
    public bool inertial = true;
    [Tooltip("Drag coefficient is only applied to the unpowered velocity. if 0 no drag, <0 speeds up, 1 stops in 1 second")]
    public float drag = 0.25f;
    private void ApplyDrag(){
        if (inertial){
            Vector3 physicsDelta = body.velocity - prevVelocity;
            if(physicsDelta.sqrMagnitude < 0.00001f ){
                physicsDelta = Vector3.zero;
            }
            unpoweredVelocity = unpoweredVelocity + body.transform.InverseTransformDirection(physicsDelta);
        }
        unpoweredVelocity -= drag * unpoweredVelocity * Time.fixedDeltaTime;
    }

    [Header("Grounded Alignment")]
    [Tooltip("How long in the air before the player is considered not grounded. If it is 0f, this is immediately. If it is <0f, then the character is never grounded.")]
    public float coyoteTime = 0.1f;
    public float orientationSpeed = 4f;
    public void AlignSurface(){
        Quaternion targetRotation = Quaternion.FromToRotation(body.transform.up, groundNormal) * body.rotation;
        body.MoveRotation(Quaternion.Slerp(body.rotation, targetRotation, orientationSpeed * Time.fixedDeltaTime));
    }

    [Header("Falling")]
    public float gravity = -9.8f;
    [Tooltip("How long in free fall (no ground spotted) until re-orienting to global down, If coyoteOrientationTime == 0, the object is never reorientated")]
    public float coyoteOrientationTime = 2f;
    [Tooltip("How long in free fall (no ground spotted) until destroying the object. If aerialTimeLimit == 0, the object is never destroyed")]
    public float aerialTimeLimit = 0f;
    private float timer = 0f;
    private float freeFallTimer = 0f;

    void Fall(){
        ApplyGravity();//Always apply gravity, even if grounded
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
        unpoweredVelocity.y += gravity * Time.fixedDeltaTime;
        if (IsGrounded()){
            unpoweredVelocity.y = gravity * Time.fixedDeltaTime * groundDistance;
        }
    }
}
