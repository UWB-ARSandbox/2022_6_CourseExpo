using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasMirrorer : MonoBehaviour
{
    public NewPaint myCanvas;
    // Start is called before the first frame update
    void Start()
    {
        myCanvas.canvasTextureSwitch += updateTexture;
        gameObject.GetComponent<Renderer>().material.mainTexture = myCanvas.getTexture();
    }

    // Update is called once per frame
    void updateTexture(Texture2D myTex)
    {
        gameObject.GetComponent<Renderer>().material.mainTexture = myTex;
    }
    void Update()
    {
        
    }
}
