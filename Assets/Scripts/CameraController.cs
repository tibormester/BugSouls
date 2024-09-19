using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Variables for camera movement and control
    public Transform target; // The character transform to follow
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

    void LateUpdate()
    {
        if (target == null)
        {
            return;
        }

        // Get mouse input for rotation
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivityX * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivityY * Time.deltaTime;

        // Update the yaw (horizontal rotation) based on mouse movement
        currentYaw += mouseX;

        // Update the pitch (vertical rotation) and clamp it between min and max values
        currentPitch -= mouseY;
        currentPitch = Mathf.Clamp(currentPitch, minYAngle, maxYAngle);

        // Rotate the camera around the target based on yaw (horizontal rotation)
        //Quaternion refrenceRotation = Quaternion.LookRotation(Vector3.forward, target.transform.up);
        Quaternion refrenceRotation = Quaternion.FromToRotation(Vector3.up, target.transform.up);
        //Quaternion refrenceRotation = target.transform.rotation;
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);

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
}
