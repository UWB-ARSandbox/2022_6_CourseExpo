using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class BoothZoneManager : MonoBehaviour
{


    public List<string> currentUsers = new List<string>();
    ASLObject m_ASLObject;

    void Start()
    {
        m_ASLObject = GetComponent<ASLObject>();
        if(m_ASLObject == null)
            Destroy(this);
        if(m_ASLObject != null)
            m_ASLObject._LocallySetFloatCallback(FloatReceive);
        
        CollaborativeManager _myManager = gameObject.transform.parent.transform.parent.GetComponent<CollaborativeManager>();
        if(_myManager != null)
            _myManager.SetBZM(this);
    }

    // send user id over network when they have entered a booths collider
    void OnTriggerEnter(Collider other)
    {
        if (m_ASLObject == null)
            m_ASLObject = GetComponent<ASLObject>();
        if (other.GetComponent<XpoPlayer>())
        {
            float[] myFloats = new float[2];
            myFloats[0] = 600;
            myFloats[1] = GameManager.MyID;

            m_ASLObject.SendAndSetClaim(() => { m_ASLObject.SendFloatArray(myFloats); });
        }
    }

    // send user id over network when they have exited a booths collider
    void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<XpoPlayer>())
        {
            float[] myFloats = new float[2];
            myFloats[0] = 601;
            myFloats[1] = GameManager.MyID;
            m_ASLObject.SendAndSetClaim(() => { m_ASLObject.SendFloatArray(myFloats); });
        }
    }

    // add or remove a user from the current users list
    void FloatReceive(string _id, float[] _f)
    {
        string boothName;
        switch(_f[0]) {
            case 600:
                boothName = gameObject.transform.parent.transform.parent.GetComponent<BoothManager>().boothName;
                Debug.Log(GameManager.players[(int)_f[1]] + " has entered booth: " + boothName);
                currentUsers.Add(GameManager.players[(int)_f[1]]);
                break;
            case 601:
                boothName = gameObject.transform.parent.transform.parent.GetComponent<BoothManager>().boothName;
                Debug.Log(GameManager.players[(int)_f[1]] + " has left booth: " + boothName);
                currentUsers.Remove(GameManager.players[(int)_f[1]]);
                break;
        }
    }
}
