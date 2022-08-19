using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

//Script handles moving the canvas around. Canvas can be grabbed by clicking on the handle
//Vr implementation currently works only with the left hand

public class GrabCanvas : MonoBehaviour
{
    Vector3 originalStartPos; // Original position of object in the editor, for reset purposes

    Quaternion currentRotation;
    float currentY;
    bool selected = false;

    Transform previousParent;

    [SerializeField] Transform objectToMove;
    public bool lookAtParent = false;
    bool previousTriggerDownLeft;

    List<InputDevice>  leftDevices;


    void Start()
    {
        originalStartPos = objectToMove.transform.position;
        selected = false;
        leftDevices = new List<InputDevice>();
		var desiredCharacteristicsLeft = UnityEngine.XR.InputDeviceCharacteristics.HeldInHand | UnityEngine.XR.InputDeviceCharacteristics.Left | UnityEngine.XR.InputDeviceCharacteristics.Controller;
		InputDevices.GetDevicesWithCharacteristics(desiredCharacteristicsLeft, leftDevices);
        gameObject.GetComponent<ASL.ASLObject>()._LocallySetFloatCallback(setPosition);
    }

    // Update is called once per frame
    void Update()
    {
        if(PlayerController.isXRActive)
        {
            bool triggerDownLeft;
            if (leftDevices[0].TryGetFeatureValue(CommonUsages.triggerButton, out triggerDownLeft) && triggerDownLeft)
            {
                if(!previousTriggerDownLeft)
                {
                    if(CanvasInput.Instance.getRaycastHitObjectVR(0))
                    {
                        
                        RaycastHit raycastHit = CanvasInput.Instance.GetRaycastHitVR()[0];
                        

                        if (raycastHit.transform == this.transform)
                        {
                            previousParent = objectToMove.parent;
                            objectToMove.SetParent(GameObject.FindGameObjectWithTag("Player").GetComponentsInChildren<XRRayInteractor>()[0].transform);
                            selected = true;
                            currentRotation = objectToMove.rotation;
                            currentY = objectToMove.position.y;
                            previousTriggerDownLeft = true;
                            StartCoroutine(sendPosition());
                        }
                    }
                }
            }
            if(!triggerDownLeft && previousTriggerDownLeft && selected)
            {
                if (lookAtParent)
                {
                    objectToMove.transform.LookAt(new Vector3(objectToMove.parent.position.x, currentY, objectToMove.parent.position.z));
                }
                else
                {
                    objectToMove.rotation = currentRotation;
                }

                objectToMove.position = new Vector3(objectToMove.position.x, currentY, objectToMove.position.z);

                objectToMove.parent = previousParent;
                
                selected = false;
                previousTriggerDownLeft = false;
            }
		
        }
        else
        {
            if(Input.GetMouseButtonDown(0))
            {
                if(CanvasInput.Instance.GetRaycastHit().transform == this.transform)
                {
                    previousParent = objectToMove.parent;
                    objectToMove.SetParent(Camera.main.transform);
                    selected = true;
                    currentRotation = objectToMove.rotation;
                    currentY = objectToMove.position.y;
                    
                    StartCoroutine(sendPosition());
                }
            }
            if(Input.GetMouseButtonUp(0) && selected)
            {
                if (lookAtParent)
                {
                    objectToMove.transform.LookAt(new Vector3(objectToMove.parent.position.x, currentY, objectToMove.parent.position.z));
                }
                else
                {
                    objectToMove.rotation = currentRotation;
                }

                objectToMove.position = new Vector3(objectToMove.position.x, currentY, objectToMove.position.z);

                objectToMove.parent = previousParent;
                
                selected = false;
            }
        }
        
    }
    IEnumerator sendPosition()
    {
        
        
        while(selected)
        {
            if (lookAtParent)
            {
                objectToMove.transform.LookAt(new Vector3(objectToMove.parent.position.x, currentY, objectToMove.parent.position.z));
            }
                
            else
            {
                objectToMove.rotation = currentRotation;
            }
            objectToMove.position = new Vector3(objectToMove.position.x, currentY, objectToMove.position.z);
            float[] fArray = {ASL.GameLiftManager.GetInstance().m_PeerId, objectToMove.position.x, currentY, objectToMove.position.z, objectToMove.parent.position.x, objectToMove.parent.position.z};
            gameObject.GetComponent<ASL.ASLObject>().SendAndSetClaim(() => {
                gameObject.GetComponent<ASL.ASLObject>().SendFloatArray(fArray); 
            });
            yield return new WaitForSeconds(1 / 20.0f);
        }
        
        
    }
    void setPosition(string _id, float[] _f)
    {
        if(_f[0] != ASL.GameLiftManager.GetInstance().m_PeerId)
        {
            if (lookAtParent)
                objectToMove.transform.LookAt(new Vector3(_f[4], _f[2], _f[5]));

            objectToMove.position = new Vector3(_f[1], _f[2], _f[3]);
        }
    }

    public void ResetPosition()
    {
        float[] fArray = { -1f, originalStartPos.x, originalStartPos.y, originalStartPos.z, 0f,  0f };
        gameObject.GetComponent<ASL.ASLObject>().SendAndSetClaim(() => {
            gameObject.GetComponent<ASL.ASLObject>().SendFloatArray(fArray);
        });
    }
}
