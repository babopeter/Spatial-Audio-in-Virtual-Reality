using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class sumOfDistances : MonoBehaviour
{

    public List <GameObject> soundSources = new List<GameObject>();
    public List <GameObject> visible = new List<GameObject>();
    public float sum;
    public string[] trackNames;

    // Start is called before the first frame update
    void Start()
    {
        sum = 0;
        trackNames = new string[soundSources.Count];

        for(int i = 0; i < soundSources.Count; i++) {
            trackNames[i] = soundSources[i].name;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P)) {
            for(int i = 0; i < soundSources.Count; i++) {
                Debug.Log(trackNames[i] + " " + GetDistance(visible[i], soundSources[i]));
                sum += GetDistance(visible[i], soundSources[i]);
            }
        Debug.Log("sum " + sum);
        //Debug.Log("visible: " + visible.Count);
       // Debug.Log("Sound Sources: " + soundSources.Count); 
        }

        
        
        //float difference = visible[i] - soundSources[i];
        
    }

    public static float GetDistance(GameObject visible, GameObject soundSource ) {
        return Vector3.Distance(visible.transform.position, soundSource.transform.position);
    }
}
