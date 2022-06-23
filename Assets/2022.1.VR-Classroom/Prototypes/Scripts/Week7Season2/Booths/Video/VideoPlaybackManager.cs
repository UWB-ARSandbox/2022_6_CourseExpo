using System.Collections;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Video;
using UnityEngine.InputSystem;
using ASL;
using TMPro;

public class VideoPlaybackManager : MonoBehaviour
{
    [Header("Volume Range")]
    [Tooltip("The minimum range at which volume will start decreasing")]
    public float minVolumeDecreaseDist = 2f;
    [Tooltip("The maximum range at which volume can be heard")]
    public float maxVolumeDist = 10f;

    //for changing button to be either a play or pause button
    [Header("Material Changing")]
    public Material pauseButton;
    public Material playButton;
    public Material restartButton;
    public Renderer playPauseButton;
    
    //for checking raycast collisions
    [Header("Collider Stuff")]
    public Collider[] timeBarColliders;
    public Collider menuCollider;
    private Transform timeBarStart;
    private Transform timeBarEnd;
    public Collider[] playbackToggleColliders;
    private Ray ray;
    private RaycastHit hit;
    
    //for updating time/UI
    [Header("UI Management")]
    public VideoPlayer videoPlayer;
    public TMP_Text currentTime;
    public TMP_Text totalTime;
    public GameObject UI;
    public VideoSyncManager videoSyncManager;
    public PlayheadMover[] playheadMovers;
    private float currentFrame;
    private Camera raycastCamera => GetRaycastCamera();

    //Getting youtube id
    private const string YoutubeLinkRegex = "(?:.+?)?(?:\\/v\\/|watch\\/|\\?v=|\\&v=|youtu\\.be\\/|\\/v=|^youtu\\.be\\/)([a-zA-Z0-9_-]{11})+";
    private Regex regexExtractId = new Regex(YoutubeLinkRegex, RegexOptions.Compiled);
    private string[] validYoutubeLinks = { "youtube.com", "www.youtube.com", "youtu.be", "www.youtu.be" };

    void Start()
    {
        timeBarStart = playheadMovers[0].startPoint;
        timeBarEnd = playheadMovers[0].endPoint;
        if (gameObject.name == "PanoramaControls") {
            videoPlayer = GameObject.Find("Skybox").GetComponent<VideoPlayer>();
        }
        else {
            if (videoPlayer.url != null) {
                SetURL(videoPlayer.url);
            } else {
                videoPlayer.Prepare();
            }
        }
    }

    Camera GetRaycastCamera()
    //Called when getting the camera for any raycast operations
    //If a VideoFullscreenBehavior exists, it'll inherit the script's camera (camera.main or the prefab's fullscreen camera),
    //otherwise, it'll use camera.main
    {
        var fullscreenScript = GetComponent<VideoFullscreenBehavior>();
        if (fullscreenScript) {
            return fullscreenScript.raycastCamera;
        }
        else {
            return Camera.main;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (videoPlayer.length != 0) { // length remains 0 until video is fully loaded
            SetTotalTimeUI();
        }
        
        if (Mouse.current.leftButton.wasPressedThisFrame && raycastCamera != null) {
            ray = raycastCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out hit)) {
                Func<bool>[] checks = new Func<bool>[] {CheckAndTryPlaybackToggle, CheckAndTryNewVideoPosition, CheckAndTryMenuToggle};
                //Go through and check for collision hits. Breaks once a valid collision was detected, and the action for it was executed.
                foreach(Func<bool> playbackControlCheck in checks) {
                    if (playbackControlCheck()) {
                        break;
                    }
                }
            }
        }

        if (videoPlayer.isPlaying) {
            currentFrame = videoPlayer.frame;
            float fractionOfVideo = CurrentTimeFraction();
            foreach(PlayheadMover playheadMover in playheadMovers) {
                playheadMover.MovePlayhead(fractionOfVideo);
            }
            playPauseButton.material = pauseButton;
            SetCurrentTimeUI();
        }
        else {
            if (GetCurrentMinute() == GetTotalMinutes() && GetCurrentSecond() == GetTotalSeconds()) {
                playPauseButton.material = restartButton;
            }
            else {
                playPauseButton.material = playButton;
            }
        }
        
        if (gameObject.name != "PanoramaControls") {
            SetVolumeBasedOnDistance();
        }
    }

    private bool CheckAndTryMenuToggle() 
    //Only called in Update(), and when the left mouse button is clicked
    //Toggles the menu visibility for panorama video controls if the menu collider was hit
    {
    
        if (menuCollider != null && hit.collider == menuCollider) {
            ToggleDisplayOfControls();
            return true;
        }
        else {
            return false;
        }
    }

    private bool CheckAndTryNewVideoPosition()
    //Only called in Update(), and when the left mouse button is clicked
    //Sets the video time if the proper collider(s) was hit, either async or sync
    {
        foreach(Collider collider in timeBarColliders) {
            if (hit.collider == collider) {
                Transform colliderTransform = hit.collider.transform;
                timeBarStart = colliderTransform.Find("Start");
                timeBarEnd = colliderTransform.Find("End");
                if (videoSyncManager != null) {
                    videoSyncManager.SendNewVideoPosition(GetNewTimeFraction());
                }
                else {
                    SetCurrentTime();
                }
                return true;
            }
        }
        return false;
    }

    private bool CheckAndTryPlaybackToggle()
    //Only called in Update(), and when the left mouse button is clicked
    //Toggles video playback, either async or sync
    {
        foreach(Collider collider in playbackToggleColliders) {
            if (collider == hit.collider) {
                if (videoSyncManager != null) {
                    videoSyncManager.ToggleVideoPlayback();
                }
                else {
                    PlayPauseToggle();
                }
                return true;
            }
        }
        return false;
    }

    public void PlayPauseToggle()
    {
        if (videoPlayer.isPlaying) {
            videoPlayer.Pause();
        }
        else {
            videoPlayer.Play();
        }
    }

    public void ToggleDisplayOfControls()
    //Simply toggles the visibility of (Panorama) video controls
    {
        if (UI.activeSelf) {
            UI.SetActive(false);
        }
        else {
            UI.SetActive(true);
        }
    }

    public void OnVRSelectEnter(SelectEnterEventArgs args)
    //Called from the XR interactable of any video timebars
    //Basically just the XR version of going to a specific spot in video / SetCurrentTime
    {
        Physics.Raycast(args.interactorObject.transform.position, args.interactorObject.transform.forward, out hit);
        if (videoSyncManager != null) {
            videoSyncManager.SendNewVideoPosition(GetNewTimeFraction());
        }
        else {
            SetCurrentTime();
        }
    }

    public void VRPlayPause()
    //Called from the XR interactable of anything used for plaback toggling
    //Basically just the XR version of playback toggling
    {
        if (videoSyncManager != null) {
            videoSyncManager.ToggleVideoPlayback();
        }
        else {
            PlayPauseToggle();
        }
    }

    float GetNewTimeFraction()
    //Used for getting the fraction of video time relative to where on a transform a raycast hit
    {
        float timeBarWidth = Mathf.Abs(timeBarStart.localPosition.z) + Mathf.Abs(timeBarEnd.localPosition.z);
        return timeBarStart.InverseTransformPoint(hit.point).z / timeBarWidth;
    }

    int GetCurrentMinute()
    {
        float currentSecondsIn = (float)currentFrame / (float)videoPlayer.frameRate;
        return (int)Mathf.Floor((int) currentSecondsIn / 60);
    }

    int GetTotalMinutes()
    {
        return (int)Mathf.Floor((int) (videoPlayer.frameCount / videoPlayer.frameRate) / 60);
    }

    int GetCurrentSecond()
    {
        float currentSecondsIn = (float)currentFrame / (float)videoPlayer.frameRate;
        return ((int) currentSecondsIn % 60);
    }

    int GetTotalSeconds()
    {
        return ((int) (videoPlayer.frameCount / videoPlayer.frameRate) % 60);
    }

    public void SetCurrentTime(float fractionOfVideo = 0f)
    //Forces the video to jump to a particular frame as specified by a user
    {
        if (fractionOfVideo == 0)
            fractionOfVideo = GetNewTimeFraction();
        int totalFrames = (int)videoPlayer.frameCount;
        int newFramePosition = (int)(fractionOfVideo * totalFrames);
        videoPlayer.frame = newFramePosition;
        currentFrame = newFramePosition;
        foreach(PlayheadMover playheadMover in playheadMovers) {
            playheadMover.MovePlayhead(fractionOfVideo);
        }
        SetCurrentTimeUI();
    }
    void SetCurrentTimeUI()
    //Sets the UI text indiciating the current time
    {
        string minutes = GetCurrentMinute().ToString("00");
        string seconds = GetCurrentSecond().ToString("00");
        currentTime.text = minutes + ":" + seconds;
    }

    public void SetURL(string newURL) 
    {
        newURL = TryParseYoutubeID(newURL);
        videoPlayer.url = newURL;
        videoPlayer.Prepare();
    }

    private string TryParseYoutubeID(string URL) {
        try {
            string authority = new UriBuilder(URL).Uri.Authority.ToLower();
            if (Array.Exists(validYoutubeLinks, element => element == authority)) {
                var regRes = regexExtractId.Match(URL.ToString());
                if (regRes.Success) {
                    return "https://unity-youtube-dl-server.herokuapp.com/watch?v=" + regRes.Groups[1].Value;
                }
            }
        } catch {return URL;}
        return URL;
    }

    void SetTotalTimeUI()
    //Sets the UI text indiciating the total time
    {
        string minutes = GetTotalMinutes().ToString("00");
        string seconds = GetTotalSeconds().ToString("00");
        totalTime.text = "/  " + minutes + ":" + seconds;
    }

    float CurrentTimeFraction()
    //returns the current time as a fraction of the total time
    {
        return (float)videoPlayer.time / (float)videoPlayer.length;
    }

    void SetVolumeBasedOnDistance()
    {
        if (raycastCamera == null)
            return;
        float distance = Vector3.Distance(raycastCamera.transform.position, transform.position);
        float slope = -1f / (maxVolumeDist - minVolumeDecreaseDist);
        if (distance > minVolumeDecreaseDist && distance < maxVolumeDist) {
            //Distance to the camera is within thresholds to adjust volume between
            float newVolume = slope * (distance - 2) + 1;
            videoPlayer.SetDirectAudioVolume(0, newVolume);
        }
        else {
            if (distance < minVolumeDecreaseDist) //set to max/default volume when under minVolumeDecreaseDist
                videoPlayer.SetDirectAudioVolume(0, 1);
            if (distance > maxVolumeDist) //set to mute/0 volume when past maxVolumeDist
                videoPlayer.SetDirectAudioVolume(0, 0);
        }
    }
}