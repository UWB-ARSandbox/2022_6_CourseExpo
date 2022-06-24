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
    }

    public void FindPlayer()
    {
        if (Player != null)
        {
            Player.transform.GetComponent<CharacterController>().enabled = false;
            Player.transform.position = GameObject.Find(username).transform.position + (Vector3.up);
            Player.transform.GetComponent<CharacterController>().enabled = true;
        }
    }
}
