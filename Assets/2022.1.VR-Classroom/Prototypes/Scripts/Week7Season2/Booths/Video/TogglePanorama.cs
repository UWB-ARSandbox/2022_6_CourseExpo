using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TogglePanorama : MonoBehaviour, IClickable
{
    public PanoramaVideoPlayer panoramaVideoPlayer;
    public string panoramaURL;
    public GameObject[] objectLayersToChange;
    public GameObject[] objectsToToggleActive;
    private LayerMask[] layerMaskCopy;
    
    void Start()
    {
        layerMaskCopy = new LayerMask[objectLayersToChange.Length];
    }

    public void IClickableClicked() 
    //Called on both PC click and from the XR interactable script on the toggle button
    {
        panoramaVideoPlayer.ToggleVideo(panoramaURL);
        if (panoramaVideoPlayer.PanoramaIsPlaying()) {
            SetPanoramaEnvironment();
        }
        else {
            ClearPanoramaEnvironment();
        }
    }

    private void SetPanoramaEnvironment()
    //Changes and saves the layers of the objects specified in objectLayersToChange so that they're
    //visible during the panorama, and toggles the active state of objects specified in objectsToToggleActive
    //so that they can be interacted with
    {
        for (int i = 0; i < objectLayersToChange.Length; i++) {
            layerMaskCopy[i] = objectLayersToChange[i].layer;
            objectLayersToChange[i].layer = LayerMask.NameToLayer("VisibleDuringPanorama");
        }
        foreach(GameObject go in objectsToToggleActive)
            go.SetActive(true);
    }

    private void ClearPanoramaEnvironment()
    //Reverts the layers of objects to what they previously were, 
    //and sets objects that were set as active to now be inactive
    {
        for (int i = 0; i < objectLayersToChange.Length; i++) {
            objectLayersToChange[i].layer = layerMaskCopy[i];
        }
        foreach(GameObject go in objectsToToggleActive)
            go.SetActive(false);
    }
}