using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class SendAndPlayAudio : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField]
    AudioClip input;
    [SerializeField]
    AudioClip output;
    [SerializeField]
    AudioSource audioPlayer;

    [SerializeField]
    bool audioInput = false;
    [SerializeField]
    int count = 0; //honestly no idea what this is for - Derek
    int hrz = 39000;
    int audioPerSecond = 40; //If changing, will need to make sure stuff in update gets called less as well;
    
    [SerializeField]
    ASLObject m_ASLObject;

    // hrz/audiopersecond must be less than 1000 as per SendFloatArray's limitation
    void Start()
    {
        StartCoroutine(DelayedInit());
        audioPlayer = GetComponent<AudioSource>();
        output = AudioClip.Create("MyMic", hrz * 2, 1, hrz, false);
    }

    IEnumerator DelayedInit() 
    {
        m_ASLObject = GetComponent<ASLObject>();
        while (m_ASLObject == null) {
            yield return new WaitForSeconds(0.25f);
            m_ASLObject = GetComponent<ASLObject>();
        }
    }

    public void SetupInput()
    //Called by Xpo player when it finds the ghost player it owns
    {
        audioInput = true;
        input = Microphone.Start(Microphone.devices[0], true, 1, hrz);
    }

    public void SendAudio() //was previously Update()
    //called from Update() of Xpo player, since we have it so your own ghost player gets disabled
    {
        if(audioInput == true) {
            if(count < audioPerSecond) {
                if (m_ASLObject != null) {
                    m_ASLObject.SendAndSetClaim(() =>
                    {
                        float[] audioData = new float[(hrz / audioPerSecond) + 1];
                        input.GetData(audioData, (count * hrz) / audioPerSecond);
                        for (int i = audioData.Length - 1; i > 0; i--) {
                            audioData[i] = audioData[i - 1];
                        }
                        audioData[0] = 102f;
                        m_ASLObject.SendFloatArray(audioData); 
                    });
                }
            }
            else {
                count = 0;
            }
        }
    }

    public void AudioReceive(float[] _f) 
    //called from within one of the floatReceive switch cases for the GhostPlayer this script is a component of
    {
        if(count < audioPerSecond) {
            count++;
            try {
                output.SetData(_f, ((count - 1) * hrz) / audioPerSecond);
            }
            catch (ArgumentException e) {}
            if (count == 3) {
                audioPlayer.clip = output;
                audioPlayer.Play();
            }
        }
        else {
            count = 0;
        } 
    }
}
