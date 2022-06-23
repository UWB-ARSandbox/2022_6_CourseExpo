using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ASL;

// Author: Gary Yuen
// The class MenuManager keeps track of references to UI elements for other scripts to call upon. 
// This is intended to prevent the timing of scripts from causing unexpected behaviors.
public class MenuManager : MonoBehaviour
{
    GameObject saveButton, SaveConfirmMenu, Kicker;

    public GameObject loadMenu, FaceButton, leaveClassButton, disbandClassButton;

    GameLiftManager manager;
    
    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.Find("GameLiftManager").GetComponent<GameLiftManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown("q"))
        {
            //loadMenu.SetActive(!loadMenu.activeSelf);
        }
    }

    public void Initialize()
    {
        FaceButton = GameObject.Find("FaceButton");
        loadMenu = GameObject.Find("Load Menu");
        SaveConfirmMenu = GameObject.Find("SaveConfirmMenu");
        saveButton = GameObject.Find("SaveButton");

        leaveClassButton = GameObject.Find("LeaveTheClassButton");
        disbandClassButton = GameObject.Find("DisbandClass");
        
        if (manager.AmLowestPeer() == false)
		{
            disbandClassButton.SetActive(false);
        }
        else
        {
            leaveClassButton.SetActive(false);
        }

        
    }

    // Toggles object based on argument.
    public void ToggleActive(GameObject button)
    {
        button.SetActive(!button.activeSelf);

    }

    public void SetFaceReference(GameObject faceButton)
    {
        FaceButton = faceButton;
    }

}
