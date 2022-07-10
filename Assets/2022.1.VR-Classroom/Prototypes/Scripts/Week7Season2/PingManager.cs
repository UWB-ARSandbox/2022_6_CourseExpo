using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class PingManager : MonoBehaviour
{

    public float waitTime = 5;

    void Start()
    {
        if (GameManager.AmTeacher)
        {
            StartCoroutine(SendPing());
        }
        else
        {
            this.enabled = false;
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
                        GameObject.Find(ghost.gameObject.name).transform.parent.gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}
