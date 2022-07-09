using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class AudioManager : MonoBehaviour
{ 
    /*  AudioManager
        Intent: The AudioManager is intended to facilitate the configuration and communcation between the Mumble Actor 
        and the course Expo. The AudioManager contains VoiceUI configuration settings and facilitates the creation of the Voice UI prefabs
        using calls from the MenuScreen.
           
    */

    public GameObject VoiceUI;
   
    //prefabs
    public GameObject PrefabVoIP_UI;
    public GameObject PrefabVRVoIP_UI;

    public GameObject PrefabTeacherVoiceUI;
    public PlayerController my_Controller;
    
    public Dictionary<string,bool> PlayerMuteStatus = new Dictionary<string, bool>();

    public bool VoiceChatEnabled = false;

    private bool VoiceUIEnabled = false;

    public MenuScreen PCMenu;
    public MenuScreen VRMenu;

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
    public bool runningTest = false;
    public bool TestSuccess_bool = false;

    private void Awake() {
        ASL_GameManager = gameObject.GetComponent<ASLObject>();
    }
    // Start is called before the first frame update
    void Start(){
        Username = GameManager.players[GameManager.MyID];
        if(PCMenu == null){
            PCMenu = GameObject.Find("PC Menu").GetComponent<MenuScreen>();
        }
    }

    #region MuteUser
    //Intent is to provide the username of the user you want to mute
    //May be better to use a list of all players and search them to see if they should be muted or unmuted
    //and Update the audiosource of the target 
    public bool GetUserState(string TargetUser){
        if(PlayerMuteStatus.ContainsKey(TargetUser)){
        }
        else{
            PlayerMuteStatus.Add(TargetUser,false);
        }
        return PlayerMuteStatus[TargetUser];
    }
    public void MuteUser(string TargetUser){
        if(!PlayerMuteStatus.ContainsKey(TargetUser)){
            PlayerMuteStatus.Add(TargetUser,false);
        }
        if(VoiceChatEnabled){
            PlayerMuteStatus[TargetUser] = true;
            if(UpdateUserStates != null){
                UpdateUserStates();
            }
        }
    }
    public void UnMuteUser(string TargetUser ){
        if(!PlayerMuteStatus.ContainsKey(TargetUser)){
            PlayerMuteStatus.Add(TargetUser,false);
        }
        if(VoiceChatEnabled){
            PlayerMuteStatus[TargetUser] = false;
            if(UpdateUserStates != null){
                UpdateUserStates();
            }
        }
    }
    //broadcast changes to users muted/unmuted
    //Subscribe to the event through AudioManager.UpdateUserStates += <functionName>;
    public event System.Action UpdateUserStates;

    #endregion

    //Intended to be called by the GameManager upon recieving the CNNCT command from the Teacher
    //float[] _f should contain such information such as HostName and Password where password must match exactly
    //And hostname shall match the hostname indicated in the Voice Chat Setup guide
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

        if(VoiceUI != null)
            VoiceUI.GetComponent<VoiceUI>().Destroy();
            //VoiceUI = GameObject.Instantiate(PrefabVoIP_UI);
    }

    //setVRVoiceUIEnabled and setVoiceUIEnabled are both intended to be called by their requisite buttons
    //or upon the deletion of the VoiceUI object they should not be called from within other functions
    public void setVRVoiceUIEnabled(){
        if(VoiceUIEnabled){
            VRMenu.flipScreen();
        }
        else{
            VoiceUI = GameObject.Instantiate(PrefabVRVoIP_UI);
            VoiceUI.transform.SetParent(Camera.main.transform, false);
            if(VoiceChatEnabled){
                VoiceUI.GetComponent<VoiceUI>().SetUserMicrophone(mumbleMic);
                VoiceUI.GetComponent<VoiceUI>().SetMumble(mumble);
            }  
        }
        VoiceUIEnabled = !VoiceUIEnabled;
    }
    public void setVoiceUIEnabled(){
        if(VoiceUIEnabled){
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
        if(runningTest){
            ConnectionTest_Setup(mum, mumMic);
            return;
        }
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

    //Intended to be called by the Mumble Actor upon successful connection
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

    //Move user to root channel in mumble server
    //Function should be called when the user moves from a booth boundry back into the general boundry.
    public void ReturnToRootChannel(){
        if(VoiceChatEnabled){
            if(_mumbleClient == null)
                _mumbleClient = mumble.getClient();
            if(!_mumbleClient.GetCurrentChannel().Equals("root"))
                _mumbleClient.JoinChannel("root");
            else
                Debug.Log("User is already in root");
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

    //Reconnection functionality Intent is to be used to facilitate the disconnection of the SuperUser and reconnection of the teacher user
    //Potential Use: could be used to facilitate the reconnection of any user if they disconnect from the server, although it is unknown
    //if the user needs this functionality since it is likely that if the general user disconnects from the voice server they are disconnected
    //from the actual lobby in itself.
    IEnumerator ReconnectVoIP(){
        mumble.Disconnect();
        yield return new WaitForSeconds(2f);
        _mumbleClient = null;
        mumble.ConnectionEstablished -= startChannelCreation;
        my_Controller.CreateMumbleObject();
        yield return new WaitForSeconds(2f);
    }

    //Intent is to create a list of channels that meet a specific criteria to reduce the amount of channels to be created
    public void ChannelToBeCreated(string channelName){
        if(channelName.Contains("Quiz") || channelName.Contains("Test") || channelName.Contains("Assessment"))
            ChannelList.Add(channelName);
    }

    //Channels to be created each quiz room needs a channel,
    //one private channel for help functionality
    //User must have administrative rights within the server to create a channel
    //This function should only be called while the teacher is the SuperUser
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

    //Create Channels Coroutine
    //Should only be called while the teacher is the SuperUser
    //WaitForSeconds function calls are intended to slow down the coroutine
    //Due to mumble server calls taking time to complete it is critical that channel creation be slowed down
    //so that channel creation happens while the user is still the SuperUser and not before the user is reconnected.
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
                CreateChannel("Group1",10);
                yield return new WaitForSeconds(1f);
                mumble.ConnectionEstablished -= startChannelCreation;
                yield return new WaitForSeconds(2f);
                StartCoroutine(ReconnectVoIP());
                yield return new WaitForSeconds(6f);
            }
        }
    }
    #region Test Connection
    //Test connection functionality required to ensure the Teacher is not giving the students bad information
    //upon test success send the enable message to the other clients
    //To Do: Write Test functionality so that it does not interfere with normal connection functionality
    //
    public void TestConnection(string host, string pass){
        HostName = host;
        Password = pass;
        StartCoroutine(StartConnectionTest());
    }

    IEnumerator StartConnectionTest(){
        runningTest = true;
        my_Controller.CreateMumbleObject();
        yield return new WaitForSeconds(4f);
        yield return new WaitForSeconds(2f);
        if(mumble.getClient().ConnectionSetupFinished){
            //Debug.Log("Test Successful?");
            TestSuccess();
        }
        else{
            //Debug.Log("Test Unsuccessful?");
            TestFailure();
        }
    }

    void ConnectionTest_Setup(MumbleActor mum, Mumble.MumbleMicrophone mumMic){
        mumble = mum;
        mumbleMic = mumMic;
        mumble.HostName = HostName;
        mumble.Password = Password;
    }
    //Upon test success terminate test connection & unlock enable voice chat button
    public void TestSuccess(){
    //lock inputfields so the teacher cannot modify the successful connection information
        mumble.Disconnect();
        if(_mumbleClient != null)
            _mumbleClient = null;
        runningTest = false;
        TestSuccess_bool = true;
        VoiceUI.GetComponent<VoiceUI>().TestConnectionSuccess();
    }
    //if test fails initiate error message, lock Enable Voice Chat button
    public void TestFailure(){
        mumble.Disconnect();
        if(_mumbleClient != null)
            _mumbleClient = null;
        runningTest = false;
        VoiceUI.GetComponent<VoiceUI>().TestConnectionFailure();
    }
    #endregion 

}
