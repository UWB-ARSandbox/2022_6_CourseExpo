/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFacesPlayer : MonoBehaviour
{
    Transform playerCam;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(AttachTransform());
    }

    IEnumerator AttachTransform() {
        while (GetComponent<Canvas>().worldCamera == null) {
            yield return new WaitForSeconds(0.1f);
        }
        playerCam = GetComponent<Canvas>().worldCamera.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerCam != null) {
            gameObject.transform.LookAt(playerCam);
            gameObject.transform.localRotation *= Quaternion.AngleAxis(180, Vector3.up);
        }
    }
}
*/