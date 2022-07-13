using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class BoothBounds : MonoBehaviour
{


    List<string> currentUsers = new List<string>();
    ASL_ObjectCollider m_ASLObjectCollider;
    ASLObject m_ASLObject;

    void Start()
    {
        m_ASLObject = GetComponent<ASLObject>();
        m_ASLObject._LocallySetFloatCallback(FloatReceive);
        m_ASLObjectCollider = GetComponent<ASL_ObjectCollider>();
        m_ASLObjectCollider.ASL_OnTriggerEnter(ASLOnTriggerEnter);
        m_ASLObjectCollider.ASL_OnTriggerExit(ASLOnTriggerExit);
    }
    
    void Update()
    {
        
    }

    void ASLOnTriggerEnter(Collider other)
    {
        if (other.GetComponent<XpoPlayer>())
        {
            float[] myFloats = new float[2];
            myFloats[0] = 600;
            myFloats[1] = GameManager.MyID;
            name = GameManager.players[GameManager.MyID];
            m_ASLObject.SendAndSetClaim(() => { m_ASLObject.SendFloatArray(myFloats); });
        }
    }

    void ASLOnTriggerExit(Collider other)
    {
        if (other.GetComponent<XpoPlayer>())
        {
            float[] myFloats = new float[2];
            myFloats[0] = 601;
            myFloats[1] = GameManager.MyID;
            name = GameManager.players[GameManager.MyID];
            m_ASLObject.SendAndSetClaim(() => { m_ASLObject.SendFloatArray(myFloats); });
        }
    }

    void FloatReceive(string _id, float[] _f)
    {
        switch(_f[0]) {
            case 600:
                Debug.Log(GameManager.players[(int)_f[1]] + " has entered the booth");
                currentUsers.Add(GameManager.players[(int)_f[1]]);
                break;
            case 601:
                Debug.Log(GameManager.players[(int)_f[1]] + " has left the booth");
                currentUsers.Remove(GameManager.players[(int)_f[1]]);
                break;
        }
    }
}
