using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GalleryCanvasVariables : MonoBehaviour
{
	int votes = 0;
	bool voted = false;
	string studentName = "Bob";
	ASL.GameLiftManager manager;

	Button voteButton;

	// Start is called before the first frame update
	void Start()
	{
		manager = GameObject.Find("GameLiftManager").GetComponent<ASL.GameLiftManager>();
		int host = manager.GetLowestPeerId();

		// Setup the change vote status on a button push
		voteButton = transform.GetChild(0).GetChild(0).GetComponent<Button>();
		voteButton.onClick.AddListener(ChangeVoteStatus);

		GetComponent<ASL.ASLObject>()._LocallySetFloatCallback(retrieveVote);
	}

	public void ChangeVoteStatus()
	{
		voted = true;
		float[] tempVote = new float[1];

		// Decides what color to turn the text on last input
		if(voteButton.transform.GetChild(0).GetComponent<Text>().color == Color.black)
		{
			voteButton.transform.GetChild(0).GetComponent<Text>().color = new Vector4(0, 0.5f, 0, 1);
			tempVote[0] = 1;
		}
		else
		{
			voteButton.transform.GetChild(0).GetComponent<Text>().color = Color.black;
			tempVote[0] = -1;
		}

		// Updates vote locally for each user
		GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
		{
			GetComponent<ASL.ASLObject>().SendFloatArray(tempVote);
		});
	}

	public void ChangeName(string name)
	{
		// One is addded onto the end of array so that vote is not triggered
		// for players who have one char names
		float[] fName = new float[name.Length + 1];
		for(int i = 0; i < name.Length; i++)
		{
			fName[i] = (float)name[i];
		}

		GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
		{
			GetComponent<ASL.ASLObject>().SendFloatArray(fName);
		});
	}

	public void retrieveVote(string id, float[] vote)
	{
		// Votes will always be a length of one
		if(vote.Length == 1)
		{
			votes += (int)vote[0];
			voteButton.transform.GetChild(0).GetComponent<Text>().text =
				"Votes: " + votes;
		}
		// This is for changing the name under the canvas
		else
		{
			// Null the placeholder name
			studentName = "";
			for(int i = 0; i < vote.Length - 1; i++)
			{
				studentName += (char)vote[i];
			}
			// Only teacher can see the names
			if(manager.AmLowestPeer())
			{
				transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = studentName;
				transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().color = Color.white;
			}
		}
	}
	public int GetVotes()
	{
		return votes;
	}
}
