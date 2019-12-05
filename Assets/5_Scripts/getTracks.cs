using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class getTracks : MonoBehaviour
{

    public GameObject leftHand;
    public GameObject rightHand;

    public Color mainFogCol;
    public Color refFogCol;

    public bool trackToggle = true;

    [Header("Track Lists")]
    public List <GameObject> tracks = new List<GameObject>();
    public List <GameObject> tracksRef = new List<GameObject>();

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
              if(trackToggle) {
                  RenderSettings.fogColor = mainFogCol;
                  leftHand.GetComponent<Raycaster>().enabled = true;
                  rightHand.GetComponent<Raycaster>().enabled = true;
                  leftHand.GetComponent<LineRenderer>().enabled = true;
                  rightHand.GetComponent<LineRenderer>().enabled = true;
              } else {
                  RenderSettings.fogColor = refFogCol;
                  leftHand.GetComponent<Raycaster>().enabled = false;
                  rightHand.GetComponent<Raycaster>().enabled = false;
                  leftHand.GetComponent<LineRenderer>().enabled = false;
                  rightHand.GetComponent<LineRenderer>().enabled = false;
              }
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
