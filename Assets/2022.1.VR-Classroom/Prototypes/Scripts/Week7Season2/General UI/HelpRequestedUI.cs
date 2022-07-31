using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ASL;

public class HelpRequestedUI : MonoBehaviour
{

    public GameObject scrollPanel;
    public GameObject buttonPrefab;
    ASLObject m_ASLObject;
    float[] id = new float[2];
    public GameObject requestHelpButton;

    public bool CurrentlyHelping = false;
    public float CurrentlyHelping_id = -1;
    //public GameObject HelpingFinishedPrefab;
    GameObject HelpingFinishedButton;

    void Start()
    {
        // requestHelpButton = GameObject.Find("RequestHelpButton");
        m_ASLObject = GetComponent<ASLObject>();
        m_ASLObject._LocallySetFloatCallback(FloatReceive);

        if (GameManager.AmTeacher)
            requestHelpButton.SetActive(false);
        else
            GameObject.Find("RequestHelpUI").SetActive(false);
    }

    public void HelpRequested() {
        id[0] = GameManager.MyID;
        id[1] = 100;
        requestHelpButton.GetComponent<Button>().enabled = false;
        m_ASLObject.SendAndSetClaim(() => {
            m_ASLObject.SendFloatArray(id);
        });
    }

    public void ReenableButton(float _id) {
        CurrentlyHelping = true;
        CurrentlyHelping_id = _id;
        id[0] = _id;
        id[1] = 101;
        m_ASLObject.SendAndSetClaim(() => {
            m_ASLObject.SendFloatArray(id);
        });
    }
    //call the HelpFinished function when the teacher clicks the done helping button-> should return the user being helped and the teacher to their previous channels
    public void HelpFinished(){
        GameObject.Find("GameManager").GetComponent<AudioManager>().IsBeingHelped = false;
        GameObject.Find("GameManager").GetComponent<AudioManager>().ReturnToPreviousChannel();
        if(HelpingFinishedButton != null){
            Destroy(HelpingFinishedButton);
        }
        id[0] = CurrentlyHelping_id;
        id[1] = 102;
        m_ASLObject.SendAndSetClaim(() => {
            m_ASLObject.SendFloatArray(id);
        });
        CurrentlyHelping = false;
        CurrentlyHelping_id = -1;
    }
    public void SpawnHelpFinishedButton(){
        if(HelpingFinishedButton != null){
            Destroy(HelpingFinishedButton);
        }
        HelpingFinishedButton = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        HelpingFinishedButton.GetComponent<HelpRequestButton>().username = GameManager.players[(int)CurrentlyHelping_id];
        HelpingFinishedButton.GetComponent<HelpRequestButton>().id = GameManager.MyID;
        HelpingFinishedButton.transform.parent = scrollPanel.transform;
        HelpingFinishedButton.transform.localScale = new Vector3(1, 1, 1);
    }

    void FloatReceive(string _id, float[] _f)
    {
        if ((int)_f[1] == 101 && !GameManager.AmTeacher)
        {
            if ((int)_f[0] == GameManager.MyID){
                requestHelpButton.GetComponent<Button>().enabled = true;
                //move user to private voice channel when teacher teleports to them
                GameObject.Find("GameManager").GetComponent<AudioManager>().moveChannel("Private");
                GameObject.Find("GameManager").GetComponent<AudioManager>().IsBeingHelped = true;
            }
        }
        else if ((int)_f[1] == 100 && GameManager.AmTeacher)
        {
            Debug.Log(GameManager.players[(int)_f[0]] + " has requested help");
            GameObject newButton = (GameObject)Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity, scrollPanel.transform);
            //newButton.transform.parent = scrollPanel.transform;
            newButton.transform.localScale = new Vector3(1, 1, 1);
            newButton.GetComponent<HelpRequestButton>().username = GameManager.players[(int)_f[0]];
            newButton.GetComponent<HelpRequestButton>().id = (int)_f[0];
        }
        else if((int)_f[1] == 102 && !GameManager.AmTeacher){
        //Move User back to their original channel
            if ((int)_f[0] == GameManager.MyID){
                GameObject.Find("GameManager").GetComponent<AudioManager>().IsBeingHelped = false;
                GameObject.Find("GameManager").GetComponent<AudioManager>().ReturnToPreviousChannel();
            }
        }
        
    }
}
