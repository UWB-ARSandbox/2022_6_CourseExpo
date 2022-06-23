using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class VR_InputField : MonoBehaviour
{
    InputField inputField;
    VR_Keyboard keyboard;

    // Start is called before the first frame update
    void Start()
    {
        inputField = gameObject.GetComponent<InputField>();
        keyboard = FindObjectOfType<VR_Keyboard>();
    }

    private void Update()
    {
        if (inputField.isFocused)
        {
            keyboard.SetInputField(inputField);
        }
    }
}
