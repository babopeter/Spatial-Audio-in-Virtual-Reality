using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using TMPro;
using UnityEngine;

public class GetIP : MonoBehaviour
{

    public GameObject target;
    // Start is called before the first frame update
    
    void Start()
        
    {
        string ipv4 = GetLocalIPAddress();
        gameObject.GetComponent<TextMeshPro>().text = ipv4;
    }

    public static string GetLocalIPAddress()
    {
        var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }

        throw new System.Exception("No network adapters with an IPv4 address in the system!");
    }
    /*
    private void Update()
    {
        string angleValue = target.GetComponent<Calculations>().azimuth.ToString();
        gameObject.GetComponent<TextMeshPro>().text = angleValue;
    }
    */
}

//192.168.43.193