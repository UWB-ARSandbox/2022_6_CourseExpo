using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileCanvasController : MonoBehaviour
{
    
    void Start()
    {
        gameObject.SetActive(Application.isMobilePlatform);
    }
    
}
