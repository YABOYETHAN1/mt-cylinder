using UnityEngine;
public class PlayerController : MonoBehaviour {
    // bunch of variables
    public float speed = 6.0F;
    public float jumpSpeed = 8.0F;
    public float gravity = 20.0F;
    private Vector3 moveDirection = Vector3.zero;
    public bool canJump = true;
    private CharacterController controller;
    public float y_vel;
    private bool isMoving = false;
    bool grounded;
    float x, z; //movement vars
    public GameObject CamObj;
    private float sprint = 1;
    
    void Start() {
        controller = gameObject.GetComponent<CharacterController>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        if(!CamObj) Debug.LogError("no camera!!!!");
    }

    void Update() {
        // check if we're on ground - super simple
        grounded = controller.isGrounded;
        
        // get inputs - messy but works
        x = Input.GetAxis("Horizontal");
        z = Input.GetAxis("Vertical");
        
        if(Input.GetKey(KeyCode.LeftShift)) sprint = 2; else sprint = 1; // sprint multiplier
        
        // movement direction based on camera
        var forward = CamObj.transform.forward;
        var right = CamObj.transform.right;
        forward.y = 0;
        right.y = 0;
        forward = forward.normalized;
        right = right.normalized;
        
        // calculate direction - not the cleanest way but whatever
        moveDirection = (forward * z + right * x);
        
        if(grounded) {
            moveDirection *= speed * sprint;
            if(Input.GetButton("Jump")) {
                y_vel = jumpSpeed;  // jump!
            }
        }
        
        // gravity stuff
        y_vel -= gravity * Time.deltaTime;
        moveDirection.y = y_vel;
        
        // finally move the player
        controller.Move(moveDirection * Time.deltaTime);
        
        // face movement direction - kinda jerky but works
        if(moveDirection.x != 0 || moveDirection.z != 0) {
            transform.forward = new Vector3(moveDirection.x, 0, moveDirection.z);
        }
        
        // reset stuff
        if(grounded && moveDirection.y < 0) y_vel = -1f;
    }

    // not even used but left here anyway
    void DoStuff() {
        Debug.Log("nothing here");
    }
    
    // messy debug stuff left in
    void OnGUI() {
        //GUI.Label(new Rect(0,0,100,20), "Speed: " + moveDirection.magnitude);
        //GUI.Label(new Rect(0,20,100,20), "Grounded: " + grounded);
    }
}