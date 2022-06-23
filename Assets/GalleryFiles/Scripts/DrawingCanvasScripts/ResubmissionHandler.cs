using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

/*
Desc: Handles students status for submitting to gallery.
This makes it so that if a student has submitted to the gallery
they cannot submit another piece of work again. Do not attach
this script onto an object that uses another component
that is locally sending float arrays.
*/
public class ResubmissionHandler : MonoBehaviour
{

	bool clicked = false;
    // Start is called before the first frame update
    void Start()
    {
		GetComponent<ASL.ASLObject>()._LocallySetFloatCallback(ResetSubmission);
	}

	// This changes the status for students submitting to the gallery.
	public void AllCanSubmit(float sub)
	{
		gameObject.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
		{
			float[] f = new float[1];
			f[0] = sub;
			GetComponent<ASLObject>().SendFloatArray(f);
		});
	}

	public void LocallySetClick(bool clk)
	{
		clicked = clk;
	}

	// This gets the current status of the handler locally for the player
	public bool SubmissionStatus()
	{
		return clicked;
	}

	// Local call from ASL to change the status of a student who has submitted to the gallery
	public void ResetSubmission(string _id, float[] _f)
	{
		if (_f[0] != 0)
		{
			clicked = false;
		}
		else
		{
			clicked = true;
		}
	}
}
