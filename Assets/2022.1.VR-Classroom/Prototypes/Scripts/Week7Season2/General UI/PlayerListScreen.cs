using System.Collections.Generic;
using UnityEngine;
using ASL;

public class PlayerListScreen : MonoBehaviour
{
    public Dictionary<int, string> players;
    public GameObject playerList;
    public GameObject listItem;

    public void generateList()
    {
        // clear list before populating it
        foreach (Transform child in playerList.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        // populate the playerList entry grid with a list item
        // for each connected player
        players = GameLiftManager.GetInstance().m_Players;
        foreach (string playerName in players.Values)
        {
            // skip if playerName is equal to client's name
            if (playerName == GameManager.players[GameManager.MyID])
                continue;
            AddUser(playerName);
        }
        
        // add client's name last to ensure they are on top
        // of the player list
        AddUser(GameManager.players[GameManager.MyID]);
    }

    void AddUser(string name)
    {
        GameObject newListItem = Instantiate(listItem, playerList.transform, false) as GameObject;
        newListItem.GetComponent<SinglelineContainer>().setText(name);
    }
}