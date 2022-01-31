using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGrapple : MonoBehaviour
{
    public bool isGrapping;

    [Space(3)]
    [Header("============= Player Properties =============")]
    [Space(3)]
    public Transform playerBody;
    public Rigidbody2D rb;
    public SpringJoint2D springJoint;

    [Space(3)]
    [Header("============= Rope Properties =============")]
    [Space(3)]
    public LineRenderer line;
    public float minDistance;
    [Range(0.1f, 5f)] public float grabSpeed = 1;

    [Space(3)]
    [Header("============= Hook Properties =============")]
    [Space(3)]

   
    public Transform currentHook;

    private RaycastHit2D hookHit;

    private void Start()
    {
       
        isGrapping = false;

      
        line.positionCount = 2;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
          
            GrappingStart();
        }
        else if (Input.GetMouseButton(0))
        {
            
            Grapping();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            
            GrappingEnd();
        }
    }

    private void GrappingStart()
    {
        
        isGrapping = true;

       
        Vector3 clickedPos = Camera.main.ScreenToWorldPoint(Input.mousePosition + Vector3.forward * 10);

        
        Collider2D[] colliders;
        colliders = Physics2D.OverlapCircleAll(clickedPos, 25, LayerMask.GetMask("Hook"));

        
        currentHook = GetNearestHook(colliders, clickedPos);

        
        if (currentHook == null)
        {
            isGrapping = false;
            return;
        }

        
        hookHit = Physics2D.Raycast(transform.position, (currentHook.position - transform.position).normalized, 100, LayerMask.GetMask("Hook"));

        
        springJoint.connectedAnchor = hookHit.point;

       
        springJoint.enabled = true;

       
        line.enabled = true;
    }

    private void Grapping()
    {
       
        line.SetPosition(0, transform.position);
        line.SetPosition(1, hookHit.point);

        
        springJoint.distance = Mathf.Lerp(springJoint.distance, minDistance, Time.deltaTime * grabSpeed);

        
        rb.velocity *= 0.995f;
    }

    private void GrappingEnd()
    {
       
        isGrapping = false;

        
        springJoint.enabled = false;

       
        line.enabled = false;

        
        line.SetPosition(0, transform.position);
        line.SetPosition(1, transform.position);
    }

    
    private Transform GetNearestHook(Collider2D[] colliders, Vector3 clickedPos)
    {
        
        int index = 0;

       
        float dist = Mathf.Infinity;
        float minDist = Mathf.Infinity;

        
        for (int i = 0; i < colliders.Length; i++)
        {
            dist = Vector3.Distance(colliders[i].transform.position, clickedPos);
            if (dist < minDist)
            {
                minDist = dist;
                index = i;
            }
        }

        
        return colliders[index].transform;
    }
}
