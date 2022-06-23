using UnityEngine;

namespace ASL
{
    /// <summary>
    /// Deactivates the AR Camera if the application is running on PC (not running on Android).
    /// </summary>
    public class DeactivateARCameraOnPC : MonoBehaviour
    {
        /// <summary>
        /// Called on start
        /// </summary>
        private void Awake()
        {
            //If the application is not running on Android, set game object to inactive
            if (Application.platform != RuntimePlatform.Android)
            {
                gameObject.SetActive(false);
            }
        }
    }
}