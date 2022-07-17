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

    void Start()
    {
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
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[System.Serializable]
public class Group
{
    public string name;
    public List<string> members;
}
