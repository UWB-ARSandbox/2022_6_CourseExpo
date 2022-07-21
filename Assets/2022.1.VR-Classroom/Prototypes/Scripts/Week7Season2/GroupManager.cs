using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GroupManager : MonoBehaviour
{
    // Start is called before the first frame update

    public List<Group> groups = new List<Group>();
    public TMP_Dropdown groupList;
    public Button groupsButton;
    public TMP_Text groupName;
    public TMP_Text groupMembers;
    public GameObject addPlayerContainer;

    void Start()
    {
        groupName.enabled = false;
        groupMembers.enabled = false;
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
        }
        else
        {
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
    }

    public void ShowAddPlayerScreen()
    {
        addPlayerContainer.SetActive(true);
    }

    public void CloseAddPlayerScreen()
    {
        addPlayerContainer.SetActive(false);
    }
}
