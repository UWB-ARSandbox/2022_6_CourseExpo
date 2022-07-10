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

    List<int> connectedPlayers = new List<int>();
    private const float PINGREQUEST = 1000;
    private const float PINGRESPONSE = 1001;


    void Start()
    {
        m_ASLObject = GetComponent<ASLObject>();
        m_ASLObject._LocallySetFloatCallback(FloatReceive);
        
        if (GameManager.AmTeacher)
        {
            playerList = GameLiftManager.GetInstance().m_Players;
            StartCoroutine(SendPing());
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator SendPing()
    {
        while (true)
        {
            myFloats[0] = PINGREQUEST;
            foreach (int playerID in playerList.Keys)
            {
                if (playerID != 1)
                {
                    myFloats[1] = playerID;
                    m_ASLObject.SendAndSetClaim(() => {
                    m_ASLObject.SendFloatArray(myFloats);
                });
                }
            }
            yield return new WaitForSeconds(waitTime);
            foreach(int playerID in playerList.Keys)
            {
                if(connectedPlayers.Contains(playerID))
                {
                    Debug.Log(GameManager.players[playerID] + " is connected!");
                }
            }
            connectedPlayers.Clear();
        }
    }

    void FloatReceive(string _id, float[] _f)
    {
        if((int)_f[0] == PINGREQUEST && (int)_f[1] == GameManager.MyID)
        {
            myFloats[0] = PINGRESPONSE;
            myFloats[1] = GameManager.MyID;
            m_ASLObject.SendAndSetClaim(() => {
                m_ASLObject.SendFloatArray(myFloats);
            });
        }

        if((int)_f[0] == PINGRESPONSE && GameManager.AmTeacher)
        {
            Debug.Log(GameManager.players[(int)_f[1]] + " has ponged");
            connectedPlayers.Add((int)_f[1]);
        }
    }
}
