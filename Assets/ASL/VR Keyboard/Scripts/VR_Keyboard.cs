using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VR_Keyboard : MonoBehaviour
{
    public InputField m_InputField;
    public GameObject normalButtons;
    public GameObject capsButtons;
    private bool caps;

    void Start()
    {
        caps = false;
    }

    public void InsertCharacter(string c)
    {
        m_InputField.text += c;
    }

    public void DeleteChar()
    {
        if (m_InputField.text.Length > 0)
        {
            m_InputField.text = m_InputField.text.Substring(0, m_InputField.text.Length - 1);
        }
    }

    public void InsertSpace()
    {
        m_InputField.text += " ";
    }

    public void CapsPressed()
    {
        if (!caps)
        {
            normalButtons.SetActive(false);
            capsButtons.SetActive(true);
            caps = true;
        }
        else
        {
            normalButtons.SetActive(true);
            capsButtons.SetActive(false);
            caps = false;
        }
    }

    public void ToggleKeyboard(bool toggle)
    {
        gameObject.GetComponent<Canvas>().enabled = toggle;
    }

    public void SetInputField(InputField inputField)
    {
        m_InputField = inputField;
    }
}
