using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartGalleryUIText : MonoBehaviour
{
    bool started;
    Text content;
    // Start is called before the first frame update
    void Start()
    {
        started = false;
        content = transform.GetChild(0).gameObject.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeGalleryState()
    {
        started = !started;
        ChangeText(started);
    }

    void ChangeText(bool status)
    {
        if(status)
        {
            content.text = "End Gallery";
        }
        else
        {
            content.text = "Start Gallery";
        }
    }
}
