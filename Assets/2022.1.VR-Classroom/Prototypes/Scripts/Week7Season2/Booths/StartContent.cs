using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartContent : MonoBehaviour, IClickable
{
    public ScreenManager _ScreenManager = null;

    void Start()
    {
        Debug.Assert(_ScreenManager != null);
    }

    public void IClickableClicked()
    {
        _ScreenManager.StartContent();
    }
}
