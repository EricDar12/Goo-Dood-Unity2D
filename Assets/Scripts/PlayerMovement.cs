using System.Collections;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerMovement : MonoBehaviour {

    [Header("Movement Settings")]
    [SerializeField] private float _movementSpeed = 5f;
    [SerializeField] private float _jumpForce = 10f;
    [SerializeField] private float _higherGravity = 4.5f;
    [SerializeField] private float _dashPower = 15f;
    [SerializeField] private float _dashDuration = 0.2f;
    [SerializeField] private float _wallJumpDuration = 0.2f;
    [SerializeField] private float _maxFallSpeed = 20f;
    [SerializeField] private float _wallJumpForce = 5f;
    [SerializeField] private float _maxCoyoteTime = 0.2f;
    [SerializeField] private float _maxJumpBuffer = 0.2f;
   
    [Header("Ground Check Settings")]
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.9f, 0.1f);
    [SerializeField] private float _groundCheckDistance = 0.1f;

    [Header("Wall Check Settings")]
    [SerializeField] private Vector2 _wallCheckSize = new Vector2(0.1f, 0.9f);
    [SerializeField] private float _wallCheckDistance = 0.1f;

    [Header("Movement Tuning")]
    [SerializeField] private float _groundedSlowDown = 0.05f;
    [SerializeField] private float _jumpingSlowDown = 0.1f;
    [SerializeField] private float _forwardJumpBoost = 1.2f;

    public float OriginalGravity { get; private set; }
    private Vector2 _velocity = Vector2.zero;
    private float _horizontalMove;
    private float _verticalMove;
    private bool _isGrounded;
    private float _previousVelocityY;
    private bool _isModifyingGravity;
    private float _coyoteTimer;
    private float _jumpBufferTimer;
    private bool _isFalling;
    private bool _canDash = true;
    private bool _isWallJumping;
    private bool _canWallJumpAgain = false;
    private float _fallTimer;
    private bool _isFacingLeft;
    private bool _isWalled;

    private bool _isJumpHeld;
    private bool _hasBufferedJump;
    private bool _canCutJumpHeight;

    public float XVelocity { get; private set; }
    public float YVelocity { get; private set; }
    public bool IsJumping { get; private set; }
    public bool IsDashing { get; private set; }

    private BoxCollider2D _bc;
    private Rigidbody2D _rb;
    private SpriteRenderer _sr;

    void Start() {

        _rb = GetComponent<Rigidbody2D>();
        Assert.IsNotNull(_rb, "RigidBody2D component is required");

        _sr = GetComponent<SpriteRenderer>();
        Assert.IsNotNull(_sr, "SpriteRenderer component is required");

        _bc = GetComponent<BoxCollider2D>();
        Assert.IsNotNull(_bc, "BoxCollider2D component is required");

        OriginalGravity = _rb.gravityScale;
    }

    void Update() {
        CheckJumpReleased();
        CaptureMovementInput();
        UpdateJumpBuffer();
        UpdateCoyoteTime();
        WallJump();
        Dash();
        setRigidBodyVelocites();
        FlipSprite(_horizontalMove);
    }

    void FixedUpdate() {
        GroundedCheck();
        WallCheck();
        ApplyMovementInput();
        Jump();
        CheckJumpState();
    }

    #region Horizontal Movement Input

    private void CaptureMovementInput() {
        _horizontalMove = Input.GetAxisRaw("Horizontal");
        _verticalMove = Input.GetAxisRaw("Vertical");
    }

    private void ApplyMovementInput() {

        float slowDownAmount = IsJumping ? _jumpingSlowDown : _groundedSlowDown;

        if (!IsDashing && !_isWallJumping) {
            Vector2 targetVelocityX = new Vector2(_horizontalMove * _movementSpeed, Mathf.Max(_rb.velocity.y, -_maxFallSpeed));
            _rb.velocity = Vector2.SmoothDamp(_rb.velocity, targetVelocityX, ref _velocity, slowDownAmount);
        }
    }

    #endregion
    #region Jump Input and Checks

    private void Jump() {

        if (!CanJump()) return;

        _rb.velocity = new Vector2(_rb.velocity.x * _forwardJumpBoost, _jumpForce);

        ResetJumpState();
        IsJumping = true;
    }

    private void CheckJumpState() {

        if (IsDashing) {
            ApplyGravity(0f);
            return;
        }

        if (_isModifyingGravity) {
            _previousVelocityY = _rb.velocity.y;
            return;
        }

        // Compare current and previous Y vel to determine when the player begins moving down
        float currentVelocityY = _rb.velocity.y;

        // If jump is held, briefly apply half gravity at the apex of the jump
        if ((IsJumping && _isJumpHeld) && !_isWallJumping && !_canWallJumpAgain
            && _previousVelocityY > 0f && currentVelocityY <= 0f) {
            _previousVelocityY = _rb.velocity.y;
            StartCoroutine(ReduceGravityAtJumpApex());
            return;
        }

        // If the player is falling naturally, smoothly lerp to higher gravity
        if (!_isJumpHeld && (!_isGrounded && _rb.velocity.y < 0.1f)) {
            _isFalling = true;
            _fallTimer += Time.deltaTime;
            float t = Mathf.Clamp01(_fallTimer / 0.7f);
            ApplyGravity(Mathf.Lerp(OriginalGravity, _higherGravity, t));
        }
        else {
            _isFalling = false;
            _fallTimer = 0f;
        }
        _previousVelocityY = currentVelocityY;
    }

    private IEnumerator ReduceGravityAtJumpApex() {

        _isModifyingGravity = true;
        ApplyGravity(OriginalGravity / 2f);

        yield return new WaitForSeconds(0.08f);

        ApplyGravity(OriginalGravity);
        _isModifyingGravity = false;
    }

    private void CheckJumpReleased() {

        // Only cut the jump height once per jump
        if (!_canCutJumpHeight) return;

        // If jump is released when the player is jumping && moving up, && neither dashing/wall jumping, cut the jump height 
        if (!_isJumpHeld && IsJumping && (!_isWallJumping && !IsDashing) && _rb.velocity.y > 0.1f) {
            ApplyGravity(_higherGravity);
            _rb.velocity = new Vector2(_rb.velocity.x, _rb.velocity.y * 0.65f);
            _canCutJumpHeight = false;
        }
    }

    private void UpdateCoyoteTime() {
        if (_isGrounded && !IsJumping) {
            _coyoteTimer = _maxCoyoteTime;
        }
        else if (_coyoteTimer > 0f) {
            _coyoteTimer -= Time.deltaTime;
        }
    }

    private void UpdateJumpBuffer() {
        if (Input.GetButtonDown("Jump")) {
            _jumpBufferTimer = _maxJumpBuffer;
            _hasBufferedJump = true;
            _isJumpHeld = true;
        }
        else if (_jumpBufferTimer > 0f) {
            _jumpBufferTimer -= Time.deltaTime;
        }
        if (Input.GetButtonUp("Jump")) {
            // Track if jump is released at any point
            _isJumpHeld = false;
        }
    }

    private void WallJump() {
        // If the player is against a wall && has released the jump button, or is falling naturally allow a wj input
        if (!_isWallJumping && _isWalled && (!_isJumpHeld || _canWallJumpAgain || _isFalling) && _jumpBufferTimer > 0f) {
            _jumpBufferTimer = 0f;
            StartCoroutine(PerformWallJump());
        }
    }

    private IEnumerator PerformWallJump() {

        ApplyGravity(OriginalGravity);
        _sr.flipX = !_isFacingLeft;
        _isWallJumping = true;

        // Set flag for instantaneous wall jumping
        _canWallJumpAgain = true;

        // Jump in the opposite direction the player is facing
        Vector2 wallJumpDirection = _isFacingLeft ? Vector2.right : Vector2.left;

        _isFacingLeft = !_isFacingLeft;

        _rb.velocity = new Vector2(wallJumpDirection.x * _wallJumpForce, _jumpForce);

        float originalMovementSpeed = _movementSpeed;
        _movementSpeed = 0f;

        yield return new WaitForSeconds(_wallJumpDuration);

        _movementSpeed = originalMovementSpeed;

        _isWallJumping = false;
    }

    #endregion
    #region Dash Methods

    private void Dash() {
        if (!IsDashing && (_canDash && Input.GetKeyDown(KeyCode.C))) {
            StartCoroutine(PerformDash());
        }
    }

    private IEnumerator PerformDash() {

        ApplyGravity(0f);
        IsDashing = true;
        _canDash = false;

        Vector2 dashDirection = new Vector2(_horizontalMove, _verticalMove).normalized;

        if (dashDirection == Vector2.zero) {
            dashDirection = _isFacingLeft ? Vector2.left : Vector2.right;
        }

        _rb.velocity = dashDirection * _dashPower;

        yield return new WaitForSeconds(_dashDuration);

        ApplyGravity(OriginalGravity);
        _rb.velocity = Vector2.zero;

        IsDashing = false;
    }

    #endregion
    #region Collision Checks

    private void GroundedCheck() {
        Vector2 boxCastOrigin = (Vector2)transform.position + _bc.offset;
        RaycastHit2D hit = Physics2D.BoxCast(boxCastOrigin, _groundCheckSize, 0f, Vector2.down, _groundCheckDistance, _groundLayer);

        bool wasGrounded = _isGrounded;
        _isGrounded = hit.collider != null;
        if (_isGrounded && !wasGrounded) {
            OnLanded();
        }

        // Allows dash to reset when dashing horizontally, but prevents incorrect resets when dashing off the ground
        if (_isGrounded && (!_canDash && !IsDashing)) {
            _canDash = true;
        }
    }

    private void WallCheck() {
        Vector2 boxCastOrigin = (Vector2)transform.position + _bc.offset;
        Vector2 facingDirection = _isFacingLeft ? Vector2.left : Vector2.right;
        RaycastHit2D hit = Physics2D.BoxCast(boxCastOrigin, _wallCheckSize, 0f, facingDirection, _wallCheckDistance, _groundLayer);

        _isWalled = hit.collider != null;
    }

    #endregion
    #region Helper Methods

    private bool CanJump() {
        return (!IsDashing && (_coyoteTimer > 0f && _jumpBufferTimer > 0f));
    }
    
    private void ResetJumpState() {
        _jumpBufferTimer = 0f;
        _coyoteTimer = 0f;
        _hasBufferedJump = false;
    }

    private void OnLanded() {
        IsJumping = false;
        _canDash = true;
        _isWallJumping = false;
        _canWallJumpAgain = false;
        _canCutJumpHeight = true;
        ApplyGravity(OriginalGravity);
    }

    private bool IsPlayerDead() {
        return (DeathHandler.CurrentState == DeathHandler.PlayerState.Dying || DeathHandler.CurrentState == DeathHandler.PlayerState.Dead);
    }

    private void setRigidBodyVelocites() {
        // These properties are read by the animation controller
        XVelocity = _rb.velocity.x;
        YVelocity = _rb.velocity.y;
    }

    private void FlipSprite(float horizontalMovement) {

        if (_isWallJumping || IsDashing) return;

        if (horizontalMovement != 0) {
            _isFacingLeft = _sr.flipX = horizontalMovement < 0;
        }
    }

    private void ApplyGravity(float newGravity) {
        _rb.gravityScale = IsPlayerDead() ? 0f : newGravity;
    }

    #endregion
    #region Gizmos

    private void OnDrawGizmos() {
        if (_bc != null) {

            Vector2 boxCastOrigin = (Vector2)transform.position + _bc.offset;

            // Ground Check Visualization
            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Vector2 groundCheckOrigin = boxCastOrigin - new Vector2(0, _groundCheckDistance);
            Gizmos.DrawWireCube(groundCheckOrigin, _groundCheckSize);

            // Wall Check Visualization
            Gizmos.color = _isWalled ? Color.green : Color.red;
            Vector2 facingDirection = _isFacingLeft ? Vector2.left : Vector2.right;
            Vector2 wallCheckEndPosition = boxCastOrigin + facingDirection * _wallCheckDistance;

            Gizmos.DrawWireCube(wallCheckEndPosition, _wallCheckSize);
        }
    }
    #endregion
}