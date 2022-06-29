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
        Player = GameObject.Find("FirstPersonPlayer(Clone)");
        GetComponentInChildren<TMP_Text>().text = username + " has reqested help!";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TeleportToStudent()
    {
        Player.transform.GetComponent<CharacterController>().enabled = false;
        Player.transform.position = GameObject.Find(username).transform.position + (Vector3.up);
        Player.transform.GetComponent<CharacterController>().enabled = true;
        transform.parent.parent.parent.GetComponent<HelpRequestedUI>().ReenableButton(id);
        Destroy(gameObject);
    }
}
