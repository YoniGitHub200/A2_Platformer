using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10f;
    public float acceleration = 15f;
    public float airControl = 0.5f;

    [Header("Jumping")]
    public float jumpForce = 15f;
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.15f;

    private float coyoteCounter;
    private float jumpBufferCounter;

    [Header("Dash")]
    public float dashForce = 20f;
    public float dashDuration = 0.12f;
    public float dashCooldown = 0.3f;

    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashCooldownTimer = 0f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float checkRadius = 0.1f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;

    private float moveInput;
    private bool grounded;

    public enum FacingDirection { left, right }
    private FacingDirection facing = FacingDirection.right;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        ReadInput();
        UpdateFacingDirection();
        UpdateGroundedState();
        HandleJump();
        HandleDash();
    }

    void FixedUpdate()
    {
        if (!isDashing)
            HandleMovement();
    }

    // --------------------------
    // INPUT
    // --------------------------
    void ReadInput()
    {
        moveInput = Input.GetAxisRaw("Horizontal");
    }

    // --------------------------
    // MOVEMENT
    // --------------------------
    void HandleMovement()
    {
        float control = grounded ? 1f : airControl;

        float targetSpeed = moveInput * moveSpeed;
        float newX = Mathf.Lerp(rb.linearVelocity.x, targetSpeed, acceleration * control * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);

        // Update coyote time
        if (grounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.fixedDeltaTime;
    }

    // --------------------------
    // JUMPING
    // --------------------------
    void HandleJump()
    {
        // Buffer jump input
        if (Input.GetButtonDown("Jump"))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        // Perform jump if valid
        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
        }
    }

    // --------------------------
    // DASH
    // --------------------------
    void HandleDash()
    {
        dashCooldownTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && dashCooldownTimer <= 0f)
        {
            isDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;

            float direction = (facing == FacingDirection.right) ? 1f : -1f;
            rb.linearVelocity = new Vector2(direction * dashForce, 0f);
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;

            if (dashTimer <= 0f)
            {
                isDashing = false;
            }
        }
    }

    // --------------------------
    // GROUND CHECK
    // --------------------------
    void UpdateGroundedState()
    {
        grounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
    }

    // --------------------------
    // FACING DIRECTION
    // --------------------------
    void UpdateFacingDirection()
    {
        if (moveInput > 0)
            facing = FacingDirection.right;
        else if (moveInput < 0)
            facing = FacingDirection.left;
    }

    // --------------------------
    // PUBLIC API FOR VISUALS
    // --------------------------

    public bool IsWalking()
    {
        return Mathf.Abs(moveInput) > 0.1f;
    }

    public bool IsGrounded()
    {
        return grounded;
    }

    public FacingDirection GetFacingDirection()
    {
        return facing;
    }

    // --------------------------
    // EDITOR VISUALIZATION
    // --------------------------
    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
    }
}
