using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform plyr;
    private float mouseSensitivity = 2;
    private float upDownRange = 90;
    private float verticalRotation;
    private float dist = 3;
    public Vector3 offsetfromplayer = new Vector3(0, 1, 0);
    private Vector3 currentVel = Vector3.zero;
    private bool smoothfollow = true;
    private float smoothtime = 0.1f;
    private float rotationspeed = 5;
    float rotationY;
    float x;
    float y;

    void Start()
    {
        if (!plyr) plyr = GameObject.FindGameObjectWithTag("Player").transform;
        verticalRotation = 0f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate()
    {
        if(!plyr) return;

        x = Input.GetAxis("Mouse X") * mouseSensitivity;
        y = Input.GetAxis("Mouse Y") * mouseSensitivity;

        verticalRotation -= y;
        verticalRotation = Mathf.Clamp(verticalRotation, -upDownRange, upDownRange);
        rotationY += x;

        var rotation = Quaternion.Euler(verticalRotation, rotationY, 0);
        var position = rotation * new Vector3(0, 0, -dist) + plyr.position + offsetfromplayer;

        if(smoothfollow)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationspeed);
            transform.position = Vector3.SmoothDamp(transform.position, position, ref currentVel, smoothtime);
        }
        else
        {
            transform.rotation = rotation;
            transform.position = position;
        }

        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.visible = !Cursor.visible;
            Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
        }

        if(Physics.Linecast(plyr.position + offsetfromplayer, transform.position))
        {
            transform.position = plyr.position + offsetfromplayer;
        }
    }
}