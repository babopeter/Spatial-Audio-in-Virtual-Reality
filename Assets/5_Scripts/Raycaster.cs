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
    public bool solo;
    [ColorUsageAttribute(true,true,0f,8f,0.125f,3f)]
    public Color mainFresnelColor;
    [ColorUsageAttribute(true,true,0f,8f,0.125f,3f)]
    public Color soloFresnelColor;
    public Color mainCol;
    public Color soloCol;
    public float minElev;
    public float minPlaneElev;

    public enum Controller { Left, Right }
    public Controller controller;

    private bool pickUp;
    private Vector2 joystick;
    private float targetMoveDist = 0;
    private Vector3 endPos;
    private bool lVib, rVib;
    private int totalGrabs;

    private GameObject lastHit = null;

    // Variables for accesing fresnel
    public Material material;
    MeshRenderer meshRenderer, lastMesh;
    public float fresnelPower;
    public float fresnelIntensity;
    public float fresnelPowerMin = 5;
    public float fresnelPowerMax = 1.5f;
    public float fresnelIntensityMin = 0;
    public float fresnelIntensityMax = 1;

    public static bool soloRef;

    public GameObject hitObject;

    // Start is called before the first frame update
    void Start()
    {
        line = gameObject.GetComponent<LineRenderer>();
        layermask = 1 << 8;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward) * 25;
        endPos = transform.position + forward;

        line.SetPosition(0, transform.position);
        line.SetPosition(1, endPos);
        

        CheckSoloRefInput();

        RaycastHit hit;
        if (Physics.Raycast(transform.position, forward, out hit, Mathf.Infinity, layermask)) {
            StartCoroutine(Vibrate(controller, 0.05f));

            Debug.Log(hit.transform.position);

            if(hit.transform.gameObject != lastHit && lastHit != null && !pickUp) {
                lastMesh = lastHit.GetComponent<MeshRenderer>();
                lastMesh.material.SetFloat("_FresnelIntensity", fresnelIntensity = fresnelIntensityMin);
            }

            if (hit.transform.gameObject.GetComponent<MeshRenderer>() != null && !pickUp)
            {
                lastHit = hit.transform.gameObject;
                meshRenderer = GetMeshRenderer(hit);
                meshRenderer.material.SetFloat("_FresnelIntensity", fresnelIntensity = fresnelIntensityMax);
            }
            if (hit.transform.gameObject != null) {
                HandleInput(controller, hit);
                CheckSoloInput(lastHit);
                HandleThumbstick(controller, hit);

                GameObject target = transform.Find("Grabbed Track").gameObject;
                Vector3 targetPos = target.transform.position;

                if(target.transform.position.x < 6 && target.transform.position.x > -6 && target.transform.position.z < 6 && target.transform.position.z > -6) {
                    if(target.transform.position.y < minElev) {
                    DeParent();
                    target.transform.position = new Vector3(targetPos.x, minElev + 0.15f, targetPos.z);
                    }
                } else {
                    if(target.transform.position.y < minPlaneElev) {
                    DeParent();
                    target.transform.position = new Vector3(targetPos.x, minPlaneElev + 0.15f, targetPos.z);
                    }
                }
            } else {
                DeParent();
            }
        } else {
            meshRenderer.material.SetFloat("_FresnelIntensity", fresnelIntensity = fresnelIntensityMin);
            meshRenderer = null;
            if(controller == Controller.Left)
                lVib = false;
            if(controller == Controller.Right)
                rVib = false;
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
            if ((OVRInput.Get(OVRInput.RawButton.LIndexTrigger) || Input.GetKey(KeyCode.E)) && !pickUp) {
                //hit.transform.gameObject.GetComponent<Renderer>().material = matHit;
                Parent(hit);
                pickUp = true;
            } else if (!OVRInput.Get(OVRInput.RawButton.LIndexTrigger) && !Input.GetKey(KeyCode.E)) {
                DeParent();
                pickUp = false;
            }
        }
        // RIGHT HAND
        if (this.controller == Controller.Right) {
            //meshRenderer.material.SetFloat("_FresnelIntensity", fresnelIntensity = fresnelIntensityMax);
            if ((OVRInput.Get(OVRInput.RawButton.RIndexTrigger) || Input.GetKey(KeyCode.Space)) && !pickUp) {
                Parent(hit);
                pickUp = true;
            } else if (!OVRInput.Get(OVRInput.RawButton.RIndexTrigger) && !Input.GetKey(KeyCode.Space)) {
                DeParent();
                pickUp = false;
            }
        }
    }

    void Parent(RaycastHit hit) {
        if(hit.transform.gameObject.tag == "Track") {
            meshRenderer.material.SetFloat("_FresnelPower", fresnelPower = fresnelPowerMax);
            hit.transform.gameObject.name = "Grabbed Track";
            hit.transform.parent = gameObject.transform;
            totalGrabs++;
        }
    }

    void DeParent() {
        foreach (Transform child in transform) {
            if(child.name == "CustomHandLeft" || child.name == "CustomHandRight") {
                child.transform.parent = null;
            }
            //if (child.name == "CustomHandLeft" || child.name == "CustomHandRight") {
                //child.transform.parent = null;
            if(child.tag == "Track") {
                child.transform.parent = null;
                MeshRenderer childRenderer = child.GetComponent<MeshRenderer>();
                childRenderer.material.SetFloat("_FresnelPower", fresnelPower = fresnelPowerMin);
                child.name = "Track " + totalGrabs;
            }
            //}
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
        if (this.controller == Controller.Left && !lVib) {
            lVib = true;
            OVRInput.SetControllerVibration(1, 0.5f, OVRInput.Controller.LTouch);
            yield return new WaitForSeconds(seconds);
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
        } else if (this.controller == Controller.Right && !rVib) {
            rVib = true;
            OVRInput.SetControllerVibration(1, 0.5f, OVRInput.Controller.RTouch);
            yield return new WaitForSeconds(seconds);
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
        }
    }

    MeshRenderer GetMeshRenderer(RaycastHit hit)
    {
        return hit.transform.gameObject.GetComponent<MeshRenderer>();
    }

    void CheckSoloInput(GameObject hit) {
        if(controller == Controller.Right) {
            if(OVRInput.GetDown(OVRInput.Button.One) || Input.GetKeyDown(KeyCode.A)) {
                Debug.Log("Test");
                solo = !hit.GetComponent<Send>().solo;
                hit.GetComponent<Send>().solo = solo;
                if(solo){
                    meshRenderer.material.SetColor("_FresnelColor", soloFresnelColor);
                    meshRenderer.material.SetColor("_BaseColor", soloCol);
                } else {
                    meshRenderer.material.SetColor("_BaseColor", mainCol);
                    meshRenderer.material.SetColor("_FresnelColor", mainFresnelColor);
                }
            }
        }

        if(controller == Controller.Left) {
            if(OVRInput.GetDown(OVRInput.Button.Three) || Input.GetKeyDown(KeyCode.D)) {
                solo = !hit.GetComponent<Send>().solo;
                hit.GetComponent<Send>().solo = solo;
                if(solo) {
                    meshRenderer.material.SetColor("_BaseColor", soloCol);
                    meshRenderer.material.SetColor("_FresnelColor", soloFresnelColor);
                } else {
                    meshRenderer.material.SetColor("_BaseColor", mainCol);
                    meshRenderer.material.SetColor("_FresnelColor", mainFresnelColor);
                }
            }
        }
    }

    void CheckSoloRefInput() {
        if(controller == Controller.Right) {
            if(OVRInput.GetDown(OVRInput.Button.Two) || Input.GetKeyDown(KeyCode.Y)) {
                Debug.Log("TestRef");
              soloRef = !soloRef;
              //trackToggle = !trackToggle;
              //trackSelection(trackToggle);
    
                /*if(transform.gameObject.GetComponent<Calculations>().soloRef){
                    meshRenderer.material.SetColor("_FresnelColor", soloCol);
                } else {
                    meshRenderer.material.SetColor("_FresnelColor", mainCol);
                }*/

           }
        }

        if(controller == Controller.Left) {
            if(OVRInput.GetDown(OVRInput.Button.Four) || Input.GetKeyDown(KeyCode.U)) {
                soloRef = !soloRef;
            
                /*if(hit.transform.gameObject.GetComponent<Calculations>().solo){
                    meshRenderer.material.SetColor("_FresnelColor", soloCol);
                } else {
                    meshRenderer.material.SetColor("_FresnelColor", mainCol);
                }*/
            }
      }
        
    }

    
}

// BUGS:
// When track is grabbed with both controllers at the same time the app breaks, allowing the right controller to control the object when it is grabbed with the left controller
// Controllers have constant vibration upon frame-drops