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

        Debug.Log(rb.velocity);
    }

    IEnumerator MoveTo(Vector2 target)
    {
        Vector2 startPosition = rb.gameObject.transform.position;
        float time = 0f;

        while (rb.position != target)
        {
            rb.gameObject.transform.position = Vector2.Lerp(startPosition, target, (time / Vector2.Distance(startPosition, target)) * speed);
            time += Time.deltaTime;
            yield return null;
        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        col.gameObject.transform.SetParent(gameObject.transform, true);
    }

    void OnCollisionExit2D(Collision2D col)
    {
        col.gameObject.transform.parent = null;
    }
}
