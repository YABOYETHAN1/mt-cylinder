using UnityEngine;

/// <summary>
/// Simplified third-person movement script for basic character movement in the game.
/// Requires the Player tag and Character Controller component.
/// </summary>
public class ThirdPersonController : MonoBehaviour
{
    [Tooltip("Speed at which the character moves.")]
    public float moveSpeed = 5f;
    [Tooltip("Force that pulls the player down.")]
    public float gravity = 9.8f;

    // Inputs
    float inputHorizontal;
    float inputVertical;

    Animator animator;
    CharacterController cc;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        if (animator == null)
            Debug.LogWarning("Hey buddy, you don't have the Animator component in your player. Without it, the animations won't work.");
    }

    void Update()
    {
        // Input checkers
        inputHorizontal = Input.GetAxis("Horizontal");
        inputVertical = Input.GetAxis("Vertical");

        // Animation control - add simple movement animation
        if (animator != null)
        {
            // Play movement animation based on speed
            bool isMoving = Mathf.Abs(inputHorizontal) > 0.1f || Mathf.Abs(inputVertical) > 0.1f;
            animator.SetBool("run", isMoving);
            
            // Set ground state for animation
            animator.SetBool("air", !cc.isGrounded);
        }
    }

    private void FixedUpdate()
    {
        // Direction movement
        float directionX = inputHorizontal * moveSpeed * Time.deltaTime;
        float directionZ = inputVertical * moveSpeed * Time.deltaTime;
        float directionY = -gravity * Time.deltaTime; // Just apply gravity

        // --- Character rotation --- 
        Vector3 forward = Camera.main.transform.forward;
        Vector3 right = Camera.main.transform.right;

        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        // Relate the front with the Z direction (depth) and right with X (lateral movement)
        forward = forward * directionZ;
        right = right * directionX;

        if (directionX != 0 || directionZ != 0)
        {
            float angle = Mathf.Atan2(forward.x + right.x, forward.z + right.z) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.15f);
        }

        // --- End rotation ---
        Vector3 verticalDirection = Vector3.up * directionY;
        Vector3 horizontalDirection = forward + right;

        Vector3 movement = verticalDirection + horizontalDirection;
        cc.Move(movement);
    }
}