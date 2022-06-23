using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VideoFullscreenBehavior : MonoBehaviour
{
    private bool fullscreen = false;
    public Camera fullscreenCamera;
    private Collider screenCollider => GetComponent<VideoPlaybackManager>().playbackToggleColliders[0];
    private Collider timebarCollider => GetComponent<VideoPlaybackManager>().timeBarColliders[0];
    private Collider playPauseCollider => GetComponent<VideoPlaybackManager>().playbackToggleColliders[1];
    private float translationSpeed = 250f;
    public Camera raycastCamera;
    public Transform UIToAdjust;
    public GameObject Crosshair;
    private Vector3 upperTransformPosition;
    private Vector3 lowerTransformPosition;
    // Start is called before the first frame update
    
    void Start() 
    {
        StartCoroutine(DelayedInit());
        Crosshair = GameObject.Find("PC Map Canvas/Crosshair");
        upperTransformPosition = new Vector3(0f, 90f, -0.01f);
        lowerTransformPosition = Vector3.zero;
    } 
    
    IEnumerator DelayedInit() {
        while (raycastCamera == null) {
            raycastCamera = Camera.main;
            yield return new WaitForSeconds(0.1f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current[Key.F].wasPressedThisFrame && !PlayerController.IsTypingInput) {
            ToggleFullscreen();
        }
        if (fullscreen) {
            Cursor.lockState = CursorLockMode.None;
            Ray videoRay = raycastCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit videoRayHit;
            if (Physics.Raycast(videoRay, out videoRayHit)) {
                if (videoRayHit.collider == screenCollider || videoRayHit.collider == timebarCollider || videoRayHit.collider == playPauseCollider) {
                    UIToAdjust.localPosition = Vector3.MoveTowards(UIToAdjust.localPosition, upperTransformPosition, translationSpeed * Time.deltaTime);
                }
                else {
                    UIToAdjust.localPosition = Vector3.MoveTowards(UIToAdjust.localPosition, lowerTransformPosition, translationSpeed * Time.deltaTime);
                }
            }
            else {
                UIToAdjust.localPosition = Vector3.MoveTowards(UIToAdjust.localPosition, lowerTransformPosition, translationSpeed * Time.deltaTime);
            }
        }
    }

    public void ToggleFullscreen()
    {
        if (fullscreen) {
            ExitFullscreen();
        }
        else {
            Ray videoRay = raycastCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit videoRayHit;
            if (Physics.Raycast(videoRay, out videoRayHit) && videoRayHit.collider == screenCollider) {
                EnterFullscreen();
            }
        }
    }

    private void ExitFullscreen()
    {
        Crosshair.SetActive(true);
        raycastCamera = Camera.main;
        fullscreen = false;
        fullscreenCamera.enabled = false;
        Cursor.lockState = CursorLockMode.Locked;
        UIToAdjust.localPosition = lowerTransformPosition;
    }

    private void EnterFullscreen()
    {
        Crosshair.SetActive(false);
        raycastCamera = fullscreenCamera;
        fullscreen = true;
        fullscreenCamera.enabled = true;
        Cursor.lockState = CursorLockMode.Confined;
        UIToAdjust.localPosition = upperTransformPosition;
    }
}
