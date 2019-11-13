using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstText : MonoBehaviour
{
  public GameObject textPrefab;
    // Start is called before the first frame update
    void Start()
    {
        GameObject newText = (GameObject)Instantiate(textPrefab, new Vector3(0.0f, 0.0f, 0.0f), Quaternion.identity);
        TextLook textLook = newText.GetComponent<TextLook>();
        textLook.target = gameObject;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
