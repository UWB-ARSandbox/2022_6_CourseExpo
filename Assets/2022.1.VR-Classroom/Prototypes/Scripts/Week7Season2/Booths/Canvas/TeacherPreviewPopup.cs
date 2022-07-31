using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class TeacherPreviewPopup : MonoBehaviour, IClickable
{
    // Start is called before the first frame update
    public Image img_Container;

    public CanvasMirrorer teacherCanvas;

    int _index = 0;
    bool _screenAttached = false;

    [Range(0.0f, 5.0f)]
    public float _lengthFromFace = 0.25f;

    private GameObject _firstPersonPlayer = null;
    private Vector3 initLocalPos;
    private Quaternion initLocalRot;
    private Vector3 initLocalScale;
    private Transform previousParent;
    void Start()
    {
        initLocalPos = transform.localPosition;
        initLocalRot = transform.localRotation;
        initLocalScale = transform.localScale;
        previousParent = transform.parent;
        //UpdateUI();
    }

    void FixedUpdate()
    {
        if (_firstPersonPlayer == null) _firstPersonPlayer = GameObject.Find("FirstPersonPlayer(Clone)");
        //do the screen attaching
        if (_screenAttached)
        {
            if (XRSettings.isDeviceActive)
            {
                transform.position =
                    _firstPersonPlayer.transform.position + Vector3.up * 0.9f +
                    _firstPersonPlayer.transform.forward * _lengthFromFace;
            }
            else
            {
                transform.position =
                    Camera.main.transform.position +
                    Camera.main.transform.forward * 0.5f;
            }

            if (Camera.main != null)
            {

                //find the look direction from the position of the screen to the player
                var templookDir = -(Camera.main.transform.position - transform.position).normalized;
                transform.forward = templookDir;

            }

        }

    }

    public void ViewWelcomeScreen()
    {
        if (!XRSettings.isDeviceActive) Cursor.lockState = CursorLockMode.None;
        _screenAttached = true;
        transform.SetParent(Camera.main.transform);
        transform.localRotation = Quaternion.identity;
        transform.localScale = new Vector3(0.9f, 0.6f, 1f);
        FindObjectOfType<PlayerMovementController>().enabled = false;
    }

    public void DetachWelcomeScreen()
    {
        _screenAttached = false;
        FindObjectOfType<PlayerMovementController>().enabled = true;
        transform.SetParent(previousParent);
        transform.localScale = initLocalScale;
        transform.localRotation = initLocalRot;
        transform.localPosition = initLocalPos;
        if (!XRSettings.isDeviceActive) Cursor.lockState = CursorLockMode.Locked;
    }

    public void IClickableClicked()
    {
        if (!_screenAttached)
            ViewWelcomeScreen();
        else
            DetachWelcomeScreen();
    }
}
