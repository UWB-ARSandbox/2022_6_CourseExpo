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
    public GameObject addPlayerList;
    public GameObject addPlayerListItem;
    public GameObject memberListItem;
    public GameObject memberList;
    public GameObject currentGroupWindow;
    public GameObject currentGroupListItem;
    public GameObject currentGroupList;
    public Text groupNameText;

    public Group MyGroup = null;

    public ASLObject m_ASLObject;
    void Start()
    {
        groupName.enabled = false;
        groupMembers.enabled = false;
        addMemberButton.gameObject.SetActive(false);
        addPlayerContainer.SetActive(false);
        m_ASLObject = GetComponent<ASLObject>();
        m_ASLObject._LocallySetFloatCallback(FloatReceive);
        for (int i = 0; i < 5; i++)
        {
            Group group = new Group();
            group.groupName = "Group " + (i + 1);
            group.groupNumber = (i + 1);
            groups.Add(group);
            if (GameManager.AmTeacher)
            {
                groupList.options.Add(new TMP_Dropdown.OptionData() { text = group.groupName });
            }
        }
        if (GameManager.AmTeacher)
        {
            groupsButton.gameObject.SetActive(true);
            currentGroupWindow.SetActive(false);
        }
    }

    public void AddPlayer(string playerName)
    {
        foreach (Group group in groups)
        {
            if (group.members.Contains(playerName))
            {
                List<float> myFloats = new List<float>();
                myFloats.Add(502);
                myFloats.Add(group.groupNumber);
                myFloats.AddRange(GameManager.stringToFloats(playerName));
                var myFloatsArray = myFloats.ToArray();
                m_ASLObject.SendAndSetClaim(() => { m_ASLObject.SendFloatArray(myFloatsArray); });
            }
        }

        if (!groups[groupList.value - 1].members.Contains(playerName))
        {
            List<float> myFloats = new List<float>();
            myFloats.Add(500);
            myFloats.Add(groupList.value - 1);
            myFloats.AddRange(GameManager.stringToFloats(playerName));
            var myFloatsArray = myFloats.ToArray();
            m_ASLObject.SendAndSetClaim(() => { m_ASLObject.SendFloatArray(myFloatsArray); });
        }
    }

    public void RemovePlayer(string playerName)
    {
        List<float> myFloats = new List<float>();
        myFloats.Add(501);
        myFloats.Add(groupList.value - 1);
        myFloats.AddRange(GameManager.stringToFloats(playerName));
        var myFloatsArray = myFloats.ToArray();
        m_ASLObject.SendAndSetClaim(() => { m_ASLObject.SendFloatArray(myFloatsArray); });
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
            foreach (Transform child in memberList.transform) {
                GameObject.Destroy(child.gameObject);
            }
            LoadGroupData(int.Parse(groupList.options[groupList.value].text.Split(' ')[1]) - 1);
        }

        if (!GameManager.AmTeacher)
        {
            groupNameText.text = "Group: None";
            foreach (Transform child in currentGroupList.transform)
            {
                GameObject.Destroy(child.gameObject);
            }
            foreach (var group in groups)
            {
                if (group.members.Contains(GameManager.players[GameManager.MyID]))
                {
                    groupNameText.text = group.groupName;
                    foreach (string item in group.members)
                    {
                        var listItem = Instantiate(currentGroupListItem, currentGroupList.transform, false) as GameObject;
                        listItem.GetComponent<SinglelineContainer>().setText(item);
                    }
                }
            }
        }

        Debug.Log("Value changed to " + groupList.options[groupList.value].text);
    }

    public void LoadGroupData(int index)
    {
        groupName.text = groups[index].groupName;
        if (groups[index].members.Count > 0)
        {
            foreach (string member in groups[index].members)
            {
                // groupMembers.text += (member + "\n");
                var listItem = Instantiate(memberListItem, memberList.transform, false) as GameObject;
                listItem.GetComponent<SinglelineContainer>().setText(member);
            }
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
        foreach (Transform child in addPlayerList.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        playerList = GameLiftManager.GetInstance().m_Players;
        foreach (string item in playerList.Values)
        {
            if (!groups[groupList.value - 1].members.Contains(item) && item != GameManager.players[GameManager.MyID])
            {
                var listItem = Instantiate(addPlayerListItem, addPlayerList.transform, false) as GameObject;
                listItem.GetComponent<SinglelineContainer>().setText(item);
            }
        }
    }

    public void CloseAddPlayerScreen()
    {
        addPlayerContainer.SetActive(false);
    }

    public void FloatReceive(string _id, float[] _f) {
        string username;
        switch(_f[0]) {
            case 500:
                username = "";
                for (int i = 2; i < _f.Length; i++) {
                    username += (char)(int)_f[i];
                }
                groups[(int)_f[1]].members.Add(username);
                if (username == GameManager.players[GameManager.MyID]){
                    MyGroup = groups[(int)_f[1]];
                }
                ValueChanged();
                break;
            case 501:
                username = "";
                for (int i = 2; i < _f.Length; i++) {
                    username += (char)(int)_f[i];
                }
                groups[(int)_f[1]].members.Remove(username);
                if (username == GameManager.players[GameManager.MyID]){
                    MyGroup = null;
                }
                ValueChanged();
                break;
            case 502:
                username = "";
                for (int i = 2; i < _f.Length; i++) {
                    username += (char)(int)_f[i];
                }
                groups[(int)_f[1] - 1].members.Remove(username);
                if (username == GameManager.players[GameManager.MyID]){
                    MyGroup = null;
                }
                ValueChanged();
                break;
        }
    }
}
