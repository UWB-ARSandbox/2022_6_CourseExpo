using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasMirrorer : MonoBehaviour
{
    public GameObject myCanvas;
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<Renderer>().material.mainTexture = myCanvas.GetComponent<NewPaint>().studentCanvas;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
