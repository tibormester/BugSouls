using System.Collections;
using System.Linq;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Variables for camera movement and control
    public LayerMask ignorePlayerLayerMask; // Assign a layer mask to ignore the player's layer
    public bool allowMovement = true;
    public Transform target; // The character transform to follow
    private CharacterMovement targetMovement; //The target's movement script to call movement functions on
    public float distance = 5.0f; // Distance between the camera and the target
    public float height = 2.0f; // Height above the character
    public float shoulder = 1f; //Offset from shoulder
    public float rotationSpeed = 100.0f; // Speed the camera attempts to match the pitch and yaw
    public float normalRotationSpeed = 2f; // Speed the camera attempts to match the pitch and yaw
    public float cameraSpeed = 45f;
    public float minYAngle = -75f; // Minimum vertical angle for camera rotation
    public float maxYAngle = 80f; // Maximum vertical angle for camera rotation
    public LayerMask cameraColiderIgnore;

    private float currentYaw = 0f; // Current horizontal angle (yaw)
    private float currentPitch = 35f; // Current vertical angle (pitch)

    // Sensitivity for mouse movement
    public float mouseSensitivityX = 2560f;
    public float mouseSensitivityY = 2560f;

    private Vector3 front;
    private Vector3 prevNormal;
    private Camera cam;

    private CharacterAnimation characterAnimation;

    public StaminaBar stamina;
    
    public float maxStamina = 50f;
    public float currStamina;

    void Start(){
        front = target.transform.forward;
        targetMovement = target.gameObject.GetComponent<CharacterMovement>();
        cam = GetComponent<Camera>();
        Cursor.lockState = CursorLockMode.Locked;// Lock and hide Cursor
        prevNormal = targetMovement.physicsBody.groundNormal;
        SceneDescriptor sd = gameObject.scene.GetRootGameObjects().Select(go => go.GetComponent<SceneDescriptor>()).FirstOrDefault(desc => desc != null);
        sd.PlayerEntered += RecievePlayer;
        //Ugly
        stamina = gameObject.scene.GetRootGameObjects().Select(go => go.GetComponent<Canvas>()).FirstOrDefault(desc => desc != null).GetComponentInChildren<StaminaBar>();
        stamina.setMaxStamina(maxStamina);
        currStamina = maxStamina;
        stamina.setStamina(maxStamina);

        if (target)
        {
            characterAnimation = target.GetComponent<CharacterAnimation>();
        }
    }

    private void RecievePlayer(Transform newTarget){
        target = newTarget;
        targetMovement = target.gameObject.GetComponent<CharacterMovement>();
        prevNormal = targetMovement.physicsBody.groundNormal;
        //Ensure u cant do stuff when u die
        target.GetComponent<Health>().DeathEvent += () => allowMovement = false;
        characterAnimation = target.GetComponent<CharacterAnimation>();
    }
    public LayerMask throwable;
    public LayerMask terrain;
    public Weapon weapon;
    void Update(){
        Ray ray = cam.ScreenPointToRay(Input.mousePosition); // Cast ray from camera, can also use cam.transform.forward for a different direction
        if(!targetMovement.sprinting){
            targetMovement.look_direction = ray.direction;
        }
        //Change swinging a sword to mouse button 0 and throwing gets moved to the grapple script
    }
    void LateUpdate(){
        MouseInputs();
        if (allowMovement){
            Movement();
            if(Input.GetButtonDown("Jump") && currStamina >= 5){
                targetMovement.Jump();
                currStamina -= 5f;
            } else{
                characterAnimation.jumping = false;
            }
        }
        //Stamina regen
        if(currStamina <= maxStamina){
            currStamina += 2.5f * Time.deltaTime;
            stamina.setStamina(currStamina);
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
        Vector3 offset = new Vector3(shoulder, height, -distance);

        // Set the camera position
        //transform.position = Vector3.Slerp(transform.position, target.position + cameraTargetRotation * offset, cameraSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, cameraTargetRotation, rotationSpeed * Time.deltaTime);

        RaycastHit hitinfo; 
        if(Physics.Raycast(target.position, transform.rotation*offset, out hitinfo, offset.magnitude, ~cameraColiderIgnore))
        {
            transform.position = target.position + transform.rotation * (offset.normalized * (hitinfo.distance - 1f));
        }else{
            transform.position = target.position + transform.rotation * offset;
        }

        
        // LERPS to the target rotation
        
        //transform.rotation = cameraTargetRotation;
    }


    private KeyCode lastMove;
    private int doubleTapTimer = 0;
    public int doubleTapLimit = 12;
    public float dashCooldown = 0.25f; //Since we have stamina shorten the cd
    public float dashStrength = 2f;
    void Movement(){
        // Get input for movement, use raw so that there is no input delay with switching directions
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        // Calculate the movement direction relative to the camera
        Vector3 camera_relative = ((vertical * transform.forward) + (horizontal * transform.right)).normalized;
        if(Input.GetKey(KeyCode.LeftShift)){
            targetMovement.look_direction = camera_relative;
            targetMovement.sprinting = true;
            //Sprint held and movement input given
            if(camera_relative.sqrMagnitude > 0.1f){
                currStamina -= 5f * Time.deltaTime;
                if(currStamina <= 0){
                    currStamina = 0;
                    targetMovement.sprinting = false;
                }
            }
        } else{
            targetMovement.sprinting = false;
        }
        if (moved || camera_relative.sqrMagnitude < 0.1f){//Some script so that the movement gets accumulated until the fixed update frame, but if the movement is stopped, cancel the accumulation
            moved = false;
            accum = Vector3.zero;
        }
        accum += camera_relative;

        doubleTapTimer = doubleTapTimer < 0 ? 0 : doubleTapTimer - 1; 
        if(canDash && currStamina >= 3f){
            if (Input.GetKeyDown(KeyCode.S)){
                if(lastMove == KeyCode.S && doubleTapTimer > 0){
                    targetMovement.StartCoroutine(Dash(target.transform.up + -9 * target.transform.forward));
                }
                doubleTapTimer = doubleTapLimit;
                lastMove = KeyCode.S;
            } else if (Input.GetKeyDown(KeyCode.A)){
                if(lastMove == KeyCode.A && doubleTapTimer > 0){
                    targetMovement.StartCoroutine(Dash(target.transform.up + -9 * target.transform.right));
                }
                doubleTapTimer = doubleTapLimit;
                lastMove = KeyCode.A;
            } else if (Input.GetKeyDown(KeyCode.D)){
                if(lastMove == KeyCode.D && doubleTapTimer > 0){
                    targetMovement.StartCoroutine(Dash(target.transform.up + 9 * target.transform.right));
                }
                doubleTapTimer = doubleTapLimit;
                lastMove = KeyCode.D;
            } else if (Input.GetKeyDown(KeyCode.W)){
                if(lastMove == KeyCode.W && doubleTapTimer > 0){
                    targetMovement.StartCoroutine(Dash(target.transform.up + 9 * target.transform.forward));
                }
                doubleTapTimer = doubleTapLimit;
                lastMove = KeyCode.W;
            }
        }
        
    }
    public bool canDash = true;
    public IEnumerator Dash(Vector3 direction){
        //Lower stamina
        currStamina -= 3f;
        if(currStamina <= 0) currStamina = 0;

        int dashLength = 15;
        float dashMultiplier = 1f;
        if(! targetMovement.physicsBody.IsGrounded()){
            //When in the air increase dash speed and length
            dashLength = 25;
            dashMultiplier = 1.5f;
        }
        var wait = new WaitForFixedUpdate();
        doubleTapTimer = 0;
        canDash = false;
        var rb = targetMovement.GetComponent<Rigidbody>();
        for (int i = 0; i < 15; i ++){
            rb.AddForce(direction.normalized * dashStrength * dashMultiplier, ForceMode.Impulse);
            yield return wait;
        }
        
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private bool moved = false;
    private Vector3 accum = Vector3.zero;
    void FixedUpdate(){
        moved = true;
        targetMovement.Move(accum);
    }
}
