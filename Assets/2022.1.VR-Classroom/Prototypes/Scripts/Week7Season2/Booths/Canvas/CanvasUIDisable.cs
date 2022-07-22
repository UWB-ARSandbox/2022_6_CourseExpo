using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Temp script to disable canvas UI while in non-canvas areas
public class CanvasUIDisable : MonoBehaviour
{
    public GameObject CanvasUI;
    void OnTriggerEnter(Collider other)
    {
        CanvasUI.SetActive(false);
    }

}