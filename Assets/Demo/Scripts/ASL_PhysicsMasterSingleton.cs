using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// ASL_PhysicsMasterSingleton: The ASL_PhysicsMasterSingelton detects and handles collision between ASL_ObjectColliders. One 
/// user is designated as the PhysicsMaster, by default this is the host. Only the PhysicsMaster detects and processes collisions.
/// </summary>
public class ASL_PhysicsMasterSingleton : MonoBehaviour
{
    private static ASL_PhysicsMasterSingleton _instance;

    /// <summary>A callback function contains a user-defined algorithm determining the physics master</summary>
    public delegate void DefinePhysicsMasterCallback();

    /// <summary>A callback function contains a user-defined algorithm determining the physics master but return the peer id</summary>
    public delegate int DefinePhysicsMasterIdCallback();

    public static ASL_PhysicsMasterSingleton Instance { get { return _instance; } }

    private bool isPhysicsMaster = false;

    /// <summary>An action for storing user-selected mode on determining physics master</summary>
    private Action functionToCallLater;

    private int physicsMasterPeerID;
    public int PhysicsMasterPeerID
    {
        get { return physicsMasterPeerID; }
    }
    public bool IsPhysicsMaster
    {
        get { return isPhysicsMaster; }
    }

    /// <summary>
    /// Used to return a single instance
    /// and set application frame rate to 40 as default.
    /// </summary>
    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 40;

        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    private void Start()
    {
        SetUpPhysicsMasterByLowestPeer();
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += SetUpPhysicsMaster;
    }

    /// <summary>
    /// Used to set physics master in the simplest way.
    /// </summary>
    /// <param name="_isPhysicsMaster">The boolean value determins if the current player should be the physics master</param>
    public void SetPhysicsMaster(bool _isPhysicsMaster)
    {
        isPhysicsMaster = _isPhysicsMaster;
    }

    private void SetUpPhysicsMaster(Scene arg0, LoadSceneMode arg1)
    {
        SetUpPhysicsMasterByLowestPeer();
    }

    /// <summary>
    /// Used to setup physics master by lowest peer.
    /// It is the default way of setting the physics master.
    /// </summary>
    public void SetUpPhysicsMaster()
    {
        SetUpPhysicsMasterByLowestPeer();
    }

    /// <summary>
    /// Used to setup physics master by lowest peer.
    /// It sets the physics master by lowest peer, and notices GameLiftManager who is the physics master,
    /// and saves the funtion to the action.
    /// </summary>
    public void SetUpPhysicsMasterByLowestPeer()
    {
        SetPhysicsMaster(ASL.GameLiftManager.GetInstance().AmLowestPeer());
        ASL.GameLiftManager.GetInstance().SetPhysicsMasterId(ASL.GameLiftManager.GetInstance().GetLowestPeerId());
        functionToCallLater = () => SetUpPhysicsMasterByLowestPeer();
        physicsMasterPeerID = ASL.GameLiftManager.GetInstance().GetLowestPeerId();
    }

    /// <summary>
    /// Used to setup physics master by highest peer.
    /// It sets the physics master by highest peer, and notices GameLiftManager who is the physics master,
    /// and saves the funtion to the action.
    /// </summary>
    public void SetUpPhysicsMasterByHighestPeer()
    {
        SetPhysicsMaster(ASL.GameLiftManager.GetInstance().AmHighestPeer());
        ASL.GameLiftManager.GetInstance().SetPhysicsMasterId(ASL.GameLiftManager.GetInstance().GetHighestPeerId());
        functionToCallLater = () => SetUpPhysicsMasterByHighestPeer();
        physicsMasterPeerID = ASL.GameLiftManager.GetInstance().GetHighestPeerId();
    }

    /// <summary>
    /// Used to setup physics master by a given custom function/algorithm.
    /// It sets the physics master by a given custom function/algorithm, 
    /// and notices GameLiftManager who is the physics master,
    /// and saves the funtion to the action with callbacks.
    /// </summary>
    /// <param name="functionCallback">A callback function is designed by the user. The function determines who is the physics master</param>
    /// <param name="idCallback">A callback function is designed by the user. The function determines the id of the physics master</param>
    public void SetUpPhysicsMasterByCustomFunction(DefinePhysicsMasterCallback functionCallback, DefinePhysicsMasterIdCallback idCallback)
    {
        functionCallback.Invoke();
        int id = idCallback.Invoke();
        ASL.GameLiftManager.GetInstance().SetPhysicsMasterId(id);
        functionToCallLater = () => SetUpPhysicsMasterByCustomFunction(functionCallback, idCallback);
    }

    /// <summary>
    /// Used to reassign a new physics master when the leaving player was the physics master.
    /// It calls previously stored function.
    /// </summary>
    public void ReassignPhysicsMaster()
    {
        functionToCallLater();
    }
}