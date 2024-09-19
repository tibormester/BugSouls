using System.Collections;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    // Movement variables
    public float moveSpeed = 16;
    public float rotationSpeed = 32;
    public float orientationSpeed = 4f;
    public float jumpForce = 1.5f;
    public float gravity = -1028f;
    public float feetDistance = 0.2f; // Distance from the feet's position to the outside of the player model
    public float groundedDistance = 0.1f; // Maximum distance from the feet distance to the ground to be considered grounded

    private Vector3 velocity;


    // Camera and player orientation
    public Transform cameraTransform;

    // Components
    private Rigidbody body;
    private CapsuleCollider capsuleCollider;

    void Start()
    {
        // Get the CharacterController component attached to the player
        body = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        // Lock and hide Cursor
        //Cursor.visible = false; // Not needed with CursorLockMode.Locked
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Ground detection paramters
    public Transform groundCheck; //Where to start the ground raycast from
    public float checkDistance = 10f; //How far down to check
    public LayerMask groundMask; //The terrain's layer mask
    // Ground Raycast results
    private bool isGrounded; 
    private Vector3 groundNormal;
    private Rigidbody ground;
    private float groundDistance;

    //TODO: Consider changing approach from raycasting to the ground, doing a shape cast at the feet and using the closest terrain as an object
    //Then we can raycast to the center of the terain object, and using the surface normal at that position for our orientation...? its likely to be approximately the surface we are standing on
    void GroundedRaycast(){
        RaycastHit hitInfo;
        Vector3 globalHorizontal = body.transform.TransformDirection(new Vector3(velocity.x, -0.025f, velocity.z));
        //Starts at the feet position and checks down from their orientation by ground distance for terrain
        if (Physics.Raycast(groundCheck.position,  -body.transform.up, out hitInfo, checkDistance, groundMask)
            || Physics.Raycast(groundCheck.position, globalHorizontal, out hitInfo, checkDistance, groundMask)){

            groundNormal = hitInfo.normal;
            ground = hitInfo.rigidbody;
            groundDistance = hitInfo.distance - feetDistance;

            if (groundDistance < groundedDistance){
                isGrounded = true;
            } else{
                isGrounded = false;
            }

        } else {
            isGrounded = false;
            groundNormal = Vector3.up;
        }
    }
    void AlignSurface(){
        if (isGrounded){
            // Align the character's "up" direction with the terrain's surface normal
            Quaternion targetRotation = Quaternion.FromToRotation(body.transform.up, groundNormal) * body.rotation;
            // Smoothly rotate the character towards the new orientation
            body.MoveRotation(Quaternion.Slerp(body.rotation, targetRotation, orientationSpeed * Time.fixedDeltaTime));
        } else{
        }
    }
    void Update(){
        
        
        Jump();
        
    }
    void FixedUpdate() {
        //Checks if grounded and updates appropriate terrain information
        GroundedRaycast();
        Fall();
        StickingForce();
        Movement();
        AlignSurface();
        body.velocity =  body.transform.TransformDirection(velocity);
    }


    void VelocityDampening(){
        velocity = new Vector3(0, velocity.y, 0); //Maintain vertical velocity each frame but reset horizontal
        //In the future use the surface friction to set top speeds and sliding
    }
    void StickingForce(){
        if (isGrounded && velocity.y <= 0){
            velocity.y = gravity * groundDistance * Mathf.Sign(groundDistance); // Small value to stick the player to the ground scales with distance to the ground, needs to maintain sign so is x^3, could use sign(x)*x^2
        }
    }
    void Movement(){
        // Get input for movement
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Calculate the movement direction relative to the camera
        Vector3 camera_relative = ((vertical * cameraTransform.forward) + (horizontal * cameraTransform.right)).normalized;
        if (camera_relative.magnitude >= 0.1f){
            //Project the moveDirection from the camera's coordinates onto the players coodrinates by subtracting the vertical component
            Vector3 moveDirection = camera_relative - Vector3.Dot(camera_relative, body.transform.up) * body.transform.up;  //Vector is in global coordinates
            Vector3 localDirection = body.transform.InverseTransformDirection(moveDirection) * moveSpeed; //Make it local
            
            //ADDS to velocity
            if (timer < coyoteTime){
                VelocityDampening();//can only dampen when on the ground, maybe use rigidbody drag instead?
                velocity =  new Vector3(localDirection.x, velocity.y, localDirection.z);
                // LERPS forward axis to movement axis
                if (true){//localDirection.magnitude > 1f){ //IDK if this check is necessary, but if velocity is small, dont rotate
                    float angle = Vector3.SignedAngle(body.transform.forward, moveDirection, body.transform.up);
                    angle = Mathf.LerpAngle(0, angle, rotationSpeed * Time.fixedDeltaTime);
                    body.MoveRotation(Quaternion.AngleAxis(angle, body.transform.up) * body.rotation);   
                }
            }else{
                //When in the air use acceleration instead of instant change in velocity
                Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
                horizontalVelocity = Vector3.ClampMagnitude( horizontalVelocity + (localDirection * aerialSpeedCoefficient), moveSpeed);
                velocity = horizontalVelocity +  new Vector3(0, velocity.y, 0);
                // LERPS forward axis to movement axis
                if (horizontalVelocity.magnitude > 1f){
                    float angle = Vector3.SignedAngle(body.transform.forward, moveDirection, body.transform.up);
                    angle = Mathf.LerpAngle(0, angle, rotationSpeed * aerialSpeedCoefficient * Time.fixedDeltaTime);
                    body.MoveRotation(Quaternion.AngleAxis(angle, body.transform.up) * body.rotation);   
                }
            }

        } else{
            VelocityDampening();
        }
    }
    
    void Jump(){
        if ((isGrounded || timer < coyoteTime) && Input.GetButtonDown("Jump")) {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }

   //Variables to determine how the player treats rotated orientations
    public float coyoteOrientationTime = 2f; //How long before rotating to global up
    public float coyoteTime = 0.1f; //How long in the air before reducing movement speed and ability to jump
    public float aerialSpeedCoefficient = 0.1f;
    public float aerialTimeLimit = 6f; //How long in the air before 'dying'
    private float timer = 0f;
    void Fall(){
        if (!isGrounded){
            RaycastHit hitInfo;
            //Check if we are balancing ontop of something that isnt terraint
            if (Physics.Raycast(groundCheck.position,  -body.transform.up, out hitInfo, checkDistance)
                    && hitInfo.distance - feetDistance < groundedDistance){
                isGrounded = true;
                groundDistance = hitInfo.distance - feetDistance;
            }else{
                timer += Time.fixedDeltaTime;
                velocity.y += gravity * Time.fixedDeltaTime;
                if (timer > aerialTimeLimit){
                    //Do on death stuff, reset scene and respawn
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                } else if (timer > coyoteOrientationTime){
                    // Align the character's "up" direction with the global up direction
                    Quaternion targetRotation = Quaternion.FromToRotation(body.transform.up, new Vector3(0,1,0)) * body.rotation;
                    // Smoothly rotate the character towards the new orientation
                    body.MoveRotation(Quaternion.Slerp(body.rotation, targetRotation, orientationSpeed * Time.fixedDeltaTime));
                } else {
                    // Align the character's "up" direction with the terrain's surface normal
                    Quaternion targetRotation = Quaternion.FromToRotation(body.transform.up, groundNormal) * body.rotation;
                    // Smoothly rotate the character towards the new orientation
                    body.MoveRotation(Quaternion.Slerp(body.rotation, targetRotation, orientationSpeed * Time.fixedDeltaTime));
                }
            }
        } else{
            timer = 0;
        }
    }
}