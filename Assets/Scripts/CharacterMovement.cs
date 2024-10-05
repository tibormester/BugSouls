using UnityEngine;

public class CharacterMovement : MonoBehaviour
{

    //Designed to be altered with the move and jump methods
    private Vector3 velocity; //The characters target velocity (AI's desired velocity / Input's desired Velocity), gets mixed with the physicsBody's velocity and sent to to the rigidbody

    // Components
    private Rigidbody body;
    public CustomPhysicsBody physicsBody;

    void Start(){
        // Get the CharacterController component attached to the player
        body = GetComponent<Rigidbody>(); //Useful because this transfomr might be different from the gameobject if the move or jump functions are called outside of fixedupdate
        physicsBody = GetComponent<CustomPhysicsBody>();
    }
    [Tooltip("Where in global space to move towards each physics tick, gets shortened to move speed")]
    public Vector3 moveDirection = Vector3.zero;
    void FixedUpdate() {
        verticalVelocity();//Applies a jump or not
        horizontalVelocity(moveDirection);
        body.AddForce(body.transform.TransformDirection(velocity), ForceMode.VelocityChange);
        AlignSurface();
    }

    // Movement variables
    [Header("Horizontal Movement")]
    public float moveSpeed = 1f;
    public float rotationSpeed = 32;
    public float aerialSpeedCoefficient = 0.1f;
    //Takes in a desired input vector in world coordinates, projects it into local coordinates, removes vertical velocity, scales to movement speed, rotates towards movement direction
    public void Move(Vector3 world_direction, float distance = 999f){
        moveDirection += world_direction.normalized * distance;
    }
    private void horizontalVelocity(Vector3 world_vector){
        float distance = world_vector.magnitude;
        if (distance >= 0.01f){
            //Project the moveDirection from the camera's coordinates onto the players coodrinates by subtracting the vertical component
            Vector3 moveDirection = world_vector - Vector3.Dot(world_vector, body.transform.up) * body.transform.up;  //Vector is in global coordinates
            Vector3 localDirection = body.transform.InverseTransformDirection(moveDirection.normalized) * Mathf.Min(moveSpeed, distance); //Make it local and limit it to movespeed

            //ADDS to velocity
            if (physicsBody.IsGrounded()){ //When we are grounded
                velocity =  new Vector3(localDirection.x, velocity.y, localDirection.z); //Overwrites horizontal velocity components, but keeps vertical
                // LERPS forward axis to movement axis
                if (true){//localDirection.magnitude > 1f){ //IDK if this check is necessary, but if velocity is small, dont rotate
                    float angle = Vector3.SignedAngle(body.transform.forward, moveDirection, body.transform.up);
                    angle = Mathf.LerpAngle(0, angle, rotationSpeed * Time.fixedDeltaTime);
                    body.AddTorque(body.transform.up * angle, ForceMode.Impulse);
                }
            }else{ //When we are in the air
                //When in the air use acceleration instead of instant change in velocity
                Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
                horizontalVelocity = Vector3.ClampMagnitude( horizontalVelocity + (localDirection * aerialSpeedCoefficient), moveSpeed);
                velocity = horizontalVelocity +  new Vector3(0, velocity.y, 0);
                // LERPS forward axis to movement axis
                if (horizontalVelocity.magnitude > 0.2f){
                    float angle = Vector3.SignedAngle(body.transform.forward, moveDirection, body.transform.up);
                    angle = Mathf.LerpAngle(0, angle, rotationSpeed * aerialSpeedCoefficient * Time.fixedDeltaTime);
                    body.AddTorque(body.transform.up * angle, ForceMode.Impulse);
                }
            }
        } else{
            velocity =  new Vector3(0f, velocity.y, 0f); //Overwrites horizontal velocity components, but keeps vertical
        }
        moveDirection = Vector3.zero;
    }
    [Header("Jump Settings")]
    [Tooltip("How much times gravity should the jump velocity be. Note gravity doesn't stop, so this is -2*gravity, therefore 1.5 = 1 gravity - 1.5gravity = -0.5 gravity upwards...")]
    public float jumpForce = 0.5f;
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
        if (timer >= 0f && timer < jumpTime){
            velocity.y = Mathf.Sqrt(jumpForce * -1f * physicsBody.gravity);
            timer += Time.fixedDeltaTime;
        } else{
            timer = -1f;
            velocity.y = 0f;
        }
    }

    [Header("Grounded Alignment")]
    [Tooltip("factor of orientation torque")]
    public float orientationStrength = 0.3f;
    public float orientationDampening = -0.1f;
    public void AlignSurface(){//most objects dont need alignment, only characters
        // Calculate the target up vector (ground normal) and the current up vector
        Vector3 targetUp = physicsBody.groundNormal;
        Vector3 currentUp = body.transform.up;

        // Find the axis around which to rotate to align the object
        Vector3 rotationAxis = Vector3.Cross(currentUp, targetUp);
            
        // Calculate the angle we need to rotate (dot product gives us the cosine of the angle)
        float angleDifference = Vector3.SignedAngle(currentUp, targetUp, rotationAxis);

        // Normalize the rotation axis and scale by the desired angle difference (in radians)
        Vector3 desiredTorque = rotationAxis.normalized * angleDifference * orientationStrength;
        desiredTorque += orientationDampening * body.angularVelocity;

        // Apply torque to the rigidbody to rotate it toward the target orientation
        body.AddTorque(desiredTorque, ForceMode.Impulse);
    }

}
