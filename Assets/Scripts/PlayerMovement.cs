using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.Assertions.Must;

public class PlayerMovement : MonoBehaviour {

    [Header("Movement Settings")]
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float higherGravity = 4.5f;
    [SerializeField] private float dashPower = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float wallJumpDuration = 0.2f;
    [SerializeField] private float maxFallSpeed = 20f;
    [SerializeField] private float wallJumpForce = 5f;
    [SerializeField] private float maxCoyoteTime = 0.2f;
    

    [Header("Ground Check Settings")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.9f, 0.1f);
    [SerializeField] private float groundCheckDistance = 0.1f;

    [Header("Wall Check Settings")]
    [SerializeField] private Vector2 wallCheckSize = new Vector2(0.1f, 0.9f);
    [SerializeField] private float wallCheckDistance = 0.1f;

    private float horizontalMove;
    private float verticalMove;
    private Vector2 velocity = Vector2.zero;
    internal float originalGravity { get; private set; }
    private bool isGrounded;
    private bool hasReleasedJump;
    private float previousVelocityY;
    private bool isModifyingGravity;
    private float coyoteTimer;
    private bool isFalling;
    private bool canDash = true;
    private bool isWallJumping;
    private bool canWallJumpAgain = false;
    private float fallTimer;
    private bool isFacingLeft;
    private bool isWalled;

    // Animation Dependent Properties
    internal float xVelocity { get; private set; }
    internal float yVelocity { get; private set; }
    internal bool isJumping { get; private set; }
    internal bool isDashing { get; private set; }

    private BoxCollider2D bc;
    private Rigidbody2D rb;
    private SpriteRenderer sr;

    //TODO Migrate to the new input system

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        bc = GetComponent<BoxCollider2D>();
        originalGravity = rb.gravityScale;
    }

    void Update() {
        MovementInput();
        CheckJump();
        CheckJumpState();
        CheckJumpReleased();
        CheckWallJumpInput();
        CheckDashInput();
        FlipSprite(horizontalMove);
    }

    void FixedUpdate() {
        GroundedCheck();
        WallCheck();
        CheckCoyoteTime();
        ApplyMovement();
        setRigidBodyVelocites();
    }

    private bool IsPlayerDead() {
        if (DeathHandler.CurrentState == DeathHandler.PlayerState.Dying || DeathHandler.CurrentState == DeathHandler.PlayerState.Dead) {
            return true;
        }
        return false;
    }

    private void setRigidBodyVelocites() {
        xVelocity = rb.velocity.x;
        yVelocity = rb.velocity.y;
    }

    private void MovementInput() {
        horizontalMove = Input.GetAxisRaw("Horizontal");
        verticalMove = Input.GetAxisRaw("Vertical");
    }

    private void ApplyMovement() {

        float slowDownAmount = isJumping ? 0.11f : 0.07f;

        if (!isDashing && !isWallJumping) {
            Vector2 targetVelocityX = new Vector2(horizontalMove * movementSpeed, Mathf.Max(rb.velocity.y, -maxFallSpeed));
            rb.velocity = Vector2.SmoothDamp(rb.velocity, targetVelocityX, ref velocity, slowDownAmount);
        }
    }

    private void CheckJump() {
        if (!isDashing && (coyoteTimer > 0f && Input.GetButtonDown("Jump"))) {
            rb.velocity = new Vector2(rb.velocity.x * 1.20f, jumpForce);
            rb.AddForce(rb.velocity.normalized * 1.5f, ForceMode2D.Impulse);
            isJumping = true;
        }
    }

    private void CheckJumpState() {

        if (isDashing) {
            rb.gravityScale = 0f;
            return;
        }

        if (isModifyingGravity) {
            previousVelocityY = rb.velocity.y;
            return;
        }

        float currentVelocityY = rb.velocity.y;

        if ((isJumping && !hasReleasedJump) && !isWallJumping && !canWallJumpAgain 
            && previousVelocityY > 0f && currentVelocityY <= 0f) {
            StartCoroutine(ApplyHalfGravityAtPeak());
            return;
        }

        if (!hasReleasedJump && (!isGrounded && rb.velocity.y < 0.1f)) {
            isFalling = true;
            fallTimer += Time.deltaTime;
            float t = Mathf.Clamp01(fallTimer / 0.7f);
            rb.gravityScale = Mathf.Lerp(originalGravity, higherGravity, t);
        }
        else {
            isFalling = false;
            fallTimer = 0f;
        }

        previousVelocityY = currentVelocityY;
    }

    private void CheckJumpReleased() {
        if (!isWallJumping && Input.GetButtonUp("Jump") && rb.velocity.y > 0.1f) {
            hasReleasedJump = true;
            coyoteTimer = 0f;
            rb.gravityScale = higherGravity;
            // This fucks with the dash
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.65f);
        }
    }

    private IEnumerator ApplyHalfGravityAtPeak() {

        isModifyingGravity = true;
        rb.gravityScale = originalGravity / 2;

        yield return new WaitForSeconds(0.1f);

        rb.gravityScale = originalGravity;
        isModifyingGravity = false;
    }

    private void CheckCoyoteTime() {
        if (isGrounded) {
            coyoteTimer = maxCoyoteTime;
        }
        else {
            coyoteTimer -= Time.deltaTime;
        }
    }

    private void CheckWallJumpInput() {
        if (isWalled && (canWallJumpAgain || hasReleasedJump || isFalling) && Input.GetButtonDown("Jump")) {
            StartCoroutine(PerformWallJump());
        }
    }

    private IEnumerator PerformWallJump() {

        rb.gravityScale = originalGravity;
        sr.flipX = !isFacingLeft;
        isWallJumping = true;
        // Allow instantaneous walljumping
        canWallJumpAgain = true;
        hasReleasedJump = false;

        // Jump in the opposite direction the player is facing
        Vector2 wallJumpDirection = isFacingLeft ? Vector2.right : Vector2.left;

        isFacingLeft = !isFacingLeft;

        rb.velocity = new Vector2(wallJumpDirection.x * wallJumpForce, jumpForce);

        float originalMovementSpeed = movementSpeed;
        movementSpeed = 0f;

        yield return new WaitForSeconds(wallJumpDuration);

        movementSpeed = originalMovementSpeed;

        isWallJumping = false;

    }

    private void CheckDashInput() {
        if (!isDashing && (canDash && Input.GetKeyDown(KeyCode.C))) {
            StartCoroutine(PerformDash());
        }
    }

    private IEnumerator PerformDash() {
        rb.gravityScale = 0;
        isDashing = true;
        canDash = false;
        hasReleasedJump = false;

        Vector2 dashDirection = new Vector2(horizontalMove, verticalMove).normalized;

        if (dashDirection == Vector2.zero) {
            dashDirection = isFacingLeft ? Vector2.left : Vector2.right;
        }

        rb.velocity = dashDirection * dashPower;

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = IsPlayerDead() ? 0f : originalGravity;
        rb.velocity = Vector2.zero;

        isDashing = false;
    }

    private void GroundedCheck() {
        Vector2 boxCastOrigin = (Vector2) transform.position + bc.offset;
        RaycastHit2D hit = Physics2D.BoxCast(boxCastOrigin, groundCheckSize, 0f, Vector2.down, groundCheckDistance, groundLayer);

        bool wasGrounded = isGrounded;
        isGrounded = hit.collider != null;
        if (isGrounded && !wasGrounded) {
            OnLanded();
        }

        if (isGrounded && !canDash) {   
            canDash = true;
        }
    }

    private void WallCheck() {
        Vector2 boxCastOrigin = (Vector2) transform.position + bc.offset;
        Vector2 facingDirection = isFacingLeft ? Vector2.left : Vector2.right;
        RaycastHit2D hit = Physics2D.BoxCast(boxCastOrigin, wallCheckSize, 0f, facingDirection, wallCheckDistance, groundLayer);

        isWalled = hit.collider != null;
    }

    private void OnLanded() {
        isJumping = false;
        hasReleasedJump = false;
        canDash = true;
        canWallJumpAgain = false;
        coyoteTimer = maxCoyoteTime;
        rb.gravityScale = originalGravity;
    }

    private void FlipSprite(float horizontalMovement) {

        if (isWallJumping || isDashing) return;

        if (horizontalMovement != 0) {
            isFacingLeft = sr.flipX = horizontalMovement < 0;
        }
    }

    private void OnDrawGizmos() {
        if (bc != null) {

            Vector2 boxCastOrigin = (Vector2)transform.position + bc.offset;

            // Ground Check Visualization
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Vector2 groundCheckOrigin = boxCastOrigin - new Vector2(0, groundCheckDistance);
            Gizmos.DrawWireCube(groundCheckOrigin, groundCheckSize);

            // Wall Check Visualization
            Gizmos.color = isWalled ? Color.green : Color.red;
            Vector2 facingDirection = isFacingLeft ? Vector2.left : Vector2.right;
            Vector2 wallCheckEndPosition = boxCastOrigin + facingDirection * wallCheckDistance;

            Gizmos.DrawWireCube(wallCheckEndPosition, wallCheckSize);
        }
    }
}