using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionResetButton : MonoBehaviour, IClickable
{
    public List<GrabCanvas> canvases; // References to all tables to be set in the editor

    void Start()
    {
        // Enable this object for teacher only
        if (!GameManager.AmTeacher)
            gameObject.SetActive(false);
        GetComponent<Renderer>().material.color = Color.blue;
    }

    public void IClickableClicked()
    {
        foreach (GrabCanvas canvas in canvases)
        {
            canvas.ResetPosition();
        }
    }
}
