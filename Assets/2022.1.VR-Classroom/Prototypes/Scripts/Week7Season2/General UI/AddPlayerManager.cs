using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class AddPlayerManager : MonoBehaviour
{
    public Dictionary<int, string> playerList;
    public GameObject List;
    public GameObject BoothObject;

    public void Start()
    {
        foreach (Transform child in List.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        playerList = GameLiftManager.GetInstance().m_Players;
        foreach (string item in playerList.Values)
        {
            var PlayerName = Instantiate(BoothObject, List.transform, false) as GameObject;
            PlayerName.GetComponent<SinglelineContainer>().setText(item);
        }
    }
}
