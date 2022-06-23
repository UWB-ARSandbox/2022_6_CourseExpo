using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

namespace SimpleDemos
{
    /// <summary> 
    /// A demo showcasing ASL's sync of object positions in AR according to a specified world origin across various devices/platforms, including PC.
    /// Reference points are included in the form of a cube and sphere on the plane to verify that created objects are synced.
    /// 
    /// This demo allows both PC and AR users to create objects on a pre-existing plane. Information is displayed to the user via text
    /// and informs the user if they hit an object.
    /// 
    /// Video Example: https://drive.google.com/file/d/1-YOBeCBxYKXLMQmhYFZ2ictYxwm7EnCc/view?usp=sharing
    /// </summary>
    public class ARObjectSync_Example : MonoBehaviour
    {
        /// <summary>Text that displays scene information to the user</summary>
        public Text m_DisplayInformation;

        /// <summary>Camera for the PC User</summary>
        public Camera m_PCCamera;

        /// <summary>Vector2 position of last touch</summary>
        private Vector2 m_TouchPosition;

        /// <summary>Determines if user is on PC</summary>
        private static bool IsPC
        {
            get
            {
                return (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor);
            }
        }

        /// <summary>
        /// Called on start up - creates the world origin at the origin. For simplicity, this demo creates the origin at the exact location the 
        /// AR user starts at, which may cause issues if the AR user starts in one location and then moves to another area and begins creating 
        /// objects - the objects may be far from the PC user in this case.
        /// For a better representation of accurate world origin creation, check out the ARWorldOrigin demo.
        /// </summary>
        void Start()
        {
            if (ASL.GameLiftManager.GetInstance().AmLowestPeer())
            {
                ASL.ASLHelper.InstantiateASLObject("SimpleDemoPrefabs/WorldOriginCloudAnchorObject", Vector3.zero, Quaternion.identity, string.Empty, string.Empty, SpawnWorldOrigin);
            }

            if (IsPC)
            {
                //If on PC, set normal Camera as Main Camera tag, remove AR Camera
                m_PCCamera.gameObject.tag = "MainCamera";
                Destroy(GameObject.Find("AR Camera"));
            }
        }

        /// <summary>
        /// Update function - gets touch and creates objects
        /// </summary>
        void Update()
        {
            if(!IsPC)
            {
                // Show/hide AR Planes - planes generate non-stop, so this must be in Update
                ARPlaneMeshVisualizer[] planes = (ARPlaneMeshVisualizer[])GameObject.FindObjectsOfType(typeof(ARPlaneMeshVisualizer));
                foreach (ARPlaneMeshVisualizer plane in planes)
                {
                    plane.enabled = false;
                    plane.gameObject.layer = 2;
                }
            }

            if ((Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) || (IsPC && Input.GetMouseButtonDown(0)))
            {
                //Get touch data from mobile or PC
                Touch touch = new Touch();
                int pointerID;
                if (IsPC)
                {
                    m_TouchPosition = Input.mousePosition;
                    pointerID = -1;
                }
                else
                {
                    touch = Input.GetTouch(0);
                    m_TouchPosition = touch.position;
                    pointerID = touch.fingerId;
                }

                //Check if touch on UI
                if (EventSystem.current.IsPointerOverGameObject(pointerID))
                {
                    m_DisplayInformation.text = "UI Hit";
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

                        //Create sphere at mouse position
                        ASL.ASLHelper.InstantiateASLObject(PrimitiveType.Sphere, hitObject.point, Quaternion.identity);
                    }
                }
                else
                {
                    m_DisplayInformation.text = "No Hit";
                }
            }
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
