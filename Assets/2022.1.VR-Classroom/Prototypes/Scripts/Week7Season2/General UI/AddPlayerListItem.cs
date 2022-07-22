using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddPlayerListItem : MonoBehaviour
{ 
    public string username;

    void Start()
    {
        username = GetComponentInChildren<Text>().text;
    }

    public void AddPlayer()
    {
        GameObject.Find("GroupsContainer").GetComponent<GroupManager>().AddPlayer(username);
        Destroy(gameObject);
    }
    public void RemovePlayer()
    {
        GameObject.Find("GroupsContainer").GetComponent<GroupManager>().RemovePlayer(username);
        Destroy(gameObject);
    }
}
