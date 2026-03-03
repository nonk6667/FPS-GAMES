using UnityEngine;

public class FPSController : MonoBehaviour
{
    public Transform cameraPivot;
    public Camera playerCamera;

    public float mouseSensitivity = 2f;

    public float walkSpeed = 6f;
    public float crouchSpeed = 3f;

    public float gravity = -20f;
    public float jumpHeight = 1.2f;

    public float standHeight = 1.8f;
    public float crouchHeight = 1.0f;

    public float standCameraY = 1.6f;
    public float crouchCameraY = 1.0f;
    public float cameraLerpSpeed = 12f;

    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.12f;

    private CharacterController controller;
    private Vector3 velocity;

    private float xRotation = 0f;
    private float currentSpeed;

    private float coyoteTimer;
    private float jumpBufferTimer;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        controller.height = standHeight;
        controller.center = new Vector3(0f, standHeight / 2f, 0f);

        currentSpeed = walkSpeed;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        MouseLook();

        UpdateTimers();
        Crouch();
        Move();
        JumpAndGravity();
        UpdateCameraHeight();
    }

    void MouseLook()
    {
    float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
    float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

    xRotation -= mouseY;
    xRotation = Mathf.Clamp(xRotation, -89f, 89f);

    cameraPivot.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    transform.Rotate(Vector3.up * mouseX);
}
    void UpdateTimers()
    {
        if (controller.isGrounded) coyoteTimer = coyoteTime;
        else coyoteTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space)) jumpBufferTimer = jumpBufferTime;
        else jumpBufferTimer -= Time.deltaTime;
    }

    void Move()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move.normalized * currentSpeed * Time.deltaTime);
    }

    void JumpAndGravity()
    {
        if (controller.isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        bool canJump = coyoteTimer > 0f;
        bool bufferedJump = jumpBufferTimer > 0f;

        if (canJump && bufferedJump)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            coyoteTimer = 0f;
            jumpBufferTimer = 0f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void Crouch()
    {
        bool crouching = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);

        float targetHeight = crouching ? crouchHeight : standHeight;
        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * 18f);
        controller.center = new Vector3(0f, controller.height / 2f, 0f);

        currentSpeed = crouching ? crouchSpeed : walkSpeed;
    }

    void UpdateCameraHeight()
    {
        if (cameraPivot == null) return;

        bool crouching = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);
        float targetY = crouching ? crouchCameraY : standCameraY;

        Vector3 lp = cameraPivot.localPosition;
        lp.y = Mathf.Lerp(lp.y, targetY, Time.deltaTime * cameraLerpSpeed);
        cameraPivot.localPosition = lp;
    }
}