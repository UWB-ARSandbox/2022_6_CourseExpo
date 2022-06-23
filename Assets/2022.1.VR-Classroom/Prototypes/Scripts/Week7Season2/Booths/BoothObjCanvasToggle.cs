using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class BoothObjCanvasToggle : MonoBehaviour
{
    public Canvas canvas;
    public Camera playerCam;
    private bool isHover;
    private bool isSelected;
    private XRGrabInteractable xrGrab;
    private PlayerController player;

    // Start is called before the first frame update
    void Start()
    {
        canvas = GetComponentInChildren<Canvas>();
        StartCoroutine(AssignCam());
        isHover = false;
        isSelected = false;
        xrGrab = GetComponent<XRGrabInteractable>();
        xrGrab.firstHoverEntered.AddListener(EnableIsHover);
        xrGrab.lastHoverExited.AddListener(DisableIsHover);
        //xrGrab.firstSelectEntered.AddListener(DisableIsSelected);
        //xrGrab.lastSelectExited.AddListener(EnableIsSelected);
    }

    IEnumerator AssignCam() {
        while (playerCam == null) {
            GameObject uo = GameObject.Find("FirstPersonPlayer(Clone)");
            if (uo != null) {
                player = uo.gameObject.GetComponent<PlayerController>();
                foreach (Camera cam in uo.GetComponentsInChildren<Camera>()) {
                    if (cam.name.Equals("PlayerCam")) {
                        playerCam = cam;
                        break;
                    }
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void Update() {
        //Debug.LogError("Updating...");
        bool setActive = (isHover || isSelected);

        if (player != null && player.selectedObject == GetComponent<GrabbableObject>()) {
            //Debug.LogError("TRUE!");
            setActive = true;
        } else {
            RaycastHit hit;
            Vector3 coor = Mouse.current.position.ReadValue();
            if (playerCam != null && Physics.Raycast(playerCam.ScreenPointToRay(coor), out hit)) {
                if (hit.collider != null && (
                        hit.collider.gameObject.Equals(gameObject)
                        || (hit.collider.transform.parent != null && hit.collider.transform.parent.gameObject.Equals(gameObject))
                        || (canvas != null && hit.collider.gameObject.Equals(canvas.gameObject))
                        )
                   ) {
                    //Debug.LogError("TRUE!");
                    setActive = true;
                }
            }
        }

        // Turn booth object canvas towards player
        canvas.gameObject.SetActive(setActive);
        if (playerCam != null) {
            canvas.gameObject.transform.LookAt(playerCam.transform);
            canvas.gameObject.transform.localRotation *= Quaternion.AngleAxis(180, Vector3.up);
        }
    }

    public void DisableIsHover(HoverExitEventArgs arg) {
        isHover = false;
    }

    public void EnableIsHover(HoverEnterEventArgs arg) {
        isHover = true;
    }

    public void DisableIsSelected(SelectEnterEventArgs arg) {
        isSelected = false;
    }

    public void EnableIsSelected(SelectExitEventArgs arg) {
        isSelected = true;
    }

}
