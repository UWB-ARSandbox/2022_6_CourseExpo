using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Temp script to enable canvas UI while in canvas area
public class CanvasUIEnable : MonoBehaviour
{
    public GameObject CanvasUI;
    void OnTriggerEnter(Collider other)
    {
        CanvasUI.SetActive(true);
    }

}
