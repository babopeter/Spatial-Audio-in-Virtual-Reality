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
    public bool retrieveSum = false;
    public float kick, snare, perc, bass, melody, arpeg, choir;

    // Start is called before the first frame update
    void Start()
    {
        sum = 0;
        kick = 0;
        snare = 0;
        perc = 0;
        bass = 0;
        melody = 0;
        arpeg = 0;
        choir = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(OVRInput.GetDown(OVRInput.Button.One) && OVRInput.GetDown(OVRInput.Button.Three)) {
            retrieveSum = true;
        
            sum = 0;
            kick = 0;
            snare = 0;
            perc = 0;
            bass = 0;
            melody = 0;
            arpeg = 0;
            choir = 0;

            for(int i = 0; i < soundSources.Count; i++) {
                sum += GetDistance(visible[i], soundSources[i]);
            }
            
            kick = GetDistance(visible[0], soundSources[0]);
            snare = GetDistance(visible[1], soundSources[1]);
            perc = GetDistance(visible[2], soundSources[2]);
            bass = GetDistance(visible[3], soundSources[3]);
            melody = GetDistance(visible[4], soundSources[4]);
            arpeg = GetDistance(visible[5], soundSources[5]);
            choir = GetDistance(visible[6], soundSources[6]);
        } 

        if(Input.GetKeyDown(KeyCode.O)) {
            retrieveSum = false;
        }
    
        
    }

    public static float GetDistance(GameObject visible, GameObject soundSource ) {
        return Vector3.Distance(visible.transform.position, soundSource.transform.position);
    }
}
