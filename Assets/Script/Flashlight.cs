using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    [Header("Light Settings")]
    [SerializeField] private Light flashlight;
    [SerializeField] private bool isOn = false;

    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip toggleSound;

    [Header("Delayed Movement Settings")]
    [SerializeField] private float movementDelay = 0.5f;
    [SerializeField] private float rotationSmoothing = 3.0f;
    
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    
    private Quaternion targetRotation;
    private Vector3 previousPlayerForward;

    private void Start()
    {
        // Set component references
        if (flashlight == null)
            flashlight = GetComponentInChildren<Light>();
        
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
            
        // Find player transform if not assigned
        if (playerTransform == null)
            playerTransform = transform.parent;
            
        // Initialize state
        flashlight.enabled = isOn;
        
        // Set initial forward direction
        if (playerTransform != null)
            previousPlayerForward = playerTransform.forward;
            
        targetRotation = transform.rotation;
    }

    private void Update()
    {
        // Toggle flashlight with F key
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleFlashlight();
        }
        
        // Update flashlight direction with delay based on player's body rotation
        if (playerTransform != null)
        {
            // Get the current player forward direction
            Vector3 currentPlayerForward = playerTransform.forward;
            
            // Calculate the target rotation based on player's forward direction
            // We use Lerp on the previousPlayerForward to create the delay effect
            previousPlayerForward = Vector3.Lerp(previousPlayerForward, currentPlayerForward, Time.deltaTime / movementDelay);
            
            // Create a rotation that points in the direction of the delayed forward vector
            targetRotation = Quaternion.LookRotation(previousPlayerForward);
            
            // Apply rotation with smoothing
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothing);
        }
    }
    
    private void ToggleFlashlight()
    {
        // Toggle state immediately
        isOn = !isOn;
        flashlight.enabled = isOn;
        
        // Play sound
        if (audioSource != null && toggleSound != null)
        {
            audioSource.PlayOneShot(toggleSound);
        }
    }
}