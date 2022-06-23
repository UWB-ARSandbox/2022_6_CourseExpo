using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;


namespace SimpleDemos
{
    /// <summary>
    /// An example of how to use create an ASL_AutonomousObject. Press 'Space Bar' to instantiate an ASL_AutomousObject.
    /// In order to instatiate an ASL_AutonomousObject, you must pass a reference to the object's prefab, as a GameObject
    /// to ASL_AutonomousObjectHandler.Instance.InstatiateAutonomousObject(). The prefab MUST be loacated in Resources.MyPrefabs
    /// </summary>
    public class CreateAutonomousObject_Example : MonoBehaviour
    {
        /// <summary>
        /// Reference to the Prefab of the ASL_AutonomousObject.
        /// </summary>
        public GameObject CoinPrefab;

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                ASL_AutonomousObjectHandler.Instance.InstantiateAutonomousObject(CoinPrefab);
            }
        }
    }
}
