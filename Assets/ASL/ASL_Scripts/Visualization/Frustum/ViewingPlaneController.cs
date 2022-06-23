using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// A frame made up of four cylinders with spheres at each vertex. Used to represent the near and far planes.
/// </summary>
public class ViewingPlaneController : MonoBehaviour
{
    //Todo: Make these more flexible?
    private static readonly float DEFAULT_WIDTH = 10;
    private static readonly float DEFAULT_HEIGHT = 6;
    
    private float mWidth;
    
    public float m_Width
    {
        get => mWidth;
        set => UpdateWidth(value);
    }

    private float mHeight;
    
    public float m_Height
    {
        get => mHeight;
        set => UpdateHeight(value);
    }

    public GameObject topLeft;
    public GameObject topRight;
    public GameObject bottomLeft;
    public GameObject bottomRight;
    
    
    /// <summary>
    /// Sets to default width and height
    /// </summary>
    void Start()
    {
        m_Width = DEFAULT_WIDTH;
        m_Height = DEFAULT_WIDTH;
    }
    
    private void UpdateWidth(float newWidth)
    {
        mWidth = newWidth;

        Vector3 center = transform.localPosition;
        
        SetXPosition(topLeft, center.x - (newWidth / 2.0f));
        SetXPosition(bottomLeft, center.x - (newWidth / 2.0f));
        SetXPosition(topRight, center.x + (newWidth / 2.0f));
        SetXPosition(bottomRight, center.x + (newWidth / 2.0f));
    }

    private void UpdateHeight(float newHeight)
    {
        mHeight = newHeight;

        Vector3 center = transform.localPosition;

        SetYPosition(topLeft, center.y + (newHeight / 2.0f));
        SetYPosition(topRight, center.y + (newHeight / 2.0f));
        SetYPosition(bottomLeft, center.y - (newHeight / 2.0f));
        SetYPosition(bottomRight, center.y - (newHeight / 2.0f));
    }

    private void SetXPosition(GameObject gObject, float xPosition)
    {
        Vector3 position = gObject.transform.localPosition;

        position.x = xPosition;

        gObject.transform.localPosition = position;
    }

    private void SetYPosition(GameObject gObject, float yPosition)
    {
        Vector3 position = gObject.transform.localPosition;

        position.y = yPosition;

        gObject.transform.localPosition = position;
    }
}
