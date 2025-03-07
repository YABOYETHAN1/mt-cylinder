using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offsetPosition = new Vector3(0, 2, -5);
    
    [Header("Camera Controls")]
    [SerializeField] private float mouseSensitivity = 3f;
    [SerializeField] private float distanceFromTarget = 5f;
    [SerializeField] private float smoothTime = 0.2f;
    [SerializeField] private Vector2 rotationXMinMax = new Vector2(-40, 70);

    // Camera variables
    private float rotationX;
    private float rotationY;
    private Vector3 currentRotation;
    private Vector3 smoothVelocity = Vector3.zero;

    private void Start()
    {
        // If no target specified, use the player
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
        
        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Adjust rotation based on input
        rotationY += mouseX;
        rotationX -= mouseY;
        
        // Clamp vertical rotation
        rotationX = Mathf.Clamp(rotationX, rotationXMinMax.x, rotationXMinMax.y);

        // Calculate rotation
        Vector3 nextRotation = new Vector3(rotationX, rotationY);
        
        // Apply smoothing to rotation
        currentRotation = Vector3.SmoothDamp(currentRotation, nextRotation, ref smoothVelocity, smoothTime);
        
        // Convert angles to quaternion
        Quaternion rotation = Quaternion.Euler(currentRotation.x, currentRotation.y, 0);
        
        // Calculate camera position
        Vector3 position = target.position - (rotation * Vector3.forward * distanceFromTarget) + (Vector3.up * offsetPosition.y);
        
        // Apply rotation and position
        transform.rotation = rotation;
        transform.position = position;
    }
}