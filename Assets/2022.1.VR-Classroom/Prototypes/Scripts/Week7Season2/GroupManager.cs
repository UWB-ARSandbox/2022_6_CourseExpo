using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroupManager : MonoBehaviour
{
    // Start is called before the first frame update

    public List<Group> groups = new List<Group>();

    void Start()
    {
        for (int i = 0; i < 5; i++)
        {
            Group group = new Group();
            group.name = "Group " + (i + 1);
            groups.Add(group);
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
