using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System;

/// <summary>The ARPointCloudManager ASL Extension for the ASL_ARCorePointCLoud tutorial.</summary>
public class ARPointCloudManagerExtension : MonoBehaviour
{
    /// <summary>The prefab used for building filter spheres</summary>
    public GameObject FilterSpherePreFab;

    /// <summary>The world model object</summary>
    public GameWorld TheWorld;

    /// <summary>Bool to start the tutorial scene using custom colors</summary>
    public bool UseBackgroundPointColor = true;

    /// <summary>Bool to start the tutorial scene with device movement speed filtering</summary>
    public bool UsePositionSpeedCaptureFilter = true;

    /// <summary>Bool to start the tutorial scene with point capturing enabled</summary>
    public bool ScanEnabled = false;

    /// <summary>Bool to represent if filtering is currently used.  Used for UI button colors.</summary>
    public bool IsFiltering = false;

    /// <summary>Float value representing speed in which background texture sampling is performed.  1 = 1 sec.</summary>
    public float BackgroundTextureRefreshRate = .1F;

    /// <summary>Float value 0::1 representing confidence value filter</summary>
    public float ARKitConfidenceMinimumValue = .70f;

    /// <summary>Float value representing minimum positional movement for motion filtering</summary>
    public float CapturePositionMotionSpeedMinimum = .1f;

    /// <summary>Float value representing maximum positional movement for motion filtering</summary>
    public float CapturePositionMotionSpeedMaximum = 2f;

    /// <summary> Delegate for the particle list callback event </summary>    
    /// <param name="particleList">The named tuple list containing the particles as positions and colors.</param>
    public delegate void ListGeneratedCallback(object sender, ASLPointCloudEventArgs args);

    /// <summary> Event for the notification of a newly generated raw particle list </summary> 
    public event ListGeneratedCallback ListGenerated;

    /// <summary>Represents internal filtersphere enabled state</summary> 
    private bool _filterSphereEnabled = false;

    /// <summary>Represents internal filtersphere collider</summary> 
    private SphereCollider _filterSphereCollider = null;

    /// <summary>Represents internal filtersphere instance</summary> 
    private GameObject _filterSphereInstance;

    /// <summary>Represents internal previous ARCamera position</summary> 
    private Vector3 _previousCameraPosition;

    /// <summary>Represents internal previous ARCamera position</summary> 
    private GameObject _ARCamera;

    /// <summary>Represents internal ARCore ARPointCloudManager object</summary> 
    private ARPointCloudManager _arPCManager;

    /// <summary>Represents internal ARCamera background.  Used for pixel color sampling.</summary> 
    private ARCameraBackground _arCameraBackground;

    /// <summary>Represents internal ARCamera Active Render Texture.  Used for temp storage.</summary> 
    private RenderTexture _activeTexture;

    /// <summary>Represents internal ARCamera Background Render Texture.  Used for generating textures w/o unity game objects.</summary> 
    private RenderTexture _backgroundTexture;

    /// <summary>Represents background texture for point color sampling.</summary> 
    private Texture2D _screenTexture;

    /// <summary>Duplicate detection data structure.</summary> 
    private HashSet<ulong> _previousIdsHashSet;

    /// <summary>Current lifespan of the background texture.</summary> 
    private float _backgroundTextureLifespan = 0f;

    /// <summary>
    /// Startup initialization for the ARPointCloudManagerExtension object.
    /// Sets mobile/PC settings.
    /// </summary>
    void Start()
    {
        // Initialize AR Camera and PointCloud support
        if (Application.isMobilePlatform)
        {
            _arPCManager = GameObject.FindObjectsOfType<ARPointCloudManager>()[0];

            if (_arPCManager == null)
            {
                throw new InvalidOperationException("ARPointCloud Error: No ARPointCloudManager objects found in scene.  Attach ARPointCloudManager script component to ARSession game object.");
            }

            _ARCamera ??= GameObject.FindObjectOfType<Camera>().gameObject;
            _arCameraBackground = _ARCamera.GetComponent<ARCameraBackground>();

            if (_arCameraBackground == null)
            {
                UseBackgroundPointColor = false;
                Debug.LogError("ARPointCloud Error: No ARCameraBackground component found in the ARCamera object.  Attach the ARCameraBackground component to the AR Camera object.");
            }

            _arPCManager.pointCloudsChanged += onARPointCloudChanged;
            _previousIdsHashSet = new HashSet<ulong>();
        }
    }

    //private void Update()
    //{
    //    if (transform.hasChanged)
    //    {
    //        Debug.Log("ARSessionOrigin transform changed");
    //        TheWorld.transform.position = transform.position;
    //        TheWorld.transform.rotation = transform.rotation;
    //    }
    //}

    /// <summary>
    /// Enables or disables the filter sphere.  Enabling the filter sphere will result in only capturing points inside or touching the filtersphere.
    /// </summary>
    /// <param name="isEnabled">Set to enable or disable filter sphere.</param>
    public void SetFilterSphereEnabled(bool isEnabled)
    {
        _filterSphereEnabled = isEnabled;
        if (isEnabled)
        {
            instantiateFilterSphere();
        }
        else
        {
            destroyFilterSphere();
        }
    }

    /// <summary>
    /// Gets the state of the filter sphere.
    /// </summary>
    /// <returns>A bool representing if the filter sphere is enabled or not.</returns>
    public bool GetFilterSphereEnabled()
    {
        return _filterSphereEnabled;
    }

    /// <summary>
    /// Internal helper to destroy the filtersphere object.
    /// </summary>
    private void destroyFilterSphere()
    {
        if (_filterSphereInstance != null)
        {
            Destroy(_filterSphereInstance);
            _filterSphereInstance = null;
            _filterSphereCollider = null;
        }
    }

    /// <summary>
    /// Internal helper to instantiate the filtersphere object.
    /// </summary>
    private void instantiateFilterSphere()
    {
        if (_filterSphereInstance == null && _filterSphereEnabled)
        {
            if (FilterSpherePreFab == null)
            {
                Debug.LogError("ARPointCloud Error: FilterSphere Prefab not set.  Add FilterSphere prefab to ARPointCloudMgr object.");
                _filterSphereEnabled = false;
                return;
            }
            _filterSphereInstance = Instantiate(FilterSpherePreFab);
            _filterSphereInstance.transform.position = _ARCamera.transform.position;
            _filterSphereInstance.transform.Translate(_ARCamera.transform.forward);
            _filterSphereInstance.gameObject.transform.SetParent(TheWorld.gameObject.transform);
            _filterSphereCollider = _filterSphereInstance.GetComponent<SphereCollider>();
        }
    }

    /// <summary>
    /// Internal helper to check if a given point is within the filter sphere.
    /// </summary>
    /// <param name="point">The point to check</param>
    /// <returns>A bool that represents if a given point is within the filter sphere.</returns>
    private bool isPointWithinFilterSphere(Vector3 point)
    {
        return _filterSphereCollider.bounds.Contains(point);
    }

    /// <summary>
    /// Unity method to ensure that temp textures are destroyed.
    /// </summary>
    private void OnDestroy()
    {
        if (_screenTexture != null)
        {
            GameObject.Destroy(_screenTexture);
            _screenTexture = null;
        }
    }

    /// <summary>
    /// Helper method to swap RenderTextures to build a Texture2D object _screenTexture representing the background camera image.
    /// Will refresh texture every _backgroundTextureLifespan seconds.
    /// </summary>
    private void updateBackgroundScreenTexture()
    {
        if (_backgroundTextureLifespan > BackgroundTextureRefreshRate)
        {
            _backgroundTextureLifespan = 0;
            _backgroundTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, 24);

            // Swap the active rendertexture for the background
            Graphics.Blit(null, _backgroundTexture, _arCameraBackground.material);
            _activeTexture = RenderTexture.active;
            RenderTexture.active = _backgroundTexture;

            // try to reuse old textures
            if (_screenTexture == null)
            {
                _screenTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            }
            _screenTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);

            // Restore active rendertexture and cleanup
            RenderTexture.active = _activeTexture;
            RenderTexture.ReleaseTemporary(_backgroundTexture);
        }
        else
        {
            _backgroundTextureLifespan += Time.deltaTime;
        }
    }

    /// <summary>
    /// Callback function that gets called when ARCore point cloud updates are generated.
    /// Processes and filters point cloud data then passes point list to event ListGeneratedCallback subscribers.
    /// </summary>
    /// <param name="args">The event arguments containing the updated point list.</param>
    private void onARPointCloudChanged(ARPointCloudChangedEventArgs args)
    {
        if (isActiveAndEnabled && ScanEnabled && !isMotionOutsideFilterBounds())
        {
            if (UseBackgroundPointColor)
            {
                updateBackgroundScreenTexture();
            }

            ASLPointCloudEventArgs aslArgs = new ASLPointCloudEventArgs();
            aslArgs.RawParticleList = createParticles(args.updated);
            aslArgs.UseCustomColor = UseBackgroundPointColor;

            // Send list to subscribed event delegates
            ListGenerated?.Invoke(this, aslArgs);
        }
    }

    /// <summary>
    /// Helper method to create the filtered particle list in world space   
    /// </summary>
    /// <param name="pointCloudList">The raw ARCore pointcloud list.</param>
    /// <returns>A tuple list of Vector3 position and Color colors.</returns>
    private List<(Vector3 position, Color color)> createParticles(List<ARPointCloud> pointCloudList)
    {
        List<(Vector3 position, Color color)> particles = new List<(Vector3 position, Color color)>();

        foreach (ARPointCloud myCloud in pointCloudList)
        {
            var ids = (Unity.Collections.NativeSlice<ulong>)myCloud.identifiers;
            var positions = (Unity.Collections.NativeSlice<Vector3>)myCloud.positions;
            var confidences = (Unity.Collections.NativeSlice<float>)myCloud.confidenceValues;

            if (positions.Length <= 0)
            {
                return null;
            }
            else
            {
                for (int i = 0; i < positions.Length; i++)
                {
                    if (confidences[i] > ARKitConfidenceMinimumValue &&
                        IsPointSphereFilterable(positions[i]) &&
                        !_previousIdsHashSet.Contains(ids[i]))
                    {
                        _previousIdsHashSet.Add(ids[i]);
                        
                        if (UseBackgroundPointColor)
                        {
                            Vector3 screenPos = _ARCamera.GetComponent<Camera>().WorldToScreenPoint(positions[i]);
                            particles.Add((transform.TransformPoint(positions[i]), _screenTexture.GetPixel((int)screenPos.x, (int)screenPos.y)));
                        }
                        else
                        {                            
                            particles.Add((transform.TransformPoint(positions[i]), Color.black));
                        }
                    }
                }
            }
            if (_previousIdsHashSet.Count > 100000)
            {
                _previousIdsHashSet.Clear();
            }

        }
        return particles;
    }

    /// <summary>
    /// Helper method to filter points by camera position movement speed    
    /// </summary>
    /// <returns>A bool representing if filtering should occur</returns>
    private bool isMotionOutsideFilterBounds()
    {
        if (UsePositionSpeedCaptureFilter)
        {
            float translateDeltaMagnitude = (_previousCameraPosition - _ARCamera.transform.position).magnitude / Time.deltaTime;
            IsFiltering = (translateDeltaMagnitude < CapturePositionMotionSpeedMinimum || translateDeltaMagnitude > CapturePositionMotionSpeedMaximum);
            _previousCameraPosition = _ARCamera.transform.position;
            return IsFiltering;
        }
        else
        {
            _previousCameraPosition = _ARCamera.transform.position;
            return false;
        }
    }

    /// <summary>
    /// Helper method to get filter sphere filtering state   
    /// </summary>
    /// <returns>A bool representing if point should be processed due to sphere filter</returns>
    private bool IsPointSphereFilterable(Vector3 point)
    {
        return !_filterSphereEnabled || isPointWithinFilterSphere(point);
    }
}

/// <summary>
/// ASLPointCloudEventArgs.  Used for ARPointCloudManagerExtension point cloud generated events.
/// </summary>
public class ASLPointCloudEventArgs : EventArgs
{
    // Very simple for now, but this can be extended without breaking existing consumers
    public List<(Vector3 position, Color color)> RawParticleList { get; set; }
    public bool UseCustomColor { get; set; }
}