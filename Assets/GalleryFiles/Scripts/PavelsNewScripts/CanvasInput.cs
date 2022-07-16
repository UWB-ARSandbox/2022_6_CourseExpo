using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CanvasInput : MonoBehaviour
{
    // Start is called before the first frame update


    public static CanvasInput Instance {get; private set; }

    RaycastHit raycastHit;
    bool raycastHitObject;

    private void Awake() {
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            Instance = this; 
        }
    }
    void Start()
    {
        
    }

    //Casts a ray on each update
    void Update()
    {
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            
                
            
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);


            int layerMask = 1 << 30;
            
            layerMask = ~layerMask;
            raycastHitObject = Physics.Raycast(ray, out raycastHit, Mathf.Infinity, layerMask);
        }
    }
    public RaycastHit GetRaycastHit()
    {
        return raycastHit;
    }
    public bool getRaycastHitObject()
    {
        
        
        
        return raycastHitObject && raycastHit.transform.GetComponent<NewPaint>() && !EventSystem.current.IsPointerOverGameObject();
    }

    

}
