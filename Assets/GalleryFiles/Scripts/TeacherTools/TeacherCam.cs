using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class TeacherCam : MonoBehaviour
{
    static GameLiftManager manager;
    static int host;

    // Update the teacher canvas every second
    float timer = 1;

    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.Find("GameLiftManager").GetComponent<GameLiftManager>();
        host = manager.GetLowestPeerId();
    }

    // Update is called once per frame
    void Update()
    {
        if(host == manager.m_PeerId)
		{
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                timer = 1;
                // Get the teacher object
                GameObject teachCan = GameObject.Find("Teacher2(Clone)");
                if(teachCan == null)
				{
                    return;
				}
                
                // Get the canvas parented on teacher object
                teachCan = teachCan.transform.GetChild(1).gameObject;
                teachCan.GetComponent<ASLObject>().SendAndSetClaim(() =>
                {
                    Texture2D text = (Texture2D)teachCan.GetComponent<Renderer>().material.mainTexture;
                    teachCan.GetComponent<ASLObject>().SendAndSetTexture2D(text, changeTexture);
                });
            }
        }

        if(Input.mouseScrollDelta.y > 0 && GetComponent<Camera>().fieldOfView > 0)
		{
            GetComponent<Camera>().fieldOfView -= 1f;
        }
        else if(Input.mouseScrollDelta.y < 0 && GetComponent<Camera>().fieldOfView < 60)
		{
            GetComponent<Camera>().fieldOfView += 1f;
        }


        if(Input.GetKey(KeyCode.UpArrow) && transform.position.y < 1.9f)
		{
            transform.position += new Vector3(0,0.05f,0);
		}
        else if(Input.GetKey(KeyCode.DownArrow) && transform.position.y > 0.1f)
		{
            transform.position -= new Vector3(0, 0.05f, 0);
        }

        if(Input.GetKey(KeyCode.LeftArrow) && transform.position.x > -1.4f)
		{
            transform.position -= new Vector3(0.05f, 0, 0);
        }
        else if(Input.GetKey(KeyCode.RightArrow) && transform.position.x < 1.4f)
		{
            transform.position += new Vector3(0.05f, 0, 0);
        }
    }

    public static void changeTexture(GameObject gameObject, Texture2D tex)
	{
        if(host != manager.m_PeerId)
		{
            gameObject.GetComponent<Renderer>().material.mainTexture = tex;
        }
    }
}
