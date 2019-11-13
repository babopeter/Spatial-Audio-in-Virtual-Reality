using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextLook : MonoBehaviour
{

    public GameObject target;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
      transform.position = target.transform.position + new Vector3(-0.05f, 0.7f, 0.0f);
      //transform.position = new Vector3(-0.05f, 0.734f, 0.0f);
      transform.LookAt(Camera.main.transform.position);
    }
}
