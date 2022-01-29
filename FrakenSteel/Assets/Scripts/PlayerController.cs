using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //variable that can be modfied in inspector
    [Header("Jump")]
    public float jumpForce = 10;
    public int extraJumpAmount;
    public Transform[] groundChecks; 
    public LayerMask ground;

    [Header("Walk")]
    public float Acceleration = 90;
    public float Deceleration = 60;
    public float moveClamp = 13;

    private Rigidbody2D rb;

    private float horizontalVelocity;
    private float verticalVelocity;


    private float moveInputX;
    private bool JumpDown;
    private bool JumpUp;

    private bool facingRight = true;
    private Vector3 playerScale;
    
    public bool isGrounded;
    private int extraJump; 
    bool Jumping;

    


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        GetInput();
        CalculateGrounded();
        CalculateJump();
        CalculateMovement();
        if (isGrounded == true)
            extraJump = extraJumpAmount;
    }

    private void GetInput()
    {
        JumpDown = Input.GetButtonDown("Jump");
        JumpUp = Input.GetButtonUp("Jump");
        moveInputX = Input.GetAxisRaw("Horizontal");
    }

    void CalculateJump(){
        if (JumpDown && isGrounded == true){
            rb.AddForce(Vector2.up * jumpForce);
        }
        else if (extraJump > 0 && JumpDown){
            rb.AddForce(Vector2.up * jumpForce);
            extraJump--;
        }
    }

    void CalculateMovement(){
        if (moveInputX != 0)
        {
            horizontalVelocity += moveInputX * Acceleration * Time.deltaTime;

            horizontalVelocity = Mathf.Clamp(horizontalVelocity, -moveClamp, moveClamp);
        }
        else
        {
            if(!Jumping){
                Debug.Log("slowing down..");
                horizontalVelocity = Mathf.MoveTowards(horizontalVelocity, 0, Deceleration *Time.deltaTime);
            }
        }
        rb.velocity = new Vector2(horizontalVelocity, rb.velocity.y);

    }


    void FixedUpdate()
    {   
        //Flips the player sprite towards move direction
        if ((facingRight == false && moveInputX > 0f) || (facingRight == true && moveInputX < 0f))
            Flip();
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
