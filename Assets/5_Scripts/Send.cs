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
    public bool solo;
    public bool lastSolo;
    public bool soloRef;
    public bool lastSoloRef;

    public GameObject controllerManager;

    public float totalSumOfDistance;

    public float kickDistance, snareDistance, percDistance, bassDistance, melodyDistance, arpegDistance, choirDistance;

    // Start is called before the first frame update
    void Start()
    {

        if (!oscOut) oscOut = gameObject.AddComponent<OscOut>();
        oscOut.Open(port, "192.168.43.59");
        oscOut.Send("OSC connection established");
        /*
        xAngle = gameObject.GetComponent<Calculations>().azimuth;
        yAngle = gameObject.GetComponentx<Calculations>().elevation;
        MyDistance = gameObject.GetComponent<Calculations>().Distance_;
        DistanceL = gameObject.GetComponent<GetAngle>().DistanceL;
        DistanceR = gameObject.GetComponent<GetAngle>().DistanceR;
        //switchSounds = gameObject.GetComponent<SwitchSound>().buttonPressed;
        */
        
        solo = lastSolo;
        soloRef = lastSoloRef;
    }

    // Update is called once per frame
    void Update()
    {
        

        //SendSolo();
        //SendSoloRef();
        
        if(solo != lastSolo) {
            oscOut.Send("Solo: ", solo);
        }
        lastSolo = solo;
        
        oscOut.Send("x: ", transform.position.x);
        oscOut.Send("y: ", transform.position.y);
        oscOut.Send("z: ", transform.position.z);

        xAngle = getXAngle();
        yAngle = getYAngle();
        MyDistance = getMyDistance();
        DistanceL = getLeftDistance();
        DistanceR = getRightDistance();
       // solo = getMySolo();
        soloRef = getMySoloRef();
        //switchSounds = getSwitchSound();

        oscOut.Send("angle: ", xAngle);
        oscOut.Send("y angle: ", yAngle);
        //oscOut.Send("Distance: ", MyDistance);
        oscOut.Send("Left: ", DistanceL);
        oscOut.Send("Right: ", DistanceR);

        if(controllerManager.GetComponent<sumOfDistances>().retrieveSum == true) {
            
            totalSumOfDistance = getTotalSumOfDistance();
            kickDistance = getKickDistance();
            snareDistance = getSnareDistance();
            percDistance = getPercDistance();
            bassDistance = getBassDistance();
            melodyDistance = getMelodyDistance();
            arpegDistance = getArpegDistance();
            choirDistance = getChoirDistance();

            // Debug.Log("kick: " + kickDistance);
            oscOut.Send("Total Sum: ", totalSumOfDistance);
            oscOut.Send("Kick: ", kickDistance);
            oscOut.Send("Snare: ", snareDistance);
            oscOut.Send("Perc: ", percDistance);
            oscOut.Send("Bass: ", bassDistance);
            oscOut.Send("Melody: ", melodyDistance);
            oscOut.Send("Arpeg: ", arpegDistance);
            oscOut.Send("Choir: ", choirDistance);

            
        }
        /*if (switchSounds && ((Time.time * 1000) - lastSend) > 4200)
        {
            oscOut.Send("Sound", switchSounds);
            lastSend = Time.time * 1000;
            Debug.Log("Test");
        }
        */

    }

    public void SendSolo() {
        if(solo != lastSolo) {
            oscOut.Send("Solo: ", solo);
        }
        lastSolo = solo;

        //if(soloRef != lastSoloRef) {
        //    oscOut.Send("SoloRef: ", soloRef);
        //}
        //lastSoloRef = soloRef;
    }

    void SendSoloRef() {
        if(soloRef != lastSoloRef) {
            oscOut.Send("SoloRef: ", soloRef);
        }
        lastSoloRef = soloRef;

        if(soloRef != lastSoloRef) {
            oscOut.Send("SoloRef: ", soloRef);
        }
        lastSoloRef = soloRef;
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

     /*bool getMySolo() {
         bool getSolo = gameObject.GetComponent<Calculations>().solo;
         return getSolo;
     }*/

      bool getMySoloRef() {
         bool getSoloRef = Raycaster.soloRef;
         return getSoloRef;
     }

    float getTotalSumOfDistance() {
         float totalSum = controllerManager.GetComponent<sumOfDistances>().sum;
         return totalSum;
     }

     float getKickDistance() {
         float kickSum = controllerManager.GetComponent<sumOfDistances>().kick;
         return kickSum;
     }

    float getSnareDistance() {
         float snareSum = controllerManager.GetComponent<sumOfDistances>().snare;
         return snareSum;
     }

    float getPercDistance() {
         float percSum = controllerManager.GetComponent<sumOfDistances>().perc;
         return percSum;
     }

    float getBassDistance() {
         float bassSum = controllerManager.GetComponent<sumOfDistances>().bass;
         return bassSum;
     }

    float getMelodyDistance() {
        float melodySum = controllerManager.GetComponent<sumOfDistances>().melody;
        return melodySum;
    }

    float getArpegDistance() {
        float arpegSum = controllerManager.GetComponent<sumOfDistances>().arpeg;
        return arpegSum;
    }

    float getChoirDistance() {
        float choirSum = controllerManager.GetComponent<sumOfDistances>().choir;
        return choirSum;
    }

}