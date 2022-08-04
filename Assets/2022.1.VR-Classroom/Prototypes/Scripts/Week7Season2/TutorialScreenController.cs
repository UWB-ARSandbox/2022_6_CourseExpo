using UnityEngine;

public class TutorialScreenController : MonoBehaviour
{
    TutorialScreen[] tutorialScreens;

    void Start()
    {
        // find all instances of tutorial screens and store them
        tutorialScreens = GameObject.FindObjectsOfType<TutorialScreen>();
    }

    void Update()
    {
        // toggle screen visibility when 'N' is pressed
        if (Input.GetKeyDown(KeyCode.N))
        {
            ToggleScreens();
        }
    }

    void ToggleScreens()
    {
        // go through each screen and set it's visibility to 
        // the opposite of it's current visibility
        foreach (TutorialScreen screen in tutorialScreens)
        {
            screen.transform.gameObject.SetActive(!screen.transform.gameObject.activeSelf);
        }
    }
}
