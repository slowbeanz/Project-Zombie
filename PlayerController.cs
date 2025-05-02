using System.Globalization;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    private CharacterController controller;
    private Camera cam;

    [Header("Movement")]
    private Vector3 moveDir;
    [SerializeField] private bool canMove;
    [SerializeField] private bool isGrounded;
    [SerializeField] private float currentSpeed;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float reverseSpeed;
    [SerializeField] private float timeScaleForChange;

    [Header("Jump")]
    [SerializeField] private float jumpHeight;
    [SerializeField] private float fallingSpeed;

    [Header("Gravity")]
    [SerializeField] private float gravity = -9.81f;

    [Header("Grounded")]
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float raycastOffset;
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float sphereCalc;
    [SerializeField] private float sphereRadius;

    [Header("Camera")]
    [SerializeField] private bool canMoveCamera;
    [SerializeField] private Vector2 sensitivity;
    private float xRotation;

    [Header("Crouch")]
    [SerializeField] private bool canCrouch;
    [SerializeField] private bool holdToCrouch;
    [SerializeField] private bool isCrouching;
    [SerializeField] private float crouchSpeed;
    [SerializeField] private float originalHeight;
    [SerializeField] private float crouchHeight;

    [Header("Run")]
    [SerializeField] private bool canRun;
    [SerializeField] private bool holdToRun;
    [SerializeField] private bool isRunning;
    [SerializeField] private float runSpeed;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        cam = GetComponentInChildren<Camera>();
    }
    private void Update()
    {
        ChangeCurrentSpeed();
        CheckIfGrounded();
        Gravity();
    }
    private void FixedUpdate()
    {
        if (!canMove && isGrounded) return;

        controller.Move(transform.TransformDirection(moveDir * Time.fixedDeltaTime));
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        //Gizmos.DrawWireSphere(transform.position + (transform.up * groundTransform), groundCheckRadius);
        Gizmos.DrawWireSphere(controller.transform.position, sphereRadius);
    }

    private void Gravity()
    {
        if (!isGrounded)
        {
            moveDir.y += gravity * Time.deltaTime;
        }
        if (moveDir.y < 0 && isGrounded)
        {
            moveDir.y = -2;
        }
    }
    private void CheckIfGrounded()
    {
        Vector3 raycastStart = transform.position + Vector3.up * raycastOffset;
        Debug.DrawRay(raycastStart, Vector3.down * groundCheckDistance, Color.red);

        if (Physics.Raycast(raycastStart, Vector3.down, out RaycastHit hitInfo, groundCheckDistance, groundMask))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }
    private void ChangeCurrentSpeed()
    {
        if (!canRun || !canMove) return;

        if (!isGrounded)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, fallingSpeed, timeScaleForChange * Time.deltaTime);
        }
        else if (moveDir.z < 0)
        {
            currentSpeed = Mathf.Lerp(currentSpeed, reverseSpeed, timeScaleForChange * Time.deltaTime);
        }
        else if (isCrouching)
        {
            currentSpeed = crouchSpeed;
        }
        else
        { 
            currentSpeed = isRunning ? Mathf.Lerp(currentSpeed, runSpeed, 2f * Time.deltaTime) : walkSpeed;  
        }
    }

    public void MovementInput(Vector2 input)
    {
        if (!canMove) return;

        moveDir.x = input.x * currentSpeed;
        moveDir.z = input.y * currentSpeed;
    }
    public void MoveCameraInput(Vector2 input)
    {
        if (!canMoveCamera) return;

        float mouseX = input.x;
        float mouseY = input.y;

        xRotation -= mouseY * sensitivity.y;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        cam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        transform.Rotate(mouseX * sensitivity.x * Vector3.up);
    }
    public void JumpInput()
    {
        if (!canMove) return;

        if (isGrounded)
        {
            moveDir.y += jumpHeight;
        }
    }
    public void CrouchInput(InputAction.CallbackContext ctx)
    {
        if (!canCrouch) return;

        if (holdToCrouch)
        {
            isCrouching = ctx.performed;
            controller.height = ctx.performed ? crouchHeight : originalHeight;
        }
        else
        {
            if (ctx.performed)
            {
                controller.height = controller.height == originalHeight ? crouchHeight : originalHeight;
                isCrouching = controller.height == originalHeight ? false : true;
            }
        }

    }
    public void RunInput(in InputAction.CallbackContext ctx)
    {
        if (!canRun) return;

        if (holdToRun)
        {
            isRunning = ctx.performed;
        }
        else
        {
            if (ctx.performed)
            {
                isRunning = !isRunning;
            }
        }
    }
}
