using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportTrigger : MonoBehaviour
{
    public Collider CharacterCollider;
    public string BoothName;
    public AudioManager _myAudioManager;

    //booth must have a collider and have trigger on
    //Expected to be placed on the "BoothZone" object

    void Start()
    {
        BoothName = gameObject.transform.parent.transform.parent.GetComponent<BoothManager>().boothName;        
        if(_myAudioManager == null){
            _myAudioManager = GameObject.Find("GameManager").GetComponent<AudioManager>();
        }
        if(BoothName.Contains("Quiz") || BoothName.Contains("Test") || BoothName.Contains("Assessment"))
            _myAudioManager.ChannelToBeCreated(BoothName);
        else
            Destroy(this);
    }

    void Update()
    {
        if(CharacterCollider == null){
            PlayerController player = (PlayerController)GameObject.FindObjectOfType(typeof(PlayerController));
            if(player !=null)
                CharacterCollider = player.gameObject.GetComponent<Collider>();
        }

    }

    private void OnTriggerEnter(Collider other) {
        Debug.Log(other.name+" Has entered trigger");
        if(other == CharacterCollider){
            _myAudioManager.moveChannel(BoothName);
        }
    }
    
    //ensures the user is in the appropriate channel
    private void OnTriggerStay(Collider other) {
        if(other == CharacterCollider){
            _myAudioManager.moveChannel(BoothName);
        }
    }

    private void OnTriggerExit(Collider other) {
        if(other == CharacterCollider){
            _myAudioManager.ReturnToRootChannel();
        }
    }
}
