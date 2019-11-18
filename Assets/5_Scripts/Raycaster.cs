using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raycaster : MonoBehaviour
{
  public int layermask;
  public LineRenderer line;
  public Material matNotHit;
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
    
    public Material material;
    MeshRenderer meshRenderer;
    public float fresnelPower;



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
      Vector3 forward = transform.TransformDirection(Vector3.forward) * 25;
      endPos = transform.position + forward;

      line.SetPosition(0, transform.position);
      line.SetPosition(1, endPos);

      RaycastHit hit;
      if(Physics.Raycast(transform.position, forward, out hit, Mathf.Infinity, layermask)) {
        StartCoroutine(Vibrate(controller, 0.05f));
        line.material = matHit;
        if(hit.transform.gameObject != null) {
          HandleInput(controller, hit);
          HandleThumbstick(controller, hit);
        } else {
          DeParent();
        }
      } else if(pickUp) {
        line.material = matHit;
        //Debug.Log("No hit");
      } else {
        line.material = matNotHit;
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
        if(this.controller == Controller.Left && OVRInput.Get(OVRInput.RawButton.LIndexTrigger)) {
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
      if(this.controller == Controller.Left) {
        if(OVRInput.Get(OVRInput.RawButton.LIndexTrigger) && !pickUp) {
          pickUp = true;
          meshRenderer.material.SetFloat("_FresnelPower", fresnelPower = 2);
          //hit.transform.gameObject.GetComponent<Renderer>().material = matHit;
      if (hit.transform.gameObject.tag == "Track")
          hit.transform.gameObject.name = "Grabbed Track";
          hit.transform.parent = gameObject.transform;
        } else if(!OVRInput.Get(OVRInput.RawButton.LIndexTrigger)) {
          foreach(Transform child in transform) {
            if(child.tag == "Track") {
              child.transform.parent = null;
              //child.GetComponent<Renderer>().material = matNotHit;
              meshRenderer.material.SetFloat("_FresnelPower", fresnelPower = 10);
            }
          }
          pickUp = false;
        }
    }
    // RIGHT HAND
    if (this.controller == Controller.Right) {
      if(OVRInput.Get(OVRInput.RawButton.RIndexTrigger) && !pickUp) {
        pickUp = true;
        //hit.transform.gameObject.GetComponent<Renderer>().material = matHit;
        if(hit.transform.gameObject.tag == "Track")
        hit.transform.gameObject.name = "Grabbed Track";
        hit.transform.parent = gameObject.transform;
      } else if(!OVRInput.Get(OVRInput.RawButton.RIndexTrigger)) {
        DeParent();
        pickUp = false;
      }
    }
  }

  void DeParent() {
    foreach(Transform child in transform) {
      if(child.tag == "Track") {
        child.transform.parent = null;
        //child.GetComponent<Renderer>().material = matNotHit;
      }
    }
  }

  void CheckThumbControls(GameObject target, float distance) {
    if(distance > minDist && distance < maxDist) {
      if(joystick.y > 0) {
        target.transform.position = Vector3.MoveTowards(target.transform.position, endPos, 10 * (joystick.y * Time.deltaTime));
      } else if (joystick.y < 0) {
        target.transform.position = Vector3.MoveTowards(target.transform.position, transform.position, 10 * (-joystick.y * Time.deltaTime));
      }
    } else if(distance < minDist && joystick.y > 0) {
      target.transform.position = Vector3.MoveTowards(target.transform.position, endPos, 10 * (joystick.y * Time.deltaTime));
    } else if (distance > maxDist && joystick.y < 0) {
      target.transform.position = Vector3.MoveTowards(target.transform.position, transform.position, 10 * (-joystick.y * Time.deltaTime));
    }
  }

  // FIX ISSUE WITH CONSTANT VIBRATION UPON FRAME-DROPS
  IEnumerator Vibrate(Controller controller, float seconds) {
    if(this.controller == Controller.Left /*&& !lVib*/) {
      lVib = true;
      OVRInput.SetControllerVibration (1, 0.5f, OVRInput.Controller.LTouch);
      yield return new WaitForSeconds(seconds);
      OVRInput.SetControllerVibration (0, 0, OVRInput.Controller.LTouch);
    } else if(this.controller == Controller.Right /*&& !rVib*/) {
      rVib = true;
      OVRInput.SetControllerVibration (1, 0.5f, OVRInput.Controller.RTouch);
      yield return new WaitForSeconds(seconds);
      OVRInput.SetControllerVibration (0, 0, OVRInput.Controller.RTouch);
    }
  }
}
