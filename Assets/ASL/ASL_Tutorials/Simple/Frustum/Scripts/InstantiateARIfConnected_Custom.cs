using System;
using System.Collections;
using System.Collections.Generic;
using ASL;
using UnityEngine;
using UnityEngine.UI;
using UnityEngineInternal;
public class InstantiateARIfConnected_Custom : MonoBehaviour
{
    /// <summary>
    /// Called on start
    /// </summary>
    private void Awake()
    {
        //This is the "Custom" part - prevent errors on PC
        if (!Application.isMobilePlatform)
        {
            return;
        }
        
        //If the user is connected, then spawn AR camera objects
        if (FindObjectOfType<GameLiftManager>() != null && ASL.GameLiftManager.GetInstance() != null && ASL.GameLiftManager.GetInstance().m_Client.ConnectedAndReady)
        {
            Instantiate(Resources.Load("ASL_Prefabs/ARFoundationPrefabs/ARHolder"), Vector3.zero, Quaternion.identity);
            Destroy(gameObject); //No longer need - delete to clean up resources
        }

    }
}