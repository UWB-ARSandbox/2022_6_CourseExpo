using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceRotate : MonoBehaviour
{
    public GameObject Camera;
    public GameObject Face;

    // Update is called once per frame
    void Update()
    {
        Face.transform.localRotation = Camera.transform.localRotation;
    }
}
