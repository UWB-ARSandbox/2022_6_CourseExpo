using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabCanvas : MonoBehaviour
{
    // Start is called before the first frame update
    Quaternion currentRotation;
    float currentY;
    bool selected;

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
                this.transform.parent.SetParent(Camera.main.transform);
                selected = true;
                currentRotation = this.transform.parent.rotation;
                currentY = this.transform.parent.position.y;
                StartCoroutine(sendPosition());
            }
        }
        if(Input.GetMouseButtonUp(0))
        {
            this.transform.parent.parent = null;
            
            selected = false;
        }
        
    }
    IEnumerator sendPosition()
    {
        
        
        while(selected)
        {
            this.transform.parent.rotation = currentRotation;
            this.transform.parent.position = new Vector3(this.transform.parent.position.x, currentY, this.transform.parent.position.z);
            float[] fArray = {ASL.GameLiftManager.GetInstance().m_PeerId, transform.parent.position.x, transform.parent.position.y, transform.parent.position.z};
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
            transform.parent.position = new Vector3(_f[1], _f[2], _f[3]);
        }
    }
}
