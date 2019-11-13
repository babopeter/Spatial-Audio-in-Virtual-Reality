using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetDistance : MonoBehaviour
{
    public GameObject Source1;
    public GameObject Source2;
    public float Distance_;

    void Start () {

    }

    void Update () {

        Distance_ = Vector3.Distance(Source1.transform.position, Source2.transform.position);
    }

}