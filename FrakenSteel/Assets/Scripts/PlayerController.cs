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
    
    [SerializeField] private float coyoteTime;
    [SerializeField] private LayerMask _groundLayer;
    
    public bool IsFacingRight { get; private set; }
	public bool IsJumping { get; private set; }
    public bool IsDashing { get; private set; }
	public float LastOnGroundTime { get; private set; }
    
    private int _dashesLeft;
	private float _dashStartTime;
	private Vector2 _lastDashDir;
    public float dashAttackTime;
    public float dashEndTime;
    
    public float LastPressedJumpTime { get; private set; }
	public float LastPressedDashTime { get; private set; }
	public float dragAmount;
	public float frictionAmount;
	public float dashAttackDragAmount;
    
    RigidBody2D rb;

    public float gravityScale = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        isFacingRight = true;
    }

    private void Update()
    {
        Timers();
        GetInput();
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
        JumpDown = Input.GetButtonDown("Jump");
        JumpUp = Input.GetButtonUp("Jump");
        moveInputX = Input.GetAxisRaw("Horizontal");
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


    private void Flip()
    {
        facingRight = !facingRight;
        playerScale = transform.localScale;
        playerScale.x = playerScale.x * -1;
        transform.localScale = playerScale;
    }

}
