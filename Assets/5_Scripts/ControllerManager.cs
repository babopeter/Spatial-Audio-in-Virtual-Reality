using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerManager : MonoBehaviour
{
    private bool active = false;

    public GameObject IPText;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.SecondaryThumbstick))
        {
            if (!active)
            {
                IPText.SetActive(true);
                active = true;

            }
            else
            {
                IPText.SetActive(false);
                active = false;
            }

        }
    }
}
