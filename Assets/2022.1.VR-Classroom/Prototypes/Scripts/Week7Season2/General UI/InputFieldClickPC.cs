using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputFieldClickPC : MonoBehaviour, IClickable {
    InputField txtField;
    // Start is called before the first frame update
    void Start() {
        txtField = gameObject.GetComponent<InputField>();
    }

    public void IClickableClicked() {
        txtField.Select();
    }
}
