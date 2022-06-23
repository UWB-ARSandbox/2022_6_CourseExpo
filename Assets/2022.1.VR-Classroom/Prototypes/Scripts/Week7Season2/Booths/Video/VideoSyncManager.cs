using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class VideoSyncManager : MonoBehaviour
{
    
    ASLObject m_ASLObject;
    public VideoPlaybackManager videoPlaybackManager;
    // Start is called before the first frame update
    void Start()
    {
        m_ASLObject = GetComponent<ASLObject>();
        m_ASLObject._LocallySetFloatCallback(FloatReceive);
    }

    public void SendNewVideoPosition(float newTimeFraction)
    {
        if (GameManager.AmTeacher)
        {
            float[] setNewTime = new float[2] { 101, newTimeFraction };
            m_ASLObject.SendAndSetClaim(() =>
            {
                m_ASLObject.SendFloatArray(setNewTime);
            });
        }
    }

    public void ToggleVideoPlayback()
    {
        if (GameManager.AmTeacher)
        {
            float[] toggleCode = new float[1] { 100 };
            m_ASLObject.SendAndSetClaim(() =>
            {
                m_ASLObject.SendFloatArray(toggleCode);
            });
        }
    }

    void FloatReceive(string _id, float[] _f)
    {
        int opcode = (int)_f[0];
        switch(opcode) {
            case 100:
                videoPlaybackManager.PlayPauseToggle();
                break;
            case 101:
                videoPlaybackManager.SetCurrentTime(_f[1]);
                break;
            default:
                break;
        }
    }
}