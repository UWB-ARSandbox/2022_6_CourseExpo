using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class StudentRoomSettings : MonoBehaviour
{
    GameLiftManager manager;
    int host;

    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.Find("GameLiftManager").GetComponent<GameLiftManager>();
        host = manager.GetLowestPeerId();
        // Activate student settngs panel for users who are not host
        if(host != manager.m_PeerId)
		{
            this.gameObject.SetActive(true);
		}
        // Host will have different room settings so disable student settings
        else
		{
            this.gameObject.SetActive(false);
		}
    }
}
