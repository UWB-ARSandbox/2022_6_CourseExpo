using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using ASL;
using UnityEngine.UI;
using TMPro;

public class Teleporter : MonoBehaviour, IClickable
{
    public GameObject Exit, Trigger;
    public TextMeshProUGUI text;
    public Text minimapText;
    public string textVal;
    public string minimapVal;

    private bool Entered = false;
    private GameObject Subject;

    void Start()
    {
        if(textVal != null)
        {
            text.text = textVal;
        }
        if (minimapVal != null)
        {
            minimapText.text = minimapVal;
        }
    }

    public void IClickableClicked()
    {
        if (Subject != null)
        {
            Subject.transform.GetComponent<CharacterController>().enabled = false;
            Subject.transform.position = Exit.transform.position + (Vector3.up);
            Subject.transform.GetComponent<CharacterController>().enabled = true;
        }
    }

    public void TriggerOnClick()
    {
        if(Subject != null)
        {
            Subject.transform.GetComponent<CharacterController>().enabled = false;
            Subject.transform.position = Exit.transform.position + (Vector3.up);
            Subject.transform.GetComponent<CharacterController>().enabled = true;
        }
    }

    public void LeftTrigger(bool var)
    {
        Entered = var;
        Subject = null;
    }
    public void EnterTrigger(bool var, GameObject entry)
    {
        Entered = var;
        Subject = entry;
    }
}
