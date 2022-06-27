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
        
    }

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
    //Teacher Functionality
    public void SetAdmin(){
        mumble.Username = "SuperUser";
        mumble.Password = "admin";
    }

    public void CreateChannels(){

    }
    public void MoveUser(){

    }
}
