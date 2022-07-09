using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportTrigger : MonoBehaviour
{
    public Collider CharacterCollider;
    public string BoothName;
    public AudioManager _myAudioManager;


    // Start is called before the first frame update
    void Start()
    {
        // GameObject player = (GameObject)GameObject.FindObjectOfType(typeof(PlayerController));
        // CharacterCollider = player.GetComponent<Collider>();

        // _myAudioManager = GameObject.Find("GameManager").GetComponent<AudioManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if(CharacterCollider == null){
            PlayerController player = (PlayerController)GameObject.FindObjectOfType(typeof(PlayerController));
            if(player !=null)
                CharacterCollider = player.gameObject.GetComponent<Collider>();
        }
        if(_myAudioManager == null){
            _myAudioManager = GameObject.Find("GameManager").GetComponent<AudioManager>();
        }
    }

    private void OnTriggerEnter(Collider other) {
        Debug.Log(other.name+" Has entered trigger");
        if(other == CharacterCollider){
            _myAudioManager.moveChannel(BoothName);
        }
    }

    private void OnTriggerExit(Collider other) {
        if(other == CharacterCollider){
            _myAudioManager.moveChannel("Root");
        }
    }
}
