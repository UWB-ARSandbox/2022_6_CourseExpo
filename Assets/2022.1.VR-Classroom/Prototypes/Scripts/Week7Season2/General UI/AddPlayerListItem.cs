using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddPlayerListItem : MonoBehaviour
{    public string username;

    void Start()
    {
        username = GetComponentInChildren<Text>().text;
    }
}
