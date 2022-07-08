using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MutePlayer : MonoBehaviour
{
    public bool UserMuted = false;
    public AudioManager _myAudioManager;
    public string UserName;
    public Sprite Muted;
    public Sprite UnMuted;

    // Start is called before the first frame update
    void Start()
    {
        _myAudioManager = GameObject.Find("GameManager").GetComponent<AudioManager>();
        if(!_myAudioManager.VoiceChatEnabled){
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(UserName == null || UserName == ""){
            UserName = gameObject.transform.parent.gameObject.GetComponent<PlayerTP>().username;
        }
        UserMuted = _myAudioManager.GetUserState(UserName);
        if(UserMuted){
            gameObject.GetComponent<Image>().sprite = Muted;
        }
        else{
            gameObject.GetComponent<Image>().sprite = UnMuted;
        }
    }

    public void ToggleMute(){
        if(UserMuted){
            _myAudioManager.UnMuteUser(UserName);
        }
        else{
            _myAudioManager.MuteUser(UserName);
        }
    }
}
