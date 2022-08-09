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
        if (!GameManager.AmTeacher)
        {
            gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void IClickableClicked()
    {
        
        foreach (NewPaint canvas in canvases)
        {
            for(int i = 2; i < ASL.GameLiftManager.GetInstance().m_Players.Count + 1; i++)
            {
                if(!visible)
                {
                    StartCoroutine(canvas.enableViewingForPlayer(i));
                }
                else{
                    StartCoroutine(canvas.disableViewingForPlayer(i));
                }
                
            }
        }
        visible = !visible;
        
        
        
    }
    
    
}
