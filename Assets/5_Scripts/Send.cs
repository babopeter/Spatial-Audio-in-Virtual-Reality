using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Send : MonoBehaviour
{
    public OscOut oscOut;
    public float xAngle;
    public float yAngle;
    public float MyDistance;
    public float DistanceL;
    public float DistanceR;
    public bool switchSounds;
    public float lastSend;
    public int port;

    // Start is called before the first frame update
    void Start()
    {

        if (!oscOut) oscOut = gameObject.AddComponent<OscOut>();
        oscOut.Open(port, "172.20.10.7");
        oscOut.Send("OSC connection established");
        xAngle = gameObject.GetComponent<Calculations>().azimuth;
        yAngle = gameObject.GetComponent<Calculations>().elevation;
        MyDistance = gameObject.GetComponent<Calculations>().Distance_;
        DistanceL = gameObject.GetComponent<GetAngle>().DistanceL;
        DistanceR = gameObject.GetComponent<GetAngle>().DistanceR;
        //switchSounds = gameObject.GetComponent<SwitchSound>().buttonPressed;


    }

    // Update is called once per frame
    void Update()
    {
        oscOut.Send("x: ", transform.position.x);
        oscOut.Send("y: ", transform.position.y);
        oscOut.Send("z: ", transform.position.z);

        xAngle = getXAngle();
        yAngle = getYAngle();
        MyDistance = getMyDistance();
        DistanceL = getLeftDistance();
        DistanceR = getRightDistance();
        //switchSounds = getSwitchSound();

        oscOut.Send("angle: ", xAngle);
        oscOut.Send("y angle: ", yAngle);
        //  oscOut.Send("Distance", MyDistance);
        oscOut.Send("Left: ", DistanceL);
        oscOut.Send("Right: ", DistanceR);
        /*if (switchSounds && ((Time.time * 1000) - lastSend) > 4200)
        {
            oscOut.Send("Sound", switchSounds);
            lastSend = Time.time * 1000;
            Debug.Log("Test");
        }
        */

    }

    float getXAngle()
    {
        float angleValue = gameObject.GetComponent<Calculations>().azimuth;
        return angleValue;
    }

    float getYAngle()
    {
        float yAngleValue = gameObject.GetComponent<Calculations>().elevation;
        return yAngleValue;
    }

    float getMyDistance()
    {
        float getDistance = gameObject.GetComponent<Calculations>().Distance_;
        return getDistance;
    }

     float getLeftDistance() {
       float leftDistance = gameObject.GetComponent<Calculations>().DistanceL;
       return leftDistance;
     }

     float getRightDistance() {
       float rightDistance = gameObject.GetComponent<Calculations>().DistanceR;
       return rightDistance;
     }

}
