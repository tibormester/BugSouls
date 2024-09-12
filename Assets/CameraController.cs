using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Variables for camera movement and control
    public Transform target; // The character transform to follow
    public float distance = 5.0f; // Distance between the camera and the target
    public float height = 2.0f; // Height above the character
    public float rotationSpeed = 5.0f; // Speed of camera rotation based on mouse movement
    public float minYAngle = -40f; // Minimum vertical angle for camera rotation
    public float maxYAngle = 80f; // Maximum vertical angle for camera rotation

    private float currentYaw = 0f; // Current horizontal angle (yaw)
    private float currentPitch = 0f; // Current vertical angle (pitch)

    // Sensitivity for mouse movement
    public float mouseSensitivityX = 100f;
    public float mouseSensitivityY = 80f;

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
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);

        // Calculate the new camera position behind and above the target
        Vector3 offset = new Vector3(0f, height, -distance);
        Vector3 newPosition = target.position + rotation * offset;

        // Set the camera position and rotation
        transform.position = newPosition;
        transform.LookAt(target.position + Vector3.up * height); // Look slightly above the target
    }
}
