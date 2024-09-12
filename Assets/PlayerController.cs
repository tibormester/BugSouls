using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Movement variables
    public float moveSpeed = 5f;
    public float rotationSpeed = 720f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    // Ground detection
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;

    private Vector3 velocity;
    private bool isGrounded;

    // Camera and player orientation
    public Transform cameraTransform;

    // Components
    private CharacterController controller;

    void Start()
    {
        // Get the CharacterController component attached to the player
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Ground check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small value to stick the player to the ground
        }

        // Get input for movement
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Calculate the movement direction relative to the camera
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            // Calculate the target angle based on the camera direction
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;

            // Smoothly rotate the player towards the target direction
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationSpeed, 0.1f);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Move the player in the direction of the camera
            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        }

        // Jumping
        if (Input.GetButtonDown("Jump")){
            Jump();
        }
        Fall();

        controller.Move(velocity * Time.deltaTime);
    }

    void Jump(){
        if (isGrounded){
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
    }

    void Fall(){
         // Apply gravity
        if (! isGrounded){
            velocity.y += gravity * Time.deltaTime;
        }
    }

}