using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Variables for camera movement and control
    public LayerMask ignorePlayerLayerMask; // Assign a layer mask to ignore the player's layer

    public Transform target; // The character transform to follow
    private CharacterMovement targetMovement; //The target's movement script to call movement functions on
    public float distance = 5.0f; // Distance between the camera and the target
    public float height = 2.0f; // Height above the character
    public float rotationSpeed = 100.0f; // Speed the camera attempts to match the pitch and yaw
    public float normalRotationSpeed = 2f; // Speed the camera attempts to match the pitch and yaw
    public float cameraSpeed = 45f;
    public float minYAngle = -75f; // Minimum vertical angle for camera rotation
    public float maxYAngle = 80f; // Maximum vertical angle for camera rotation

    private float currentYaw = 0f; // Current horizontal angle (yaw)
    private float currentPitch = 35f; // Current vertical angle (pitch)

    // Sensitivity for mouse movement
    public float mouseSensitivityX = 2560f;
    public float mouseSensitivityY = 2560f;

    private Vector3 front;
    private Vector3 prevNormal;

    private Camera cam;

    public float sprintFactor = 2.5f;
    private float runSpeed;
    void Start(){
        front = target.transform.forward;
        targetMovement = target.gameObject.GetComponent<CharacterMovement>();
        cam = GetComponent<Camera>();
        Cursor.lockState = CursorLockMode.Locked;// Lock and hide Cursor
        prevNormal = targetMovement.physicsBody.groundNormal;
        runSpeed = targetMovement.moveSpeed;
    }
    public LayerMask throwable;
    public LayerMask terrain;
    public Weapon weapon;
    private bool sprinting = false;
    void Update(){
        Ray ray = cam.ScreenPointToRay(Input.mousePosition); // Cast ray from camera, can also use cam.transform.forward for a different direction
        if(!sprinting){
            targetMovement.look_direction = ray.direction;
        }
        //Change swinging a sword to mouse button 0 and throwing gets moved to the grapple script

        
    }

    void LateUpdate(){
        MouseInputs();
        Movement();
        if(Input.GetButtonDown("Jump")){
            targetMovement.Jump();
        }
        if(Input.GetKey(KeyCode.LeftShift)){
            sprinting = true;
        } else{
            sprinting = false;
        }
    }
    void MouseInputs(){
        // Get mouse input for rotation
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivityX * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivityY * Time.deltaTime;

        // Update the yaw (horizontal rotation) based on mouse movement
        currentYaw += mouseX;

        // Update the pitch (vertical rotation) and clamp it between min and max values
        currentPitch -= mouseY;
        currentPitch = Mathf.Clamp(currentPitch, minYAngle, maxYAngle);

        //Set the prev normal to be normalRotationSpeed%/second towards ground normal each frame
        prevNormal = Vector3.Slerp(prevNormal, targetMovement.physicsBody.groundNormal, normalRotationSpeed * Time.deltaTime);


        // Rotate the camera around the target based on yaw (horizontal rotation)
        Vector3 right = Vector3.Cross(front, prevNormal).normalized; //Use the character's up and the previous forward to get the right angle
        if(right == Vector3.zero){
            right = Vector3.Cross(front, target.transform.up).normalized;
        }
        front = -Vector3.Cross(right, prevNormal).normalized; //set the forward based on how the up has changed
        Quaternion refrenceRotation = Quaternion.LookRotation(front, prevNormal); //get the orientation based on previous forward and up


        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f); //get mouse pitch and raw quaterion

        // Calculate the new camera position behind and above the target
        Quaternion cameraTargetRotation = refrenceRotation * rotation;
        Vector3 offset = new Vector3(0f, height, -distance);

        // Set the camera position
        //transform.position = Vector3.Slerp(transform.position, target.position + cameraTargetRotation * offset, cameraSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, cameraTargetRotation, rotationSpeed * Time.deltaTime);

        RaycastHit hitinfo; 
        if(Physics.Raycast(target.position, transform.rotation*offset, out hitinfo, offset.magnitude)){
            transform.position = target.position + transform.rotation * (offset.normalized * (hitinfo.distance - 0.1f));
        }else{
            transform.position = target.position + transform.rotation * offset;
        }

        
        // LERPS to the target rotation
        
        //transform.rotation = cameraTargetRotation;
    }



    void Movement(){
        // Get input for movement, use raw so that there is no input delay with switching directions
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        // Calculate the movement direction relative to the camera
        Vector3 camera_relative = ((vertical * transform.forward) + (horizontal * transform.right)).normalized;
        if(sprinting){
            targetMovement.look_direction = camera_relative;
            targetMovement.moveSpeed = sprintFactor * runSpeed;
        } else{
            targetMovement.moveSpeed = runSpeed;
        }
        if (moved || camera_relative.sqrMagnitude < 0.1f){//Some script so that the movement gets accumulated until the fixed update frame, but if the movement is stopped, cancel the accumulation
            moved = false;
            accum = Vector3.zero;
        }
        accum += camera_relative;
    }

    private bool moved = false;
    private Vector3 accum = Vector3.zero;
    void FixedUpdate(){
        moved = true;
        targetMovement.Move(accum);
    }
}
