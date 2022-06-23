using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StudentPanel : MonoBehaviour
{
    private TMP_Text studentName, help;
    
    [SerializeField]
    private int ID;

    public bool needsHelp;

    
    [SerializeField]
    private GameObject viewButton, preview, player;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(player == null && GameObject.Find("StudentHolder").transform.GetChild(0) != null)
        {
            GameObject holder = GameObject.Find("StudentHolder");
            foreach(Transform child in holder.transform)
            {
                int childID = child.gameObject.GetComponent<StudentEnable>().studentID;
                if(childID == ID)
                {
                    player = child.gameObject;
                    break;
                }
            }
        }

        if(player != null)
        {
            SetPreview((Texture2D)player.transform.GetChild(1).GetChild(1).gameObject.GetComponent<Renderer>().material.mainTexture);
        }
    }

    // Changes name displayed by the student panel. Should only be used at initialization of the UI.
    public void ChangeName(string newName)
    {
        studentName.text = newName;
    }

    public string GetName() { return studentName.text; }

    // Initialize is called to initialize the component, as Start is called in a random order, resulting in a nullreferenceexception.
    public void Initialize(int peerId)
    {
        studentName = this.transform.GetChild(0).GetComponent<TMP_Text>();
        help = this.transform.GetChild(1).GetComponent<TMP_Text>();
        viewButton = this.transform.GetChild(2).gameObject;
        preview = this.transform.GetChild(3).gameObject;

        help.enabled = false;
        viewButton.SetActive(false);
        preview.SetActive(false);

        ID = peerId;
    }

    // Placeholder function to toggle the "request for help" functionality of the student name.
    public void HelpToggle(int peerId)
    {
        if(ID == peerId)
        {
            help.enabled = !help.enabled;
            viewButton.SetActive(help.enabled);
            preview.SetActive(help.enabled);
            Debug.Log("Set: " + ID);
            needsHelp = help.enabled;
        }
    }

    public void SetPreview(Texture2D tex)
    {
        preview.GetComponent<RawImage>().texture = tex;
    }

    public int GetID()
    {
        return ID;
    }

    public GameObject GetPlayer()
    {
        return player;
    }
}
