using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class ASLAuto_Logic : MonoBehaviour
{
    /* public float MovementSpeed = 2;

    ASLObject m_object;
    ASL_ObjectCollider m_ObjectCollider;
    ASL_AutonomousObject m_AutonomousObject;
    Vector3 startPos;
    public float direction = 1.0f;
    

    // Start is called before the first frame update
    void Start()
    {

        m_object = GetComponent<ASLObject>();

        m_ObjectCollider = GetComponent<ASL_ObjectCollider>();
        m_ObjectCollider.ASL_OnCollisionEnter(ChangeDirectionOnTrigger);
        m_AutonomousObject = GetComponent<ASL_AutonomousObject>();
        
        startPos = transform.position;
        
    }

    // Update is called once per frame
    void Update()
    {
        m_object._LocallySetFloatCallback(ChangeDirectionEvent);
        if (transform.position.x - startPos.x >= 5)
        {
            direction = -1.0f;
        }
        else if (transform.position.x - startPos.x <= -5)
        {
            direction = 1.0f;
        }
        Vector3 m_AdditiveMovementAmount = new Vector3(Time.deltaTime * MovementSpeed * direction, 0, 0);
        m_AutonomousObject.AutonomousIncrementWorldPosition(m_AdditiveMovementAmount);
        //Debug.Log("Updating");
        
    }
    public void ChangeDirectionOnTrigger(Collision collision)
    {
        Debug.Log("Collided");
        //Debug.Log(m_object.m_FloatCallback);
        if(direction == -1.0f)
        {
            //Debug.Log("moo");
            m_object.SendAndSetClaim(() =>
            {
                float[] myValue = new float[1] { 1.0f };
                //myValue[0] = 1.0f;
                m_object.SendFloatArray(myValue);
                Debug.Log("moo");
            });
        }
        else
        {
           
            m_object.SendAndSetClaim(() =>
            {
                float[] myValue = new float[1] { -1.0f };
                //myValue[0] = -1.0f;
                m_object.SendFloatArray(myValue);
                Debug.Log("gangster");
            });
        }
        
        
        
    }
    public void ChangeDirectionEvent(string _id, float[] _f)
    {
        Debug.Log("Changing");
        direction = _f[0];
    }


 */
}
