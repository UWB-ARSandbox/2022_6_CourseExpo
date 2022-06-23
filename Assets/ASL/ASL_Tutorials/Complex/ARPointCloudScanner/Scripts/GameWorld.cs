using UnityEngine;
using ASL;

/// <summary>World model for the ASL_ARCorePointCloud tutorial.</summary>
public class GameWorld : MonoBehaviour
{  
    /// <summary>The root object tree representing ARCore player objects and components including the AR Camera and AR Session Origin</summary>
    public GameObject AndroidARRoot;

    /// <summary>The root object tree representing PC player specific objects and components including the Camera</summary>
    public GameObject PCPlayerRoot;

    /// <summary>The ARPointCloud ASL Extension object which is able to format and filter point data</summary>
    public ARPointCloudManagerExtension ARPointCloudMgr;

    /// <summary>The ASL ParticleSystem extension object specilizing in point cloud data</summary>
    public ASLParticleSystem ASLPointCloudMgr;

    /// <summary> Gets the hit position where the user touched the screen to help record where the object is verses where the user tapped</summary>
    private Pose? m_LastValidPose;

    /// <summary>Static hack for this class so that functions can be called after objects and cloud anchors are created using the same parameter they were created with</summary>
    private static GameWorld m_This;


    /// <summary>
    /// Startup initialization for the point cloud game world
    /// </summary>
    void Start()
    {
        AndroidARRoot.SetActive(Application.isMobilePlatform);
        PCPlayerRoot.SetActive(!Application.isMobilePlatform);


        ARPointCloudMgr ??= FindObjectOfType<ARPointCloudManagerExtension>();
        ASLPointCloudMgr ??= FindObjectOfType<ASLParticleSystem>();

        ARPointCloudMgr.ListGenerated += processARParticleList;        
    }

    /// <summary>
    /// Clears saved particles within the ASL PointCloud
    /// </summary>
    public void ClearParticles()
    {
        ASLPointCloudMgr.Clear();
    }

    /// <summary>
    /// Instantiates and sets the world origin cloud anchor at the origin point
    /// </summary>
    public void SetWorldOriginCloudAnchor()
    {
        ASLHelper.InstantiateASLObject("SimpleDemoPrefabs/WorldOriginCloudAnchorObject", Vector3.zero, Quaternion.identity, string.Empty, string.Empty, SpawnWorldOrigin);
    }

    /// <summary>
    /// Instantiates and sets a normal cloud anchor based on a Pose.  May throw NullReferenceException while cloud anchor is initializing.
    /// </summary>
    /// <param name="touchPose">Pose object containing position and rotation of touch selection on an ARPlane</param>
    public void SetCloudAnchor(Pose? touchPose)
    {

        if (touchPose == null)
        {
            Debug.LogWarning("Invalid Pose.  Pose is null.");
            return;
        }
        m_LastValidPose = touchPose;
        ASLHelper.InstantiateASLObject("SimpleDemoPrefabs/NormalCloudAnchorObject", touchPose.Value.position, touchPose.Value.rotation, string.Empty, string.Empty, SpawnNormalCloudAnchor);

    }

    /// <summary>
    /// Spawns a normal cloud anchor now that the cloud anchor visualization object has been created (red cylinder)
    /// </summary>
    /// <param name="normalCloudAnchorVisualizationObject">The game object that will represent a normal cloud anchor</param>
    public static void SpawnNormalCloudAnchor(GameObject normalCloudAnchorVisualizationObject)
    {
        if (m_This.m_LastValidPose == null)
        {
            Debug.LogWarning("Invalid Pose.  Pose is null.");
            return;
        }
        normalCloudAnchorVisualizationObject.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
        {
            //_worldOriginVisualizationObject will be parented to the cloud anchor which is the world origin, thus showing where the world origin is
            //ASLHelper.CreateARCoreCloudAnchor(m_This.m_LastValidPose, normalCloudAnchorVisualizationObject.GetComponent<ASL.ASLObject>(), null, true, false);
            ASLHelper.CreateARCoreCloudAnchor(m_This.m_LastValidPose, normalCloudAnchorVisualizationObject.GetComponent<ASL.ASLObject>(), null, true, false);
        });
    }

    /// <summary>
    /// Spawns the world origin cloud anchor after the world origin object visualizer has been created (blue cube)
    /// </summary>
    /// <param name="_worldOriginVisualizationObject">The game object that represents the world origin</param>
    private static void SpawnWorldOrigin(GameObject _worldOriginVisualizationObject)
    {
        _worldOriginVisualizationObject.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
        {            
            ASLHelper.CreateARCoreCloudAnchor(Pose.identity, _worldOriginVisualizationObject.GetComponent<ASL.ASLObject>(), null, true, true);
        });
    }

    /// <summary>
    /// Adapts ARPersistingPointCloud tuple data into arraylist to send to ASLPointCloudManager 
    /// </summary>
    /// <param name="particleList">The particle list data in tuple format for position and color</param>
    private void processARParticleList(object sender, ASLPointCloudEventArgs args)
    {
        const int BATCH_SIZE = 250;

        var particleList = args.RawParticleList;
        if (particleList != null && particleList.Count > 0)
        {
            // batching for loop for list sizes over 250 (250x4 is at the limit of ASL float constraint)
            for (int batchIndex = 0; batchIndex < particleList.Count; batchIndex += BATCH_SIZE)
            {
                int pCount = particleList.Count - batchIndex > BATCH_SIZE ? BATCH_SIZE : particleList.Count - batchIndex;
                Vector3[] positions = new Vector3[pCount];
                Color[] colors = new Color[args.UseCustomColor ? pCount : 0];

                for (int i = 0; i < pCount; i++)
                {
                    positions[i] = particleList[i + batchIndex].position;
                    if (args.UseCustomColor)
                    {
                        colors[i] = particleList[i + batchIndex].color;
                    }
                }

                if (args.UseCustomColor)
                {
                    ASLPointCloudMgr.AddParticles(positions, colors);
                }
                else
                {
                    ASLPointCloudMgr.AddParticles(positions);
                }
            }
        }
    }
}
