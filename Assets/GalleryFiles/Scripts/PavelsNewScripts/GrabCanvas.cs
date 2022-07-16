using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabCanvas : MonoBehaviour
{
    // Start is called before the first frame update
    Quaternion currentRotation;
    float currentY;
    bool selected = false;

    Transform previousParent;

    [SerializeField] Transform objectToMove;


    void Start()
    {
        selected = false;
        GetComponent<ASL.ASLObject>()._LocallySetFloatCallback(setPosition);
    }

    // Update is called once per frame
    void Update()
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
            objectToMove.parent = previousParent;
            
            selected = false;

            objectToMove.rotation = currentRotation;
            objectToMove.position = new Vector3(objectToMove.position.x, currentY, objectToMove.position.z);
        }
        
    }
    IEnumerator sendPosition()
    {
        
        
        while(selected)
        {
            objectToMove.rotation = currentRotation;
            objectToMove.position = new Vector3(objectToMove.position.x, currentY, objectToMove.position.z);
            float[] fArray = {ASL.GameLiftManager.GetInstance().m_PeerId, objectToMove.position.x, currentY, objectToMove.position.z};
           GetComponent<ASL.ASLObject>().SendAndSetClaim(() => {
                GetComponent<ASL.ASLObject>().SendFloatArray(fArray); 
            });
            yield return new WaitForSeconds(1 / 20.0f);
        }
        
        
    }
    void setPosition(string _id, float[] _f)
    {
        if(_f[0] != ASL.GameLiftManager.GetInstance().m_PeerId)
        {
            objectToMove.position = new Vector3(_f[1], _f[2], _f[3]);
        }
    }
}
