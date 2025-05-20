using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public float speed = 12f; // Movement speed
    public float gravity = -9.81f; // Gravity strength
    public float jumpHeight = 3f; // Jump height

    public Transform groundCheck; // To check if the player is on the ground
    public LayerMask groundMask; // Which layer is considered the ground

    private float groundDistance = 0.4f; // Distance to check for ground
    private Vector3 velocity; // Current velocity

    private bool isGrounded;

    // Camera Rotation Variables
    public Transform playerBody; // Reference to the player body (for rotating the player)
    public float mouseSensitivity = 100f; // Mouse sensitivity
    public float xRotation = 0f; // Vertical rotation of the camera (up/down)

    void Start()
    {
        // Lock cursor to the center of the screen and hide it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Check if the player is grounded
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Stick player to ground
        }

        // Get input for movement (WASD or arrow keys)
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 direction = transform.right * horizontal + transform.forward * vertical;

        // Move the player
        controller.Move(direction * speed * Time.deltaTime);

        // Handle jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity); // Calculate jump force
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;

        // Apply gravity to the character controller
        controller.Move(velocity * Time.deltaTime);

        // Camera rotation (Horizontal)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotate the player (body) horizontally (left/right)
        playerBody.Rotate(Vector3.up * mouseX);

        // Rotate the camera vertically (up/down) with clamping
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // Prevent the camera from rotating too far up/down
        Camera.main.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
