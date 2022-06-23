using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssignCanvasCam : MonoBehaviour
{
    public Canvas canvas;

    // Start is called before the first frame update
    void Start()
    {
        canvas = GetComponent<Canvas>();
        StartCoroutine(AssignCam());
    }

    private void OnEnable() {
        if (canvas != null) {
            StartCoroutine(AssignCam());
        }
    }

    IEnumerator AssignCam() {
        while (canvas.worldCamera == null) {
            // (UserObject uo in FindObjectsOfType<UserObject>()) {
                //if (!uo.IsOwner(GameManager.MyID)) {
                    //continue;
                //}
            XpoPlayer xPlayer = FindObjectOfType<XpoPlayer>();
            if (xPlayer != null) {
                foreach (Camera cam in xPlayer.GetComponentsInChildren<Camera>()) {
                    if (cam.name.Equals("PlayerCam")) {
                        canvas.worldCamera = cam;
                        break;
                    }
                }
            }
            //}
            yield return new WaitForSeconds(0.1f);
        }
    }
}
