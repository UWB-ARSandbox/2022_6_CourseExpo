using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialScreenController : MonoBehaviour
{

    TutorialScreen[] tutorialScreens;

    void Start()
    {
        tutorialScreens = GameObject.FindObjectsOfType<TutorialScreen>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            ToggleScreens();
        }
    }

    void ToggleScreens()
    {
        foreach (TutorialScreen screen in tutorialScreens)
        {
            screen.transform.gameObject.SetActive(!screen.transform.gameObject.activeSelf);
        }
    }
}
