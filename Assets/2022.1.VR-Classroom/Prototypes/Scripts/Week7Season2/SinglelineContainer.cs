using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SinglelineContainer : MonoBehaviour
{
    public Text Line;
    public void setText(string var)
    {
        Line.text = var;
    }
}
