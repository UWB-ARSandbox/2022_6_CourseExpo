using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class PingManager : MonoBehaviour
{

    public float waitTime = 5;
    ASLObject m_ASLObject;
    float[] myFloats = new float[2];
    public Dictionary<int, string> playerList;

    List<int> originalPlayers = new List<int>();
    List<int> connectedPlayers = new List<int>();
    private const float PINGREQUEST = 1000;
    private const float PINGRESPONSE = 1001;

    void Start()
    {
        m_ASLObject = GetComponent<ASLObject>();
        // m_ASLObject._LocallySetFloatCallback(FloatReceive);
        
        // Create player list and start the ping coroutine
        if (GameManager.AmTeacher)
        {

            // foreach (int playerID in playerList.Keys)
            // {
            //     if (playerID != 1)
            //     {
            //         originalPlayers.Add(playerID);
            //     }
            // }

            // playerList = GameLiftManager.GetInstance().m_Players;
            StartCoroutine(SendPing());
        }
    }

    IEnumerator SendPing()
    {
        while (true)
        {
            yield return new WaitForSeconds(waitTime);
            // // send an individual ping to each connected player
            // myFloats[0] = PINGREQUEST;
            // foreach (int playerID in originalPlayers)
            // {
            //     myFloats[1] = playerID;
            //     m_ASLObject.SendAndSetClaim(() => {
            //         m_ASLObject.SendFloatArray(myFloats);
            //     });
            // }
            // yield return new WaitForSeconds(5);
            // // go through each playerID in the playerList and remove any players
            // // that did not return a ping response
            // foreach(int playerID in originalPlayers)
            // {
            //     if(!connectedPlayers.Contains(playerID))
            //     {
            //         Debug.Log(GameManager.players[playerID] + " is not connected!");
            //         // if (GameObject.Find(GameManager.players[playerID]))
            //         // {
            //         //     GameObject.Find(GameManager.players[playerID]).transform.parent.gameObject.SetActive(false);
            //         // }
            //     }
            //     if(connectedPlayers.Contains(playerID))
            //     {
            //         Debug.Log(GameManager.players[playerID] + " is connected!");
            //     }
            // }
            // connectedPlayers.Clear();
            GhostPlayer[] ghostList = FindObjectsOfType<GhostPlayer>();
            foreach(GhostPlayer ghost in ghostList)
            {
                if (ghost.gameObject.name != GameManager.players[GameManager.MyID])
                {
                    if (!GameLiftManager.GetInstance().m_Players.ContainsValue(ghost.gameObject.name))
                    {
                        GameObject.Find(ghost.gameObject.name).transform.parent.gameObject.SetActive(false);
                    }
                }
            }
        }
    }

    // void FloatReceive(string _id, float[] _f)
    // {
    //     // if student -> return id as a ping response
    //     if((int)_f[0] == PINGREQUEST && (int)_f[1] == GameManager.MyID)
    //     {
    //         myFloats[0] = PINGRESPONSE;
    //         myFloats[1] = GameManager.MyID;
    //         m_ASLObject.SendAndSetClaim(() => {
    //             m_ASLObject.SendFloatArray(myFloats);
    //         });
    //     }

    //     // if teacher -> add student id to connectedPlayers list
    //     if((int)_f[0] == PINGRESPONSE && GameManager.AmTeacher)
    //     {
    //         Debug.Log(GameManager.players[(int)_f[1]] + " has ponged");
    //         connectedPlayers.Add((int)_f[1]);
    //     }
    // }
}
