using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayheadMover : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public Slider progressSlider;

    public void MovePlayhead(float fraction)
    {
        transform.position = Vector3.Lerp(startPoint.position, endPoint.position, fraction);
        SetProgressSliderValue(fraction);
    }

    private void SetProgressSliderValue(float fraction)
    //Sets the value of the slider to appropriately adjust its fill rect
    {
        progressSlider.value = fraction;
    }
}
