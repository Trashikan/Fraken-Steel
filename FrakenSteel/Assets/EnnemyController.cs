using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnnemyController : MonoBehaviour
{
    Rigidbody2D rb;

    public float speed;

    public Transform[] PatrolPoints;

    private Transform target;
    private int CurrentPoint = 0;

    bool Idle;
    public float timeBetweenIdle = 7;
    public int IdleProbability = 100;
    public int minimumTimeIdle = 2;
    public int maximumTimeIdle = 5;

    float _lastIdle;

    private void Start() {
        rb = GetComponent<Rigidbody2D>();
        target = PatrolPoints[0];
    }


    private void Update() {
        if(!Idle){
            Vector3 dir = (target.position - transform.position).normalized;
            transform.Translate(dir *speed *Time.deltaTime, Space.World);

            if(Vector3.Distance(transform.position, target.position) < 0.4f)
            {
               CurrentPoint = (CurrentPoint + 1) % PatrolPoints.Length;
                target = PatrolPoints[CurrentPoint];
                GetComponent<SpriteRenderer>().flipX = !GetComponent<SpriteRenderer>().flipX;
            }
            
            if(Random.Range(0, IdleProbability) == 0 && _lastIdle + timeBetweenIdle < Time.time){
                StartCoroutine(goIdle());
            }
        }
    }

    IEnumerator goIdle()
    {
        Idle = true;
        _lastIdle = Time.time;
        yield return new WaitForSeconds(Random.Range(minimumTimeIdle,maximumTimeIdle));
        Idle = false;
    }
}
