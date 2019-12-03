using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenRaycaster : MonoBehaviour
{
  public float minDist;
  public float maxDist;
  public int layerMask = 1 << 8;
  [ColorUsageAttribute(true,true,0f,8f,0.125f,3f)]
  public Color mainFresnelCol;
  [ColorUsageAttribute(true,true,0f,8f,0.125f,3f)]
  public Color soloFresnelCol;
  public Color mainCol;
  public Color soloCol;
  public bool solo;
  public float minElev, minPlaneElev;

  private MeshRenderer meshRenderer, lastMesh;
  private Vector3 endPos;
  private bool pickUp;
  private int totalGrabs;
  private bool mouseReleased;

  private GameObject lastHit;

  public float fresnelPower;
  public float fresnelIntensity;
  public float fresnelPowerMin = 5;
  public float fresnelPowerMax = 1.5f;
  public float fresnelIntensityMin = 0;
  public float fresnelIntensityMax = 1;

    // Update is called once per frame
  void Update()
  {
    RaycastHit hit;
    Ray ray = new Ray(transform.position, transform.forward);

    endPos = transform.forward * 10000;

    if(Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask)) {
      if (hit.transform.gameObject.GetComponent<MeshRenderer>() != null && !pickUp) {
        meshRenderer = GetMeshRenderer(hit);
        meshRenderer.material.SetFloat("_FresnelIntensity", fresnelIntensity = fresnelIntensityMax);
      }

      if (hit.transform.gameObject != lastHit && lastHit != null && !pickUp)
      {
        Debug.Log("Not same");
        lastMesh = lastHit.GetComponent<MeshRenderer>();
        lastMesh.material.SetFloat("_FresnelIntensity", fresnelIntensity = fresnelIntensityMin);
      }
      lastHit = hit.transform.gameObject;
      Debug.Log(lastHit);

      if(hit.transform.gameObject != null) {
        CheckMouseInput(hit);
        CheckButtonInput();

        GameObject target = transform.Find("Grabbed Track").gameObject;
        Vector3 targetPos = target.transform.position;

        if(target.transform.position.x < 6 && target.transform.position.x > -6 && target.transform.position.z < 6 && target.transform.position.z > -6) {
          if(target.transform.position.y < minElev) {
            DeParent(hit);
            target.transform.position = new Vector3(targetPos.x, minElev + 0.15f, targetPos.z);
          }
        } else {
          if(target.transform.position.y < minPlaneElev) {
            DeParent(hit);
            target.transform.position = new Vector3(targetPos.x, minPlaneElev + 0.15f, targetPos.z);
          }
        }
      }

    } else {
      if(!pickUp) {
        meshRenderer.material.SetFloat("_FresnelIntensity", fresnelIntensity = fresnelIntensityMin);
        DeParent(hit);
      }
    }
  }

  void CheckMouseInput(RaycastHit hit) {
    if(Input.GetMouseButton(0) && !pickUp && mouseReleased) {
      Parent(hit);
      mouseReleased = false;
    } else if(!Input.GetMouseButton(0)) {
      DeParent(hit);
      mouseReleased = true;
    }

    if(Input.GetKeyDown(KeyCode.Q)) {
        //solo = !hit.transform.gameObject.GetComponent<Calculations>().solo;
        //hit.transform.gameObject.GetComponent<Calculations>().solo = solo;
      if(solo){
          meshRenderer.material.SetColor("_FresnelColor", soloFresnelCol);
          meshRenderer.material.SetColor("_BaseColor", soloCol);
      } else {
          meshRenderer.material.SetColor("_FresnelColor", mainFresnelCol);
          meshRenderer.material.SetColor("_BaseColor", mainCol);
      }
    }
  }

    void CheckButtonInput() {
      GameObject target = transform.Find("Grabbed Track").gameObject;
      float distance = Vector3.Distance(target.transform.position, transform.position);
      if(Input.GetKey(KeyCode.W)) {
        if(distance < maxDist) {
          target.transform.position = Vector3.MoveTowards(target.transform.position, endPos, 10 * Time.deltaTime);
        }
      } else if(Input.GetKey(KeyCode.S)) {
          if(distance > minDist) {
          target.transform.position = Vector3.MoveTowards(target.transform.position, transform.position, 10 * Time.deltaTime);
      }
    }
  }

  void Parent(RaycastHit hit) {
    meshRenderer.material.SetFloat("_FresnelPower", fresnelPower = fresnelPowerMax);
    hit.transform.parent = gameObject.transform;
    hit.transform.gameObject.name = "Grabbed Track";
    totalGrabs++;
    pickUp = true;
  }

  void DeParent(RaycastHit hit) {
    meshRenderer.material.SetFloat("_FresnelPower", fresnelPower = fresnelPowerMin);
    foreach (Transform child in transform) {
        if (child.tag == "Track") {
        child.transform.parent = null;
        MeshRenderer childRenderer = child.GetComponent<MeshRenderer>();
        childRenderer.material.SetFloat("_FresnelPower", fresnelPower = fresnelPowerMin);
        child.name = "Track " + totalGrabs;
        Debug.Log(childRenderer.material.GetFloat("_FresnelIntensity"));
      }
    }
    pickUp = false;
  }

  MeshRenderer GetMeshRenderer(RaycastHit hit)
  {
      return hit.transform.gameObject.GetComponent<MeshRenderer>();
  }
}