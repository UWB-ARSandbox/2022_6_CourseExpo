using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ASL;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>                                                                                                           
/// Links an AR camera to a frustum display object. Allows you to view the camera's virtual view range. Updates the     
/// mobile and connected desktop clients when the frustum is moved.                                                     
///
/// Assumes that an ASLObject is attached to this Object
///  
/// </summary>                                                                                                          
public class ARFrustumController : MonoBehaviour
{
    #region Instance Variables
    
    public GameObject m_FrustumObject;
    public Frustum m_Frustum { get; private set; }

    private ASLObject m_FrustumASLObject;
    private ASLObject m_ARCameraASLObject;

    public float m_CLIP_DISTANCE = 5.0f;
    public float m_NEAR_DISTANCE = 0.0f;

    public float m_FRUSTUM_THICKNESS = 0.1f;
    
    #endregion
    
    #region Unity Functions
    
    /// <summary>
    /// Initializes Frustum. The frustum dimensions are dependent on the camera, and this sometimes takes a few seconds
    /// to initialize on mobile. This function is delayed for one second to account for this delay.
    /// </summary>
    void Start()
    {
        m_Frustum = m_FrustumObject.GetComponent<Frustum>();
        m_Frustum.m_CylinderWidth = m_FRUSTUM_THICKNESS;

        Debug.Assert(GetComponent<ASLObject>() != null, "Frustum Display Requires an ASLObject!");
        
        m_FrustumASLObject = m_FrustumObject.GetComponent<ASLObject>();

        m_ARCameraASLObject = GetComponent<ASLObject>();
        
        m_ARCameraASLObject._LocallySetFloatCallback(OnFloatsReceived);

        StartCoroutine(DelayedFrustumInit());
        
    }

    /// <summary>
    /// Every frame, update the position and rotation of the frustum across all connected devices
    /// </summary>
    void Update()
    {
        if (!Application.isMobilePlatform)
            return;
        
        UpdateFrustumPosition();
    }
    
    #endregion
    
    #region Internal

    //Delay method to give the AR Camera time to initialize
    /// <summary>
    /// Delayed initialization of camera-dependent parameters as AR takes a bit to initialize.
    /// </summary>
    /// <returns></returns>
    IEnumerator DelayedFrustumInit()
    {
        yield return new WaitForSeconds(1);
        
        Camera deviceCamera = getARCamera().GetComponent<Camera>();
        
        SetFrustumSize(deviceCamera);
    }
    
    
    /// <summary>
    /// Sets the frustum's size and sends it to all connected devices.
    /// </summary>
    /// <param name="camera"></param>
    private void SetFrustumSize(Camera camera)
    {
        m_Frustum.SetFrustumSize(camera, m_NEAR_DISTANCE, m_CLIP_DISTANCE);
        
        float tanFOV = Mathf.Tan(Mathf.Deg2Rad * 0.5f * camera.fieldOfView);
        
        // near plane dimension
        float n = camera.nearClipPlane + m_NEAR_DISTANCE;
        float nearPlaneHeight = 2f * n * tanFOV;
        float nearPlaneWidth = camera.aspect * nearPlaneHeight;
        
        // far plane dimension
        float f = camera.nearClipPlane + m_CLIP_DISTANCE;
        float farPlaneHeight = 2f * f * tanFOV;
        float farPlaneWidth = camera.aspect * farPlaneHeight;
        
        
        //Send these dimensions as an array to the connected desktop
        float[] sendArray = {nearPlaneWidth, nearPlaneHeight, farPlaneWidth, farPlaneHeight, m_Frustum.m_FarDist};
        
        m_ARCameraASLObject.SendAndSetClaim(() =>
        {
            m_ARCameraASLObject.SendFloatArray(sendArray);
        });
    }
    
    /// <summary>
    /// Updates the frustum's position and rotation across all connected devices.
    /// </summary>
    private void UpdateFrustumPosition()
    {
        Transform cameraTransform = getARCamera().transform;

        //This happens sometimes as it takes a bit for the camera to initialize
        if (cameraTransform == null)
            return;
        
        m_FrustumASLObject.SendAndSetClaim(() =>
        {
            m_FrustumASLObject.SendAndSetLocalPosition(cameraTransform.position);
            m_FrustumASLObject.SendAndSetLocalRotation(cameraTransform.rotation);
        });
    }

    /// <summary>
    /// Helper function for getting the Unity Camera representing the mobile device from ARCore
    /// </summary>
    /// <returns></returns>
    private GameObject getARCamera()
    {
        if (!Application.isMobilePlatform)
            return null;
        
        Transform deviceTransform = ASL.ARWorldOriginHelper.GetInstance().ARCoreDeviceTransform;

        return deviceTransform.GetChild(0).gameObject; //First child is the camera object
    }
    
    /// <summary>
    /// Callback that runs on Desktop when a float array is received
    /// This method must be static, so the first step is finding our specific frustum object
    /// Then we decode the float array and update our dimensions
    /// </summary>
    /// <param name="_id"></param>
    /// <param name="floatArray"></param>
    private static void OnFloatsReceived(string _id, float[] floatArray)
    {
        if (Application.isMobilePlatform) //Only run this code on desktop
            return;
        
        ASLObject sender;
        ARFrustumController cameraController;

        if (ASL.ASLHelper.m_ASLObjects.TryGetValue(_id, out sender))
        {
            cameraController = sender.GetComponent<ARFrustumController>();

            if (cameraController == null)
            {
                Debug.LogError("ARFrustumController - OnFloatsReceived: ASL Object does not have ARFrustumController");

                Debug.LogError(sender.name);
                
                return;
            }
        }
        else
        {
            Debug.LogError("ARFrustumController - OnFloatsReceived: Could not find sender");
            return;
        }

        Frustum frustum = cameraController.m_Frustum;

        frustum.SetNearPlaneSize(floatArray[0], floatArray[1]);
        frustum.SetFarPlaneSize(floatArray[2], floatArray[3]);

        frustum.m_FarDist = floatArray[4];
    }
    
    #endregion
}
