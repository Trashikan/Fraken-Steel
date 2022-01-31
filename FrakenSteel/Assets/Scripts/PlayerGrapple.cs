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

    // Current hook transform
    public Transform currentHook;

    // Hook hit will return hook border point for grapping
    private RaycastHit2D hookHit;

    private void Start()
    {
        // Set isGrapping boolean false
        isGrapping = false;

        // Set Line pointscount to 2
        // It means 2 points available for rope 1st start and 2nd end
        line.positionCount = 2;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Grapping Start Stuff
            GrappingStart();
        }
        else if (Input.GetMouseButton(0))
        {
            // Whiel Grapping is Running
            Grapping();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            // Grapping End Stuff
            GrappingEnd();
        }
    }

    private void GrappingStart()
    {
        // True the boolean
        isGrapping = true;

        // get Clicked Position with reduce camera distance
        Vector3 clickedPos = Camera.main.ScreenToWorldPoint(Input.mousePosition + Vector3.forward * 10);

        // Store colliders which are overlap the area
        Collider2D[] colliders;
        colliders = Physics2D.OverlapCircleAll(clickedPos, 25, LayerMask.GetMask("Hook"));

        // Get Neareast Hook
        currentHook = GetNearestHook(colliders, clickedPos);

        // If there is no hooks found then no Grapping
        if (currentHook == null)
        {
            isGrapping = false;
            return;
        }

        // If current is available then raycast to hook position and grab the corner of hook
        hookHit = Physics2D.Raycast(transform.position, (currentHook.position - transform.position).normalized, 100, LayerMask.GetMask("Hook"));

        // Set Spring Joint to hookhit point
        springJoint.connectedAnchor = hookHit.point;

        // Trun on the spring joint
        springJoint.enabled = true;

        // If Current hook is Available then Enable true line renderer
        line.enabled = true;
    }

    private void Grapping()
    {
        // Set 2 points of Line
        // 1st is player and 2nd is Hook
        line.SetPosition(0, transform.position);
        line.SetPosition(1, hookHit.point);

        // Reduce Distance Between Hook And Player
        springJoint.distance = Mathf.Lerp(springJoint.distance, minDistance, Time.deltaTime * grabSpeed);

        // Reduce the rigibody speed to stop spining the player around the hook
        rb.velocity *= 0.995f;
    }

    private void GrappingEnd()
    {
        // False the boolean
        isGrapping = false;

        // Trun off the spring joint
        springJoint.enabled = false;

        // Enable False line renderer
        line.enabled = false;

        // Reset The Line points Positions
        line.SetPosition(0, transform.position);
        line.SetPosition(1, transform.position);
    }

    // It will Return Nearest hook from clicked position and with colliders transform
    private Transform GetNearestHook(Collider2D[] colliders, Vector3 clickedPos)
    {
        // Selected Index
        int index = 0;

        // Set Infinity to determine the minimum distance
        float dist = Mathf.Infinity;
        float minDist = Mathf.Infinity;

        // Finding minimum distance from colliders transform to clicked position
        for (int i = 0; i < colliders.Length; i++)
        {
            dist = Vector3.Distance(colliders[i].transform.position, clickedPos);
            if (dist < minDist)
            {
                minDist = dist;
                index = i;
            }
        }

        // return transform of hook
        return colliders[index].transform;
    }
}
