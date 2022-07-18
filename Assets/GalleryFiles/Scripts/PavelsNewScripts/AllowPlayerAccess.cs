using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllowPlayerAccess : MonoBehaviour, IClickable
{
    // Start is called before the first frame update
    public NewPaint canvas;
    void Start()
    {
        
    }

    // Update is called once per frame
    public void IClickableClicked()
    {
        StartCoroutine(canvas.enableCanvasForPlayer(ASL.GameLiftManager.GetInstance().m_PeerId));
        StartCoroutine(canvas.enableViewingForPlayer(ASL.GameLiftManager.GetInstance().m_PeerId));
    }

}
