using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class PlayerListScreen : MonoBehaviour
{
    public Dictionary<int, string> playerList;
    public GameObject List;
    public GameObject BoothObject;
    public GameObject TitleObject;
    public GameObject Player;
    public bool GenerateType;

    public void generateRangeList()
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

    public void generateList()
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
    
    //Leaving in incase we want to open it by key press.
    public void flipScreen()
    {
        if (gameObject.activeSelf == false)
        {
            Cursor.lockState = CursorLockMode.Confined;
            gameObject.SetActive(true);
            generateList();
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            gameObject.SetActive(false);
        }
    }
}
