using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableViewCanvases : MonoBehaviour, IClickable
{
    [SerializeField] NewPaint[] canvases;

    bool visible = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void IClickableClicked()
    {
        if(ASL.GameLiftManager.GetInstance().m_PeerId == 1)
        {
            foreach (NewPaint canvas in canvases)
            {
                for(int i = 2; i < ASL.GameLiftManager.GetInstance().m_Players.Count; i++)
                {
                    if(!visible)
                    {
                        canvas.enableViewingForPlayer(i);
                    }
                    else{
                        canvas.disableViewingForPlayer(i);
                    }
                    
                }
            }
            visible = !visible;
        }
        
        
    }
    
}
