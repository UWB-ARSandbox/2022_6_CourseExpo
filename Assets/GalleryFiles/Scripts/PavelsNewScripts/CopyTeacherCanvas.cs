using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class CopyTeacherCanvas : MonoBehaviour, IClickable
{
    // Start is called before the first frame update

    [SerializeField] GameObject teacherMirror;
    [SerializeField] NewPaint studentCanvas;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void IClickableClicked()
    {
        
        studentCanvas.SendTexture((Texture2D)teacherMirror.GetComponent<Renderer>().material.mainTexture);
    }
}

