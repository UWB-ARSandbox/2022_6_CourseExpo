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
            groups[0].members.Add("Bobby 1");
            groups[0].members.Add("Bobby 2");
            groups[0].members.Add("Bobby 3");
            groups[1].members.Add("Dobby 1");
            groups[1].members.Add("Dobby 2");
            groups[1].members.Add("Dobby 3");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ValueChanged()
    {
        groupName.text = "";
        groupMembers.text = "";
        Debug.Log("Value changed to " + groupList.options[groupList.value].text);
        LoadGroupData(int.Parse(groupList.options[groupList.value].text.Split(' ')[1]) - 1);
    }

    public void LoadGroupData(int index)
    {
        groupName.text = groups[index].name;
        foreach (string member in groups[index].members)
        {
            groupMembers.text += (member + "\n");
        }
    }

    public void ShowAddPlayerScreen()
    {
        addPlayerContainer.SetActive(true);
    }
}
