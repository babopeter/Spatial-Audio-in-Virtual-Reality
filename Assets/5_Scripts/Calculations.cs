using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Calculations : MonoBehaviour
{
    public float azimuth;
    public float elevation;
    public Transform target;
    public Vector3 dir;
    public float camRot;
    public Camera cam;
    Renderer rend, rend2;

    // ------- CALCULATE DISTANCE VARIABLES ------------
    public GameObject Source1;
    public GameObject Source2;
    public float Distance_;
    public float DistanceL;
    public float DistanceR;

    public float distanceOfSides;
    Vector3 center;
    Vector3 leftSide;
    Vector3 rightSide;
    Vector3 myCenter;
    public Transform myObject;
    public Transform head;

    // Start is called before the first frame update
    void Start()
    {
        rend = myObject.GetComponent<Renderer>();
        rend2 = target.GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        // ----- Calculate Angle part
        if (!target) return;
        camRot = cam.transform.rotation.eulerAngles.y;
        var myPos = transform.position;
        Vector3 direction = (myPos - target.position).normalized;
        //	angle = ((Mathf.Atan2(toOther.z, toOther.x) * Mathf.Rad2Deg) + 270 + camRot) % 360;
        azimuth = Mathf.Abs(((Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg) + 720 - camRot)) % 360;
        elevation = Mathf.Atan2(direction.y, Mathf.Sqrt(direction.x * direction.x + direction.z * direction.z)) * Mathf.Rad2Deg;
        // dir = Quaternion.Euler(azimuth, elevation, 0) * Vector3.forward;
        dir.x = azimuth;
        dir.y = elevation;

        // ----- Calculate Distance part
        Distance_ = Vector3.Distance(Source1.transform.position, Source2.transform.position);

        // ----- Calculate Distance L and Distance R part
        center = rend.bounds.center;
        float radius = rend.bounds.extents.magnitude;
        leftSide = center;
        leftSide.x = center.x - (radius / 2);
        rightSide = center;
        rightSide.x = center.x + (radius / 2);
        myCenter = rend2.bounds.center;

        DistanceL = Vector3.Distance(head.transform.position, leftSide);
        DistanceR = Vector3.Distance(head.transform.position, rightSide);

    }


    public static Vector3 GetDirection(float aAzimuth, float aElevation)
    {
        // aAzimuth *= Mathf.Deg2Rad;
        // aElevation *= Mathf.Deg2Rad;
        float c = Mathf.Cos(aElevation);
        return new Vector3(Mathf.Sin(aAzimuth) * c, Mathf.Sin(aElevation), Mathf.Cos(aAzimuth) * c);
    }
}
