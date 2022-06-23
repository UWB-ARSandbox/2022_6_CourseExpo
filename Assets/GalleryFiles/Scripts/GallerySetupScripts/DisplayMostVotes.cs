using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DisplayMostVotes : MonoBehaviour
{
	Button getMostVotes;
	// Start is called before the first frame update
	void Start()
	{
		getMostVotes = GameObject.Find("MostVoteButton").GetComponent<Button>();
		getMostVotes.onClick.AddListener(GetMostVotes);
	}

	// Update is called once per frame
	void Update()
	{
		
	}

	public void GetMostVotes()
	{
		GameObject[] canvasArray = GameObject.FindGameObjectsWithTag("StuCanvas");
		GameObject selectedCanvas = null;
		int mostVotes = 0;
		for(int i = 0; i < canvasArray.Length; i++)
		{
			if(selectedCanvas == null && canvasArray[i] != null)
			{
				selectedCanvas = canvasArray[i];
				mostVotes = canvasArray[i].GetComponent<GalleryCanvasVariables>().GetVotes();
			}
			else if(canvasArray[i] != null && canvasArray[i].GetComponent<GalleryCanvasVariables>().GetVotes() > mostVotes)
			{
				selectedCanvas = canvasArray[i];
				mostVotes = canvasArray[i].GetComponent<GalleryCanvasVariables>().GetVotes();
			}
		}
		if(selectedCanvas != null)
		{
			Texture2D text = (Texture2D)selectedCanvas.GetComponent<Renderer>().material.mainTexture;
			GameObject teachCanvas = GameObject.Find("Teacher2(Clone)").transform.GetChild(1).gameObject;
			teachCanvas.GetComponent<Renderer>().material.mainTexture = text;
			Texture2D teachText = (Texture2D)teachCanvas.GetComponent<Renderer>().material.mainTexture;
			teachText.Apply();
		}
	}
}
