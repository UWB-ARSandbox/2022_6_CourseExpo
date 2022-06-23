using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoothStatsContainer : MonoBehaviour
{
    public Text BName;
    public Text BTime;
    public Text BScore;
    public Text BTimeTaken;
    public Text BComplete;
    public Text BTimeOut;

    public void SetStats(string name, int time = default, float score = default, float timeTaken = default, bool completed = default, int timedOut = default)
    {

        if(timeTaken <= 0)
        {
            BScore.text = "";
            BTimeTaken.text = "";
            BComplete.text = "";
        }
        else
        {
            BScore.text = score.ToString();
            BTimeTaken.text = timeTaken.ToString();
            BComplete.text = completed.ToString();
        }

        if(timedOut <= 0)
        {
            BTimeOut.text = "";
        }
        else
        {
            BTimeOut.text = timedOut.ToString();
        }

        BName.text = name;
        BTime.text = time.ToString();
    }
}
