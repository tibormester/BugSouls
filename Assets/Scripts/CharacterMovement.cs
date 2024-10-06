using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class CharacterMovement : MonoBehaviour
{
    // Components
    private Rigidbody rb;
    public CustomPhysicsBody physicsBody;

    void Start(){
        // Get the CharacterController component attached to the player
        rb = GetComponent<Rigidbody>(); //Useful because this transfomr might be different from the gameobject if the move or jump functions are called outside of fixedupdate
        physicsBody = GetComponent<CustomPhysicsBody>();
    }
    
    void FixedUpdate() {
        horizontalVelocity(moveDirection); //Applies movement forces towards move direction
        verticalVelocity();//Applies a jump or not
        rotateTowards(look_direction); //Apply horizontal torque towards look direction
        AlignSurface(); //Applies vertical torque towards surface normal
    }

    // Movement variables
    [Header("Horizontal Movement")]
    [Tooltip("Where in global space to move towards each physics tick, gets shortened to move speed")]
    public Vector3 moveDirection = Vector3.zero;
    [Tooltip("Maximum velocity in the move direction before acceleration turns off. m/s")]
    public float moveSpeed = 8f;
    [Tooltip("default change in velcoity in (m/s)/s. Applied towards move_dir")]
    public float acceleration = 6f;
    [Tooltip("acceleration += deacceleration * 1/2(1 - (move_dir dot current velcoity)... when accelerating towards current velocity this does nothing, when accelerating away (towards 0,0,0 is always awat) this sums acceleration and deacceleration. A higher value for this makes it so that movement is more responsive")]
    public float additional_deacceleration = 10f;
    [Tooltip("Angular velocity applied to orientation to match towards movement direction")]
    public float rotationSpringForce = 0.5f;
    //Takes in a desired input vector in world coordinates, projects it into local coordinates, removes vertical velocity, scales to movement speed, rotates towards movement direction
    public void Move(Vector3 world_direction, float distance = 999f){
        moveDirection += world_direction.normalized * distance;
    }
    private void horizontalVelocity(Vector3 world_vector){
        //Take a global direction and get the local horizontal component as a target horizontal velocity
        Vector3 localDirection = rb.transform.InverseTransformDirection(world_vector);  //Make it local
        Vector3 targetHorizontalVelocity = new Vector3(localDirection.x, 0f, localDirection.z).normalized * moveSpeed; //remove vertical and scale to move speed

        //Get the local horizontal rb velocity
        Vector3 localHorizontalVelocity = rb.transform.InverseTransformDirection(rb.velocity);
        localHorizontalVelocity.y = 0f;
        
        //Calculate the acceleration based on change in direction
        float delta_direction = Vector3.Dot(targetHorizontalVelocity.normalized, localHorizontalVelocity.normalized);
        if (targetHorizontalVelocity.sqrMagnitude < 0.1f){ //trying to stop so help more
            delta_direction = -1.5f;
        }if (localHorizontalVelocity.sqrMagnitude < 0.1f){ //starting from standstill so dont help
            delta_direction = 1f;
        }
        float accel = acceleration + (additional_deacceleration * 0.5f * (1f - delta_direction));

        //If we can keep accelerating towards the move direction do it
        float currentSpeed = Vector3.Dot(localHorizontalVelocity, targetHorizontalVelocity.normalized);
        if ( currentSpeed < moveSpeed){
            Vector3 localDelta = (targetHorizontalVelocity -  localHorizontalVelocity).normalized * accel;
            rb.AddForce(rb.transform.TransformDirection(localDelta), ForceMode.Acceleration);
        }
        localHorizontalVelocity += -currentSpeed * targetHorizontalVelocity.normalized;
        localHorizontalVelocity *= -drag;
        rb.AddForce(localHorizontalVelocity, ForceMode.Acceleration);
        moveDirection = Vector3.zero;
    }
    public float drag = 0.3f;

    public Vector3 look_direction = Vector3.forward;
    
    private void rotateTowards(Vector3 looking_direction){
        float angle = Vector3.SignedAngle(rb.transform.forward, looking_direction, physicsBody.groundNormal);
        float accel = angle * rotationSpringForce;
        rb.AddTorque(physicsBody.groundNormal * accel, ForceMode.Acceleration);
    }

    [Header("Jump Settings")]
    [Tooltip("How much times gravity should the jump velocity be. Note gravity doesn't stop, so this is -2*gravity, therefore 1.5 = 1 gravity - 1.5gravity = -0.5 gravity upwards...")]
    public float jumpVelocity = 8f;
    [Tooltip("How long should ")]
    public float jumpTime = 0.2f;
    private float timer = -1f;
    public void Jump(){ //Starts the jump timer, so for each fixed update applies the jump velocity for the jump duration
        if (physicsBody.IsGrounded()) {
            if (timer == -1f){
                timer = 0f;
            }   
        }
    }
    [Header("Grounded Alignment")]
    public float groundSpringStrength = 5f;
    public float groundSpringDampen = -0.3f;
    public float orientationSpringStrength = 10f;
    private void verticalVelocity(){//Called each frame to either apply the jump velocity or to reset the vertical velocity

        //Get the local horizontal rb velocity
        Vector3 verticalVelocity = rb.transform.InverseTransformDirection(rb.velocity);
        float verticalSpeed =  verticalVelocity.y;
        
        float targetSpeed = 0f;
        if (timer >= 0f && timer < jumpTime){//Jumping, want to accelerat to a target velocity
            targetSpeed = jumpVelocity;
            timer += Time.fixedDeltaTime;
            float localDelta = targetSpeed - verticalSpeed;
            rb.AddForce(physicsBody.groundNormal * localDelta, ForceMode.Acceleration);
        } else{ //Not jumping, so gravity takes over...
            timer = -1f;
            if(physicsBody.IsGrounded()){//If we are grounded add a spring force to keep it at ground height
                //We want neutral to be 80% of the maximum grounded distance away
                float displacement = -physicsBody.groundDistance + (physicsBody.groundedDistance * 0.5f);
                //spring acceleration = strength * displacement -  dampen * velocity
                if (displacement >= (physicsBody.groundedDistance * 0.5f)){
                    displacement *= 5f;
                }
                float accel = displacement * groundSpringStrength - groundSpringDampen * Vector3.Dot(rb.velocity, -physicsBody.groundNormal);
                rb.AddForce(accel * physicsBody.groundNormal, ForceMode.Acceleration);
            }
        }
        
    }
    public void AlignSurface(){//Applies a torque so that the player stands upright
        // Calculate the target up vector (ground normal) and the current up vector
        Vector3 targetUp = physicsBody.groundNormal;
        Vector3 currentUp = rb.transform.up;
        // Find the axis around which to rotate to align the object
        Vector3 rotationAxis = -Vector3.Cross(currentUp, targetUp);
        // Calculate the angle we need to rotate
        float angleDifference = Vector3.SignedAngle(currentUp, targetUp, rotationAxis);
        //Spring equation
        float accel = angleDifference * orientationSpringStrength;
        Vector3 torque = accel * rotationAxis;
        //We would need a secondary axis for dampening....
        
        rb.AddTorque(torque, ForceMode.Acceleration);


    }

}
