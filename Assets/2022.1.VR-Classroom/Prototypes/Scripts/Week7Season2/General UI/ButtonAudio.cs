using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

// Attach this script to buttons you want to play the 'clicking' audio
public class ButtonAudio : MonoBehaviour
{
    private Button thisButton;
    private AudioSource gm_Source;
    void Start()
    {
        thisButton = GetComponent<Button>();
        Debug.Assert(thisButton != null);
        gm_Source = FindObjectOfType<GameManager>().GetComponent<AudioSource>(); // Game Manager contains the audio for UI clicking
        Debug.Assert(gm_Source != null);

        thisButton.onClick.AddListener(PlayAudio);
    }

    void PlayAudio()
    {
        gm_Source.Play();
    }
}
