using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class StudentEnable : MonoBehaviour
{
    // Start is called before the first frame update
    public Camera studentCamera;

    Transform[] studentSpawn;

    GameObject PlayerManagerObject;
    SpawnPlayer spawnComponent;
    GameLiftManager manager;

    GameObject StudentHolder;
    public Object MumblePreFab;
    public int studentID;

    
    public string playerManager;
    void Start()
    {
        manager = GameObject.Find("GameLiftManager").GetComponent<GameLiftManager>();

        if (playerManager == null)
        {
            Debug.Log("playerManager object name not set, set it to the name of the object with the spawn player script");
            return;
        } 
        PlayerManagerObject = GameObject.Find("PlayerManager");
        if(PlayerManagerObject == null)
        {
            Debug.Log("Could not find PlayerManager object, make sure the playerManager field is set to name of the object");
            return;
        }
        spawnComponent = PlayerManagerObject.GetComponent<SpawnPlayer>();
        if(PlayerManagerObject == null)
        {
            Debug.Log("PlayerManager is missing the PlayerSpawn script");
            return;
        }
        studentSpawn = spawnComponent.studentSpawn;
        int tempID = manager.m_PeerId;


        for(int i = 0; i < studentSpawn.Length; i++)
        {
            if(transform.position == studentSpawn[i].position)
            {
                studentID = i + 2;
                break;
            }
        }
        if(tempID == manager.GetLowestPeerId())
        {
            //Don't actually return, there's code that executes at the end of it.
            //return;
        }
        else
        {

            if(tempID > studentSpawn.Length + 1)
            {
                Debug.Log("Student attempted to spawn without a valid spawn location");
                return;
            }

            else if(transform.position == studentSpawn[tempID - 2].position)
            {
                Transform studentBody = this.transform.Find("StudentBody");
                studentCamera = Camera.main;
                studentCamera.transform.SetParent(studentBody);
        
                if(studentCamera == null)
                {
                    Debug.Log("No camera attached to student");
                }
                else
                {
                    studentCamera.GetComponent<FirstPersonCamera>().ReinitializeParent(studentBody.gameObject);
                }
                GameObject mumble = (GameObject)Instantiate(MumblePreFab, this.transform.position, Quaternion.identity, gameObject.transform);
                mumble.SetActive(true);
                this.transform.GetComponentInChildren<PlayerFace>().enabled = true;
                this.transform.GetComponentInChildren<PaintOnCanvas>().enabled = true;
                this.transform.GetComponentInChildren<Pavel_Player>().enabled = true;
                //this.gameObject.GetComponent<Pavel_Player>().enabled = true;
            }
        }
        StudentHolder = GameObject.Find("StudentHolder");
        if(StudentHolder == null)
        {
            Debug.Log("Couldn't find student holder");
        }
        this.transform.SetParent(StudentHolder.transform);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
