using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class TutorialScreen : MonoBehaviour, IClickable
{
    // Start is called before the first frame update
    public Image img_Container;

    public Button btn_Next;
    public Button btn_Previous;
    public Button btn_Exit;

    int _index = 0;
    bool _screenAttached = false;

    [SerializeField]
    public Sprite[] ImagesContainer = new Sprite[4];

    [Range(0.0f, 5.0f)]
    public float _lengthFromFace = 2.25f;

    [Range(0.5f, 50f)]
    public float _distanceFromGround = 8f;
    private GameObject _firstPersonPlayer = null;
    private Vector3 startPos;
    void Start()
    {
        startPos = transform.position;
        Debug.Assert(ImagesContainer != null);
        UpdateUI();
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
                    Camera.main.transform.forward * 1.75f;
            }
        }
        else
        {
            transform.position = transform.position;
        }
        //do the screen player look-at tracking
        if (Camera.main != null)
        {
            //find the look direction from the position of the screen to the player
            var templookDir = -(Camera.main.transform.position - transform.position).normalized;
            //we do not want the screen to care about up and down rotation
            if (!_screenAttached) templookDir.y = 0;
            transform.forward = templookDir;

        }

        GetComponent<Collider>().isTrigger = _screenAttached;
    }

    private void UpdateUI()
    {
        if (_index == ImagesContainer.Length) _index = 0;
        if (_index < 0) _index = ImagesContainer.Length - 1;
        btn_Next.gameObject.SetActive(_index != ImagesContainer.Length - 1);
        btn_Previous.gameObject.SetActive(_index != 0);

        var tempWidth = ImagesContainer[_index].textureRect.width;
        var tempHeight = ImagesContainer[_index].textureRect.height;

        var tempRatio = tempHeight / tempWidth;
        if (tempRatio > 1)
        {
            tempRatio = 1 / tempRatio;
            tempHeight = tempRatio;
            tempWidth = 1.0f;
        }
        else
        {
            tempWidth = tempRatio;
            tempHeight = 1.0f;
        }


        img_Container.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, tempWidth * 6000f);
        img_Container.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, tempHeight * 6000f);

        img_Container.sprite = ImagesContainer[_index];

    }

    public void NextScreen()
    {
        _index++;
        UpdateUI();
    }

    public void PreviousScreen()
    {
        _index--;
        UpdateUI();
    }

    public void ViewScreen()
    {
        if (!XRSettings.isDeviceActive) Cursor.lockState = CursorLockMode.None;
        _screenAttached = true;
    }

    public void DetachScreen()
    {
        _screenAttached = false;
        if (!XRSettings.isDeviceActive) Cursor.lockState = CursorLockMode.Locked;
    }

    public void IClickableClicked()
    {
        ViewScreen();
    }
}
