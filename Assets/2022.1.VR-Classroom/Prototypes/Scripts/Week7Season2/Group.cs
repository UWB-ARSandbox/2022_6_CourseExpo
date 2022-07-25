using System.Collections.Generic;

[System.Serializable]
public class Group {
    public string groupName;
    public int groupNumber;
    public List<string> members = new List<string>();
}
