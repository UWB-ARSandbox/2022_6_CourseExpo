using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

namespace SimpleDemos
{
    /// <summary> 
    /// A demo showcasing how to use the SendARPlaneAsMesh functionality with AR Plane surface detection to create mesh surfaces 
    /// or platforms of the same vertices.
    /// 
    /// This demo allows AR users to tap on scanned AR Planes to instantiate a mesh of the same vertices, position, and rotation.
    /// 
    /// Users can toggle the visibility of AR Planes for visual clarity.
    /// 
    /// Video Example: https://drive.google.com/file/d/1znVxR65LzHuOXVHKnzwtykXlUxd1cwqT/view?usp=sharing
    /// </summary>
    public class ARMeshFromPlane_Example : MonoBehaviour
    {
        /// <summary>Text that displays scene information to the user</summary>
        public Text m_DisplayInformation;

        /// <summary>Toggle that sets if AR Planes should be shown</summary>
        public Toggle m_ShowARPlanesToggle;

        /// <summary>Vector2 position of last touch</summary>
        private Vector2 m_TouchPosition;

        /// <summary>Vector3 array of the currently selected mesh vertices</summary>
        private static Vector3[] m_ARPlaneVertices;

        /// <summary>
        /// Called on start up - creates the world origin at the origin and adds a listener for the AR Planes Toggle. For simplicity, this tutorial 
        /// creates the origin at the exact location the AR user starts at.
        /// For a better representation of accurate world origin creation, check out the ARWorldOrigin Example.
        /// </summary>
        void Start()
        {
            if (ASL.GameLiftManager.GetInstance().AmLowestPeer())
            {
                ASL.ASLHelper.InstantiateASLObject("SimpleDemoPrefabs/WorldOriginCloudAnchorObject", Vector3.zero, Quaternion.identity, string.Empty, string.Empty, SpawnWorldOrigin);
            }

            m_ShowARPlanesToggle = GameObject.Find("ShowARPlanesToggle").GetComponent<Toggle>();
        }

        /// <summary>
        /// Update function - gets touch and creates and sends meshes from AR Planes
        /// </summary>
        void Update()
        {
            // Show/hide AR Planes - planes generate non-stop, so this must be in Update
            ARPlaneMeshVisualizer[] planes = (ARPlaneMeshVisualizer[])GameObject.FindObjectsOfType(typeof(ARPlaneMeshVisualizer));
            foreach (ARPlaneMeshVisualizer plane in planes)
            {
                if (m_ShowARPlanesToggle.isOn)
                {
                    plane.enabled = true;
                    plane.gameObject.layer = 0;
                }
                else
                {
                    plane.enabled = false;
                    plane.gameObject.layer = 2;
                }
            }

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                //Get touch data from mobile or PC
                Touch touch = Input.GetTouch(0);
                m_TouchPosition = touch.position;
                int pointerID = touch.fingerId;

                //Check if touch on UI
                if (EventSystem.current.IsPointerOverGameObject(pointerID))
                {
                    return;
                }

                //Raycast on touch position
                Ray ray = Camera.main.ScreenPointToRay(m_TouchPosition);
                RaycastHit hitObject;

                //Check for raycast hit
                if (Physics.Raycast(ray, out hitObject))
                {
                    if (hitObject.collider != null)
                    {
                        m_DisplayInformation.text = "Hit: " + hitObject.collider.gameObject.name + " (ID: " + hitObject.collider.gameObject.GetInstanceID() + ")";

                        if(hitObject.collider.gameObject.name.Contains("ARPlane"))
                        {
                            //Save vertices to be sent after
                            m_ARPlaneVertices = hitObject.collider.GetComponent<MeshCollider>().sharedMesh.vertices;

                            //Create new platform object, set position and rotation
                            ASL.ASLHelper.InstantiateASLObject("SimpleDemoPrefabs/PlatformPlane", hitObject.collider.transform.position, 
                                hitObject.collider.transform.rotation, string.Empty, string.Empty, SpawnPlatform);
                        }
                    }
                    else
                    {
                        m_DisplayInformation.text = "No Hit";
                    }
                }
            }
        }

        /// <summary>
        /// Sends the vertices of the AR Plane to the new mesh once created
        /// </summary>
        /// <param name="_platform">The game object that represents the newly created platform</param>
        private static void SpawnPlatform(GameObject _platform)
        {
            _platform.GetComponent<ASL.ASLObject>().SendARPlaneAsMesh(m_ARPlaneVertices);
        }

        /// <summary>
        /// Spawns the world origin cloud anchor after the world origin object visualizer has been created (blue cube)
        /// </summary>
        /// <param name="_worldOriginVisualizationObject">The game object that represents the world origin</param>
        private static void SpawnWorldOrigin(GameObject _worldOriginVisualizationObject)
        {
            _worldOriginVisualizationObject.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
                ASL.ASLHelper.CreateARCoreCloudAnchor(Pose.identity, _worldOriginVisualizationObject.GetComponent<ASL.ASLObject>(), _waitForAllUsersToResolve: false);
            });
        }
    }
}
