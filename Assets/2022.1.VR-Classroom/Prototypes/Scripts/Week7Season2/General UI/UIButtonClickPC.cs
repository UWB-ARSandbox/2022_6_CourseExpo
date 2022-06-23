using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIButtonClickPC : MonoBehaviour, IClickable
{
    Button button;
    // Start is called before the first frame update
    void Start()
    {
        button = gameObject.GetComponent<Button>();
    }

    public void IClickableClicked() {
        button.onClick.Invoke();
    }
}
