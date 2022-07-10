using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class PingManager : MonoBehaviour
{

    public float waitTime = 5;
    ASLObject m_ASLObject;

    void Start()
    {
        m_ASLObject = GetComponent<ASLObject>();
        m_ASLObject._LocallySetFloatCallback(FloatReceive);
        
        if (GameManager.AmTeacher)
        {
            StartCoroutine(SendPing());
        }
    }

    IEnumerator SendPing()
    {
        while (true)
        {
            yield return new WaitForSeconds(waitTime);
            GhostPlayer[] ghostList = FindObjectsOfType<GhostPlayer>();
            foreach(GhostPlayer ghost in ghostList)
            {
                if (ghost.gameObject.name != GameManager.players[GameManager.MyID])
                {
                    if (!GameLiftManager.GetInstance().m_Players.ContainsValue(ghost.gameObject.name))
                    {
                        List<float> nameFloats = new List<float>();
                        nameFloats.AddRange(GameManager.stringToFloats(ghost.gameObject.name));
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
        GameObject.Find(username).transform.parent.gameObject.SetActive(false);
    }
}
