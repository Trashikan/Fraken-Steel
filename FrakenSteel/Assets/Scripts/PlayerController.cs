using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //variable that can be modfied in inspector
    [Header("Input")]
    [SerializeField] PlayerControls controls;
    [Header("Checks")] 
	[SerializeField] private Transform[] _groundCheckPoints;
	[SerializeField] private float _groundChecksSize;
    
    [Header("Data")] 
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private LayerMask _groundLayer;
    public float jumpForce=12f;
    public float dragAmount = 0.22f;
	public float frictionAmount = 0.55f;
	public float gravityScale = 1.1f;
	public bool doKeepRunMomentum;
	public float runMaxSpeed = 9.5f;
	public float runAccel= 9.3f;
  	public float runDeccel=15f;
  	public float accelInAir=0.65f;
 	public float deccelInAir= 0.65f;
	public float stopPower=1.23f;
	public float turnPower=1.13f;
	public float accelPower= 1.05f;
	public float jumpBufferTime = 0.1f;
	public float dashBufferTime = 0.05f;
	public float quickFallGravityMult =2.2f;
	public float fallGravityMult =1.35f;
	public float jumpCutMultiplier = 0.5f;
	public int dashAmount= 1;
  	public float dashSpeed= 17f;
  	public float dashAttackTime= 0.15f;
  	public float dashAttackDragAmount= 0.22f;
  	public float dashEndTime= 0.1f;
  	public float dashUpEndMult= 0.6f;
  	public float dashEndRunLerp= 0.3f;
	public float runLerp;

    
    public bool IsFacingRight { get; private set; }
	public bool IsJumping { get; private set; }
    public bool IsDashing { get; private set; }
    public bool _dashAttacking { get; private set; }
	public float LastOnGroundTime { get; private set; }
    
    private int _dashesLeft;
	private float _dashStartTime;
	private Vector2 _lastDashDir;
    
    public float LastPressedJumpTime { get; private set; }
	public float LastPressedDashTime { get; private set; }

	Vector2 userInput;

	private InputAction playerMovement;
	private InputAction playerJump;
	private InputAction playerDash;
    
    Rigidbody2D rb;

	private void Awake() {
		controls = new PlayerControls();
	}

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        IsFacingRight = true;
    }

    private void Update()
    {
        Timers();
        PhysicsChecks();
        userInput = playerMovement.ReadValue<Vector2>();
        
        // Gravity when falling for better jump
        if (!IsDashing)
		{
			if (rb.velocity.y >= 0)
				SetGravityScale(gravityScale);
			else if (userInput.y < 0)
				SetGravityScale(gravityScale * quickFallGravityMult);
			else
				SetGravityScale(gravityScale * fallGravityMult);
		}
        
        CalculateJump();
        CalculateDash();
    }
    
    private void FixedUpdate()
    {   
        if (IsDashing)
			Drag(DashAttackOver()? dragAmount : dashAttackDragAmount);
		else if(LastOnGroundTime <= 0)
			Drag(dragAmount);
        else
			Drag(frictionAmount);


        if (!IsDashing)
        {
            Run(runLerp);
        }
        else if (DashAttackOver())
        {
            Run(dashEndRunLerp);
        }
    }

    private void Timers(){
        LastOnGroundTime -= Time.deltaTime;
		LastPressedJumpTime -= Time.deltaTime;
		LastPressedDashTime -= Time.deltaTime;
    }
    
    private void PhysicsChecks(){
        //Ground Check
        foreach (Transform groundCheck in _groundCheckPoints)
        {
            if (Physics2D.OverlapCircle(groundCheck.transform.position, _groundChecksSize, _groundLayer))
            {
                LastOnGroundTime = coyoteTime;
                break;
            }
    	}
	}

    private void CalculateJump(){
        if (IsJumping && rb.velocity.y < 0)
		{
			// falling
            IsJumping = false;
		}
        
        if (!IsDashing)
		{
			//Jump
			if (CanJump() && LastPressedJumpTime > 0)
			{
				IsJumping = true;
				Debug.Log("should jump");
				Jump();
			}
        }
    }
    
    private void CalculateDash(){
        if (DashAttackOver())
		{
			if (_dashAttacking)
			{
				_dashAttacking = false;
				StopDash(_lastDashDir); 
			}
			else if (Time.time - _dashStartTime > dashAttackTime + dashEndTime)
			{
				IsDashing = false; 
			}
		}
	
    
    	if (CanDash() && LastPressedDashTime > 0)
		{
			if (userInput != Vector2.zero)
			{
				_lastDashDir = userInput;
			}
			else
				// _lastDashDir = IsFacingRight ? Vector2.right : Vector2.left;
			if(LastOnGroundTime > 0) _lastDashDir = IsFacingRight ? Vector2.right : Vector2.left;
			else _lastDashDir = Vector2.up;
			_dashStartTime = Time.time;
			_dashesLeft--;
			_dashAttacking = true;
			IsDashing = true;
			IsJumping = false;
			StartDash(_lastDashDir);
		}
	}
    
	private void Jump()
	{

		LastPressedJumpTime = 0;
		LastOnGroundTime = 0;

		float force = jumpForce;
		if (rb.velocity.y < 0)
			force -= rb.velocity.y;

		rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
	
	}
	
	private void Run(float lerpAmount)
	{
		float targetSpeed = userInput.x * runMaxSpeed; 
		float speedDif = targetSpeed - rb.velocity.x; 

		float accelRate;
		
		if (LastOnGroundTime > 0)
			accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? runAccel : runDeccel;
		else
			accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? runAccel * accelInAir : runDeccel * deccelInAir;

	
		if (((rb.velocity.x > targetSpeed && targetSpeed > 0.01f) || (rb.velocity.x < targetSpeed && targetSpeed < -0.01f)) && doKeepRunMomentum)
		{
			accelRate = 0; 
		}
	

		float velPower;
		if (Mathf.Abs(targetSpeed) < 0.01f)
		{
			velPower = stopPower;
		}
		else if (Mathf.Abs(rb.velocity.x) > 0 && (Mathf.Sign(targetSpeed) != Mathf.Sign(rb.velocity.x)))
		{
			velPower = turnPower;
		}
		else
		{
			velPower = accelPower;
		}
	

		
		float movement = Mathf.Pow(Mathf.Abs(speedDif) * accelRate, velPower) * Mathf.Sign(speedDif);
		movement = Mathf.Lerp(rb.velocity.x, movement, lerpAmount); 

		rb.AddForce(movement * Vector2.right); 

		if (userInput.x != 0)
			CheckDirectionToFace(userInput.x > 0);
	}

    
    public void SetGravityScale(float scale)
	{
		rb.gravityScale = scale;
	}

	private void Drag(float amount)
	{
		Vector2 force = amount * rb.velocity.normalized;
		force.x = Mathf.Min(Mathf.Abs(rb.velocity.x), Mathf.Abs(force.x));
		force.y = Mathf.Min(Mathf.Abs(rb.velocity.y), Mathf.Abs(force.y));
		force.x *= Mathf.Sign(rb.velocity.x); 
		force.y *= Mathf.Sign(rb.velocity.y);

		rb.AddForce(-force, ForceMode2D.Impulse);
	}
	
	private void JumpCut()
	{
		rb.AddForce(Vector2.down * rb.velocity.y * (1 - jumpCutMultiplier), ForceMode2D.Impulse);
	}
	
	private void StartDash(Vector2 dir)
	{
		LastOnGroundTime = 0;
		LastPressedDashTime = 0;

		SetGravityScale(0);

		rb.velocity = dir.normalized * dashSpeed;
	}

	private void StopDash(Vector2 dir)
    {
		SetGravityScale(gravityScale);

		if (dir.y > 0)
		{
			if (dir.x == 0)
				rb.AddForce(Vector2.down * rb.velocity.y * (1 - dashUpEndMult), ForceMode2D.Impulse);
			else
				rb.AddForce(Vector2.down * rb.velocity.y * (1 - dashUpEndMult) * .7f, ForceMode2D.Impulse);
		}
	}

    private void Turn()
    {
        IsFacingRight = !IsFacingRight;
        Vector3 playerScale = transform.localScale;
        playerScale.x = playerScale.x * -1;
        transform.localScale = playerScale;
    }

    public void CheckDirectionToFace(bool isMovingRight)
	{
		if (isMovingRight != IsFacingRight)
			Turn();
	}

	private bool CanJump()
    {
		return LastOnGroundTime > 0 && !IsJumping;
    }

	private bool CanJumpCut()
    {
		return IsJumping && rb.velocity.y > 0;
    }
    
    private bool CanDash()
	{
		if (_dashesLeft < dashAmount && LastOnGroundTime > 0)
			_dashesLeft = dashAmount;

		return _dashesLeft > 0;
	}

	private bool DashAttackOver()
    {
		return IsDashing && Time.time - _dashStartTime > dashAttackTime;
	}

	private void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        
        foreach (Transform groundCheck in _groundCheckPoints)
        {
            Gizmos.DrawWireSphere(groundCheck.position, _groundChecksSize);
        }
	}
	

	private void OnDisable() {
        playerMovement.Disable();
        controls.Player.Jump.Disable();
        controls.Player.dash.Disable();
	}

	private void OnEnable() {
        playerMovement = controls.Player.Move;
        playerMovement.Enable();

		controls.Player.dash.performed += OnDash;
        controls.Player.dash.Enable();

        controls.Player.Jump.performed += OnJump;
        controls.Player.Jump.canceled += OnJumpUp;
        controls.Player.Jump.Enable();
	}

	private void OnJump(InputAction.CallbackContext ctx){
        LastPressedJumpTime = jumpBufferTime;
	}
    private void OnJumpUp(InputAction.CallbackContext ctx)
    {
        if (CanJumpCut())
            JumpCut();
    }
    private void OnDash(InputAction.CallbackContext ctx)
    {
        LastPressedDashTime = dashBufferTime;
    }
}

	

