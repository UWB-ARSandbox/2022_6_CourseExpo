using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public GameObject AudioUI;
    public GameObject TeacherAudioUI;

    public string Username;
    public string HostName;
    public string Password;

    private MumbleActor mumble;
    private Mumble.MumbleMicrophone mumbleMic;
    private Mumble.MumbleClient _mumbleClient;

    // Start is called before the first frame update
    void Start()
    {        
        if(mumble == null){
            mumble = GameObject.Find("Mumble").GetComponent<MumbleActor>();
        }
        if(mumbleMic == null){
            mumbleMic = GameObject.Find("MumbleMicrophone").GetComponent<Mumble.MumbleMicrophone>();
        }
        if(GameObject.Find("GameManager")){
            if(!GameManager.AmTeacher)
                Username = GameManager.players[GameManager.MyID];
            else{
                SetAdmin();
                _mumbleClient = mumble.getClient();
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        
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
