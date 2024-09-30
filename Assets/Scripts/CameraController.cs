using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Variables for camera movement and control
    public Transform target; // The character transform to follow
    private CharacterMovement targetMovement; //The target's movement script to call movement functions on
    public float distance = 5.0f; // Distance between the camera and the target
    public float height = 2.0f; // Height above the character
    public float rotationSpeed = 100.0f; // Speed the camera attempts to match the pitch and yaw
    public float cameraSpeed = 45f;
    public float minYAngle = -75f; // Minimum vertical angle for camera rotation
    public float maxYAngle = 80f; // Maximum vertical angle for camera rotation

    private float currentYaw = 0f; // Current horizontal angle (yaw)
    private float currentPitch = 35f; // Current vertical angle (pitch)

    // Sensitivity for mouse movement
    public float mouseSensitivityX = 2560f;
    public float mouseSensitivityY = 2560f;

    private Vector3 front;


    void Start(){
        front = target.transform.forward;
        targetMovement = target.gameObject.GetComponent<CharacterMovement>();
        Cursor.lockState = CursorLockMode.Locked;// Lock and hide Cursor
    }
    void LateUpdate(){
        
        MouseInputs();
        Movement();
        if(Input.GetButtonDown("Jump")){
            targetMovement.Jump();
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

        // Rotate the camera around the target based on yaw (horizontal rotation)
        Vector3 right = Vector3.Cross(front, target.transform.up).normalized; //Use the character's up and the previous forward to get the right angle
        front = -Vector3.Cross(right, target.transform.up).normalized; //set the forward based on how the up has changed
        Quaternion refrenceRotation = Quaternion.LookRotation(front, target.transform.up); //get the orientation based on previous forward and up


        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f); //get mouse pitch and raw quaterion

        // Calculate the new camera position behind and above the target
        Quaternion cameraTargetRotation = refrenceRotation * rotation;
        Vector3 offset = new Vector3(0f, height, -distance);

        // Set the camera position
        //transform.position = Vector3.Slerp(transform.position, target.position + cameraTargetRotation * offset, cameraSpeed * Time.deltaTime);
        transform.position = target.position + cameraTargetRotation * offset;
        // LERPS to the target rotation
        //transform.rotation = Quaternion.Slerp(transform.rotation, cameraTargetRotation, rotationSpeed * Time.deltaTime);
        transform.rotation = cameraTargetRotation;
    }



    void Movement(){
        // Get input for movement
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        // Calculate the movement direction relative to the camera
        Vector3 camera_relative = ((vertical * transform.forward) + (horizontal * transform.right)).normalized;

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
