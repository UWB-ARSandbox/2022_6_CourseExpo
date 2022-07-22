using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;
public class CanvasInput : MonoBehaviour
{
    // Start is called before the first frame update


    public static CanvasInput Instance {get; private set; }

    RaycastHit raycastHit;

    RaycastHit[] raycastHitsVR = new RaycastHit[2];
    bool raycastHitObject;
    bool[] raycastHitObjectVR = new bool[2];

    bool foundPlayer = false;

    XRRayInteractor[] VRRaycasters;

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
        StartCoroutine(findPlayer());
        
    }

    //Casts a ray on each update
    void Update()
    {
        if(PlayerController.isXRActive)
        {
            
            if(foundPlayer)
            {
                for(int i = 0; i < VRRaycasters.Length; i++)
                {
                    
                    raycastHitObjectVR[i] = VRRaycasters[i].TryGetCurrent3DRaycastHit(out raycastHitsVR[i]);
                     
                }
            }
        }
        else{

        
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                
                    
                
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);


                int layerMask = 1 << 30;
                
                layerMask = ~layerMask;
                raycastHitObject = Physics.Raycast(ray, out raycastHit, Mathf.Infinity, layerMask);
            }
        }
    }
    public RaycastHit GetRaycastHit()
    {
        return raycastHit;
    }
    public RaycastHit[] GetRaycastHitVR()
    {
        return raycastHitsVR;
    }
    public bool getRaycastHitObject()
    {
        
        
        
        return raycastHitObject && !EventSystem.current.IsPointerOverGameObject();
    }
    public bool getRaycastHitObjectVR(int i)
    {
        return raycastHitObjectVR[i];
    }
    IEnumerator findPlayer()
    {
        while(!foundPlayer)
        {
            
            if(GameObject.FindGameObjectWithTag("Player") != null)
            {
                
                VRRaycasters = GameObject.FindGameObjectWithTag("Player").GetComponentsInChildren<XRRayInteractor>(); 
                foundPlayer = true;

            }
            yield return null;
        }
        
    }

    

}
