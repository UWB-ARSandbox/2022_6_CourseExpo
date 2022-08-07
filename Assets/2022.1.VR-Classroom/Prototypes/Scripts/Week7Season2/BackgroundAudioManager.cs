using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Manager script for "background audio" within the expo. Should be expandable to include more audio tracks.
// Future Devleopers: Check out the BackgroundAudio prefab under the GameManager in the scene. The current two
// audio tracks "OceanWaves" and "SteadyEnjoyment" can be consulted as examples for how to add more.
public class BackgroundAudioManager : MonoBehaviour
{
    public List<AudioSource> audioClips; // configure in the Unity editor. clips should route to the same AudioMixer as the volume slider

    private int currentClip = -1; // Initial value will be -1 so the first toggle will start at index 0

    private Button switchButton;

    void Start()
    {
        switchButton = GetComponent<Button>();
        Debug.Assert(switchButton != null);
        switchButton.onClick.AddListener(SwitchAudio);

        Debug.Assert(audioClips.Count > 0); // Need at least one audio clip
    }

    public void SwitchAudio()
    {
        AudioSource backgroundAudio;
        if (currentClip != -1)
        {
            backgroundAudio = audioClips[currentClip];
            backgroundAudio.Stop();
        }
        
        currentClip++;  // update to the next index in the list
        if (currentClip == audioClips.Count)
            currentClip = 0; // wrap back to the beginning

        backgroundAudio = audioClips[currentClip];
        backgroundAudio.Play();
    }
}
