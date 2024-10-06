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
        rotateTowards(look_direction, (!physicsBody.IsGrounded()) ? aerialSpeedCoefficient : 1f); //Apply horizontal torque towards look direction
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
    public float rotationSpeed = 16;
    public float aerialSpeedCoefficient = 0.1f;
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
        if (targetHorizontalVelocity.sqrMagnitude < 0.1f){
            delta_direction = -1.5f;
        }if (localHorizontalVelocity.sqrMagnitude < 0.1f){
            delta_direction = 1f;
        }
        float accel = acceleration + (additional_deacceleration * 0.5f * (1f - delta_direction));
        

        //Calculate the needed accelerration and clamp it to the max acceleration during this frame
        Vector3 localDelta = targetHorizontalVelocity -  localHorizontalVelocity;
        Vector3 localAccel = Vector3.ClampMagnitude(localDelta, accel * Time.fixedDeltaTime);
        moveDirection = Vector3.zero;
        rb.AddForce(rb.transform.TransformDirection(localAccel), ForceMode.VelocityChange);
    }

    public Vector3 look_direction = Vector3.forward;
    public float rotationSpringForce = 0.5f;
    public float rotationSpringDampen = 0.1f;
    public void rotateTowards(Vector3 looking_direction, float speedFactor = 1f, bool spring = true){
        float angle = Vector3.SignedAngle(rb.transform.forward, looking_direction, physicsBody.groundNormal);
        if (spring){
            float accel = angle * rotationSpringForce - Vector3.Dot(rb.angularVelocity, physicsBody.groundNormal) * rotationSpringDampen;
            rb.AddTorque(physicsBody.groundNormal * accel, ForceMode.Acceleration);
        }else{
            float speed = rotationSpeed * speedFactor;
            angle = Mathf.Clamp( angle, -speed, speed);
            rb.AddTorque(physicsBody.groundNormal * angle, ForceMode.Acceleration);
        }
    }

    [Header("Jump Settings")]
    [Tooltip("How much times gravity should the jump velocity be. Note gravity doesn't stop, so this is -2*gravity, therefore 1.5 = 1 gravity - 1.5gravity = -0.5 gravity upwards...")]
    public float jumpVelocity = 16f;
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
    private void verticalVelocity(){//Called each frame to either apply the jump velocity or to reset the vertical velocity

        //Get the local horizontal rb velocity
        Vector3 verticalVelocity = rb.transform.InverseTransformDirection(rb.velocity);
        float verticalSpeed =  verticalVelocity.y;
        
        float targetSpeed = 0f;
        if (timer >= 0f && timer < jumpTime){//Jumping, want to accelerat to a target velocity
            targetSpeed = jumpVelocity;
            timer += Time.fixedDeltaTime;
            float localDelta = targetSpeed - verticalSpeed;
            rb.AddForce(physicsBody.groundNormal * localDelta, ForceMode.VelocityChange);
        } else{ //Not jumping, so gravity takes over...
            timer = -1f;
        }
        //Calculate the needed accelerration and clamp it to the max acceleration during this frame
        
        
    }

    [Header("Grounded Alignment")]
    public float orientationSpeed = 360f;
    public float orientationDampening = -0.1f;
    public void AlignSurface(){//Applies a torque so that the player stands upright
        // Calculate the target up vector (ground normal) and the current up vector
        Vector3 targetUp = physicsBody.groundNormal;
        Vector3 currentUp = rb.transform.up;

        // Find the axis around which to rotate to align the object
        Vector3 rotationAxis = -Vector3.Cross(currentUp, targetUp);
            
        // Calculate the angle we need to rotate (dot product gives us the cosine of the angle)
        float angleDifference = Vector3.SignedAngle(currentUp, targetUp, rotationAxis);
        float speed = orientationSpeed * Time.fixedDeltaTime;
        angleDifference = Mathf.Clamp(angleDifference, -speed , speed);

        // Normalize the rotation axis and scale by the desired angle difference
        Vector3 desiredAngularVelocity = rotationAxis.normalized * angleDifference;
        Vector3 normalDesired = desiredAngularVelocity.normalized;
        desiredAngularVelocity += -Vector3.Dot(normalDesired, rb.angularVelocity) * normalDesired;//project current velocity onto desired, subtract that from desired = total velocity
        rb.AddTorque(desiredAngularVelocity, ForceMode.VelocityChange);
    }

}
