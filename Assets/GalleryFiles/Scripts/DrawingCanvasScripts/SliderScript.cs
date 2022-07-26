using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SliderScript : MonoBehaviour
{
    GameObject value;
    Slider thisSlider;
    TextMeshProUGUI valueText;
    InputField input;
    // Start is called before the first frame update
    void Start()
    {
        thisSlider = this.transform.gameObject.GetComponent<Slider>();
        value = this.transform.GetChild(3).gameObject;
        input = value.GetComponent<InputField>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeValue()
    {
        //valueText.text = thisSlider.value.ToString();
    }

    public void ChangeValue(InputField i)
    {
        //i.text = thisSlider.value.ToString();
    }
}
