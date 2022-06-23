using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesktopCanvasController : MonoBehaviour
{
    void Start()
    {
        gameObject.SetActive(!Application.isMobilePlatform);
    }
    
}
