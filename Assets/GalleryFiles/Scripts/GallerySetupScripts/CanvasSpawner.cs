using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Desc: This handles changing the size of the walls and floor, additionally
handles spawing in the canvases when gallery mode is turned on by the user.
*/
public class CanvasSpawner : MonoBehaviour
{
    static GameObject[] WallArray = new GameObject[4];
    static List<GameObject> allCans = new List<GameObject>();
    int totalStu = 0;

    ResubmissionHandler subHandler;

    bool clicked = false;

    static float point1 = 4;
    static float point2 = 3.5f;
    static float point3 = 5;
    bool firstFrame = true;

	// Start is called before the first frame update
	void Start()
    {
        subHandler = GameObject.Find("Resubmission").GetComponent<ResubmissionHandler>();
        int wallNum = 1;
        for(int i = 0; i < 4; i++)
		{
            WallArray[i] = GameObject.Find("Wall" + wallNum.ToString());
            wallNum += 1;
		}


        //Locally expands the place
        ASL.GameLiftManager manager = GameObject.Find("GameLiftManager").
            GetComponent<ASL.GameLiftManager>();
        
        float amountStu = manager.m_Players.Count;
        //amountStu = 20;

        totalStu = (int)amountStu;
        //amountStu = 6;
        float wallSize;
        if(amountStu <= 3)
        {
            wallSize =  1f;
        }
        else if(amountStu <= 9)
        {
            wallSize =  2f;
        }
        else
        {
            wallSize =  3f;
        }
        
        

        GameObject ground = GameObject.Find("Ground");
        Vector3 size = ground.transform.localScale;
        Vector3 expandSize = new Vector3(wallSize, size.y, wallSize);
        ground.transform.localScale = expandSize;
        for (int i = 0; i < WallArray.Length; i++)
        {
            GameObject wall = WallArray[i];
            Vector3 sizeWall = wall.transform.localScale;
            Vector3 expandSizeWall = new Vector3(10 * wallSize, sizeWall.y, sizeWall.z);
            wall.transform.localScale = expandSizeWall;

            float wallX = 0;
            float wallZ = 0;
            if(wall.transform.position.x != 0)
			{
                if(wall.transform.position.x < 0)
				{
                    wallX = -1;
                }
                else
				{
                    wallX = 1;
                }
			}
            if (wall.transform.position.z != 0)
			{
                if (wall.transform.position.z < 0)
                {
                    wallZ = -1;
                }
                else
                {
                    wallZ = 1;
                }
            }

            Vector3 newPos = new Vector3((5 * wallSize + 0.5f) * wallX,
                wall.transform.position.y, (5 * wallSize + 0.5f) * wallZ);
            wall.transform.position = newPos;

            if(wall.gameObject.name.Contains("Wall1"))
			{
                point1 = expandSizeWall.x / 2 - 1;
                point3 = newPos.z - 0.5f;
            }
        }
        
        
        
    }
    
    // When a teacher clicks start/end gallery this function is called
    // to do the following.
	public void GalleryOpitions()
	{
        if(clicked)
		{
            ClearGallery();
            clicked = false;
            // Makes sure that user can now submit work to gallery
            subHandler.AllCanSubmit(1);
		}
        else
		{
            StartGallery();
            clicked = true;
		}
	}

    // Start the deletion of objects
    public void ClearGallery()
    {
        GameObject[] stuCanvases = GameObject.FindGameObjectsWithTag("StuCanvas");
        // Deletes all student canvases
        for (int i = 0; i < stuCanvases.Length; i++)
        {
            GameObject delCan = stuCanvases[i];
            DeleteCanvas(delCan);
        }
        point1 = 4;
        point2 = 3.5f;
        point3 = 5;
    }

    // Deletes canvases from the gallery
    void DeleteCanvas(GameObject canvas)
    {
        canvas.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
        {
            canvas.GetComponent<ASL.ASLObject>().DeleteObject();
        });
    }

    // Starts the creation of blank canvases
    public void StartGallery()
	{
        ASL.GameLiftManager manager = GameObject.Find("GameLiftManager").
            GetComponent<ASL.GameLiftManager>();

        int amountStu = manager.m_Players.Count;

        totalStu = amountStu;
        int wallSize;
        if(amountStu <= 3)
        {
            wallSize =  1;
        }
        else if(amountStu <= 9)
        {
            wallSize =  2;
        }
        else
        {
            wallSize =  3;
        }
        
        ChangeWallSizePos(wallSize);

        for (int i = 0; i < amountStu; i++)
        {
            SpawnObj();
        }
    }

    // Changes all wall and floor size to the value of the
    // passed in size value.
    public void ChangeWallSizePos(int wallSize)
	{
        
        GameObject ground = GameObject.Find("Ground");
        ground.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
        {
            Vector3 size = ground.transform.localScale;
            Vector3 expandSize = new Vector3(wallSize, size.y, wallSize);
            ground.GetComponent<ASL.ASLObject>().SendAndSetLocalScale(expandSize);
        });

        for (int i = 0; i < WallArray.Length; i++)
        {
            SendWallSize(WallArray[i], wallSize);
        }
	}

    // Changes the wall size and position within the gallery space
    // to fit with the new size of the scaled floor.
    void SendWallSize(GameObject wall, int wallSize)
    {
        wall.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
        {
            Vector3 size = wall.transform.localScale;
            Vector3 expandSize = new Vector3(10 * wallSize, size.y, size.z);
            wall.GetComponent<ASL.ASLObject>().SendAndSetLocalScale(expandSize);

            float wallX = 0;
            float wallZ = 0;
            if(wall.transform.position.x != 0)
			{
                if(wall.transform.position.x < 0)
				{
                    wallX = -1;
                }
                else
				{
                    wallX = 1;
                }
			}
            if (wall.transform.position.z != 0)
			{
                if (wall.transform.position.z < 0)
                {
                    wallZ = -1;
                }
                else
                {
                    wallZ = 1;
                }
            }

            Vector3 newPos = new Vector3((5 * wallSize + 0.5f) * wallX,
                wall.transform.position.y, (5 * wallSize + 0.5f) * wallZ);
            wall.GetComponent<ASL.ASLObject>().SendAndSetWorldPosition(newPos);

            if(wall.gameObject.name.Contains("Wall1"))
			{
                point1 = expandSize.x / 2 - 1;
                point3 = newPos.z - 0.5f;
            }
        });
    }

    // Spawns an object in for all users 
    void SpawnObj()
	{
        ASL.ASLHelper.InstantiateASLObject("StuCanvas",
            new Vector3(0, 2, 5), Quaternion.identity, "", "", RecievedGameObj);
    }

    // The object has been recieved by all users
    public static void RecievedGameObj(GameObject spawnedObject)
    {
        allCans.Insert(0, spawnedObject);
        int i = allCans.Count;
        spawnedObject.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
        {
            // Name is changed at to fit the time it was recieved
            spawnedObject.name = "StuCanvas" + i.ToString();

            // Wall 1
            if (Mathf.CeilToInt(i / WallArray[0].transform.localScale.x) == 1)
            {
                Vector3 newSpot = new Vector3(-point1, point2, point3);
                spawnedObject.GetComponent<ASL.ASLObject>().SendAndSetWorldPosition(newSpot);

                Quaternion rot = Quaternion.Euler(0, 180, 0);
                spawnedObject.GetComponent<ASL.ASLObject>().SendAndSetWorldRotation(rot);

                Advance(spawnedObject);
                ChangeCurCanPos(i);
            }
            // Wall 2
            else if (Mathf.CeilToInt(i / WallArray[0].transform.localScale.x) == 2)
            {
                Vector3 newSpot = new Vector3(point3, point2, point1);
                spawnedObject.GetComponent<ASL.ASLObject>().SendAndSetWorldPosition(newSpot);

                Quaternion rot = Quaternion.Euler(0, 270, 0);
                spawnedObject.GetComponent<ASL.ASLObject>().SendAndSetWorldRotation(rot);

                Advance(spawnedObject);
                ChangeCurCanPos(i);
            }
            // Wall 3
            else if (Mathf.CeilToInt(i / WallArray[0].transform.localScale.x) == 3)
            {
                Vector3 newSpot = new Vector3(point1, point2, -point3);
                spawnedObject.GetComponent<ASL.ASLObject>().SendAndSetWorldPosition(newSpot);

                Quaternion rot = Quaternion.Euler(0, 0, 0);
                spawnedObject.GetComponent<ASL.ASLObject>().SendAndSetWorldRotation(rot);

                Advance(spawnedObject);
                ChangeCurCanPos(i);
            }
            // Wall 4
            else if (Mathf.CeilToInt(i / WallArray[0].transform.localScale.x) == 4)
            {
                Vector3 newSpot = new Vector3(-point3, point2, -point1);
                spawnedObject.GetComponent<ASL.ASLObject>().SendAndSetWorldPosition(newSpot);

                Quaternion rot = Quaternion.Euler(0, 90, 0);
                spawnedObject.GetComponent<ASL.ASLObject>().SendAndSetWorldRotation(rot);

                Advance(spawnedObject);
                ChangeCurCanPos(i);
            }
        });
    }

    // Changes canvas position after spawn to fit
    // into row column postion.
    static void ChangeCurCanPos(int index)
	{
        if (point2 == 3.5)
        {
            point2 = 1.5f;
        }
        // Checks to see if a wall transfer is coming next
        else if (Mathf.CeilToInt(index / WallArray[0].transform.localScale.x) ==
            Mathf.CeilToInt((index + 1) / WallArray[0].transform.localScale.x))
        {
            point1 -= 2f;
            point2 = 3.5f;
        }
        else
		{
            point1 = WallArray[0].transform.localScale.x / 2 - 1;
            point2 = 3.5f;
        }
    }

    // Removes canvas object from the spawn list
    static void Advance(GameObject spawnedObject)
    {
        allCans.Remove(spawnedObject);
    }
}
