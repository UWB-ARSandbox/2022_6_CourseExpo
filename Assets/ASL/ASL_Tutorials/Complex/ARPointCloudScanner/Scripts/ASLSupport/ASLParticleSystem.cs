using System.Collections.Generic;
using System;
using UnityEngine;
using ASL;

/// <summary>An ASL enabled ParticleSystem.  Use AddParticles to add new synced particles to a ASL particlesystem object.
/// Currently supports only Position and Color.  Useful in PointCloud applications.</summary>
public class ASLParticleSystem : MonoBehaviour
{
    /// <summary>The ASLObject component for the ASLParticleSystem object</summary>
    private ASLObject _aslThis;

    /// <summary>The Unity ParticleSystem component for the ASLParticleSystem object</summary>
    private ParticleSystem _particleSystem;

    /// <summary>The time frequency between internal object cleanup</summary>
    private float _CleanupCollectionTimespan = 5f;

    /// <summary>The time since last internal object cleanup</summary>
    private float _CleanupCollectionLifetime = 0;

    /// <summary>The running ID counter for the internal processing data structure</summary>
    private ulong _particleID = 1;

    /// <summary>The internal processing data structure</summary>
    private Dictionary<ulong, (ASLParticleListBuilder particleList, int generation)> _particleListHoldingCollection;

    /// <summary>
    /// Startup initialization for the ASLParticleSystem
    /// </summary>
    void Start()
    {
        _particleListHoldingCollection = new Dictionary<ulong, (ASLParticleListBuilder particleList, int generation)>();

        _aslThis = gameObject.GetComponent<ASLObject>();
        _aslThis ??= gameObject.AddComponent<ASLObject>();
        _aslThis._LocallySetFloatCallback(onASLParticleFloatChanged);

        _particleSystem = gameObject.GetComponent<ParticleSystem>();
    }

    /// <summary>
    /// Update function to run scheduled internal object cleanup
    /// </summary>
    void Update()
    {
        // In case exception stops particleSystem holding list item from being completed and removed, clean via garbage sweep
        if (_CleanupCollectionLifetime > _CleanupCollectionTimespan)
        {
            _CleanupCollectionLifetime = 0f;
            cleanupParticleList();
        }
        else
        {
            _CleanupCollectionLifetime += Time.deltaTime;
        }
    }

    /// <summary>
    /// Clears saved particles for all network instances of this ASLParticleSystem object
    /// </summary>
    public void Clear()
    {
        _aslThis.SendAndSetClaim(() =>
        {
            Debug.Log("ASLPointCloudManager:Clear: Successfully claimed ParticleSystem object!");
        });

        if (_aslThis.m_Mine)
        {
            float[] clearFloatArr = new float[2];
            clearFloatArr[0] = (float)ASLParticleFloatType.Clear;
            clearFloatArr[1] = 0f;
            _aslThis.SendFloatArray(clearFloatArr);
        }
        else
        {
            Debug.LogError("ASLPointCloudManager:Clear: ParticleSystem not owned");
        }
    }

    /// <summary>
    /// Adds Particles to the network instances of the ParticleSystem 
    /// </summary>
    /// <param name="positions">The Vector3 array of positions to add to the particle system</param>
    public void AddParticles(Vector3[] positions)
    {
        AddParticles(positions, null, false);
    }

    /// <summary>
    /// Adds Particles to the network instances of the ParticleSystem 
    /// </summary>
    /// <param name="positions">The Vector3 array of positions to add to the particle system</param>
    /// <param name="colors">The Color array representing the colors of particles to add to the particle system</param>
    public void AddParticles(Vector3[] positions, Color[] colors)
    {
        AddParticles(positions, colors, true);
    }

    /// <summary>
    /// Internal helper to send raw particle float data to the gameliftmanager 
    /// </summary>
    /// <param name="positions">The Vector3 array of positions to add to the particle system</param>
    /// <param name="colors">The Color array representing the colors of particles to add to the particle system</param>
    /// <param name="useColors">true: uses color array for particle colors.  false: uses default color for particles</param>
    private void AddParticles(Vector3[] positions, Color[] colors, bool useColors)
    {
        if (useColors && positions.Length != colors.Length)
        {
            throw new ArgumentException("ASLPointCloudManager:AddParticles Error: positions and colors array length does not match");
        }
        _aslThis.SendAndSetClaim(() =>
        {
            Debug.Log("ASLPointCloudManager:AddParticles: Successfully claimed ParticleSystem object!");
        });

        if (_aslThis.m_Mine)
        {
            float[] pointsFloatArr = GameLiftManager.GetInstance().ConvertVector3ArrayToFloatArray(positions);
            Array.Resize(ref pointsFloatArr, pointsFloatArr.Length + 3);
            pointsFloatArr[pointsFloatArr.Length - 3] = useColors ? 1f : 0f;
            pointsFloatArr[pointsFloatArr.Length - 2] = (float)ASLParticleFloatType.Position;
            pointsFloatArr[pointsFloatArr.Length - 1] = _particleID;
            _aslThis.SendFloatArray(pointsFloatArr);

            if (useColors)
            {
                float[] colorsFloatArr = GameLiftManager.ConvertColorArrayToFloatArray(colors);
                Array.Resize(ref colorsFloatArr, colorsFloatArr.Length + 2);
                colorsFloatArr[colorsFloatArr.Length - 2] = (float)ASLParticleFloatType.Color;
                colorsFloatArr[colorsFloatArr.Length - 1] = _particleID;
                _aslThis.SendFloatArray(colorsFloatArr);
            }
            _particleID++;
        }
        else
        {
            Debug.LogError("ASLPointCloudManager:AddParticles Error: Could not claim ParticleSystem for float[] upload");
        }
    }

    /// <summary>
    /// Internal callback to convert raw particle float data back to typed arrays and add particles to particle system
    /// </summary>
    /// <param name="id">The id of the sending object</param>
    /// <param name="floatArr">The raw float array.  Can represent positions, colors, or clearing values</param>    
    private void onASLParticleFloatChanged(string id, float[] floatArr)
    {
        ulong particleListId = Convert.ToUInt64(floatArr[floatArr.Length - 1]);
        ASLParticleFloatType floatArrType = (ASLParticleFloatType)Convert.ToInt32(floatArr[floatArr.Length - 2]);

        // Get list object from holding list, otherwise create new 
        ASLParticleListBuilder partList = getParticleList(particleListId);

        // get type
        switch (floatArrType)
        {
            case ASLParticleFloatType.Position:
                partList.setUseCustomColor(floatArr[floatArr.Length - 3] == 1);
                // strip id/type and set
                Array.Resize(ref floatArr, floatArr.Length - 3);
                partList.SetParticlePoints(floatArr);
                break;
            case ASLParticleFloatType.Color:
                partList.setUseCustomColor(true);
                // strip id/type and set
                Array.Resize(ref floatArr, floatArr.Length - 2);
                partList.SetParticleColors(floatArr);
                break;
            case ASLParticleFloatType.Clear:
                _particleSystem.Clear();
                break;
            default:
                Debug.LogError("ASLParticleSystem:onASLParticleFloatChanged Error: unable to get particle float type:" + floatArr[floatArr.Length - 2]);
                break;
        }

        if (partList.IsComplete)
        {
            ParticleSystem.Particle[] newParticles = partList.Particles;
            // Bug in unity 2020.3.0F1 not handling any offset values.  Throws exception in ParticleSystem.SetParticles
            // Need to upgrade unity version if using the ASLParticleSystem
            _particleSystem.SetParticles(newParticles, newParticles.Length, _particleSystem.particleCount);
            Debug.Log("ASLPointCloudManager:onASLParticleFloatChanged: ParticleSystem Updated. New Size= " + _particleSystem.particleCount);
            _particleListHoldingCollection.Remove(particleListId);
        }
    }

    /// <summary>
    /// Internal helper to retrieve or set a ASLParticleListBuilder object 
    /// </summary>
    /// <param name="particleListId">The id particle list to find</param>
    /// <returns>Will find and return an ASLParticleListBuilder object if the given ID exists, otherwise will create a new object and add to the internal store.</returns>
    private ASLParticleListBuilder getParticleList(ulong particleListId)
    {
        if (_particleListHoldingCollection.ContainsKey(particleListId))
        {
            return _particleListHoldingCollection[particleListId].particleList;
        }
        else
        {
            ASLParticleListBuilder partList = new ASLParticleListBuilder();
            _particleListHoldingCollection.Add(particleListId, (partList, 0));
            return partList;
        }
    }

    /// <summary>
    /// Internal cleanup helper for the incomplete particle holding data structure.  Removes aged particle lists.
    /// </summary>
    private void cleanupParticleList()
    {
        foreach (ulong key in _particleListHoldingCollection.Keys)
        {
            var partList = _particleListHoldingCollection[key];
            partList.generation++;

            if (partList.generation > 3)
            {
                _particleListHoldingCollection.Remove(key);
            }
        }
    }
}

/// <summary>
/// Particle Float Type enumeration.  Handles Position, Color, and Clear types
/// </summary>
public enum ASLParticleFloatType
{
    Position,
    Color,
    Clear
}
