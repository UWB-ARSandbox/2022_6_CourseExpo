using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlsScreen : MonoBehaviour
{
    public void flipScreen() {
        if (gameObject.activeSelf == false) {
            Cursor.lockState = CursorLockMode.Confined;
            gameObject.SetActive(true);
        } else {
            Cursor.lockState = CursorLockMode.Locked;
            gameObject.SetActive(false);
        }
    }
}
