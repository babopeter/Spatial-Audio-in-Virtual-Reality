using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raymanager : MonoBehaviour
{
    LineRenderer line;
    Ray pointer;
    void Start()
    {
      line = gameObject.AddComponent<LineRenderer>();


    }

    // Update is called once per frame
    void Update()
    {
      pointer = new Ray(transform.position, transform.forward);
      line.SetPosition(0, pointer.origin);
      line.SetPosition(0, pointer.origin + pointer.direction * 500.0f);

    }
}
