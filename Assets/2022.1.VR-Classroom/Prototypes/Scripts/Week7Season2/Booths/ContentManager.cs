using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using ASL;
using CourseXpo;
using Microsoft.MixedReality.Toolkit;
using UnityEngine;
using TMPro;
using UnityEditor;
using UnityEngine.Networking;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class ContentManager : MonoBehaviour {
    /*
     * New model for content manager:
     *
     * lists of different types of content objects within
     * the single booth, say, multiple screens,
     * or multiple grabbable objects or even a mix of the two
     * (or more) types of content in a single booth
     *
     * Content manager:
     * 
     */

    public bool Completed => _lookAtScreen.Completed;

    private string unlockAfterCompletion = "";

    private string initalDescription = "";

    private List<object> Content = new List<object>();

    private List<object> Content2 = null;

    public ScreenManager _lookAtScreen = null;

    //Welcome Screen
    public GameObject pnl_WelcomeScreen;
    public TextMeshProUGUI txt_BoothName;
    public TextMeshProUGUI txt_BoothDesc;

    private ContentType contentType;
    private int num_lengthTotalContent = -1;
    private BoothManager boothManager;
    private int _contentIterator = -1;
    private bool _screenAttached = false;
    public enum ContentType {
        example,
        content, discussion, media
    }

    void Start() {
        LinkObjects();
        Debug.Assert(_lookAtScreen != null);
        boothManager = GetComponent<BoothManager>();
        txt_BoothName.text = "Booth Unavailable.";
        initalDescription = "There is no booth loaded at this location." +
                            "\nPlease contact the Expo host if this is a mistake.";
        txt_BoothDesc.text = initalDescription;
        StartCoroutine(CheckLoadedAndVerify());
        StartCoroutine(CheckContentCompleted());

    }

    private IEnumerator CheckContentCompleted() {
        while (!Completed) yield return new WaitForSecondsRealtime(0.5f);
        Debug.Log("UNLOCKING: " + unlockAfterCompletion);
        BoothManager.UnlockAfterCompletion(unlockAfterCompletion);
    }

    private IEnumerator CheckLoadedAndVerify() {
        while (!boothManager.boothVerified) {
            //Debug.LogError("boothName: " + txt_boothName.text + "\nboothManager.boothVerified: " + boothManager.boothVerified);
            bool isVerified = true;
            if (Content == null || num_lengthTotalContent < 0) {
                //Debug.LogWarning("CM: Booth \"" + boothManager.boothName + "\" Content == null || num_lengthTotalContent < 0");
                isVerified = false;
            } else {
                //Example needs 2 requests
                if (contentType != ContentType.example) {
                    if (Content.Count != num_lengthTotalContent) {
                        if (Content.Count > num_lengthTotalContent) {
                            Content.RemoveRange(num_lengthTotalContent, Content.Count - num_lengthTotalContent);
                        } else {
                            //Debug.LogWarning("CM: Booth \"" + boothManager.boothName + "\"Content.Count != num_lengthTotalContent");
                            isVerified = false;
                        }
                    }
                    /*for (int i = 0; i < num_lengthTotalContent; i++) {
                        if (Content[i] == null) {
                            Debug.LogWarning("CM: Booth \"" + boothManager.boothName + "\"contentType != ContentType.example + Content[i] == null");
                            isVerified = false;
                        }
                    }*/
                } else {
                    if (Content2 != null) {
                        if (Content.Count + Content2.Count != num_lengthTotalContent) {
                            //Debug.LogWarning("CM: Booth \"" + boothManager.boothName + "\"Content.Count + Content2.Count != num_lengthTotalContent");
                            isVerified = false;
                        } else {
                            /*for (int i = 0; i < Content.Count; i++) {
                                if (Content[i] == null) {
                                    Debug.LogWarning("CM: Booth \"" + boothManager.boothName + "\"Content2 != null + Content[i] == null");
                                    isVerified = false;
                                }
                            }

                            for (int i = 0; i < Content2.Count; i++) {
                                if (Content2[i] == null) {
                                    Debug.LogWarning("CM: Booth \"" + boothManager.boothName + "\"Content2 != null + Content2[i] == null");
                                    isVerified = false;
                                }
                            }*/
                        }
                    } else {
                        //Debug.LogWarning("CM: Booth \"" + boothManager.boothName + "\"Content2 == null");
                        isVerified = false;
                    }

                }
            }
            boothManager.boothVerified = isVerified;
            if (isVerified) {
                //Debug.LogError("CM: Booth \"" + boothManager.boothName + "\" verified.");
            }
            //Debug.LogWarning("boothVerified: " + txt_boothName.text);
            yield return new WaitForSeconds(3.0f);
        }
    }

    #region Content ASL Requests

    public void GetContent(string boothName, string boothDesc) {
        txt_BoothName.text = boothName;

        //todo move to screen manager, or make function to set
        //txt_ContentName.text = boothName;

        initalDescription = boothDesc;
        txt_BoothDesc.text = initalDescription;
        GameManager.RequestContent(boothName);
    }

    public static void ReceiveContent(float[] _f) {
        if (_f[1] != GameManager.MyID) {
            return;
        }

        //Get offset
        int postUnlockLength = (int)_f[4];
        int boothNameLength = (int)_f[5];

        //Get boothName
        string boothName = "";
        for (int i = 6 + postUnlockLength;
             i <= postUnlockLength + boothNameLength + 5;
             i++) {
            boothName += (char)(int)_f[i];
        }

        //Debug.LogError("ReceiveContent in " + boothName);

        foreach (var contMgr in FindObjectsOfType<ContentManager>()) {
            if (boothName == contMgr.gameObject.GetComponent<BoothManager>().boothName) {
                contMgr.gameObject.GetComponent<ContentManager>().LoadContent(_f);
                return;
            }
        }
    }
    public void LoadContent(float[] _f) {
        //_f[0]: CONT + 1 = Content Header Response
        //_f[1]: playerId already checked
        //See above todo
        switch (_f[2]) {
            //Example Solution
            case 0:
                contentType = ContentType.example;
                //txt_ContentDesc.text = "Example: Problem & Solution";
                break;

            //Media
            case 1:
                contentType = ContentType.media;
                //txt_ContentName.text = "Media";
                break;

            //Discussion
            case 2:
                contentType = ContentType.discussion;
                //txt_ContentName.text = "Discussion";
                break;

            //Content
            case 3:
                contentType = ContentType.content;
                //txt_ContentName.text = "Content";
                break;

            //Example Problem
            case 4:
                //Debug.LogError("Received Example Solution");
                contentType = ContentType.example;
                Content2 = new List<object>();
                break;

            default:
                Debug.LogError("Unknown Content Type!");
                break;
        }

        //Debug.LogError("LoadContent _f[2]:" + _f[2]);

        num_lengthTotalContent = (int)_f[3];

        //Debug.LogError("LoadContent lengthTotalContent _f[3]:" + _f[3]);
        var tempUnlock = "";
        int postUnlockLength = (int)_f[4];
        for (int i = 6;
                 i <= postUnlockLength + 5;
                 i++) {
            tempUnlock += (char)(int)_f[i];
        }

        if (unlockAfterCompletion.Length == 0) unlockAfterCompletion = tempUnlock;
    }

    public static void ReceiveInOrderData(float[] _f) {
        //_f[0] = CONT + 2 = Content In-order Data
        //receive data in-order and convert it back to the data type on this end:
        //can receive text, image so far (TODO: add links type in schema and update here too)

        if (_f[1] != GameManager.MyID) {
            return;
        }

        //get the actual content
        var rawContent = "";
        var rawContentLength = (int)_f[4];

        for (int i = 6;
             i <= rawContentLength + 5;
             i++) {
            rawContent += _f[i];
        }

        //Debug.LogError("ReceiveInorderContent length _f[4]:" + _f[4]);

        var boothName = "";
        var boothNameLength = (int)_f[5];
        for (int i = 6 + rawContentLength;
             i <= rawContentLength + boothNameLength + 5;
             i++) {
            boothName += (char)(int)_f[i];
        }

        //Debug.LogError("ReceiveInorderContent boothName:" + boothName);

        //Find correct booth and forward data
        foreach (var contMgr in FindObjectsOfType<ContentManager>()) {
            if (boothName == contMgr.gameObject.GetComponent<BoothManager>().boothName) {
                contMgr.gameObject.GetComponent<ContentManager>().LoadInOrderContent(_f);
                return;
            }
        }
    }

    public void LoadInOrderContent(float[] _f) {
        StartCoroutine(DelayLoadInOrderContent(_f));
        StartCoroutine(DelayLoadContentIntoScreen());
    }

    IEnumerator DelayLoadInOrderContent(float[] _f) {
        while (num_lengthTotalContent == -1) {
            yield return new WaitForSecondsRealtime(1.0f);
        }

        var rawContent = "";

        var rawContentLength = (int)_f[4];

        for (int i = 6;
             i <= rawContentLength + 5;
             i++) {
            rawContent += (char)(int)_f[i];
        }

        //Debug.LogError("LoadInorderContent raw content length:" + _f[4]);
        //Debug.LogError("LoadInorderContent raw content type:" + _f[3]);

        switch ((int)_f[3]) {
            //generic text
            case 0:
                var newContent = new string(rawContent.ToCharArray());
                AddNewContent(newContent);
                //Debug.Log("Content Added:" + newContent);
                break;
            //image
            case 1:
                var newImage = new image();
                newImage.link = rawContent;
                AddNewContent(newImage);
                //Debug.Log("Content Added [Image]:" + newImage.link);
                break;
            case 2:
                var newLinks = new links();
                newLinks.link = rawContent.Split('|');
                AddNewContent(newLinks);
                //Debug.Log("Content Added [links]: " + string.Join(", ", newLinks.link));
                break;
        }

    }

    /// <summary>
    /// Decides whether to add the content to the original array or the one used for an example's solution
    /// </summary>
    /// <param name="o">Object to add to either Content list</param>
    private void AddNewContent(object o) {
        if (Content2 == null) {
            Content.Add(o);
        } else {
            Content2.Add(o);
        }

        //Andy - Makeshift fix for duplicated slides
        if (Content.Count > num_lengthTotalContent) {
            Content.RemoveRange(num_lengthTotalContent, Content.Count - num_lengthTotalContent);
        }
    }

    #endregion
    private IEnumerator DelayLoadContentIntoScreen() {
        while (boothManager.boothVerified == false) yield return new WaitForSecondsRealtime(0.5f);
        _lookAtScreen.LoadContent(boothManager.boothName, Content, Content2, contentType);
    }

    /// <summary>
    /// Display an error message to the booth
    /// </summary>
    /// <param name="errorMessage">Error message to display</param>
    /// <returns>Waits 5 seconds before returning text to the original</returns>
    private IEnumerator DisplayBoothNotLoadedError(string errorMessage) {
        txt_BoothDesc.text = errorMessage;
        yield return new WaitForSecondsRealtime(5.0f);
        txt_BoothDesc.text = initalDescription;
    }

    private void LinkObjects() {
        foreach (CanvasRenderer obj in GetComponentsInChildren<CanvasRenderer>(true)) {
            switch (obj.gameObject.name) {
                case "pnl_WelcomeScreen":
                    pnl_WelcomeScreen = obj.gameObject;
                    pnl_WelcomeScreen.SetActive(true);
                    break;
                case "txt_BoothName":
                    txt_BoothName = obj.GetComponent<TextMeshProUGUI>();
                    txt_BoothName.gameObject.SetActive(true);
                    break;
                case "txt_BoothDesc":
                    txt_BoothDesc = obj.GetComponent<TextMeshProUGUI>();
                    txt_BoothDesc.gameObject.SetActive(true);
                    break;

                default:
                    break;
            }
        }
    }

}
