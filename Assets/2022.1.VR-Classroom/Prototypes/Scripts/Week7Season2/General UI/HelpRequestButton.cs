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
        StartCoroutine(TeleportToStudentCoRoutine());
    }

    IEnumerator TeleportToStudentCoRoutine(){
                if(id != 1){
            if(transform.parent.parent.parent.GetComponent<HelpRequestedUI>().CurrentlyHelping && 
            transform.parent.parent.parent.GetComponent<HelpRequestedUI>().CurrentlyHelping_id != id){
                transform.parent.parent.parent.GetComponent<HelpRequestedUI>().HelpFinished();
                Debug.Log("I must be done helping someone");
                yield return new WaitForSeconds(1f);
            }
            Player.transform.GetComponent<CharacterController>().enabled = false;
            Player.transform.position = GameObject.Find(username).transform.position + (Vector3.up);
            Player.transform.GetComponent<CharacterController>().enabled = true;
            //check to see if the teacher is currently helping a student if so end the chat with that student and return them to their previous channel
            transform.parent.parent.parent.GetComponent<HelpRequestedUI>().ReenableButton(id);
            transform.parent.parent.parent.GetComponent<HelpRequestedUI>().SpawnHelpFinishedButton();
            GameObject.Find("GameManager").GetComponent<AudioManager>().moveChannel("Private");
            GameObject.Find("GameManager").GetComponent<AudioManager>().IsBeingHelped = true;
            Destroy(gameObject);
        }
        else{
            transform.parent.parent.parent.GetComponent<HelpRequestedUI>().HelpFinished();
            Destroy(gameObject);
        }
    }
}
