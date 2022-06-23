using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class StudentNames : MonoBehaviour
{
    bool allSet = false;

    GameLiftManager manager;
    float[] nameArray;
    
    int mySpot = -1;
    int myID = -1;

    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.Find("GameLiftManager").GetComponent<GameLiftManager>();
        nameArray = new float[manager.m_Players.Count];

        GetComponent<ASL.ASLObject>()._LocallySetFloatCallback(SetName);
    }

    // Update is called once per frame
    void Update()
    {
        // Update the id array if number was not pushed
        if(myID != -1 && mySpot != -1 && myID != nameArray[mySpot])
		{
            SendName(mySpot, myID);
		}
        if(allSet == false)
		{
            GameObject[] textName = GameObject.FindGameObjectsWithTag("StuNames");
            ChangePlayerNames(textName);
        }
    }

    public void RecievedName(int spot, int playerID)
	{
        myID = playerID;
        mySpot = spot;

        SendName(mySpot, myID);
    }

    void SendName(int spot, int playerID)
	{
        gameObject.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
        {
            float[] temp = new float[manager.m_Players.Count];
            temp[spot] = playerID;
            GetComponent<ASLObject>().SendFloatArray(temp);
        });
    }

    void ChangePlayerNames(GameObject[] textMeshes)
    {
        // Check to make sure all player keys are pushed into array.
        for(int i = 0; i < nameArray.Length; i++)
		{
            if(nameArray[i] == 0)
			{
                return;
			}
		}

        for(int i = 0; i < textMeshes.Length; i++)
		{
            textMeshes[i].GetComponent<TextMesh>().text = manager.m_Players[(int)nameArray[i]];
        }
        allSet = true;
    }

    void SetName(string _id, float[] _f)
	{
        // Change order
        for(int i = 0; i < _f.Length; i++)
		{
            if(nameArray[i] == 0)
			{
                nameArray[i] = _f[i];
            }
		}
	}
}
