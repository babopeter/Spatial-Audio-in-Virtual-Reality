using UnityEngine;
using System.Collections;

public class CalculateAngle : MonoBehaviour {

public float azimuth;
public float elevation;

public Transform target;

public Vector3 dir;

public float camRot;
public Camera cam;


void Update() {

    if(!target) return;

    camRot = cam.transform.rotation.eulerAngles.y;

    var myPos = transform.position;

    Vector3 direction = (myPos - target.position).normalized;

    azimuth = ((Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg) + 360 - camRot) % 360;

    elevation = Mathf.Atan2 (direction.y, Mathf.Sqrt(direction.x * direction.x + direction.z * direction.z)) * Mathf.Rad2Deg;

    // dir = Quaternion.Euler(azimuth, elevation, 0) * Vector3.forward;

    dir.x = azimuth;
    dir.y = elevation;
}


public static Vector3 GetDirection(float aAzimuth, float aElevation) {
	// aAzimuth *= Mathf.Deg2Rad;
	// aElevation *= Mathf.Deg2Rad;

	float c = Mathf.Cos(aElevation);
	return new Vector3(Mathf.Sin(aAzimuth) * c, Mathf.Sin(aElevation), Mathf.Cos(aAzimuth) * c);
}
}
