using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Simple button that interfaces with the CanvasTableFeatures component of the drawing tables.
// Currently assigned to the purple button; toggles the vertical view of the drawing canvas.
public class CanvasPopUpButton : MonoBehaviour, IClickable
{
    public CanvasTableFeatures m_Features;
    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(m_Features != null);
    }

    public void IClickableClicked()
    {
        m_Features.PopUpCanvas();
    }
}
