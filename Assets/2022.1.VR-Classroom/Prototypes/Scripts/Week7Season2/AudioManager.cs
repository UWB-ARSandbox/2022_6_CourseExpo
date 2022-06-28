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

    private MumbleActor mumble;
    private Mumble.MumbleMicrophone mumbleMic;
    private Mumble.MumbleClient _mumbleClient;

    private bool AudioAttached = false;

    public bool AdminFlag = false;
    public void SetController(PlayerController cont){my_Controller = cont;}
    // Start is called before the first frame update
    void Start()
    {        

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.I)){
            ReconnectVoIP();
        }
        if(Input.GetKeyDown(KeyCode.V)){
            AttachAudio();
        }
        // if(Input.GetKeyDown(KeyCode.V)){          
        //     if(VoiceUI == null && !VoiceUIEnabled){
        //         VoiceUI = GameObject.Instantiate(PrefabVoIP_UI);
        //         VoiceUI.GetComponent<VoiceUI>().SetUserMicrophone(mumbleMic);
        //         VoiceUI.GetComponent<VoiceUI>().SetMumble(mumble);
        //         setVoiceUIEnabled();
        //     }
        //     else
        //         VoiceUI.GetComponent<VoiceUI>().Destroy();
        // }
    }
    public void setVoiceUIEnabled(){
        VoiceUIEnabled = !VoiceUIEnabled;
    }
    //called by the mumble actor to setup the actor and the Audio Manager.
    public void Setup(MumbleActor mum, Mumble.MumbleMicrophone mumMic){
        mumble = mum;
        mumbleMic = mumMic;
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
                    //connecting VoIP Prefabs to ghostplayer
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
                }
                else{
                    Debug.Log("Failed to find AudioPlayer for: " + i.worldspaceUsername.transform.parent.name);
                }
            }
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
        if(!_mumbleClient.GetCurrentChannel().Equals(RoomName))
            _mumbleClient.JoinChannel(RoomName);
        else
            Debug.Log("User is already in :" + RoomName);
    }
    public bool GetAdminFlag(){
        return AdminFlag;
    }
    //Teacher Functionality
    //SuperUser cannot talk so we want to instantiate the super user, then create required channels and disconnect the super user
    //Teacher should connect as well so the teacher should have two instances of mumble active.
    //SuperUser instance needs to be deafened
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
    //Channels to be created each quiz room needs a channel,
    //one private channel for help functionality
    public bool CreateChannels(string RoomName, uint RoomSize){
        if(_mumbleClient == null)
            _mumbleClient = mumble.getClient();
        if(_mumbleClient.IsChannelAvailable(RoomName)){
            _mumbleClient.DestroyChannel(RoomName);  
        }
        _mumbleClient.CreateChannel(RoomName,true,0,"QuizRoom: with room allocation for n+1",RoomSize);
        return _mumbleClient.IsChannelAvailable(RoomName);
    }
    public void MoveUser(){

    }
}
