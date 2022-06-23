using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ASL;
using UnityEngine;
/// <summary>
/// Links an AR camera to a frustum display object. Allows you to view the camera's virtual view range. Updates the
/// mobile and connected desktop clients when the frustum is moved.
///  
/// </summary>
public class ARCameraController : MonoBehaviour
{
    #region Instance Variables
    
    public GameObject frustumObject;
    public Frustum frustum { get; private set; }

    private ASLObject frustumASLObject;
    private ASLObject ARCameraASLObject;

    public float CLIP_DISTANCE = 5.0f;
    public float NEAR_DISTANCE = 0.0f;

    public float FRUSTUM_THICKNESS = 0.1f;

        #endregion
    
    #region Unity Functions
    
    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        frustum = frustumObject.GetComponent<Frustum>();
        frustum.m_CylinderWidth = FRUSTUM_THICKNESS;
        
        frustumASLObject = frustumObject.GetComponent<ASLObject>();

        ARCameraASLObject = GetComponent<ASLObject>();
        
        ARCameraASLObject._LocallySetFloatCallback(OnFloatsReceived);
        
        setFrustumSize(getARCamera().GetComponent<Camera>());
    }

    /// <summary>
    /// 
    /// </summary>
    void Update()
    {
        if (!Application.isMobilePlatform)
            return;
        
        updateFrustumPosition();
    }
    
    #endregion

    //Sets the frustum dimensions and sends them to all connected devices
    /// <summary>
    /// 
    /// </summary>
    /// <param name="camera"></param>
    private void setFrustumSize(Camera camera)
    {
        //From: https://github.com/myuwbclasses/CSS451/blob/master/ClassExamples/Week9/Week9_Examples/
        //1.DrawCameraFrustum/Assets/Source/UISupport/CameraManipulation_DrawFrustum.cs
        
        float tanFOV = Mathf.Tan(Mathf.Deg2Rad * 0.5f * camera.fieldOfView);
        
        // near plane dimension
        float n = camera.nearClipPlane + NEAR_DISTANCE;
        float nearPlaneHeight = 2f * n * tanFOV;
        float nearPlaneWidth = camera.aspect * nearPlaneHeight;
        
        frustum.SetNearPlaneSize(nearPlaneWidth, nearPlaneHeight);
        
        // far plane dimension
        float f = camera.nearClipPlane + CLIP_DISTANCE;
        float farPlaneHeight = 2f * f * tanFOV;
        float farPlaneWidth = camera.aspect * farPlaneHeight;
        
        frustum.SetFarPlaneSize(farPlaneWidth, farPlaneHeight);
        
        frustum.m_FarDist = f - n;

        //Todo: Separate this networked portion from the frustum resizing code
        
        //Send these dimensions as an array to the connected desktop
        float[] sendArray = {nearPlaneWidth, nearPlaneHeight, farPlaneWidth, farPlaneHeight, frustum.m_FarDist};
        
        ARCameraASLObject.SendAndSetClaim(() =>
        {
            ARCameraASLObject.SendFloatArray(sendArray);
        });
    }
    
    /// <summary>
    /// 
    /// </summary>
    private void updateFrustumPosition()
    {
        Transform cameraTransform = getARCamera().transform;  
        
        frustumASLObject.SendAndSetClaim(() =>
        {
            frustumASLObject.SendAndSetWorldPosition(cameraTransform.position);
            frustumASLObject.SendAndSetWorldRotation(cameraTransform.rotation);
        });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private GameObject getARCamera()
    {
        if (!Application.isMobilePlatform)
            return null;
        
        Transform deviceTransform = ASL.ARWorldOriginHelper.GetInstance().ARCoreDeviceTransform;

        return deviceTransform.GetChild(0).gameObject; //First child is the camera object
    }
    
    //Callback that runs on Desktop when a float array is received
    //This method must be static, so the first step is finding our specific frustum object
    //Then we decode the float array and update our dimensions
    private static void OnFloatsReceived(string _id, float[] floatArray)
    {
        if (Application.isMobilePlatform) //Only run this code on desktop
            return;
        
        ASLObject sender;
        ARCameraController cameraController;

        if (ASL.ASLHelper.m_ASLObjects.TryGetValue(_id, out sender))
        {
            cameraController = sender.GetComponent<ARCameraController>();

            if (cameraController == null)
            {
                Debug.LogError("ARCameraController - OnFloatsReceived: ASL Object does not have ARCameraController");

                Debug.LogError(sender.name);
                
                return;
            }
        }
        else
        {
            Debug.LogError("ARCameraController - OnFloatsReceived: Could not find sender");
            return;
        }

        Frustum frustum = cameraController.frustum;

        frustum.SetNearPlaneSize(floatArray[0], floatArray[1]);
        frustum.SetFarPlaneSize(floatArray[2], floatArray[3]);

        frustum.m_FarDist = floatArray[4];
    }
}
