using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class getTracks : MonoBehaviour
{


   
    public List <GameObject> tracks = new List<GameObject>();
    public List <GameObject> tracksRef = new List<GameObject>();

    public bool trackToggle = true;
    // Start is called before the first frame update
    void Start()
    {
        UpdateTrackToggle();
    }

    // Update is called once per frame
    void Update()
    {
        
          if(OVRInput.GetDown(OVRInput.Button.Two) || Input.GetKeyDown(KeyCode.K)) {
              trackToggle = !trackToggle;
              UpdateTrackToggle();

    }
}

    void UpdateTrackToggle() {
        for (int i = 0; i < tracks.Count; i ++){
                  tracks[i].GetComponent<Send>().enabled = trackToggle;
              }

              for (int i = 0; i < tracksRef.Count; i ++){
                  tracksRef[i].GetComponent<Send>().enabled = !trackToggle;
              }
    }
}
