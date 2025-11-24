using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
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

    [Header("Dash (Zoom Mode)")]
    public float dashForce = 20f;
    public float dashDuration = 0.12f;
    public float dashCooldown = 0.3f;
    private float dashTimer;
    private bool isDashing = false;
    private float dashCooldownTimer;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float checkRadius = 0.1f;
    public LayerMask groundLayer;

    [Header("Squash & Stretch")]
    public float squashAmount = 0.25f;
    public float squashSpeed = 12f;

    private Rigidbody2D rb;
    private Vector3 baseScale;

    float inputX;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        baseScale = transform.localScale;
    }

    void Update()
    {
        HandleInput();
        HandleJump();
        HandleDash();
        ApplySquashStretch();
    }

    void FixedUpdate()
    {
        if (!isDashing)
            ApplyMovement();
    }

    // ----------------------------
    // INPUT
    // ----------------------------

    void HandleInput()
    {
        inputX = Input.GetAxisRaw("Horizontal");

        // Flip player sprite when moving
        if (inputX != 0)
            transform.localScale = new Vector3(Mathf.Sign(inputX) * Mathf.Abs(transform.localScale.x),
                                               transform.localScale.y,
                                               transform.localScale.z);
    }

    // ----------------------------
    // MOVEMENT
    // ----------------------------

    void ApplyMovement()
    {
        float control = IsGrounded() ? 1f : airControl;

        float targetSpeed = inputX * moveSpeed;
        float newSpeed = Mathf.Lerp(rb.linearVelocity.x, targetSpeed, acceleration * control * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector2(newSpeed, rb.linearVelocity.y);

        // update coyote timer
        if (IsGrounded())
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.fixedDeltaTime;
    }

    // ----------------------------
    // JUMPING
    // ----------------------------

    void HandleJump()
    {
        // Jump buffer — store jump input for a small time
        if (Input.GetButtonDown("Jump"))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        // Perform jump if buffer + coyote allow it
        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            Jump();
            jumpBufferCounter = 0f;
            coyoteCounter = 0f;
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

        // squash down then up
        StartSquash(new Vector3(1 + squashAmount, 1 - squashAmount, 1));
    }

    // ----------------------------
    // DASH
    // ----------------------------

    void HandleDash()
    {
        dashCooldownTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && dashCooldownTimer <= 0f)
        {
            isDashing = true;
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;

            float dashDir = transform.localScale.x > 0 ? 1 : -1;
            rb.linearVelocity = new Vector2(dashDir * dashForce, 0f);

            // stretch horizontally during dash
            StartSquash(new Vector3(1 + squashAmount, 1 - squashAmount, 1));
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;
                StartSquash(Vector3.one);
            }
        }
    }

    // ----------------------------
    // SQUASH & STRETCH
    // ----------------------------

    Vector3 targetScale;

    void StartSquash(Vector3 sq)
    {
        targetScale = sq;
    }

    void ApplySquashStretch()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale == Vector3.zero ? baseScale : targetScale, squashSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.localScale, baseScale) < 0.05f)
            targetScale = baseScale;
    }

    // ----------------------------
    // GROUND DETECTION
    // ----------------------------

    bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }
    }
}
