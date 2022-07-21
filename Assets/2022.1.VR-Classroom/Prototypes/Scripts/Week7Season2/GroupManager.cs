using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using ASL;

public class GroupManager : MonoBehaviour
{
    public List<Group> groups = new List<Group>();
    public TMP_Dropdown groupList;
    public Button groupsButton;
    public Button addMemberButton;
    public TMP_Text groupName;
    public TMP_Text groupMembers;
    public GameObject addPlayerContainer;

    public Dictionary<int, string> playerList;
    public GameObject List;
    public GameObject BoothObject;

    void Start()
    {
        groupName.enabled = false;
        groupMembers.enabled = false;
        addMemberButton.gameObject.SetActive(false);
        addPlayerContainer.SetActive(false);
        if (GameManager.AmTeacher)
        {
            groupsButton.gameObject.SetActive(true);
            for (int i = 0; i < 5; i++)
            {
                Group group = new Group();
                group.name = "Group " + (i + 1);
                groups.Add(group);
                groupList.options.Add(new TMP_Dropdown.OptionData() { text = group.name });
            }
            // groups[0].members.Add("Bobby 1");
            // groups[0].members.Add("Bobby 2");
            // groups[0].members.Add("Bobby 3");
        }
    }

    public void AddPlayer(string playerName)
    {
        if (!groups[groupList.value - 1].members.Contains(playerName))
        {
            groups[groupList.value - 1].members.Add(playerName);
            ValueChanged();
        }
    }

    public void ValueChanged()
    {
        if (groupList.value == 0)
        {
            groupName.enabled = false;
            groupMembers.enabled = false;
            addMemberButton.gameObject.SetActive(false);
        }
        else
        {
            addMemberButton.gameObject.SetActive(true);
            groupName.enabled = true;
            groupMembers.enabled = true;
            groupName.text = "";
            groupMembers.text = "";            
            LoadGroupData(int.Parse(groupList.options[groupList.value].text.Split(' ')[1]) - 1);
        }

        Debug.Log("Value changed to " + groupList.options[groupList.value].text);
    }

    public void LoadGroupData(int index)
    {
        groupName.text = groups[index].name;
        if (groups[index].members.Count > 0)
        {
            foreach (string member in groups[index].members)
            {
                groupMembers.text += (member + "\n");
            }
        }
        else
        {
            groupMembers.text = "There are no members in this group!";
        }
        UpdateAddPlayerList();
    }

    public void ShowAddPlayerScreen()
    {
        UpdateAddPlayerList();
        addPlayerContainer.SetActive(true);
    }

    void UpdateAddPlayerList()
    {
        foreach (Transform child in List.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        playerList = GameLiftManager.GetInstance().m_Players;
        foreach (string item in playerList.Values)
        {
            if (!groups[groupList.value - 1].members.Contains(item))
            {
                var PlayerName = Instantiate(BoothObject, List.transform, false) as GameObject;
                PlayerName.GetComponent<SinglelineContainer>().setText(item);
            }
        }
    }

    public void CloseAddPlayerScreen()
    {
        addPlayerContainer.SetActive(false);
    }
}
