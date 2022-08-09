using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using ASL;

public class GroupManager : MonoBehaviour
{
    public List<Group> groups = new List<Group>();
    public ASLObject m_ASLObject;
    public bool VR_UI_Script;

    // teacher variables
    public int maxGroups = 5; // keep at five for now
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
    public Text groupNameText;

    // student variables
    public GameObject currentGroupWindow;
    public GameObject currentGroupListItem;
    public GameObject currentGroupList;
    public Group MyGroup = null;

    // ASL variables
    const float ADD_PLAYER = 500;
    const float REMOVE_PLAYER = 501;

    void Start()
    {
        groupName.enabled = false;
        groupMembers.enabled = false;
        addMemberButton.gameObject.SetActive(false);
        addPlayerContainer.SetActive(false);
        if(VR_UI_Script && PlayerController.isXRActive)
            m_ASLObject._LocallySetFloatCallback(FloatReceive);
        else if(!VR_UI_Script && !PlayerController.isXRActive)
            m_ASLObject._LocallySetFloatCallback(FloatReceive);
        
        // create groups and populate groupList dropdown for teacher's UI
        for (int i = 0; i < maxGroups; i++)
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

    // called when teacher clicks on a player list item in the AddPlayer UI
    public void AddPlayer(string playerName)
    {   
        // remove player from group if they are already in one
        foreach (Group group in groups)
        {
            if (group.members.Contains(playerName))
            {
                List<float> myFloats = new List<float>();
                myFloats.Add(REMOVE_PLAYER);
                myFloats.Add(group.groupNumber);
                myFloats.AddRange(GameManager.stringToFloats(playerName));
                var myFloatsArray = myFloats.ToArray();
                m_ASLObject.SendAndSetClaim(() => { m_ASLObject.SendFloatArray(myFloatsArray); });
            }
        }

        // add the player if they are already not in the group
        if (!groups[groupList.value - 1].members.Contains(playerName))
        {
            List<float> myFloats = new List<float>();
            myFloats.Add(ADD_PLAYER);
            myFloats.Add(groupList.value);
            myFloats.AddRange(GameManager.stringToFloats(playerName));
            var myFloatsArray = myFloats.ToArray();
            m_ASLObject.SendAndSetClaim(() => { m_ASLObject.SendFloatArray(myFloatsArray); });
        }
    }

    // called when teacher clicks on a player list item in the groups UI
    public void RemovePlayer(string playerName)
    {
        List<float> myFloats = new List<float>();
        myFloats.Add(REMOVE_PLAYER);
        myFloats.Add(groupList.value);
        myFloats.AddRange(GameManager.stringToFloats(playerName));
        var myFloatsArray = myFloats.ToArray();
        m_ASLObject.SendAndSetClaim(() => { m_ASLObject.SendFloatArray(myFloatsArray); });
    }

    // called when a player is added or removed from a group
    // or when a new group is selected from the teacher's group UI
    public void ValueChanged()
    {
        // update teachers group manager UI
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

        // update students group UI
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
                    foreach (string playerName in group.members)
                    {
                        if (playerName == GameManager.players[GameManager.MyID])
                            continue;
                        AddUserToList(playerName, currentGroupListItem, currentGroupList);
                    }
                    AddUserToList(GameManager.players[GameManager.MyID], currentGroupListItem, currentGroupList);
                }
            }
        }
    }

    // used by teacher to update group manager UI
    public void LoadGroupData(int index)
    {
        groupName.text = groups[index].groupName;
        if (groups[index].members.Count > 0)
        {
            foreach (string member in groups[index].members)
            {
                AddUserToList(member, memberListItem, memberList);   
            }
        }
        UpdateAddPlayerList();
    }

    // called when teacher clicks on add player from the group manager UI
    public void ShowAddPlayerScreen()
    {
        UpdateAddPlayerList();
        addPlayerContainer.SetActive(true);
    }

    void UpdateAddPlayerList()
    {
        // clear list before populating it
        foreach (Transform child in addPlayerList.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        playerList = GameLiftManager.GetInstance().m_Players;
        foreach (string playerName in playerList.Values)
        {
            if (!groups[groupList.value - 1].members.Contains(playerName) && playerName != GameManager.players[GameManager.MyID])
            {
                AddUserToList(playerName, addPlayerListItem, addPlayerList);
            }
        }
    }

    public void CloseAddPlayerScreen()
    {
        addPlayerContainer.SetActive(false);
    }

    void AddUserToList(string name, GameObject listItem, GameObject list)
    {
        GameObject newListItem = Instantiate(listItem, list.transform, false) as GameObject;
        newListItem.GetComponent<SinglelineContainer>().setText(name);
    }

    public void FloatReceive(string _id, float[] _f) {
        string username;
        switch(_f[0]) {
            case ADD_PLAYER:
                username = "";
                for (int i = 2; i < _f.Length; i++) {
                    username += (char)(int)_f[i];
                }
                groups[(int)_f[1] - 1].members.Add(username);
                if (username == GameManager.players[GameManager.MyID]){
                    MyGroup = groups[(int)_f[1]];
                }
                ValueChanged();
                break;
            case REMOVE_PLAYER:
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