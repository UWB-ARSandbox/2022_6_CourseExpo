using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public GameObject AudioUI;
    public GameObject TeacherAudioUI;

    private string Username;
    public string HostName;
    public string Password;

    private MumbleActor mumble;
    private Mumble.MumbleMicrophone mumbleMic;
    private Mumble.MumbleClient _mumbleClient;

    // Start is called before the first frame update
    void Start()
    {        

    }

    // Update is called once per frame
    void Update()
    {
        // if(Input.GetKeyDown(KeyCode.I)){
        //     _mumbleClient = mumble.getClient();
        //     CreateChannels("QuizRoom_1",3);
        // }
    }

    //called by the mumble actor to setup the actor and the Audio Manager.
    public void Setup(MumbleActor mum, Mumble.MumbleMicrophone mumMic){
        mumble = mum;
        mumbleMic = mumMic;
        if(!GameManager.AmTeacher){
            Username = GameManager.players[GameManager.MyID];
            mumble.Username = Username;
            if(HostName != null && Password != null){

            }
        }
        else{
            SetAdmin();
        }
        
    }
    //Call the moveChannel function when user is in desired area and needs to move VoIP channels
    //maybe subscribe to C# actions so when Action X happens user is moved
    //default room is always "root"
    public void moveChannel(string RoomName){
        if(_mumbleClient == null)
            _mumbleClient = mumble.getClient();
        if(_mumbleClient.GetCurrentChannel != RoomName)
            _mumbleClient.JoinChannel(RoomName);
        else
            Debug.Log("User is already in :" + RoomName);
    }
    //Teacher Functionality
    public void SetAdmin(){
        mumble.Username = "SuperUser";
        mumble.Password = "Admin";
    }
    /*
    Create channel by providing desired RoomName and desired Size
    */
    public bool CreateChannels(string RoomName, string RoomSize){
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
