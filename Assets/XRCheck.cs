using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.InputSystem.XR;

public class XRCheck : MonoBehaviour
{
    private bool isXRActive => XRSettings.isDeviceActive;
    private LineRenderer[] _lineRenderers;

    //TrackedPoseDriver for VR
    private TrackedPoseDriver _tpd;

    public GameObject ui_LobbyManager;
    private const float UIVRScale = 0.002f;
    private const float UIPCScale = 0.333f;

    public GameObject ui_Keyboard;

    private GameObject XROrigin;
    private Vector3 originPos;


    private void Awake() {
        _lineRenderers = FindObjectsOfType<LineRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //Find Origin and store world space position
        XROrigin = FindObjectOfType<XROrigin>().gameObject;
        originPos = XROrigin.transform.position;

        //Set VR Keyboard position
        ui_Keyboard.transform.position = new Vector3(originPos.x, originPos.y + .035f, originPos.z + 2);

        //TrackedPoseDriver for VR
        _tpd = Camera.main.GetComponent<TrackedPoseDriver>();

        //Toggle linerenderers and TrackedPoseDrivers
        UpdateXR();

        //Continuously check for VR headset to switch map canvas
        StartCoroutine(VRCheck());
    }

    /// <summary>
    /// Updates the XR elements on the player
    /// </summary>
    private void UpdateXR() {
        foreach (LineRenderer lr in _lineRenderers) {
            lr.gameObject.SetActive(isXRActive);
        }
        _tpd.enabled = isXRActive;
    }

    /// <summary>
    /// Switches between PC and VR versions of LobbyManager UI
    /// </summary>
    IEnumerator VRCheck() {
        while (true) {
            if (isXRActive) {
                ui_LobbyManager.transform.position = new Vector3(originPos.x, originPos.y + 1, originPos.z + 3);
                ui_LobbyManager.transform.localScale = new Vector3(UIVRScale, UIVRScale, UIVRScale);
                ui_LobbyManager.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
            } else {
                ui_LobbyManager.transform.position = Vector3.zero;
                ui_LobbyManager.transform.localScale = new Vector3(UIPCScale, UIPCScale, UIPCScale);
                ui_LobbyManager.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            }
            UpdateXR();
            yield return new WaitForSeconds(1.0f);
        }
    }
}
