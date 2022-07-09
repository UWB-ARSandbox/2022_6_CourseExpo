using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class PingManager : MonoBehaviour
{

    public float waitTime = 5;
    ASLObject m_ASLObject;
    float[] id = new float[2];

    void Start()
    {
        m_ASLObject = GetComponent<ASLObject>();
        m_ASLObject._LocallySetFloatCallback(FloatReceive);

        if (GameManager.AmTeacher)
            StartCoroutine(SendPing());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator SendPing()
    {
        while (true)
        {
            yield return new WaitForSeconds(waitTime);
            id[0] = 100;
            m_ASLObject.SendAndSetClaim(() => {
                m_ASLObject.SendFloatArray(id);
            });
        }
    }

    void FloatReceive(string _id, float[] _f)
    {
        if((int)_f[0] == 100)
        {
            Debug.Log("Recieved Teachers Ping");
        }
    }
}
