using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class VolumeSlider : MonoBehaviour
{
    public AudioMixer backgroundAudioMixer;

    public void SetVolume(float volValue)
    {
        backgroundAudioMixer.SetFloat("BackgroundVolume", Mathf.Log10(volValue) * 20); // Converts '0.001 -> 1' slider value to a value from '-80 -> 0' on a logarithmic scale
    }
}
