using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
/// <summary>
/// This represents the frustum prefab - No fancy networking going on here!
/// Controls the position of the near and far plane
/// </summary>
public class Frustum : MonoBehaviour
{
    #region Instance Variables
    
    public GameObject m_NearPlaneObject;
    public GameObject m_FarPlaneObject;
    
    private ViewingPlaneController m_NearPlane;
    private ViewingPlaneController m_FarPlane;

    private float mFarDist;
    
    public float m_FarDist 
    { 
        get => mFarDist;
        set
        {
            mFarDist = value;
            UpdateDist(value, m_FarPlaneObject);
        }
    }
    
    private float mNearDist;
    
    public float m_NearDist
    {
        get => mNearDist;
        
        set
        {
            mNearDist = value;
            UpdateDist(value, m_NearPlaneObject);
        }
    }

    private float mCylinderWidth;

    public float m_CylinderWidth
    {
        get => mCylinderWidth;

        set
        {
            mCylinderWidth = value;
            UpdateCylinderWidth(value);
        }

    }
    
    #endregion
    
    #region Public Functions
    
    void Start()
    {
        m_NearPlane = m_NearPlaneObject.GetComponent<ViewingPlaneController>();
        m_FarPlane = m_FarPlaneObject.GetComponent<ViewingPlaneController>();
    }
    

    /// <summary>
    /// Sets the frustum's size based on the camera's parameters
    ///
    /// near and clip distances provided because you may not want to use the _actual_ distance for these
    ///
    /// </summary>
    /// <param name="camera"></param>
    public void SetFrustumSize(Camera camera, float nearDistance, float clipDistance)
    {
        //From: https://github.com/myuwbclasses/CSS451/blob/master/ClassExamples/Week9/Week9_Examples/
        //1.DrawCameraFrustum/Assets/Source/UISupport/CameraManipulation_DrawFrustum.cs
        
        float tanFOV = Mathf.Tan(Mathf.Deg2Rad * 0.5f * camera.fieldOfView);
        
        // near plane dimension
        float n = camera.nearClipPlane + nearDistance;
        float nearPlaneHeight = 2f * n * tanFOV;
        float nearPlaneWidth = camera.aspect * nearPlaneHeight;
        
        SetNearPlaneSize(nearPlaneWidth, nearPlaneHeight);
        
        // far plane dimension
        float f = camera.nearClipPlane + clipDistance;
        float farPlaneHeight = 2f * f * tanFOV;
        float farPlaneWidth = camera.aspect * farPlaneHeight;
        
        SetFarPlaneSize(farPlaneWidth, farPlaneHeight);

        m_NearDist = n;
        m_FarDist = f;
    }

    /// <summary>
    /// Enables/Disables frustum display
    /// </summary>
    /// <param name="visible"></param>
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Sets the size of the near plane in world units
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void SetNearPlaneSize(float width, float height)
    {
        m_NearPlane.m_Height = height;
        m_NearPlane.m_Width = width;
    }
    
    /// <summary>
    /// Sets the size of the far plane in world units
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void SetFarPlaneSize(float width, float height)
    {
        m_FarPlane.m_Height = height;
        m_FarPlane.m_Width = width;
    }
    
    #endregion
    
    #region Internal
    
    /// <summary>
    /// Updates the distance between the camera and either of the planes
    /// </summary>
    /// <param name="distanceToCamera"></param>
    /// <param name="plane"></param>
    private void UpdateDist(float distanceToCamera, GameObject plane)
    {
        Vector3 cameraPosition = transform.localPosition;
        Vector3 planePosition = plane.transform.localPosition;

        planePosition.z = cameraPosition.z + distanceToCamera;

        plane.transform.localPosition = planePosition;
    }

    /// <summary>
    /// Updates the width of all cylinders in the frustum
    /// </summary>
    /// <param name="newCylinderWidth"></param>
    private void UpdateCylinderWidth(float newCylinderWidth)
    {
        RecursivelyUpdateCylinderWidth(gameObject, newCylinderWidth);
    }

    /// <summary>
    /// Recursively finds all of the cylinders and spheres in the frustum and adjusts their size
    /// </summary>
    /// <param name="gObject"></param>
    /// <param name="newCylinderWidth"></param>
    private void RecursivelyUpdateCylinderWidth(GameObject gObject, float newCylinderWidth)
    {
        //Check if current GameObject is a cylinder - update its width
        StretchCylinder cylinder = gObject.GetComponent<StretchCylinder>();
        
        if (cylinder != null)
        {
            cylinder.m_Width = newCylinderWidth;
            return;
        }

        //A bit messy - if the GameObject is a sphere, give it new dimensions
        SphereCollider collider = gObject.GetComponent<SphereCollider>();

        if (collider != null)
        {
            gObject.transform.localScale = new Vector3(newCylinderWidth, newCylinderWidth, newCylinderWidth);
            return;
        }

        foreach (Transform child in gObject.transform)
        {
            RecursivelyUpdateCylinderWidth(child.gameObject, newCylinderWidth);
        }
    }
    
    #endregion
    
}
