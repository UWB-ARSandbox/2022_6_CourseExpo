using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class CanvasTableFeatures : MonoBehaviour
{
    public ASLObject top;
    private Vector3 canvasInitLocalPos;
    private Quaternion canvasInitLocalRot;
    private bool flippedUp = false;

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
            //top.SendAndSetLocalRotation(Quaternion.Euler(new Vector3(90f, 0f, 0f)));
            //top.SendAndSetLocalPosition(new Vector3(top.transform.localPosition.x, 8f, top.transform.localPosition.z));

            // This is done locally so canvases are not blocking everyone's vision, and to save network
            top.transform.localRotation = Quaternion.Euler(new Vector3(90f, 0f, 0f));
            top.transform.localPosition = new Vector3(top.transform.localPosition.x, 1.75f, top.transform.localPosition.z);
        }
        else
        {
            //top.SendAndSetLocalRotation(canvasInitLocalRot);
            //top.SendAndSetLocalPosition(canvasInitLocalPos);

            top.transform.localRotation = canvasInitLocalRot;
            top.transform.localPosition = canvasInitLocalPos;
        }
        flippedUp = !flippedUp; // convert from true <---> false
    }

}
