using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuteAudioSource_Player : MonoBehaviour
{
    public string UserName;
    public AudioManager _myAudioManager;
    public AudioSource _myAudioSource;
    // Start is called before the first frame update
    void Start()
    {
        _myAudioManager = GameObject.Find("GameManager").GetComponent<AudioManager>();
        string FullUserName = gameObject.name;
        string[] subs = FullUserName.Split('_');
        UserName = subs[0];
        _myAudioManager.UpdateUserStates += SetAudioSourceMute;

        _myAudioSource = gameObject.GetComponent<AudioSource>();
        
        SetAudioSourceMute();
    }
    public void SetAudioSourceMute(){
        _myAudioSource.mute = _myAudioManager.GetUserState(UserName);
    }
    private void OnDestroy() {
        _myAudioManager.UpdateUserStates -= SetAudioSourceMute;
    }
}
