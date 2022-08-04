using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using UnityEngine.XR;

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

    List<InputDevice> leftDevices;

    List<InputDevice> rightDevices;

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
        leftDevices = new List<InputDevice>();
		var desiredCharacteristicsLeft = UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Left | UnityEngine.XR.InputDeviceCharacteristics.Controller;
		InputDevices.GetDevicesWithCharacteristics(desiredCharacteristicsLeft, leftDevices);

		rightDevices = new List<InputDevice>();
		var desiredCharacteristicsRight = UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Right | UnityEngine.XR.InputDeviceCharacteristics.Controller;
		InputDevices.GetDevicesWithCharacteristics(desiredCharacteristicsRight, rightDevices);
        
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
                //For sliders in world space
                bool triggerDownLeft;
                if (leftDevices[0].TryGetFeatureValue(CommonUsages.triggerButton, out triggerDownLeft) && triggerDownLeft)
                {
                    RaycastResult result;
                    if(VRRaycasters[0].TryGetCurrentUIRaycastResult(out result))
                    {
                        
                        //Debug.Log(result.gameObject.name);
                        if(result.gameObject.name == "Handle")
                        {
                            PointerEventData data = new PointerEventData(EventSystem.current);
                            data.pointerPressRaycast = result;
                            data.pressPosition = result.screenPosition;
                            data.position = result.screenPosition;;
                            result.gameObject.transform.parent.parent.GetComponent<Slider>().OnPointerDown(data);
                        }
                    }
                }
                bool triggerDownRight;
		        if (rightDevices[0].TryGetFeatureValue(CommonUsages.triggerButton, out triggerDownRight) && triggerDownRight)
		        {
                    RaycastResult result;
                    if(VRRaycasters[1].TryGetCurrentUIRaycastResult(out result))
                    {
                        
                        //Debug.Log(result.gameObject.name);
                        if(result.gameObject.name == "Handle")
                        {
                            PointerEventData data = new PointerEventData(EventSystem.current);
                            data.pointerPressRaycast = result;
                            data.pressPosition = result.screenPosition;
                            data.position = result.screenPosition;;
                            result.gameObject.transform.parent.parent.GetComponent<Slider>().OnPointerDown(data);
                        }
                    }
                }
            }
            
		
        }
        else{

        
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                
                    
                
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);


                LayerMask layerMask = LayerMask.GetMask("PlayerBody");
                
                layerMask = ~layerMask;
                raycastHitObject = Physics.Raycast(ray, out raycastHit, Mathf.Infinity, layerMask);
            }

            //For sliders in world space
            if(Input.GetMouseButton(0))
            {
                List<RaycastResult> results = new List<RaycastResult>();
                PointerEventData data = new PointerEventData(EventSystem.current);
                data.position = new Vector2(Screen.width / 2, Screen.height / 2);
                data.pressPosition = new Vector2(Screen.width / 2, Screen.height / 2);

                
                
                
                EventSystem.current.RaycastAll(data, results);
                
                foreach(RaycastResult result in results)
                {
                    
                    //Debug.Log(result.gameObject.name);
                    if(result.gameObject.name == "Handle")
                    {
                        
                        data.pointerPressRaycast = result;
                        result.gameObject.transform.parent.parent.GetComponent<Slider>().OnPointerDown(data);
                        break;
                        
                    }
                    
                    
                }
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
