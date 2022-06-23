using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class SpawnPlayer : MonoBehaviour
{
    // Start is called before the first frame update
    static public int playerID;

    public string teacherPrefab = "Teacher";

    public string studentPrefab = "Students";

    public Transform teacherSpawn;
    public Transform[] studentSpawn;

    static GameLiftManager manager;
    static StudentNames namer;

    void Start()
    {
        namer = GameObject.Find("StudentNamer").GetComponent<StudentNames>();
        playerID = ASL.GameLiftManager.GetInstance().m_PeerId;
        manager = GameObject.Find("GameLiftManager").GetComponent<GameLiftManager>();

        if (playerID == 1)
        {
            ASLHelper.InstantiateASLObject(teacherPrefab, teacherSpawn.position, teacherSpawn.rotation,
                "", "", GameObjRecevied);
        }
        else
        {
            ASLHelper.InstantiateASLObject(studentPrefab, 
                studentSpawn[playerID - 2].position, studentSpawn[playerID - 2].rotation, "", "", 
                GameObjRecevied);
        }
    }

    static void GameObjRecevied(GameObject gameObj)
	{
        // Gets the current order that this player was spawned.
        GameObject[] textName = GameObject.FindGameObjectsWithTag("StuNames");
        int arraySpot = textName.Length - 1;

        namer.RecievedName(arraySpot, playerID);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
