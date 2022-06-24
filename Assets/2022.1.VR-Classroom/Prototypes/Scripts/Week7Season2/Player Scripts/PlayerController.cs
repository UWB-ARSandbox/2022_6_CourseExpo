using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.XR;

public class PlayerController : MonoBehaviour {

    MapToggle mapTogglePC;
    MapToggle mapToggleVR;
    MapNavigation mapNavigation;

    private PlayerInputActions playerInputActions;

    //TrackedPoseDriver for VR
    private TrackedPoseDriver _tpd;

    private InputAction grab;
    private InputAction switchmap;
    private InputAction click;
    private InputAction rotateGrabbed;
    private InputAction toggleCursorLock;
    private InputAction fileOpen;
    private InputAction sendAnnouncement;
    private InputAction openChatDialog;
    private InputAction openStatsScreen;
    private InputAction togglePlayerVisibility;
    private InputAction moveMapCamera;
    private InputAction quit;

    public GrabbableObject selectedObject = null;
    public static bool IsTypingInput = false;
    private PlayerStatScreen statsScreen = null;
    private MenuScreen PCmenuScreen = null;
    private MenuScreen VRmenuScreen = null;

    public UnityEngine.Object MumblePreFab;

    //MouseLook replacement:
    //New input movement handling:
    private LineRenderer[] _lineRenderers;

    public bool HasObject => selectedObject != null;

    /// <summary>
    /// Checks if there is an XR device plugged in
    /// </summary>
    private bool isXRActive => XRSettings.isDeviceActive;

    /// <summary>
    /// Checks if the cursor is locked
    /// </summary>
    private bool isCursorLocked => Cursor.lockState != CursorLockMode.None;

    private void Awake() {
        playerInputActions = new PlayerInputActions();
        _lineRenderers = FindObjectsOfType<LineRenderer>();
    }

    private void Start()
    {
        _tpd = Camera.main.GetComponent<TrackedPoseDriver>();

        Cursor.lockState = CursorLockMode.Locked;

        UpdateXR();

        //statsScreen = GameObject.Find("Stats Container").GetComponent<PlayerStatScreen>();
        PCmenuScreen = GameObject.Find("PC Menu").GetComponent<MenuScreen>();
        PCmenuScreen.flipScreen();
        VRmenuScreen = GameObject.Find("VR Menu").GetComponent<MenuScreen>();
        VRmenuScreen.flipScreen();

        foreach (MapToggle mt in FindObjectsOfType<MapToggle>(true)) {
            if (mt.gameObject.name.Equals("PC Map Canvas")) {
                mapTogglePC = mt;
            } else {
                mapToggleVR = mt;
            }
        }

        //Name orientations + camera movement
        StartCoroutine(GetMapNavigation());
        //Attach VRCanvas as child to Player
        StartCoroutine(AttachVRCanvas());
        //Continuously check for VR headset to switch map canvas
        StartCoroutine(VRCheck());
    }

    IEnumerator GetMapNavigation() {
        while (mapNavigation == null) {
            yield return new WaitForSeconds(0.2f);
            mapNavigation = FindObjectOfType<MapNavigation>(true);
        }
    }

    void Update() {
        if (mapNavigation != null && mapNavigation.gameObject.activeSelf) {
            Vector2 input = moveMapCamera.ReadValue<Vector2>();
            mapNavigation.PanCamera(input);
        }
    }

    /// <summary>
    /// Updates the XR elements on the player
    /// </summary>
    private void UpdateXR()
    {
        foreach (LineRenderer lr in _lineRenderers)
        {
            lr.gameObject.SetActive(isXRActive);
        }
        _tpd.enabled = isXRActive;
    }

    IEnumerator AttachVRCanvas() {
        while (mapToggleVR == null) {
            yield return new WaitForSeconds(0.1f);
        }
        //if (gameObject.GetComponent<UserObject>().ownerID == ASL.GameLiftManager.GetInstance().m_PeerId) {
            mapToggleVR.transform.SetParent(Camera.main.transform, false);
        //}
    }

    IEnumerator VRCheck() {
        while (true) {
            if (isXRActive) {
                mapTogglePC.gameObject.SetActive(false);
                mapToggleVR.gameObject.SetActive(true);
            } else {
                mapTogglePC.gameObject.SetActive(true);
                mapToggleVR.gameObject.SetActive(false);
            }
            UpdateXR();
            yield return new WaitForSeconds(1.0f);
        }
    }

    private void OnEnable() {

        gameObject.GetComponent<PlayerMovementController>().enabled = true;
        gameObject.GetComponent<PlayerRotationController>().enabled = true;

        //interaction
        grab = playerInputActions.Player.Grab;
        grab.performed += TryInteract;
        grab.Enable();

        //interaction
        switchmap = playerInputActions.Player.SwitchMap;
        switchmap.performed += ToggleMap;
        switchmap.Enable();

        //interaction
        click = playerInputActions.Player.Click;
        click.performed += RaycastClick;
        click.Enable();

        //interaction
        rotateGrabbed = playerInputActions.Player.RotateGrabbed;
        rotateGrabbed.performed += RotateGrabbedObject;
        rotateGrabbed.Enable();

        //interaction
        toggleCursorLock = playerInputActions.Player.ToggleCursorLock;
        toggleCursorLock.performed += ToggleCursorLock;
        toggleCursorLock.Enable();

        //interaction
        fileOpen = playerInputActions.Player.OpenFileDialog;
        fileOpen.performed += OpenFile;
        fileOpen.Enable();

        //interaction
        sendAnnouncement = playerInputActions.Player.SendAnnouncementDialog;
        sendAnnouncement.performed += SendAnnouncement;
        sendAnnouncement.Enable();

        //interaction
        openChatDialog = playerInputActions.Player.OpenChatDialog;
        openChatDialog.performed += OpenChatDialog;
        openChatDialog.Enable();

        //interaction
        openStatsScreen = playerInputActions.Player.OpenStatsScreen;
        openStatsScreen.performed += ToggleStatsScreen;
        openStatsScreen.Enable();

        //interaction
        togglePlayerVisibility = playerInputActions.Player.TogglePlayerVisibility;
        togglePlayerVisibility.performed += TogglePlayerVisibility;
        togglePlayerVisibility.Enable();

        moveMapCamera = playerInputActions.Player.MoveMapCamera;
        moveMapCamera.Enable();

        //interaction
        quit = playerInputActions.Player.Quit;
        quit.performed += TryQuit;
        quit.Enable();

        GameObject mumble = (GameObject)Instantiate(MumblePreFab, this.transform.position, Quaternion.identity, gameObject.transform);
        mumble.SetActive(true);
    }

    private void TryQuit(InputAction.CallbackContext obj)
    {
        if (!IsTypingInput)
        {
            PCmenuScreen.Quit.GetComponent<QuitPanel>().flipScreen();
            VRmenuScreen.Quit.GetComponent<QuitPanel>().flipScreen();
        }
    }

    private void SendAnnouncement(InputAction.CallbackContext obj) {
        if (GameManager.AmTeacher) {
            //Open Dialog Box for teacher
            if (!IsTypingInput) {
                if (FindObjectOfType<AnnouncementManager>().pnl_SendAnnouncement.activeSelf)
                {
                    FindObjectOfType<AnnouncementManager>().CloseAnnouncementDialog();
                }
                else
                {
                    FindObjectOfType<AnnouncementManager>().OpenAnnouncementDialog();
                }
            }
        }
    }

    private void OpenChatDialog(InputAction.CallbackContext obj) {
        if (!GameManager.isTakingAssessment)
        {
            FindObjectOfType<ChatManager>().OpenInput();
        }
    }

    private void TogglePlayerVisibility(InputAction.CallbackContext obj) {
        //Toggle
        if (!IsTypingInput) {
            GameManager.TogglePlayerVisibility(!GameManager.PlayersVisible);
        }
    }

    private void OpenFile(InputAction.CallbackContext obj)
    {
        if (GameManager.AmTeacher && isCursorLocked && !IsTypingInput) GameManager.LoadExpoFile();
    }

    private void ToggleCursorLock(InputAction.CallbackContext obj)
    {
        if (ScreenManager.IsAnyScreenAttached)
        {
            ScreenManager.DetachScreensFromPlayer();
        }
        else if (HasObject)
        {
            if (isCursorLocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    private void RotateGrabbedObject(InputAction.CallbackContext obj)
    {
        if (HasObject)
        {
            float rotationalForceCoefficient = 7f;
            var input = obj.ReadValue<Vector2>() * Time.deltaTime * rotationalForceCoefficient;
            //angular drag of the object's rigidbody can be adjusted to make it quicker/slower to stop rotating 
            selectedObject.GetComponent<Rigidbody>().AddTorque(selectedObject.transform.parent.transform.up * input.x);
            selectedObject.GetComponent<Rigidbody>().AddTorque(selectedObject.transform.parent.transform.right * input.y);
        }
    }
    
    private void OnDisable() {
        grab.Disable();
        switchmap.Disable();
        click.Disable();
        rotateGrabbed.Disable();
        toggleCursorLock.Disable();
        fileOpen.Disable();
        sendAnnouncement.Disable();
        openChatDialog.Disable();
        openStatsScreen.Disable();
        togglePlayerVisibility.Disable();
    }

    private void RaycastClick(InputAction.CallbackContext obj) {
        RaycastHit hit;
        Vector3 coor = Mouse.current.position.ReadValue();
        if (Physics.Raycast(Camera.main.ScreenPointToRay(coor), out hit) && isCursorLocked) {
            hit.collider.GetComponent<IClickable>()?.IClickableClicked();
        }
    }


    private void TryInteract(InputAction.CallbackContext obj)
    {
        Debug.Log("TryInteract!");

        //cast a ray corresponding to the object the mouse is over
        var ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity)) {
            //if we hit the object, we try to select the object and call the GrabbableObject internal functions
            Debug.Log("Hit:" + hit.transform.gameObject.name);
            Debug.Log("Hit Collider:" + hit.collider.name);

            if (EventSystem.current.IsPointerOverGameObject()) {
                //we have hit the UI in the world, keep our cursor unlocked
                //Cursor.lockState = CursorLockMode.None;
                return;
            }

            if (!isCursorLocked && Application.isFocused)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }

            ScreenManager.DetachScreensFromPlayer();
            SelectObject(hit.transform.GetComponent<GrabbableObject>());
        }

        //Andy Testing (STILL DOES NOT DETECT CANVAS COLLIDER)
        /*float maxRayDistance = 5;
        RaycastHit[] hits = Physics.RaycastAll(ray, maxRayDistance);
        if (hits.Length > 0) {
            foreach (RaycastHit hit in hits) {
                Debug.Log("Hit:" + hit.transform.gameObject.name);

                if (hit.transform.GetComponent<CanvasRenderer>() != null) {
                    //we have hit the UI in the world, keep our cursor unlocked
                    Cursor.lockState = CursorLockMode.None;
                    return;
                }

                if (hit.transform.GetComponent<GrabbableObject>() != null) {
                    SelectObject(hit.transform.GetComponent<GrabbableObject>());
                    return;
                }
            }
            SelectObject(null);
        }*/

    }

    private void SelectObject(GrabbableObject uObject)
    {
        if (uObject == selectedObject) return;
        DeselectObject();
        selectedObject = uObject;
        if (uObject != null) uObject.Grab(gameObject);
    }

    private void DeselectObject()
    {
        if (HasObject)
        {
            selectedObject.LetGo();
            selectedObject = null;
        }
    }

    private void ToggleMap(InputAction.CallbackContext obj) {
        if (isCursorLocked && !IsTypingInput)
        {
            mapTogglePC.ToggleMap();
            mapToggleVR.ToggleMap();
        }
    }

    private void ToggleStatsScreen(InputAction.CallbackContext obj)
    {
        if (!IsTypingInput) {
            if (isXRActive)
            {
                VRmenuScreen.flipScreen();
            }
            else
            {
                PCmenuScreen.flipScreen();
            }
        }
    }

}
