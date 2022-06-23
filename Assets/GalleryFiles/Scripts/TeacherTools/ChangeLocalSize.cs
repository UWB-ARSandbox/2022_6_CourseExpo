using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class ChangeLocalSize : MonoBehaviour
{
	GameObject[] studentArray;
	static GameLiftManager gameManager;
	// Start is called before the first frame update
	void Start()
	{
		gameManager = GameObject.Find("GameLiftManager").GetComponent<GameLiftManager>();
		studentArray = GameObject.FindGameObjectsWithTag("StudentPlayer");
		if (gameManager.m_PeerId == gameManager.GetLowestPeerId())
		{
			for (int i = 0; i < studentArray.Length; i++)
			{
				GameObject canvas = studentArray[i].transform.GetChild(1).gameObject;
				canvas.transform.localScale = new Vector3(canvas.transform.localScale.x / 2, canvas.transform.localScale.y / 2, canvas.transform.localScale.z / 2);
			}
		}
	}

	// Update is called once per frame
	void Update()
	{
		
	}
	
	public void ScaleStudentCanvas(GameObject canvas, float x, float y, float z)
	{
		canvas.transform.localScale = new Vector3(x, y, z);
	}
}
