using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class StandardThirdPersonController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 2.0f;
    [SerializeField] private float runSpeed = 6.0f;
    [SerializeField] private float jumpHeight = 1.0f;
    [SerializeField] private float turnSmoothTime = 0.1f;
    [SerializeField] private float speedSmoothTime = 0.1f;
    [SerializeField] private float gravity = -9.81f;
    
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    // Private variables
    private CharacterController controller;
    private Transform cameraTransform;
    private float turnSmoothVelocity;
    private float speedSmoothVelocity;
    private float currentSpeed;
    private Vector3 velocity;
    private bool isGrounded;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        cameraTransform = Camera.main.transform;
        
        // Hide cursor for game
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        // Ground check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // Reset vertical velocity when grounded
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Small negative value to ensure grounding
        }
        
        // Get input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        bool isRunning = Input.GetKey(KeyCode.LeftShift);
        
        // Calculate input direction
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        
        // Move the player
        if (direction.magnitude >= 0.1f)
        {
            // Calculate target angle based on input direction
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            
            // Smooth rotation
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            
            // Get forward direction based on camera
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            
            // Calculate target speed
            float targetSpeed = isRunning ? runSpeed : walkSpeed;
            
            // Smooth speed changes
            currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, speedSmoothTime);
            
            // Move the character
            controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime);
        }
        else
        {
            // Gradually slow down when no input
            currentSpeed = Mathf.SmoothDamp(currentSpeed, 0, ref speedSmoothVelocity, speedSmoothTime);
        }
        
        // Handle jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        
        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        
        // Apply vertical movement
        controller.Move(velocity * Time.deltaTime);
    }
}