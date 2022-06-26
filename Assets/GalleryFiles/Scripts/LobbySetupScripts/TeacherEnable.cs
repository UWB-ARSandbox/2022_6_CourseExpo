using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class TeacherEnable : MonoBehaviour
{
    // Start is called before the first frame update
    // Start is called before the first frame update
    public Camera teacherCamera;

    Transform teacherSpawn;

    GameObject PlayerManagerObject;
    SpawnPlayer spawnComponent;
    GameLiftManager manager;
    public GameObject MumblePreFab;
    
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

        teacherSpawn = spawnComponent.teacherSpawn;
        int tempID = manager.m_PeerId;

        // Check if this is host.
        if(tempID == manager.GetLowestPeerId())
        {
            Transform teacherBody = this.transform.Find("TeacherBody");
            teacherCamera = Camera.main;
            teacherCamera.transform.SetParent(teacherBody);
    
            if(teacherCamera == null)
            {
                Debug.Log("No camera attached to teacher");
            }
            else
            {
                teacherCamera.GetComponent<FirstPersonCamera>().ReinitializeParent(teacherBody.gameObject);
            }
            
            GameObject mumble = (GameObject)Instantiate(MumblePreFab, this.transform.position, Quaternion.identity, gameObject.transform);
            mumble.SetActive(true);
            this.transform.GetComponentInChildren<Pavel_Player>().enabled = true;
            this.transform.GetComponentInChildren<PaintOnCanvas>().enabled = true;
            this.transform.GetComponentInChildren<PlayerFace>().enabled = true;
            //this.gameObject.GetComponent<Pavel_Player>().enabled = true;
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
