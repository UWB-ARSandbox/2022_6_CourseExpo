using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerText : MonoBehaviour
{
    public Text Timer;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(Timer != null);
    }

    // Update is called once per frame
    void Update()
    {
        Timer.text = Time.time.ToString();
    }
}
