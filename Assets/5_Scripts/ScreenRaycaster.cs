using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenRaycaster : MonoBehaviour
{
  public float minDist;
  public float maxDist;
  public int layerMask = 1 << 8;
  [ColorUsageAttribute(true,true,0f,8f,0.125f,3f)]
  public Color mainCol;
  [ColorUsageAttribute(true,true,0f,8f,0.125f,3f)]
  public Color soloCol;
  public bool solo;

  private MeshRenderer meshRenderer, lastMesh;
  private Vector3 endPos;
  private bool pickUp;
  private int totalGrabs;

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
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

    endPos = transform.forward * 10000;

    if(Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask)) {
      if (hit.transform.gameObject.GetComponent<MeshRenderer>() != null && !pickUp) {
        meshRenderer = GetMeshRenderer(hit);
        meshRenderer.material.SetFloat("_FresnelIntensity", fresnelIntensity = fresnelIntensityMax);
      }

      if(hit.transform.gameObject != null) {
        CheckMouseInput(hit);
        CheckButtonInput(hit);
      }

      if (hit.transform.gameObject != lastHit && lastHit != null && !pickUp)
      {
        Debug.Log("Not same");
        lastMesh = lastHit.GetComponent<MeshRenderer>();
        lastMesh.material.SetFloat("_FresnelIntensity", fresnelIntensity = fresnelIntensityMin);
      }
      lastHit = hit.transform.gameObject;
      Debug.Log(lastHit);
    } else {
      if(!pickUp && !solo) {
        meshRenderer.material.SetFloat("_FresnelIntensity", fresnelIntensity = fresnelIntensityMin);
        DeParent(hit);
      }
    }
  }

  void CheckMouseInput(RaycastHit hit) {
    if(Input.GetMouseButton(0) && !pickUp) {
      Debug.Log("Press");
      meshRenderer.material.SetFloat("_FresnelPower", fresnelPower = fresnelPowerMax);
      Parent(hit);
      pickUp = true;

    } else if(!Input.GetMouseButton(0)) {
      Debug.Log("No Press");
      meshRenderer.material.SetFloat("_FresnelPower", fresnelPower = fresnelPowerMin);
      DeParent(hit);
      pickUp = false;
    }

    if(Input.GetKeyDown(KeyCode.Q)) {
      solo = !hit.transform.gameObject.GetComponent<Calculations>().solo;
        hit.transform.gameObject.GetComponent<Calculations>().solo = solo;
      if(solo){
          meshRenderer.material.SetColor("_FresnelColor", soloCol);
      } else {
          meshRenderer.material.SetColor("_FresnelColor", mainCol);
      }
    }
  }

    void CheckButtonInput(RaycastHit hit) {
      if(hit.transform.gameObject.name == "Grabbed Track") {
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
  }

  void Parent(RaycastHit hit) {
    hit.transform.parent = gameObject.transform;
    hit.transform.gameObject.name = "Grabbed Track";
    totalGrabs++;
  }

  void DeParent(RaycastHit hit) {
    foreach (Transform child in transform) {
        if (child.tag == "Track") {
        child.transform.parent = null;
        MeshRenderer childRenderer = child.GetComponent<MeshRenderer>();
        childRenderer.material.SetFloat("_FresnelPower", fresnelPower = fresnelPowerMin);
        child.name = "Track " + totalGrabs;
        Debug.Log(childRenderer.material.GetFloat("_FresnelIntensity"));
      }
    }
  }

  MeshRenderer GetMeshRenderer(RaycastHit hit)
  {
      return hit.transform.gameObject.GetComponent<MeshRenderer>();
  }
}
