using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class AudioManager : MonoBehaviour
{
    public GameObject VoiceUI;
    
    //prefab
    public GameObject PrefabVoIP_UI;

    public GameObject PrefabTeacherVoiceUI;
    public PlayerController my_Controller;
    
    public bool VoiceChatEnabled = false;

    private bool VoiceUIEnabled = false;

    public MenuScreen PCMenu;

    public ASLObject ASL_GameManager;

    private string Username;
    public string HostName;
    public string Password;

    public List<string> ChannelList = new List<string>();
    private MumbleActor mumble;
    private Mumble.MumbleMicrophone mumbleMic;
    private Mumble.MumbleClient _mumbleClient;

    private bool AudioAttached = false;

    public bool AdminFlag = false;
    private string previousChannel;
    public void SetController(PlayerController cont){my_Controller = cont;}
    bool RunOnce = false;
    
    private void Awake() {
        // if(PrefabTeacherVoiceUI == null){
        //     //debug line until Teacher voiceChat enable is in
        //     VoiceChatEnabled = true;
        // }
        ASL_GameManager = gameObject.GetComponent<ASLObject>();
    }
    // Start is called before the first frame update
    void Start(){
        PrefabVoIP_UI = (GameObject)Resources.Load("MyPrefabs/VoIP_UI");
        Username = GameManager.players[GameManager.MyID];
        if(PCMenu == null){
            PCMenu = GameObject.Find("PC Menu").GetComponent<MenuScreen>();
        }
    }

    // Update is called once per frame
    void Update(){

    }

    public void EnableVoiceChat(){
        //my_Controller.CreateMumbleObject();
        //Need to trigger a cascade across all clients to  
        //enable voice chat and send connection info
    }

    public void RecieveConnectionInfo_FromGamemanager(float[] _f){
        //Get length of string
        Debug.Log("Password and HostName Recieved");
        int length = (int)_f[1];
        string announcementText = "";
        for (int i = 2; i <= length + 1; i++) {
            announcementText += (char)(int)_f[i];
        }
        string[] temp = announcementText.Split(':');        
        string H = temp[0];
        string P = temp[1];
        Debug.Log("Password Set to: " +P);
        Debug.Log("HostName Set to: " +H);
        HostName = H;
        Password = P;
        my_Controller.CreateMumbleObject();

            setVoiceUIEnabled();
            //VoiceUI = GameObject.Instantiate(PrefabVoIP_UI);
    }


    public void setVoiceUIEnabled(){
        if(VoiceUIEnabled){
            VoiceUI.GetComponent<VoiceUI>().Destroy();
            PCMenu.flipScreen();
        }
        else{
            VoiceUI = GameObject.Instantiate(PrefabVoIP_UI);
            if(VoiceChatEnabled){
                VoiceUI.GetComponent<VoiceUI>().SetUserMicrophone(mumbleMic);
                VoiceUI.GetComponent<VoiceUI>().SetMumble(mumble);
            }
        }
        VoiceUIEnabled = !VoiceUIEnabled;
    }
    //called by the mumble actor to setup the actor and the Audio Manager.
    public void Setup(MumbleActor mum, Mumble.MumbleMicrophone mumMic){
        mumble = mum;
        //current Debugging variables taken from mumble.Hostname and mumble.Password

        mumbleMic = mumMic;
        mumble.ConnectionEstablished += startChannelCreation;
        if(!GameManager.AmTeacher || AdminFlag){
            Username = GameManager.players[GameManager.MyID];
            mumble.Username = Username;
            if(HostName != null && Password != null){
                mumble.HostName = HostName;
                mumble.Password = Password;
            }
            else{
                HostName = mumble.HostName;
                Password = mumble.Password;
            }
        }
        else{
            SetAdmin();
        }
    }
    public void startChannelCreation(){
        VoiceChatEnabled = true;
        if(GameManager.AmTeacher && AdminFlag && !AudioAttached)
            StartCoroutine(CreateChannels());
        else{
            mumble.ConnectionEstablished -= startChannelCreation;
        }
    }
    public void SetConnectionInfo(string hostname, string password){
        HostName = hostname;
        Password = password;
        GameManager.SendEnableMessage(hostname + ":" + password);
    }
    //Audio Attachment happens on prefab creation time instead of a script
    //connecting VoIP Prefabs to ghostplayers
    //Note your individual ghostplayer will not have a prefab attached to it
    public void AttachAudio(){
        //     Debug.Log("Attempting to Attach AudioOutputs to Ghosts");
        //     GhostPlayer[] GhostPlayers = (GhostPlayer[])GameObject.FindObjectsOfType(typeof(GhostPlayer));
        //     foreach(GhostPlayer i in GhostPlayers){
        //         Debug.Log("Found GhostPlayer: " + i.worldspaceUsername.transform.parent.name +" attempting to find AudioPlayer");
        //         GameObject Ghosts_AudioPlayer = GameObject.Find(i.worldspaceUsername.transform.parent.name + "_MumbleAudioPlayer");
                
        //         if(Ghosts_AudioPlayer != null){
        //             Debug.Log("Successfully Found AudioPlayer for: "+ i.worldspaceUsername.transform.parent.name);
        //             Ghosts_AudioPlayer.transform.parent = i.gameObject.transform;
        //             Ghosts_AudioPlayer.transform.position = i.gameObject.transform.position;
        //         }
        //         else{
        //             Debug.Log("Failed to find AudioPlayer for: " + i.worldspaceUsername.transform.parent.name);
        //         }
        //     //Attach your ghost prefab to yourself
        //     GameObject.Find(Username + "_MumbleAudioPlayer").transform.parent = my_Controller.gameObject.transform;
        //     AudioAttached = true;
        // }
    }
    public void AttachIndividualAudio(GameObject audioPlayer){
        // if(audioPlayer.name == (Username + "_MumbleAudioPlayer")){
        //     audioPlayer.transform.SetParent(my_Controller.gameObject.transform);
        // }
        // else{
        //     string temp = audioPlayer.name;
        //     string target = temp.Substring(0, audioPlayer.name.Length - "_MumbleAudioPlayer".Length);
        //     GameObject targetObj = GameObject.Find(target);
        //     if(targetObj != null)
        //         audioPlayer.transform.SetParent(targetObj.transform);
        //     else
        //         Debug.Log("Failed to find target Object");
        // }
    }
    //Call the moveChannel function when user is in desired area and needs to move VoIP channels
    //maybe subscribe to C# actions so when Action X happens user is moved
    //default room is always "root"
    public void moveChannel(string RoomName){
        if(VoiceChatEnabled){
            if(_mumbleClient == null)
                _mumbleClient = mumble.getClient();
            previousChannel = _mumbleClient.GetCurrentChannel();
            if(!_mumbleClient.GetCurrentChannel().Equals(RoomName)){
                _mumbleClient.JoinChannel(RoomName);
                Debug.Log(Username+ " Moved to: " +RoomName);
            }
            else
                Debug.Log(Username+ " is already in :" + RoomName);
        }
    }
    //Return user to previous channel in mumble server
    //Function should be called after the user is moved to a private room with the teacher following the help function
    public void ReturnToPreviousChannel(){
        if(VoiceChatEnabled){
            if(_mumbleClient == null)
                _mumbleClient = mumble.getClient();
            if(!_mumbleClient.GetCurrentChannel().Equals(previousChannel))
                _mumbleClient.JoinChannel(previousChannel);
            else
                Debug.Log("User is already in :" + previousChannel);
        }
    }

    public bool GetAdminFlag(){
        return AdminFlag;
    }
    //Teacher Functionality
    //SuperUser cannot talk so we want to instantiate the super user, then create required channels and disconnect the super user
    //and connect the teacher
    public void SetAdmin(){
        mumble.Username = "SuperUser";
        mumble.Password = "Admin";
        AdminFlag = true;
    }
    /*
    Create channel by providing desired RoomName and desired Size
    */
    IEnumerator ReconnectVoIP(){
        mumble.Disconnect();
        yield return new WaitForSeconds(2f);
        _mumbleClient = null;
        mumble.ConnectionEstablished -= startChannelCreation;
        my_Controller.CreateMumbleObject();
        yield return new WaitForSeconds(2f);
    }

    public void ChannelToBeCreated(string channelName){
        if(channelName.Contains("Quiz") || channelName.Contains("Test"))
            ChannelList.Add(channelName);
    }

    //Channels to be created each quiz room needs a channel,
    //one private channel for help functionality
    public bool CreateChannel(string RoomName, uint RoomSize){
        Debug.Log("Attempting to Create Channel: " +RoomName);
        if(_mumbleClient == null)
            _mumbleClient = mumble.getClient();
        if(_mumbleClient.IsChannelAvailable(RoomName)){
            _mumbleClient.DestroyChannel(RoomName);  
        }
        _mumbleClient.CreateChannel(RoomName,false,0,"",RoomSize);
        return _mumbleClient.IsChannelAvailable(RoomName);
    }
    IEnumerator CreateChannels(){
        if(!RunOnce){
            RunOnce = true;
            if(GameManager.AmTeacher && AdminFlag && !AudioAttached){
                foreach(string s in ChannelList){
                    Debug.Log(s);
                    CreateChannel(s,10);
                    yield return new WaitForSeconds(.8f);
                    if(_mumbleClient.IsChannelAvailable(s))
                        Debug.Log("Channel Created successfully for: "+s);
                    else
                        Debug.LogError("Failed to create channel for booth: " +s);
                    yield return new WaitForSeconds(.6f);
                }
                CreateChannel("Private",2);
                yield return new WaitForSeconds(1f);
                mumble.ConnectionEstablished -= startChannelCreation;
                yield return new WaitForSeconds(2f);
                StartCoroutine(ReconnectVoIP());
                yield return new WaitForSeconds(6f);
            }
        }
    }

}
