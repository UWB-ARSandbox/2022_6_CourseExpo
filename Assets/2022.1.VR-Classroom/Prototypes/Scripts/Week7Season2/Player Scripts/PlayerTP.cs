using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTP : MonoBehaviour
{
    private GameObject Player;
    string username;

    void Start()
    {
        username = GetComponentInChildren<Text>().text;
        Player = GameObject.Find("FirstPersonPlayer(Clone)");
        if (username == GameManager.players[GameManager.MyID])
        {
            GetComponent<Button>().interactable = false;
        }
    }

    public void FindPlayer()
    {
        // if the player exists and is not taking a test then teleport
        // them to the selected user
        if (Player != null && !GameManager.isTakingAssessment)
        {
            Player.transform.GetComponent<CharacterController>().enabled = false;
            Player.transform.position = GameObject.Find(username).transform.position + (Vector3.up);
            Player.transform.GetComponent<CharacterController>().enabled = true;
        }
    }
}
