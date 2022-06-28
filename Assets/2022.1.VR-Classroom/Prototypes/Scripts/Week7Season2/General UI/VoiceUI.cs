using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VoiceUI : MonoBehaviour
{
    public Dropdown User_Microphones;
    public Dropdown User_MicType;
    public GameObject MicrophonePanel;
    public GameObject VoiceConnectionPanel;
    Mumble.MumbleMicrophone UserMicrophone;
    MumbleActor myMumble;
    AudioManager _AudioManager;

    public void SetUserMicrophone(Mumble.MumbleMicrophone mumbleMic){
        UserMicrophone = mumbleMic;
    }
    public void SetMumble(MumbleActor s){
        myMumble = s;
    }
    private void Awake() {
        _AudioManager = GameObject.Find("GameManager").GetComponent<AudioManager>();
        Debug.Assert(_AudioManager != null);
    }
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        // Get list of Microphone devices and print the names to the log
        foreach (var device in Microphone.devices)
        {
            Debug.Log("Name: " + device);
            PopulateMicrophoneDropDown();
        }
        switch(UserMicrophone.GetMicType()){
            case Mumble.MumbleMicrophone.MicType.AlwaysSend:{User_MicType.value = 0;break;}
            case Mumble.MumbleMicrophone.MicType.Amplitude:{User_MicType.value = 1;break;}
            case Mumble.MumbleMicrophone.MicType.PushToTalk:{User_MicType.value = 2;break;}
            case Mumble.MumbleMicrophone.MicType.MethodBased:{User_MicType.value = 3;break;}
        }
        User_Microphones.value = UserMicrophone.MicNumberToUse;
    }
    //populate dropdown with list of microphone devices
    void PopulateMicrophoneDropDown(){
        List<string> options = new List<string> ();
        foreach (var device in Microphone.devices){
            options.Add(device);
        }
        User_Microphones.ClearOptions();
        User_Microphones.AddOptions(options);
    }

    public void ShowConnectionPanel(){
        MicrophonePanel.SetActive(false);
        VoiceConnectionPanel.SetActive(true);
    }
    public void HideConnectionPanel(){
        VoiceConnectionPanel.SetActive(false);
        MicrophonePanel.SetActive(true);
    }

    public void Destroy(){
        Destroy(gameObject);
    }
    //On destroy update Microphone settings
    private void OnDestroy() {
        switch(User_MicType.value){
            case 0:
                UserMicrophone.setSettings(Mumble.MumbleMicrophone.MicType.AlwaysSend,User_Microphones.value);
                break;
            case 1:
                UserMicrophone.setSettings(Mumble.MumbleMicrophone.MicType.Amplitude,User_Microphones.value);
                break;
            case 2:
                UserMicrophone.setSettings(Mumble.MumbleMicrophone.MicType.PushToTalk,User_Microphones.value);
                break;
            case 3:
                UserMicrophone.setSettings(Mumble.MumbleMicrophone.MicType.MethodBased,User_Microphones.value);
                break;
       }
        _AudioManager.setVoiceUIEnabled();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
