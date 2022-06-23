using System;
using UnityEngine;

/// <summary>Touch controller for the Filter Sphere</summary>
public class FilterSphereTouchController : MonoBehaviour
{
    /// <summary>The root object tree representing PC player specific objects and components including the Camera</summary>
    public Camera ARCamera;

    /// <summary>The pinch zoom sensitivity factor</summary>
    private const float _ZOOM_SCALE_FACTOR = .01f;

    /// <summary>The xyz touch movement sensitivity factor</summary>
    private const float _TRANSLATE_TOUCH_SCALE_FACTOR = .01f;

    /// <summary>The previous sphere diameter state</summary>
    private float _pinchPreviousDiameter = 0f;

    /// <summary>The Unity object instantiation function.  Sets up the ARCamera.  Requires being ran on a mobile device.</summary>
    void Start()
    {
        if (ARCamera == null && Application.isMobilePlatform)
        {
            // Get AR Camera
            ARCamera = GameObject.FindObjectOfType<Camera>();
        }
        if (Application.isMobilePlatform)
        {
            // Halt in event where no touch controls are possible
            throw new InvalidOperationException("FilterSphereTouchController needs to be used within a Mobile Device.");
        }
    }

    /// <summary>The Unity object update function.  Processes touch inputs.</summary>
    void Update()
    {
        processDeviceTouch();
    }

    /// <summary>Helper method to process touch input types.  
    /// Supports 1 finger xy translation, 2 finger pinch zoom, and 3 finger z translation.</summary>
    private void processDeviceTouch()
    {
        if (Input.touchCount > 0)
        {
            switch (Input.touchCount)
            {
                // XY Position
                case 1:
                    processSingleTouch();
                    break;
                // Pinch Zoom
                case 2:
                    processPinchZoomScale();
                    break;
                // Z Position
                case 3:
                    process3FingerZTouch();
                    break;
                // Gorilla fist?
                default:
                    Debug.Log("Unexpected Touch Count: " + Input.touchCount);
                    break;
            }
        }
    }

    /// <summary>Helper method to process single touch input.  
    /// Translates filter sphere in relative xy planes.</summary>
    private void processSingleTouch()
    {
        if (Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            Vector2 deltaPos = Input.GetTouch(0).deltaPosition;
            Vector3 deltaPos3 = new Vector3(deltaPos.x, deltaPos.y, 0);
            transform.Translate(deltaPos3 * Time.deltaTime * _TRANSLATE_TOUCH_SCALE_FACTOR, ARCamera.transform);
        }
    }

    /// <summary>Helper method to process pinch zoom input.  
    /// Scales filter sphere by pinch diameter changes.</summary>
    private void processPinchZoomScale()
    {
        if (Input.GetTouch(0).phase == TouchPhase.Began || Input.GetTouch(1).phase == TouchPhase.Began)
        {
            _pinchPreviousDiameter = getPinchDiameter();
        }
        if (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(1).phase == TouchPhase.Moved)
        {
            float pinchCurrentDiameter = getPinchDiameter();
            float delta = pinchCurrentDiameter - _pinchPreviousDiameter;
            _pinchPreviousDiameter = pinchCurrentDiameter;
            transform.localScale += getScaleVector(delta);
        }
        if (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(1).phase == TouchPhase.Ended)
        {
            _pinchPreviousDiameter = 0f;
        }
    }

    /// <summary>Helper method to process three finger input.  
    /// Translates filter sphere in the relative z plane.</summary>
    private void process3FingerZTouch()
    {
        // Use middle finger delta as position key
        if (Input.GetTouch(1).phase == TouchPhase.Moved)
        {
            Vector2 deltaPos = Input.GetTouch(1).deltaPosition;
            Vector3 deltaPos3 = new Vector3(0, 0, deltaPos.y);
            transform.Translate(deltaPos3 * Time.deltaTime * _TRANSLATE_TOUCH_SCALE_FACTOR, ARCamera.transform);
        }
    }

    /// <summary>Helper method to get 2 finger pinch diameter.  
    private float getPinchDiameter()
    {
        return Vector2.Distance(Input.GetTouch(1).position, Input.GetTouch(0).position);
    }

    /// <summary>Helper method to get the new zoom'ed vector3 for the filter sphere.  
    private Vector3 getScaleVector(float scaleChange)
    {
        return new Vector3(_ZOOM_SCALE_FACTOR * Time.deltaTime * scaleChange,
                           _ZOOM_SCALE_FACTOR * Time.deltaTime * scaleChange,
                           _ZOOM_SCALE_FACTOR * Time.deltaTime * scaleChange);
    }
}
