using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class CanvasTableFeatures : MonoBehaviour
{
    public ASLObject top;
    private Vector3 canvasInitLocalPos;
    private Quaternion canvasInitLocalRot;
    private bool flippedUp;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(top != null);
        canvasInitLocalPos = top.transform.localPosition;
        canvasInitLocalRot = top.transform.localRotation;
    }

    public void PopUpCanvas()
    {
        if (!flippedUp)
        {
            top.SendAndSetLocalPosition(new Vector3(top.transform.localPosition.x, 1.25f, top.transform.localPosition.z));
            top.SendAndSetLocalRotation(Quaternion.Euler(new Vector3(90f, 0f, 0f)));
        } 
        else
        {
            top.SendAndSetLocalPosition(canvasInitLocalPos);
            top.SendAndSetLocalRotation(canvasInitLocalRot);
        }
        flippedUp = !flippedUp; // convert from true <---> false
    }

}
