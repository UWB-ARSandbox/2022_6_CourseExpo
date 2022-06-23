using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ASL;

public class ToggleTrigger : MonoBehaviour
{
    public Collider PlayerCollider;
    public Toggle IsTrigger;

    ASLObject m_ASLObject;

    private void Start()
    {
        Debug.Assert(PlayerCollider != null);
        Debug.Assert(IsTrigger != null);
        m_ASLObject = GetComponent<ASLObject>();
        Debug.Assert(m_ASLObject != null);
        m_ASLObject._LocallySetFloatCallback(SendToggleSignal);
    }

    private void Update()
    {
        IsTrigger.onValueChanged.AddListener(delegate
        {
            m_ASLObject.SendAndSetClaim(() =>
            {
                float[] mFloat = new float[2];
                mFloat[0] = 1;
                if (IsTrigger.isOn)
                {
                    mFloat[1] = 1;
                }
                else mFloat[1] = 0;
                m_ASLObject.SendFloatArray(mFloat);
            });
        });
    }

    public static void SendToggleSignal(string _id, float[] _myFloats)
    {
        ASL.ASLHelper.m_ASLObjects.TryGetValue(_id, out ASL.ASLObject _myObject);
        if (_myFloats[0] == 1)
        {
            if (_myFloats[1] == 1)
            {
                _myObject.GetComponent<ToggleTrigger>().IsTrigger.isOn = true;
                _myObject.GetComponent<ToggleTrigger>().PlayerCollider.isTrigger = true;
            }
            else if (_myFloats[1] == 0)
            {
                _myObject.GetComponent<ToggleTrigger>().IsTrigger.isOn = false;
                _myObject.GetComponent<ToggleTrigger>().PlayerCollider.isTrigger = false;
            }
        }
    }
}
