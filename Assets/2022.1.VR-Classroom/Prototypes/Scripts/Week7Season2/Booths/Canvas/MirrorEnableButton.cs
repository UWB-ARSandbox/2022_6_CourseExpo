using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Simple button that interfaces with the CanvasTableFeatures component of the drawing tables.
// Currently assigned to the black button; toggles the overhead preview of the students' canvases (local change for each user).
public class MirrorEnableButton : MonoBehaviour, IClickable
{
    public CanvasTableFeatures m_Features;
    void Start()
    {
        Debug.Assert(m_Features != null);
    }

    public void IClickableClicked()
    {
        m_Features.ToggleOverheadPreview();
    }
}