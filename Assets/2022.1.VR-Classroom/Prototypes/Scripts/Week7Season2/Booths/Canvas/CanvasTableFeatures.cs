using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class CanvasTableFeatures : MonoBehaviour
{
    public bool isTeacherTable = false;

    // For hiding/showing the table mirror object: see ToggleOverheadPreview()
    public GameObject tableMirror;
    private Vector3 tableMirrorInitScale;

    // Drawing table pop-up functionality: see PopUpCanvas()
    public ASLObject top;
    private Vector3 canvasInitLocalPos;
    private Quaternion canvasInitLocalRot;
    private bool flippedUp = false;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(tableMirror != null);
        tableMirrorInitScale = tableMirror.transform.localScale;
        // Disable overhead previews by default
        ToggleOverheadPreview();

        Debug.Assert(top != null);
        canvasInitLocalPos = top.transform.localPosition;
        canvasInitLocalRot = top.transform.localRotation;

        // Remove overhead preview for teacher table
        if (isTeacherTable)
            GetComponentInChildren<Billboard>().gameObject.SetActive(false);
    }

    public void ToggleOverheadPreview()
    {
        if (tableMirror.transform.localScale == Vector3.zero)
            tableMirror.transform.localScale = tableMirrorInitScale;
        else
            tableMirror.transform.localScale = Vector3.zero;
    }

    public void PopUpCanvas()
    {
        if (!flippedUp)
        {
            //top.SendAndSetLocalRotation(Quaternion.Euler(new Vector3(90f, 0f, 0f)));
            //top.SendAndSetLocalPosition(new Vector3(top.transform.localPosition.x, 1.75f, top.transform.localPosition.z));

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
