using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;

    public float speed = 5;

    private float moveInputX;
    private bool JumpDown;
    private bool JumpUp;

    private bool facingRight = true;
    private Vector3 playerScale;
    
    public float jumpForce = 10;
    private bool isGrounded;
    public Transform[] groundChecks; 
    public LayerMask ground;
    private int extraJump; 
    public int extraJumpAmount;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        GetInput();
        CalculateGrounded();
        ApplyJump();
        if (isGrounded == true)
            extraJump = extraJumpAmount;
    }

    private void GetInput()
    {
        JumpDown = Input.GetButtonDown("Jump");
        JumpUp = Input.GetButtonUp("Jump");
        moveInputX = Input.GetAxisRaw("Horizontal");
    }

    void ApplyJump(){
        if (JumpDown && isGrounded == true)
            Jump();
        else if (extraJump > 0 && JumpDown){
            Jump();
        extraJump--;
        }
    }

    void FixedUpdate()
    {   
        //Horizontal movement
        rb.velocity = new Vector2(moveInputX * speed, rb.velocity.y);

        //Flips the player sprite towards move direction
        if ((facingRight == false && moveInputX > 0f) || (facingRight == true && moveInputX < 0f))
            Flip();
    }

    private void Jump()
    {
        Debug.Log("Jumping");
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }

    private void CalculateGrounded(){
        foreach (Transform groundCheck in groundChecks)
        {
            if (Physics2D.OverlapCircle(groundCheck.transform.position, 0.01f, ground))
            {
                isGrounded = true;
                break;
            }
            else
                isGrounded = false;
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        playerScale = transform.localScale;
        playerScale.x = playerScale.x * -1;
        transform.localScale = playerScale;
    }
}
