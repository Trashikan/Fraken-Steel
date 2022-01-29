using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{

    public GameObject startPos;
    public GameObject endPos;
    public int speed;
    private Vector2 Pos1;
    private Vector2 Pos2;
    private Rigidbody2D rb;
    Rigidbody2D PlayerRb;
    bool PlayerOnPlatform;

    void Start()
    {
        Pos1 = startPos.transform.position;
        Pos2 = endPos.transform.position;
        rb = GetComponent<Rigidbody2D>();
        transform.position = Pos1;
        StartCoroutine(MoveTo(Pos2));
    }

    void Update()
    {
        if(transform.position == (Vector3)Pos1)
            StartCoroutine(MoveTo(Pos2));
        if (transform.position == (Vector3)Pos2)
            StartCoroutine(MoveTo(Pos1));
    }

    IEnumerator MoveTo(Vector2 target){
        Vector2 position = transform.position;
        float time = 0f;
        while(transform.position != (Vector3)target)
        {
            rb.MovePosition(Vector2.Lerp(position, target, (time / Vector2.Distance(position, target)) * speed));
            time += Time.deltaTime;
            yield return null;
        }
    }

    private void OnCollisionEnter2D(Collision2D other) {
        if(other.gameObject.tag == "Player"){
            PlayerRb = other.gameObject.GetComponent<Rigidbody2D>();
            PlayerOnPlatform = true;
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            PlayerRb = null;
            PlayerOnPlatform = false;
        }
    }

    void FixedUpdate()
    {
        if (PlayerOnPlatform)
        {
            PlayerRb.velocity = PlayerRb.velocity + rb.velocity;
        }
    }
}
