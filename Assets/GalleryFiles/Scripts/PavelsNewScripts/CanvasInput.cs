using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;
using UnityEngine.XR;

//Singleton script handles the raycasting for the canvas
//Script determines whether to use XR or mouse input at startup
//Script also handles some wacky stuff for getting the sliders to work in world space, needs to be moved out of that script and a more elegant solution would be nice.
public class CanvasInput : MonoBehaviour
{
    

    //The singleton instance to access
    public static CanvasInput Instance {get; private set; }

    //The raycast hit of the mouse
    RaycastHit raycastHit;

    //The raycast hit from the two vr controllers
    RaycastHit[] raycastHitsVR = new RaycastHit[2];

    //Whether the raycast hit an object, exists to help avoid null reference exceptions
    bool raycastHitObject;

    //Same as raycastHitObject but for the two VR inputs
    bool[] raycastHitObjectVR = new bool[2];

    //variable used to find the raycaster components for the VR
    bool foundPlayer = false;

    //The raycaster components for the VR
    XRRayInteractor[] VRRaycasters;

    //The input devices for the VR
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
        //Finds the raycast components for the VR, waits to make sure the player is actually spawned
        StartCoroutine(findPlayer());

        //Gets the left and right input devices for the VR
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
        //Performs
        if(PlayerController.isXRActive)
        {
            
            if(foundPlayer)
            {
                //Gets the raycast hits for both of the controllers
                for(int i = 0; i < VRRaycasters.Length; i++)
                {
                    
                    raycastHitObjectVR[i] = VRRaycasters[i].TryGetCurrent3DRaycastHit(out raycastHitsVR[i]);
                }

                //For sliders to work in world space --------------------------------------------------------
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
                //-------------------------------------------------------------------------------------------
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

            ///For sliders to work in world space --------------------------------------------------------
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
            //-------------------------------------------------------------------------------------------
            
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

    //Gets whether the raycast hit an object. Should always be checked to be true before getting the raycast information to make sure the raycast is correct
    public bool getRaycastHitObject()
    {
        return raycastHitObject && !EventSystem.current.IsPointerOverGameObject();
    }
    //Same as above but for VR
    public bool getRaycastHitObjectVR(int i)
    {
        return raycastHitObjectVR[i];
    }
    //Waits for the player to spawn and gets it's vr components
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
