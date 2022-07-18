using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasMirrorer : MonoBehaviour
{
    public NewPaint myCanvas;
    bool gotTexture = false;
    // Start is called before the first frame update
    void Awake()
    {
        myCanvas.canvasTextureSwitch += updateTexture;
        //StartCoroutine(getTexture());
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
