using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeacherVoiceUI : MonoBehaviour
{
    private MumbleActor mumble;
    // Start is called before the first frame update
    void Start()
    {
        if(mumble == null){
            mumble = GameObject.Find("Mumble").GetComponent<MumbleActor>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
