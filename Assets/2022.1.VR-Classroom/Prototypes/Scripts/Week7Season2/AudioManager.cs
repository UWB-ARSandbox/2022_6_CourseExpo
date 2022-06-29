using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public GameObject VoiceUI;
    
    //prefab
    public GameObject PrefabVoIP_UI;

    public GameObject PrefabTeacherAudioUI;
    public PlayerController my_Controller;

    private bool VoiceUIEnabled = false;

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
    // Start is called before the first frame update
    void Start()
    {        
        VoiceUI = (GameObject)Resources.Load("MyPrefabs/VoIP UI");
        PrefabVoIP_UI = (GameObject)Resources.Load("MyPrefabs/VoIP UI");
    }

    // Update is called once per frame
    void Update()
    {

        // if(Input.GetKeyDown(KeyCode.I)){
        //     ReconnectVoIP();
        // }
        // if(Input.GetKeyDown(KeyCode.V)){
        //     AttachAudio();
        // }
        if(Input.GetKeyDown(KeyCode.V)){          
            if(VoiceUI == null && !VoiceUIEnabled){
                VoiceUI = GameObject.Instantiate(PrefabVoIP_UI);
                VoiceUI.GetComponent<VoiceUI>().SetUserMicrophone(mumbleMic);
                VoiceUI.GetComponent<VoiceUI>().SetMumble(mumble);
                setVoiceUIEnabled();
            }
            else
                VoiceUI.GetComponent<VoiceUI>().Destroy();
        }
    }
    public void setVoiceUIEnabled(){
        VoiceUIEnabled = !VoiceUIEnabled;
    }
    //called by the mumble actor to setup the actor and the Audio Manager.
    public void Setup(MumbleActor mum, Mumble.MumbleMicrophone mumMic){
        mumble = mum;
        mumbleMic = mumMic;
        mumble.ConnectionEstablished += startChannelCreation;
        if(!GameManager.AmTeacher || AdminFlag){
            Username = GameManager.players[GameManager.MyID];
            mumble.Username = Username;
            if(HostName != null && Password != null){

            }
        }
        else{
            SetAdmin();
        }
    }
    public void startChannelCreation(){
        if(GameManager.AmTeacher && AdminFlag && !AudioAttached)
            StartCoroutine(CreateChannels());
        else{
            AttachAudio();
            mumble.ConnectionEstablished -= startChannelCreation;
        }
    }
    //connecting VoIP Prefabs to ghostplayers
    //Note your individual ghostplayer will not have a prefab attached to it
    public void AttachAudio(){
        if(!AudioAttached){
            Debug.Log("Attempting to Attach AudioOutputs to Ghosts");
            GhostPlayer[] GhostPlayers = (GhostPlayer[])GameObject.FindObjectsOfType(typeof(GhostPlayer));
            foreach(GhostPlayer i in GhostPlayers){
                Debug.Log("Found GhostPlayer: " + i.worldspaceUsername.transform.parent.name +" attempting to find AudioPlayer");
                GameObject Ghosts_AudioPlayer = GameObject.Find(i.worldspaceUsername.transform.parent.name + "_MumbleAudioPlayer");
                
                if(Ghosts_AudioPlayer != null){
                    Debug.Log("Successfully Found AudioPlayer for: "+ i.worldspaceUsername.transform.parent.name);
                    Ghosts_AudioPlayer.transform.parent = i.gameObject.transform;
                    Ghosts_AudioPlayer.transform.position = i.gameObject.transform.position;
                }
                else{
                    Debug.Log("Failed to find AudioPlayer for: " + i.worldspaceUsername.transform.parent.name);
                }
            }
            //Attach your ghost prefab to yourself
            GameObject.Find(Username + "_MumbleAudioPlayer").transform.parent = my_Controller.gameObject.transform;
            AudioAttached = true;
        }
        else
            Debug.Log("Audio already attached to ghosts");
    }
    //Call the moveChannel function when user is in desired area and needs to move VoIP channels
    //maybe subscribe to C# actions so when Action X happens user is moved
    //default room is always "root"
    public void moveChannel(string RoomName){
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
    //Return user to previous channel in mumble server
    //Function should be called after the user is moved to a private room with the teacher following the help function
    public void ReturnToPreviousChannel(){
        if(_mumbleClient == null)
            _mumbleClient = mumble.getClient();
        if(!_mumbleClient.GetCurrentChannel().Equals(previousChannel))
            _mumbleClient.JoinChannel(previousChannel);
        else
            Debug.Log("User is already in :" + previousChannel);
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
    public void ReconnectVoIP(){
        mumble.Disconnect();
        my_Controller.CreateMumbleObject();
    }
    public void ChannelToBeCreated(string channelName){
        if(channelName.Contains("Quiz") || channelName.Contains("Test"))
            ChannelList.Add(channelName);
    }
    // public void CreateChannels(){
    //     if(GameManager.AmTeacher && AdminFlag && !AudioAttached){
    //         foreach(string s in ChannelList){
    //             Debug.Log(s);
    //             if(CreateChannel(s,10)){
    //                 Debug.Log("Channel Created successfully for: "+s);
    //             }
    //             else
    //             Debug.Log("Failed to create channel for booth: " +s);
    //         }
    //         ReconnectVoIP();   
    //     }
    //     AttachAudio();
    //     mumble.ConnectionEstablished -= CreateChannels;
    // }
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
            ReconnectVoIP();
            yield return new WaitForSeconds(2f);
        }
        AttachAudio();
    }

}
