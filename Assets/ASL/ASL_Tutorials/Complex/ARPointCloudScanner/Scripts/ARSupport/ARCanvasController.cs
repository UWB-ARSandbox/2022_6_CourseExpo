using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>UI Controller for the ASL_ARCorePointCloud tutorial.</summary>
public class ARCanvasController : MonoBehaviour
{
    /// <summary>Button to toggle point cloud capturing</summary>
    public Button TogglePointCapture;

    /// <summary>Button to toggle a filter sphere for point capture filtering</summary>
    public Button ToggleFilterSphere;

    /// <summary>Button to toggle background render color capture for points</summary>
    public Button ToggleBackgroundColor;

    /// <summary>Button to clear all saved/persisted point cloud data</summary>
    public Button ClearPoints;

    /// <summary>Button to enable or disable cloud anchor placement</summary>
    public Button RecordARAnchorPoints;

    /// <summary>Button to set world origin</summary>
    public Button SetWorldOrigin;

    /// <summary>The tutorial gameworld model.  Handles UI controller to game state and links the ARPointCloudManagerExtension events to ASLParticleSystem.</summary>
    public GameWorld TheWorld;

    /// <summary>The ASLParticleSystem object component.  This is the ASL persisting particle system for the point cloud.</summary>
    public ASLParticleSystem ASLPCManager;

    /// <summary>The ARPointCloudManagerExtension object component.  This is normally a component in the AR Session Origin object.</summary>
    public ARPointCloudManagerExtension ARPointCloudMgr;

    /// <summary>Used to return to original button grey colors</summary>
    private Color _originalButtonColor;

    /// <summary>State for AR Anchor touch recording enabled</summary>
    private bool _recordARAnchorsEnabled = false;

    /// <summary>De-duplication value for setting world origin on same device</summary>
    private bool _worldOriginSet = false;

    
    /// <summary>Unity start method.  Will add button listeners and capture original button colors.</summary>
    void Start()
    {
        TogglePointCapture.onClick.AddListener(togglePointCloudEnabled);
        ToggleFilterSphere.onClick.AddListener(toggleFilterSphereEnabled);
        ToggleBackgroundColor.onClick.AddListener(toggleBackgroundColor);
        RecordARAnchorPoints.onClick.AddListener(setRecordARPointsEnabled);
        SetWorldOrigin.onClick.AddListener(setWorldOrigin);
        ClearPoints.onClick.AddListener(clearAllPoints);
        _originalButtonColor = TogglePointCapture.image.color;

    }

    /// <summary>
    /// Update method to control the TogglePointCloudEnabled button color and touch recording.  
    /// Will show the following colors for PointCloud recording:  
    ///     Grey: point capture not enabled.  
    ///     Red: point capture recording.  
    ///     Yellow: Movement is too fast/slow.  Recording temporarily paused.
    /// If AR Cloud Anchor recording is enabled, will process touches as poses and instantiate new anchors.
    /// </summary>
    void Update()
    {
        if (ARPointCloudMgr.ScanEnabled)
        {
            TogglePointCapture.image.color = ARPointCloudMgr.IsFiltering ? Color.yellow : Color.red;
        }
        else
        {
            TogglePointCapture.image.color = _originalButtonColor;
        }

        if (_recordARAnchorsEnabled && Input.touchCount >= 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                Pose? m_LastHitPose = ASL.ARWorldOriginHelper.GetInstance().Raycast(touch.position);                
                TheWorld.SetCloudAnchor(m_LastHitPose);
            }
        }
    }

    /// <summary>
    /// Internal onClick helper for the TogglePointCapture button.  Will enable/disable point capture.
    /// Toggles the ARPointCloudManager.ScanEnabled bool.
    /// </summary>
    private void togglePointCloudEnabled()
    {
        ARPointCloudMgr.ScanEnabled = !ARPointCloudMgr.ScanEnabled;
    }

    /// <summary>
    /// Internal onClick helper for the ToggleFilterSphere button.  Will enable/disable a filter sphere.
    /// Toggles the ARPointCloudManager.SetFilterSphereEnabled bool.
    /// </summary>
    private void toggleFilterSphereEnabled()
    {
        ARPointCloudMgr.SetFilterSphereEnabled(!ARPointCloudMgr.GetFilterSphereEnabled());
    }

    /// <summary>
    /// Internal onClick helper for the ToggleBackgroundColor button.  Will either use the background texture points to generate 
    /// particle colors, or use a default color of Green.  Toggles the ARPointCloudManager.UseBackgroundPointColor bool.
    /// If default color is used, the button will trun green
    /// </summary>
    private void toggleBackgroundColor()
    {
        ARPointCloudMgr.UseBackgroundPointColor = !ARPointCloudMgr.UseBackgroundPointColor;
        ToggleBackgroundColor.image.color = ARPointCloudMgr.UseBackgroundPointColor ? _originalButtonColor : Color.green;
    }

    /// <summary>
    /// Internal onClick helper for the ClearPoints button.  Will remove all points from the persisting particle system.
    /// </summary>
    private void clearAllPoints()
    {
        TheWorld.ClearParticles();
    }

    /// <summary>
    /// Internal onClick helper for setting the world origin.  
    /// </summary>
    private void setWorldOrigin()
    {
        if (!_worldOriginSet)
        {
            Debug.Log("Setting world origin");
            _worldOriginSet = true;
            TheWorld.SetWorldOriginCloudAnchor();            
        }
    }

    /// <summary>
    /// Internal onClick helper for toggling AR cloud anchor instantiation via touch points
    /// Button will show red if touch recording is enabled.
    /// </summary>
    private void setRecordARPointsEnabled()
    {
        _recordARAnchorsEnabled = !_recordARAnchorsEnabled;
        RecordARAnchorPoints.image.color = _recordARAnchorsEnabled ? Color.red : _originalButtonColor;
    }
}
