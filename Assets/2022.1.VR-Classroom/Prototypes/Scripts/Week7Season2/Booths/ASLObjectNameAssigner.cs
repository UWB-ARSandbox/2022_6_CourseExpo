using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ASLObjectNameAssigner : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        int i = 0;
        foreach (LockToggle go in FindObjectsOfType<LockToggle>()) {
            go.gameObject.name = "LockToggle" + i.ToString();
            i++;
        }

        i = 0;
        foreach (ForumManager fm in FindObjectsOfType<ForumManager>()) {
            fm.gameObject.name = "Forum" + i.ToString();
            i++;
        }
    }
}