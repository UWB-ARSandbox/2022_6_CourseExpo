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

    public Button TestConnection;
    public Button ConnectUsers;

    public Text ErrorOutput;

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
        ConnectUsers.enabled = false;
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
        MuteSelf.isOn = _AudioManager.isSelfMuted;
        
        if(_AudioManager.VoiceChatEnabled)
            HideConnectionPanel();
        else
            ShowConnectionPanel();

        if(!GameManager.AmTeacher){
            TestConnection.enabled = false;
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
        if(_AudioManager.TestSuccess_bool){
            ConnectUsers.enabled = true;
            TestConnection.enabled = false;
            ErrorOutput.text = "SUCCESS";
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

    #region Input Assignment
    //Current limitation no check to see if the keybind conflicts with existing keybinds
    //Potential fix is to create an interactable controls menu to allow the user to rebind all keys
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
    #endregion

    public void ChangeVoiceSetting(){
        switch(User_MicType.value){
            case 0:
                UserMicrophone.VoiceSendingType = Mumble.MumbleMicrophone.MicType.AlwaysSend;
                UserMicrophone.MicNumberToUse = User_Microphones.value;
                UserMicrophone.StartSendingAudio(myMumble.getClient().EncoderSampleRate);
                break;
            case 1:
                UserMicrophone.VoiceSendingType = Mumble.MumbleMicrophone.MicType.Amplitude;
                UserMicrophone.MicNumberToUse = User_Microphones.value;
                UserMicrophone.StartSendingAudio(myMumble.getClient().EncoderSampleRate);
                break;
            case 2:
                UserMicrophone.VoiceSendingType = Mumble.MumbleMicrophone.MicType.PushToTalk;
                UserMicrophone.MicNumberToUse = User_Microphones.value;
                UserMicrophone.StopSendingAudio();
                break;
        }                
    }

    public void UpdateVoiceSensitivity(){
        UserMicrophone.MinAmplitude = MicrophoneSensitivity.value;
    }

    public void ToggleMute(){
        if(MuteSelf.isOn){
            myMumble.getClient().SetSelfMute(true);
            _AudioManager.isSelfMuted = true;
        }
        else if(!MuteSelf.isOn){
            myMumble.getClient().SetSelfMute(false);
            _AudioManager.isSelfMuted = false;
        }
    }

    public void ShowConnectionPanel(){
        MicrophonePanel.SetActive(false);
        VoiceConnectionPanel.SetActive(true);
    }

    public void HideConnectionPanel(){
        VoiceConnectionPanel.SetActive(false);
        MicrophonePanel.SetActive(true);
    }

    //Create test function/button to test connection settings
    #region TestConnection
    public void RunTestConnection(){
        _AudioManager.TestConnection(HostName.text,Password.text);
        TestConnection.enabled = false;
    }
    public void TestConnectionSuccess(){
        //lock inputfields
        HostName.interactable = false;
        Password.interactable = false;
        //unlock EnableVoiceChat button
        ErrorOutput.text = "SUCCESS";
        ConnectUsers.enabled = true;
        TestConnection.enabled = false;
        
    }
    public void TestConnectionFailure(){
        Debug.LogError("Test Connection Failed");
        //spawn debug output
        TestConnection.enabled = true;
        ErrorOutput.text = "ERROR: Test Connection Failed";
    }
    #endregion

    //To Do: Create error handling for bad input/test input before sending to everyone else
    //IE only send connection info if the teacher is able to successfully connect to the server
    public void EnableVoiceChat(){
        _AudioManager.SetConnectionInfo(HostName.text,Password.text);
        ConnectUsers.enabled = false;
    }
    public void Destroy(){
        Destroy(gameObject);
    }
    //On destroy update Microphone settings
    private void OnDestroy() { 
        _AudioManager.setVoiceUIEnabled();
    }


}
