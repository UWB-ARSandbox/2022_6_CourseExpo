using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

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
