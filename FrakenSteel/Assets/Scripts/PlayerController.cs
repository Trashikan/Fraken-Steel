using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //variable that can be modfied in inspector
    
    [Header("Checks")] 
	[SerializeField] private Transform[] _groundCheckPoints;
	[SerializeField] private Vector2 _groundChecksSize;
	[Space(5)]
	[SerializeField] private Transform _frontWallCheckPoint;
	[SerializeField] private Transform _backWallCheckPoint;
	[SerializeField] private Vector2 _wallCheckSize;
    
    [Header("Data")] 
    [SerializeField] private float coyoteTime;
    [SerializeField] private LayerMask _groundLayer;
    public float jumpForce
    public float dashAttackTime;
    public float dashEndTime;
    public float dragAmount;
	public float frictionAmount;
	public float dashAttackDragAmount;
	public float gravityScale = 1f;
	public float dashUpEndMult;
	public bool doKeepRunMomentum;
	public float runMaxSpeed;
	public float stopPower;
	public float turnPower;
	public float accelPower;
    
    public bool IsFacingRight { get; private set; }
	public bool IsJumping { get; private set; }
    public bool IsDashing { get; private set; }
	public float LastOnGroundTime { get; private set; }
    
    private int _dashesLeft;
	private float _dashStartTime;
	private Vector2 _lastDashDir;
    
    public float LastPressedJumpTime { get; private set; }
	public float LastPressedDashTime { get; private set; }
    
    RigidBody2D rb;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        isFacingRight = true;
    }

    private void Update()
    {
        Timers();
        PhysicsChecks();
        
        // Gravity when falling for better jump
        if (!IsDashing)
	{
		if (rb.velocity.y >= 0)
			SetGravityScale(gravityScale);
		else if (Input.GetAxis("Vertical") < 0)
			SetGravityScale(gravityScale * quickFallGravityMult);
		else
			SetGravityScale(gravityScale * fallGravityMult);
	}
        
        CalculateJump();
        CalculateDash();
    }
    
    private void Timers(){
        LastOnGroundTime -= Time.deltaTime;
	LastOnWallTime -= Time.deltaTime;
	LastOnWallRightTime -= Time.deltaTime;
	LastOnWallLeftTime -= Time.deltaTime;
	LastPressedJumpTime -= Time.deltaTime;
	LastPressedDashTime -= Time.deltaTime;
    }

    private void GetInput()
    {
        
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

    void CalculateJump(){
        if (IsJumping && RB.velocity.y < 0)
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
			Jump();
		}
        }
    }
    
    void CalculateDash(){
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
		if (new Vector2(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical"))) != Vector2.zero)
			_lastDashDir = new Vector2(Input.GetAxis("Horizontal"),Input.GetAxis("Vertical"));
		else
			_lastDashDir = IsFacingRight ? Vector2.right : Vector2.left;
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
	
	
	//ngl I got this big block on internet
	private void Run(float lerpAmount)
	{
		float targetSpeed = Input.GetAxis("Horizontal") * runMaxSpeed; 
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

		if (Input.GetAxis("Horizontal") != 0)
			CheckDirectionToFace(Input.GetAxis("Horizontal") > 0);
	}

    void FixedUpdate()
    {   
        if (IsDashing)
		Drag(DashAttackOver()? dragAmount : dashAttackDragAmount);
	else if(LastOnGroundTime <= 0)
		Drag(dragAmount);
        else
		Drag(frictionAmount);
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
		RB.AddForce(Vector2.down * RB.velocity.y * (1 - data.jumpCutMultiplier), ForceMode2D.Impulse);
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

	private bool CanWallJump()
    {
		return LastPressedJumpTime > 0 && LastOnWallTime > 0 && LastOnGroundTime <= 0 && (!IsWallJumping ||
			 (LastOnWallRightTime > 0 && _lastWallJumpDir == 1) || (LastOnWallLeftTime > 0 && _lastWallJumpDir == -1));
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


}
