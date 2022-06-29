using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorChanger : MonoBehaviour
{
    public bool basedOnImgColor = false; // Overrides the colorOfChoice, uses the color associated with the imgOfChoice's color field
    public Image imgOfChoice;

    public Color colorOfChoice; // Set this in the Unity Inspector, and make sure basedOnImgColor = false for it to be used

    Button m_Button;
    
    void Start()
    {
        m_Button = GetComponent<Button>();
        m_Button.onClick.AddListener(SetUserColor);
    }

    public void SetUserColor()
    {
        // Set the local user's model color, for all players
        if (basedOnImgColor)
        {
            // The color of the image associated with this button (used with the color picker and the Selected Color)
            FindObjectOfType<XpoPlayer>().ColorUser(imgOfChoice.color);
            //Debug.Log("Selected Color: "+ imgOfChoice.color.ToString());
        }
        else
        {
            // The color directly associated with this button
            FindObjectOfType<XpoPlayer>().ColorUser(colorOfChoice);
        }
        
    }
}
