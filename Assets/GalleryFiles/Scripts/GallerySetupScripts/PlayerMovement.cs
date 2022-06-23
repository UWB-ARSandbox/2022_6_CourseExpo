using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    float speed = 0.1f;
    bool stop = false;

    Camera mCamera = null;

    Vector3 lastMousePos = new Vector3(0,0,0);

    // Start is called before the first frame update
    void Start()
    {
        mCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            //Just set position
            Vector3 moveDir = this.transform.forward * speed;
            GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                Vector3 newSpot = moveDir + this.transform.position;
                GetComponent<ASL.ASLObject>().SendAndSetLocalPosition(newSpot);
            });
        }
        else if (Input.GetKey(KeyCode.S))
        {
            //Just set position
            Vector3 moveDir = this.transform.forward * -speed;
            GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                Vector3 newSpot = moveDir + this.transform.position;
                GetComponent<ASL.ASLObject>().SendAndSetLocalPosition(newSpot);
            });
        }

        else if (Input.GetKey(KeyCode.A))
        {
            //Just set position
            Vector3 moveDir = this.transform.right * -speed;
            GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                Vector3 newSpot = moveDir + this.transform.position;
                GetComponent<ASL.ASLObject>().SendAndSetLocalPosition(newSpot);
            });
        }
        else if (Input.GetKey(KeyCode.D))
        {
            //Just set position
            Vector3 moveDir = this.transform.right * speed;
            GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                Vector3 newSpot = moveDir + this.transform.position;
                GetComponent<ASL.ASLObject>().SendAndSetLocalPosition(newSpot);
            });
        }

        // Right Mouse to move camera
        if(Input.GetKey(KeyCode.Mouse1))
		{
            if (Input.mousePosition.x < lastMousePos.x)
			{
                //mCamera.transform.eulerAngles -= new Vector3(0, 2, 0);
                transform.eulerAngles -= new Vector3(0, 2, 0);
            }
            else if(Input.mousePosition.x > lastMousePos.x)
			{
                //mCamera.transform.eulerAngles += new Vector3(0, 2, 0);
                transform.eulerAngles += new Vector3(0, 2, 0);
            }

            if (Input.mousePosition.y < lastMousePos.y)
            {
                mCamera.transform.eulerAngles += new Vector3(2, 0, 0);
            }
            else if (Input.mousePosition.y > lastMousePos.y)
            {
                mCamera.transform.eulerAngles -= new Vector3(2, 0, 0);
            }
        }
        lastMousePos = Input.mousePosition;
    }

	private void OnCollisionEnter(Collision collision)
	{
        if (collision.gameObject.name.Contains("Wall"))
        {
            stop = true;
        }
    }

	private void OnCollisionExit(Collision collision)
	{
        if(collision.gameObject.name.Contains("Wall"))
		{
            stop = false;
        }   
	}
}
