using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Teacher-only feature currently implemented as a physical button in the back of the canvas room.
// Allows the teacher to reset the drawing table positions in the event that students take them
// outside of the room etc. or they become inaccessible for some other reason.
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
