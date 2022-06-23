using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraClippingPlaneSizer : MonoBehaviour
{
    public Transform transformOfFocus;
    // Start is called before the first frame update
    void Start()
    {
        AdjustNearClippingPlane(GetComponent<Camera>());
    }

    private void AdjustNearClippingPlane(Camera cameraComponent)
    {
        if (transformOfFocus != null) {
            float distance = Vector3.Distance(cameraComponent.transform.position, transformOfFocus.position);
            cameraComponent.nearClipPlane = distance;
        }
    }
}
