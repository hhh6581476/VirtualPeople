using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagedStripped : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (Microphone.devices.Length>10)
        {
            Microphone.Start(Microphone.devices[0], true, 0, 0);
            GameObject.Find("Camera").GetComponent<Camera>().targetTexture = null;

        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
