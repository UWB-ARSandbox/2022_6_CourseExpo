using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class PingManager : MonoBehaviour
{

    public float waitTime = 60; // 1 minute - measured in seconds
    ASLObject m_ASLObject;

    void Start()
    {
        m_ASLObject = GetComponent<ASLObject>();
        m_ASLObject._LocallySetFloatCallback(FloatReceive);
        
        // start the ping routine if the connected user is a teacher
        if (GameManager.AmTeacher)
        {
            StartCoroutine(SendPing());
        }
    }

    IEnumerator SendPing()
    {
        // continuously compare the connected users player list to the ghost players in the scene
        // if a ghost player does not exist in the connected players list then send the name of the
        // disconnect user to all players to remove it from their scenes
        while (true)
        {
            yield return new WaitForSeconds(waitTime);
            GhostPlayer[] ghostList = FindObjectsOfType<GhostPlayer>();
            foreach(GhostPlayer ghost in ghostList)
            {
                string name = ghost.gameObject.name.Split('_')[0];
                if (name != GameManager.players[GameManager.MyID])
                {
                    if (!GameLiftManager.GetInstance().m_Players.ContainsValue(name))
                    {
                        List<float> nameFloats = new List<float>();
                        nameFloats.AddRange(GameManager.stringToFloats(name));
                        var nameFloatsArray = nameFloats.ToArray();
                        m_ASLObject.SendAndSetClaim(() => { m_ASLObject.SendFloatArray(nameFloatsArray); });
                    }
                }
            }
        }
    }

    void FloatReceive(string _id, float[] _f)
    {
        string username = "";
        for (int i = 0; i < _f.Length; i++) {
            username += (char)(int)_f[i];
        }
        GameObject.Find(username + "_GhostPlayer").gameObject.SetActive(false);
    }
}
