using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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