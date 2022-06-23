using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class HMDInfoManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("is device active " + XRSettings.isDeviceActive);
        Debug.Log("Device name is : " + XRSettings.loadedDeviceName);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
