using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IsTypingInputSetter : MonoBehaviour
{
    InputField inputField;

    void Start()
    {
        inputField = GetComponent<InputField>();
    }

    void Update()
    {
        PlayerController.IsTypingInput = inputField.isFocused;
    }
}
