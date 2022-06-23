using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CourseXpo;

public class BoothManager : MonoBehaviour
{
    public string boothName; //Assumed filled in from editor
    private string boothDesc;
    private string boothType;
    private Text minimapName;

    public static ChatManager[] chatManagers;

    public static Dictionary<string, GameObject> boothNames = new Dictionary<string, GameObject>();
    public static bool verified = false;
    public bool boothVerified = false;

    private LockToggle lockToggle;
    public bool startUnlocked = false;

    // Start is called before the first frame update
    void Start() {
        chatManagers = FindObjectsOfType<ChatManager>(true);
        lockToggle = GetComponentInChildren<LockToggle>(true);
        if (startUnlocked) {
            lockToggle.startUnlocked = true;
            lockToggle.StartDelayedUnlock();
        }

        foreach (Canvas c in GetComponentsInChildren<Canvas>()) {
            if (c.name == "MinimapName") {
                minimapName = c.GetComponentInChildren<Text>();
                break;
            }
        }

        StartCoroutine(setBoothType());

        if (boothName == null || boothName.Replace(" ", "").Length == 0) {
            Debug.LogWarning("Empty booth name detected. Booth will NOT be added to boothNames list.");
        } else {
            if (!boothNames.ContainsKey(boothName)) {
                boothNames.Add(boothName, gameObject);
                minimapName.text = boothName;
            } else {
                Debug.LogError("Duplicate Booth Name \"" + boothName + "\" found in Scene and will not be counted for boothVerify"); 
            }
        }

        //Immediately Verfiy Teleport Booths
        if (GetComponentInChildren<Teleporter>() != null) {
            boothVerified = true;
        }
        //Immediately Verfiy Panorama Booths
        if (GetComponentInChildren<TogglePanorama>() != null) {
            boothVerified = true;
        }
        //Immediately Verfiy Video Booths
        if (GetComponentInChildren<VideoPlaybackManager>() != null) {
            boothVerified = true;
        }
        //Immediately Verfiy Object Booths
        if (GetComponentInChildren<GrabbableObject>() != null) {
            boothVerified = true;
        }
    }

    IEnumerator setBoothType() {
        //Wait for booth verification
        while (!verified) {
            yield return new WaitForSeconds(5.0f);
        }

        //Set boothDesc
        boothDesc = GameManager.BoothNamesAndDescriptions[boothName];
        boothType = GameManager.BoothNamesAndTypes[boothName];

        //Continuously attempt to get content until it has loaded
        while (!boothVerified) {
            GetBoothContent();
            float randomWaitTime = Random.Range(1.0f, 5.0f); 
            yield return new WaitForSeconds(randomWaitTime);
        }
        
    }

    private void GetBoothContent() {
        //Switch-case on boothType
        switch (boothType) {
            case "Test":
                GetComponent<AssessmentManager>().GetAssessment(boothName, boothDesc);
                break;
            case "Quiz":
                GetComponent<AssessmentManager>().GetAssessment(boothName, boothDesc);
                break;
            case "Assignment":
                GetComponent<AssessmentManager>().GetAssessment(boothName, boothDesc);
                break;
            case "Content":
                GetComponent<ContentManager>().GetContent(boothName, boothDesc);
                break;
            case "Media":
                //GetComponent<ContentManager>().GetContent(boothName, boothDesc);
                break;
            case "Example":
                GetComponent<ContentManager>().GetContent(boothName, boothDesc);
                break;
            case "Discussion": //this should be handled by the ContentManager
                GetComponent<ContentManager>().GetContent(boothName, boothDesc);
                break;
            default:
                break;
        }
    }

    public static void SetVerifiedStatus() {
        verified = true;
    }

    public static void UnlockAfterCompletion(string boothsToUnlock) {
        //Split UnlockAfterCompletion with comma as separator
        string[] names = boothsToUnlock.Split(',');
        //Find each booth and unlock it
        foreach (string name in names) {
            if (boothNames.ContainsKey(name)) {
                boothNames[name].GetComponentInChildren<LockToggle>().Unlock(true);
            }
        }
    }
}
