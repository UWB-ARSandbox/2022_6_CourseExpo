using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HelpRequestButton : MonoBehaviour
{

    public string username;
    private GameObject Player;
    public float id;
    void Start()
    {
        if(id != 1){
            Player = GameObject.Find("FirstPersonPlayer(Clone)");
            GetComponentInChildren<TMP_Text>().text = username + " has reqested help!";
        }
        else{
            GetComponentInChildren<TMP_Text>().text = "Finished helping " +username ;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TeleportToStudent()
    {
        if(id != 1){
            GameObject.Find("GameManager").GetComponent<AudioManager>().moveChannel("Private");
            Player.transform.GetComponent<CharacterController>().enabled = false;
            Player.transform.position = GameObject.Find(username).transform.position + (Vector3.up);
            Player.transform.GetComponent<CharacterController>().enabled = true;
            //check to see if the teacher is currently helping a student if so end the chat with that student and return them to their previous channel
            if(transform.parent.parent.parent.GetComponent<HelpRequestedUI>().CurrentlyHelping && 
            transform.parent.parent.parent.GetComponent<HelpRequestedUI>().CurrentlyHelping_id != id &&
            transform.parent.parent.parent.GetComponent<HelpRequestedUI>().CurrentlyHelping_id > 1){
                transform.parent.parent.parent.GetComponent<HelpRequestedUI>().HelpFinished();
            }
            transform.parent.parent.parent.GetComponent<HelpRequestedUI>().ReenableButton(id);
            transform.parent.parent.parent.GetComponent<HelpRequestedUI>().SpawnHelpFinishedButton();
            Destroy(gameObject);
        }
        else{
            transform.parent.parent.parent.GetComponent<HelpRequestedUI>().HelpFinished();
            Destroy(gameObject);
        }
    }
}
