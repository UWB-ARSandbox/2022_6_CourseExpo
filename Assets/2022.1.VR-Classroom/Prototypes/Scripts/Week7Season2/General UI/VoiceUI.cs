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

    public Button VoiceConnectionSettings;
    public Button User_PTTalkBind;
    public Text PushToTalkText;
    public Toggle MuteSelf;
    public Slider MicrophoneSensitivity;

    public InputField HostName;
    public InputField Password;

    public Button ConnectUsers;

    Mumble.MumbleMicrophone UserMicrophone;
    MumbleActor myMumble;
    AudioManager _AudioManager;

    Event keyEvent;
    KeyCode newKey;
    bool waitingForKey = false;

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
        if(UserMicrophone != null){
            switch(UserMicrophone.GetMicType()){
                case Mumble.MumbleMicrophone.MicType.AlwaysSend:{User_MicType.value = 0;break;}
                case Mumble.MumbleMicrophone.MicType.Amplitude:{User_MicType.value = 1;break;}
                case Mumble.MumbleMicrophone.MicType.PushToTalk:{User_MicType.value = 2;break;}
                case Mumble.MumbleMicrophone.MicType.MethodBased:{User_MicType.value = 3;break;}
            }
            User_Microphones.value = UserMicrophone.MicNumberToUse;
            MicrophoneSensitivity.value = UserMicrophone.MinAmplitude;
        }
        UpdatePushToTalkText();
        if(myMumble != null){
            MuteSelf.isOn = myMumble.getClient().IsSelfMuted();
        }

        if(_AudioManager.VoiceChatEnabled)
            HideConnectionPanel();
        else
            ShowConnectionPanel();

        if(!GameManager.AmTeacher){
            HostName.interactable = false;
            Password.interactable = false;
            VoiceConnectionSettings.enabled = false;
        }
        // Password.text = _AudioManager.Password;
        // HostName.text = _AudioManager.HostName;
        HostName.text = "50.35.25.58";
        Password.text = "test";
        if(_AudioManager.VoiceChatEnabled){
            ConnectUsers.enabled = false;
        }
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
    void OnGUI() {
        keyEvent = Event.current;
        if(keyEvent.isKey && waitingForKey){
            newKey = keyEvent.keyCode;
            waitingForKey = false;
        }
    }
    public void GetPushToTalkBinding(){
        if(!waitingForKey)
            StartCoroutine(AssignKey());
    }
    IEnumerator WaitForKey(){
        while(!keyEvent.isKey)
            yield return null;
    }
    public IEnumerator AssignKey(){
        waitingForKey = true;
        yield return WaitForKey();
        UserMicrophone.PushToTalkKeycode = newKey;
        UpdatePushToTalkText();
        yield return null;
    }
    public void UpdatePushToTalkText(){
        if(UserMicrophone != null){
            string inputText = UserMicrophone.PushToTalkKeycode.ToString();
            PushToTalkText.text = inputText;
        }
    }
    public void ChangeVoiceSetting(){
        switch(User_MicType.value){
            case 0:
                UserMicrophone.setSettings(Mumble.MumbleMicrophone.MicType.AlwaysSend,User_Microphones.value);
                UserMicrophone.StartSendingAudio(myMumble.getClient().EncoderSampleRate);
                break;
            case 1:
                UserMicrophone.setSettings(Mumble.MumbleMicrophone.MicType.Amplitude,User_Microphones.value);
                UserMicrophone.StartSendingAudio(myMumble.getClient().EncoderSampleRate);
                break;
            case 2:
                UserMicrophone.setSettings(Mumble.MumbleMicrophone.MicType.PushToTalk,User_Microphones.value);
                UserMicrophone.StopSendingAudio();
                break;
            case 3:
                UserMicrophone.setSettings(Mumble.MumbleMicrophone.MicType.MethodBased,User_Microphones.value);
                UserMicrophone.StopSendingAudio();
                break;
        }                
    }
    public void UpdateVoiceSensitivity(){
        UserMicrophone.MinAmplitude = MicrophoneSensitivity.value;
    }
    public void ToggleMute(){
        if(myMumble.getClient().IsSelfMuted())
            myMumble.getClient().SetSelfMute(false);
        else
            myMumble.getClient().SetSelfMute(true);
    }

    public void ShowConnectionPanel(){
        MicrophonePanel.SetActive(false);
        VoiceConnectionPanel.SetActive(true);
    }
    public void HideConnectionPanel(){
        VoiceConnectionPanel.SetActive(false);
        MicrophonePanel.SetActive(true);
    }
    public void EnableVoiceChat(){
        _AudioManager.SetConnectionInfo(HostName.text,Password.text);
        _AudioManager.EnableVoiceChat();
        ConnectUsers.enabled = false;
    }
    public void Destroy(){
        Destroy(gameObject);
    }
    //On destroy update Microphone settings
    private void OnDestroy() { 
        _AudioManager.setVoiceUIEnabled();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


}
