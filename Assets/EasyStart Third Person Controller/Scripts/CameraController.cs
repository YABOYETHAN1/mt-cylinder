using UnityEngine;
using System.Collections;

/// <summary>
/// Camera movement script for third person games with immediate movement, collision detection,
/// idle state behavior, and directional following.
/// This Script should not be applied to the camera! It is attached to an empty object and inside
/// it (as a child object) should be your game's MainCamera.
/// </summary>
public class CameraController : MonoBehaviour
{
    [Tooltip("Enable to move the camera by holding the right mouse button. Does not work with joysticks.")]
    public bool clickToMoveCamera = false;
    [Tooltip("Enable zoom in/out when scrolling the mouse wheel. Does not work with joysticks.")]
    public bool canZoom = true;
    [Space]
    [Tooltip("The higher it is, the faster the camera moves. It is recommended to increase this value for games that uses joystick.")]
    public float sensitivity = 5f;
    [Tooltip("Camera Y rotation limits. The X axis is the maximum it can go up and the Y axis is the maximum it can go down.")]
    public Vector2 cameraLimit = new Vector2(-45, 40);
    [Space]
    [Tooltip("Distance to keep camera from walls")]
    public float collisionOffset = 0.2f;
    [Space]
    [Tooltip("Enable camera to return to behind player when idle")]
    public bool returnToIdlePosition = true;
    [Tooltip("Time in seconds before camera returns to idle position")]
    public Vector2 idleTimeRange = new Vector2(3f, 5f);
    [Tooltip("How fast camera returns to idle position (higher = faster)")]
    public float returnSpeed = 2f;
    [Tooltip("How much the camera follows player movement direction (0 = none, 1 = full)")]
    public float directionFollowFactor = 0.5f;
    [Tooltip("Toggle camera between left and right shoulder")]
    public KeyCode switchShoulderKey = KeyCode.H;
    [Space]
    [Tooltip("Minimum zoom level (FOV)")]
    public float minZoom = 35f;
    [Tooltip("Maximum zoom level (FOV)")]
    public float maxZoom = 50f;
    [Tooltip("Default zoom level (FOV)")]
    public float defaultZoom = 40f;

    float mouseX;
    float mouseY;
    float offsetDistanceY;
    float lastInputTime;
    bool returningToIdle = false;
    float idleTimer = 0f;
    float timeUntilIdle;
    Vector3 lastPlayerPosition;
    Quaternion idleRotation;
    
    private Transform cameraTransform;
    private Vector3 rightShoulderPosition = new Vector3(0.8f, 0.6f, -2.0f);
private Vector3 leftShoulderPosition = new Vector3(-0.8f, 0.6f, -2.0f);
private Vector3 defaultCameraLocalPosition;
private Vector3 targetCameraLocalPosition;
private bool isRightShoulder = true;
private float shoulderSwitchSpeed = 5f;
private float shoulderSwitchCooldown = 1f;
private float lastSwitchTime;
    private float originalRotationY;

    Transform player;
    Rigidbody playerRigidbody;

    void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
        playerRigidbody = player.GetComponent<Rigidbody>();
        
        offsetDistanceY = transform.position.y;
        cameraTransform = Camera.main.transform;
        defaultCameraLocalPosition = rightShoulderPosition;
        targetCameraLocalPosition = defaultCameraLocalPosition;
        cameraTransform.localPosition = defaultCameraLocalPosition;
        lastSwitchTime = -shoulderSwitchCooldown; // Allow immediate first switch
        lastPlayerPosition = player.position;
        
        // Set default FOV
        Camera.main.fieldOfView = defaultZoom;
        
        // Store original rotation
        originalRotationY = transform.eulerAngles.y;
        idleRotation = Quaternion.Euler(0, originalRotationY, 0);

        // Generate first random idle time
        timeUntilIdle = Random.Range(idleTimeRange.x, idleTimeRange.y);

        // Lock and hide cursor if option isn't checked
        if (!clickToMoveCamera)
        {
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.visible = false;
        }
    }

    void LateUpdate()
    {
        // Follow player position - camera offset
        transform.position = player.position + new Vector3(0, offsetDistanceY, 0);
        
        // Smoothly move camera between shoulder positions
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, targetCameraLocalPosition, shoulderSwitchSpeed * Time.deltaTime);
        
        // Detect player movement direction
        Vector3 playerMovement = player.position - lastPlayerPosition;
        lastPlayerPosition = player.position;
        
        // Handle mouse input for camera rotation
        bool hasInput = HandleMouseInput();
        
        // Handle idle behavior
        if (returnToIdlePosition)
        {
            if (hasInput || playerMovement.magnitude > 0.01f)
            {
                // Reset idle timer on input
                idleTimer = 0;
                returningToIdle = false;
                lastInputTime = Time.time;
            }
            else
            {
                // Increment idle timer
                idleTimer += Time.deltaTime;
                
                // Check if we should start returning to idle
                if (idleTimer >= timeUntilIdle && !returningToIdle)
                {
                    returningToIdle = true;
                    
                    // Update idle rotation to face the direction player is facing
                    if (playerRigidbody != null && playerRigidbody.velocity.magnitude > 0.1f)
                    {
                        Vector3 playerForward = playerRigidbody.velocity.normalized;
                        if (playerForward != Vector3.zero)
                        {
                            idleRotation = Quaternion.LookRotation(new Vector3(playerForward.x, 0, playerForward.z));
                        }
                    }
                    else
                    {
                        // Use player's forward direction when not moving
                        idleRotation = Quaternion.Euler(0, player.eulerAngles.y, 0);
                    }
                }
                
                // Return to idle position if needed
                if (returningToIdle)
                {
                    // Smoothly rotate toward idle position
                    transform.rotation = Quaternion.Slerp(transform.rotation, idleRotation, returnSpeed * Time.deltaTime);
                    
                    // If we're close enough to idle rotation, pick a new idle time
                    if (Quaternion.Angle(transform.rotation, idleRotation) < 1f)
                    {
                        timeUntilIdle = Random.Range(idleTimeRange.x, idleTimeRange.y);
                        idleTimer = 0;
                        returningToIdle = false;
                    }
                }
            }
        }
        
        // Follow player direction
        if (directionFollowFactor > 0 && playerRigidbody != null && playerRigidbody.velocity.magnitude > 0.5f)
        {
            Vector3 playerForward = playerRigidbody.velocity.normalized;
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(playerForward.x, 0, playerForward.z));
            
            // Only affect the camera's Y rotation for direction following
            float currentYRotation = transform.eulerAngles.y;
            float targetYRotation = targetRotation.eulerAngles.y;
            
            // Blend between current and target rotation based on directionFollowFactor
            float newYRotation = Mathf.LerpAngle(currentYRotation, targetYRotation, directionFollowFactor * Time.deltaTime);
            
            // Only apply direction following if not returning to idle
            if (!returningToIdle)
            {
                // Adjust only the Y component of mouseX to apply direction following
                mouseX += (newYRotation - currentYRotation) * 0.1f;
                transform.rotation = Quaternion.Euler(-mouseY, mouseX, 0);
            }
        }
        
        // Handle camera collision detection
        HandleCameraCollision();
    }
    
    bool HandleMouseInput()
    {
        bool hasInput = false;
        
        // Handle shoulder switch with cooldown
        if (Input.GetKeyDown(switchShoulderKey) && Time.time >= lastSwitchTime + shoulderSwitchCooldown)
        {
            isRightShoulder = !isRightShoulder;
            targetCameraLocalPosition = isRightShoulder ? rightShoulderPosition : leftShoulderPosition;
            lastSwitchTime = Time.time;
            hasInput = true;
        }
        
        // Set camera zoom when mouse wheel is scrolled
        if (canZoom && Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            float newFOV = Camera.main.fieldOfView - Input.GetAxis("Mouse ScrollWheel") * sensitivity * 10;
            Camera.main.fieldOfView = Mathf.Clamp(newFOV, minZoom, maxZoom);
            hasInput = true;
        }

        // Check for right click to move camera if enabled
        if (clickToMoveCamera && Input.GetAxisRaw("Fire2") == 0)
            return hasInput;
            
        // Get mouse input
        float mouseInputX = Input.GetAxis("Mouse X");
        float mouseInputY = Input.GetAxis("Mouse Y");
        
        if (Mathf.Abs(mouseInputX) > 0.01f || Mathf.Abs(mouseInputY) > 0.01f)
        {
            hasInput = true;
        
            // Calculate new rotation directly
            mouseX += mouseInputX * sensitivity;
            mouseY += mouseInputY * sensitivity;
            
            // Apply camera limits
            mouseY = Mathf.Clamp(mouseY, cameraLimit.x, cameraLimit.y);
            
            // Apply rotation immediately without smoothing
            transform.rotation = Quaternion.Euler(-mouseY, mouseX, 0);
        }
        
        return hasInput;
    }
    
    void HandleCameraCollision()
    {
        // Get the desired camera position in world space
        Vector3 desiredCameraPos = transform.TransformPoint(targetCameraLocalPosition);
        
        // Cast a ray from the pivot to the camera's desired position
        RaycastHit hit;
        if (Physics.Linecast(transform.position, desiredCameraPos, out hit))
        {
            // If we hit something, adjust the camera position to the hit point
            float distanceToHit = Vector3.Distance(transform.position, hit.point);
            Vector3 adjustedCameraPos = transform.position + 
                (desiredCameraPos - transform.position).normalized * 
                (distanceToHit - collisionOffset);
                
            // Set the camera's position
            cameraTransform.position = adjustedCameraPos;
        }
    }
}