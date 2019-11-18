using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raycaster : MonoBehaviour
{
    public int layermask;
    public LineRenderer line;
    //public Material matNotHit;
    public Material matHit;
    public int minDist;
    public int maxDist;

    public enum Controller { Left, Right }
    public Controller controller;

    private bool pickUp;
    private Vector2 joystick;
    private float targetMoveDist = 0;
    private Vector3 endPos;
    private bool lVib, rVib;

    // Variables for accesing fresnel
    public Material material;
    MeshRenderer meshRenderer;
    public float fresnelPower;
    public float fresnelIntensity;
    public float fresnelPowerMin = 5;
    public float fresnelPowerMax = 1.5f;
    public float fresnelIntensityMin = 0;
    public float fresnelIntensityMax = 1;



    //public GameObject hitObject;

    // Start is called before the first frame update
    void Start()
    {
        line = gameObject.GetComponent<LineRenderer>();
        layermask = 1 << 8;
        GameObject track = GameObject.FindWithTag("Track");
        meshRenderer = track.GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        meshRenderer.material.SetFloat("_FresnelIntensity", fresnelIntensity = fresnelIntensityMin);
        Vector3 forward = transform.TransformDirection(Vector3.forward) * 25;
        endPos = transform.position + forward;

        line.SetPosition(0, transform.position);
        line.SetPosition(1, endPos);

        RaycastHit hit;
        if (Physics.Raycast(transform.position, forward, out hit, Mathf.Infinity, layermask)) {
            StartCoroutine(Vibrate(controller, 0.05f));
            FresnelHighlight(controller, hit);
            //line.material = matHit;
            if (hit.transform.gameObject != null) {
                HandleInput(controller, hit);
                HandleThumbstick(controller, hit);
            } else {
                DeParent();
            }
            //} else if(pickUp) {
            //line.material = matHit;
            //Debug.Log("No hit");
            //} else if (Physics.Raycast(transform.position, forward, Mathf.Infinity, layermask)) {
            //line.material = matNotHit;
            // THIS FUCKS IT UP
            //meshRenderer.material.SetFloat("_FresnelIntensity", fresnelIntensity = fresnelIntensityMin);
            /*
            if(controller == Controller.Left) {
              lVib = false;
            } else if (controller == Controller.Right) {
              rVib = false;
            }
            */
        }
    }

    void HandleThumbstick(Controller controller, RaycastHit hit) {
        GameObject target = transform.Find("Grabbed Track").gameObject;
        float distance = Vector3.Distance(target.transform.position, transform.position);
        if (this.controller == Controller.Left && OVRInput.Get(OVRInput.RawButton.LIndexTrigger)) {
            joystick = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
            CheckThumbControls(target, distance);
        }

        if (this.controller == Controller.Right && OVRInput.Get(OVRInput.RawButton.RIndexTrigger)) {
            joystick = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
            CheckThumbControls(target, distance);
        }
    }


    void HandleInput(Controller controller, RaycastHit hit) {
        // LEFT HAND

        if (this.controller == Controller.Left) {
            //meshRenderer.material.SetFloat("_FresnelIntensity", fresnelIntensity = fresnelIntensityMax);
            if (OVRInput.Get(OVRInput.RawButton.LIndexTrigger) && !pickUp) {
                pickUp = true;
                meshRenderer.material.SetFloat("_FresnelPower", fresnelPower = fresnelPowerMax);
                //hit.transform.gameObject.GetComponent<Renderer>().material = matHit;
                if (hit.transform.gameObject.tag == "Track")
                    hit.transform.gameObject.name = "Grabbed Track";
                hit.transform.parent = gameObject.transform;
            } else if (!OVRInput.Get(OVRInput.RawButton.LIndexTrigger)) {
                /*foreach(Transform child in transform) {
                  if(child.tag == "Track") {
                    child.transform.parent = null;
                    //child.GetComponent<Renderer>().material = matNotHit;
                    meshRenderer.material.SetFloat("_FresnelPower", fresnelPower = fresnelPowerMin);
                  }
                } */
                DeParent();
                pickUp = false;
            }
        }
        // RIGHT HAND
        if (this.controller == Controller.Right) {
            //meshRenderer.material.SetFloat("_FresnelIntensity", fresnelIntensity = fresnelIntensityMax);
            if (OVRInput.Get(OVRInput.RawButton.RIndexTrigger) && !pickUp) {
                pickUp = true;
                meshRenderer.material.SetFloat("_FresnelPower", fresnelPower = fresnelPowerMax);
                //hit.transform.gameObject.GetComponent<Renderer>().material = matHit;
                if (hit.transform.gameObject.tag == "Track")
                    hit.transform.gameObject.name = "Grabbed Track";
                hit.transform.parent = gameObject.transform;
            } else if (!OVRInput.Get(OVRInput.RawButton.RIndexTrigger)) {
                DeParent();
                pickUp = false;
            }
        }
    }

    void DeParent() {
        foreach (Transform child in transform) {
            if (child.tag == "Track") {
                child.transform.parent = null;
                //child.GetComponent<Renderer>().material = matNotHit;
                meshRenderer.material.SetFloat("_FresnelPower", fresnelPower = fresnelPowerMin);
            }
        }
    }

    void CheckThumbControls(GameObject target, float distance) {
        if (distance > minDist && distance < maxDist) {
            if (joystick.y > 0) {
                target.transform.position = Vector3.MoveTowards(target.transform.position, endPos, 10 * (joystick.y * Time.deltaTime));
            } else if (joystick.y < 0) {
                target.transform.position = Vector3.MoveTowards(target.transform.position, transform.position, 10 * (-joystick.y * Time.deltaTime));
            }
        } else if (distance < minDist && joystick.y > 0) {
            target.transform.position = Vector3.MoveTowards(target.transform.position, endPos, 10 * (joystick.y * Time.deltaTime));
        } else if (distance > maxDist && joystick.y < 0) {
            target.transform.position = Vector3.MoveTowards(target.transform.position, transform.position, 10 * (-joystick.y * Time.deltaTime));
        }
    }


    IEnumerator Vibrate(Controller controller, float seconds) {
        if (this.controller == Controller.Left /*&& !lVib*/) {
            lVib = true;
            OVRInput.SetControllerVibration(1, 0.5f, OVRInput.Controller.LTouch);
            yield return new WaitForSeconds(seconds);
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
        } else if (this.controller == Controller.Right /*&& !rVib*/) {
            rVib = true;
            OVRInput.SetControllerVibration(1, 0.5f, OVRInput.Controller.RTouch);
            yield return new WaitForSeconds(seconds);
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
        }
    }

    void FresnelHighlight(Controller controller, RaycastHit hit)
    {
        if (this.controller == Controller.Left || this.controller == Controller.Right)
        {
            meshRenderer.material.SetFloat("_FresnelIntensity", fresnelIntensity = fresnelIntensityMax);
        }
    }
}

// BUGS:
// When track is grabbed with both controllers at the same time the app breaks, allowing the right controller to control the object when it is grabbed with the left controller
// Controllers have constant vibration upon frame-drops