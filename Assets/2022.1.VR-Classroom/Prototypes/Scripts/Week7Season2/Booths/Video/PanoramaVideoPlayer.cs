using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.InputSystem;

public class PanoramaVideoPlayer : MonoBehaviour
//Class used specifically for handling the playback of panorama videos
{
    private VideoPlayer videoPlayer;
    private string defaultSkyboxVideo;
    public LayerMask defaultMask;
    public LayerMask panoramaMask;
    public RenderTexture panoramaTexture;

    // Start is called before the first frame update
    void Start()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        defaultSkyboxVideo = videoPlayer.url;
        videoPlayer.SetDirectAudioVolume(0, 0.025f);
    }

    public void ToggleVideo(string videoURL)
    {
        if (!PanoramaIsPlaying()) {
            ShowAndPlayVideo(videoURL);
        }
        else {
            HideAndStopVideo();
        }
    }

    public bool PanoramaIsPlaying()
    {
        return videoPlayer.url != defaultSkyboxVideo;
    }

    void ShowAndPlayVideo(string videoURL)
    //Helper function -- sets the video to be that of the passed-in URL,
    //and displays only the skybox and layers specified in the panoramaMask
    //videoURL = either (relative/absolute) filepath or internet URL
    {
        videoPlayer.SetDirectAudioVolume(0, 0.5f);
        videoPlayer.url = videoURL;
        Camera.main.cullingMask = panoramaMask;
        videoPlayer.Play();
    }

    void HideAndStopVideo()
    //Helper function -- reverts the video to be that of the default skybox video,
    //and displays only the skybox and layers specified in the panoramaMask
    {
        videoPlayer.SetDirectAudioVolume(0, 0.025f);
        videoPlayer.url = defaultSkyboxVideo;
        Camera.main.cullingMask = defaultMask;
        videoPlayer.Play();
    }
}
