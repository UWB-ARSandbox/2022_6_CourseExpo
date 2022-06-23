using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using ASL;

namespace ASL
{
    /// <summary>
    /// ASL_AutonomousObjectHandler: ASL_AutonomousObjectHanlder contains methods for instatiating 
    /// ASL_AutonomousObjects. It also handles changes to world position, rotation and scale for
    /// ASL_AutonomousObjects.
    /// </summary>
    public class ASL_AutonomousObjectHandler : MonoBehaviour
    {
        public delegate void ReturnInstantatedObjectCallback(GameObject instantiatedObject);

        private static ASL_AutonomousObjectHandler _instance;
        public static ASL_AutonomousObjectHandler Instance { get { return _instance; } }

        List<ASLObject> autonomousObjects = new List<ASLObject>();
        ASL_PhysicsMasterSingleton physicsMaster;

        Dictionary<string, ReturnInstantatedObjectCallback> instatiatedObjects = new Dictionary<string, ReturnInstantatedObjectCallback>();
        Dictionary<string, int> instatiatedObjectOwners = new Dictionary<string, int>();

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            physicsMaster = ASL_PhysicsMasterSingleton.Instance;
        }

        private void OnLevelWasLoaded(int level)
        {
            List<ASLObject> autonomousObjects = new List<ASLObject>();
            Dictionary<string, ReturnInstantatedObjectCallback> instatiatedObjects = new Dictionary<string, ReturnInstantatedObjectCallback>();
            Dictionary<string, int> instatiatedObjectOwners = new Dictionary<string, int>();
        }

        private void Update()
        {
            ASLObject aSLObject;
            List<string> objectsToRemove = new List<string>();
            foreach (string guid in instatiatedObjects.Keys)
            {
                ASL.ASLHelper.m_ASLObjects.TryGetValue(guid, out aSLObject);
                if (aSLObject != null)
                {
                    ASL_AutonomousObject autonomousObject = aSLObject.GetComponent<ASL_AutonomousObject>();
                    if (autonomousObject == null)
                    {
                        autonomousObject = aSLObject.gameObject.AddComponent<ASL_AutonomousObject>();
                    }
                    autonomousObject.Owner = instatiatedObjectOwners[guid];
                    //objectsAndOwners.Add(autonomousObject, instatiatedObjectOwners[guid]);
                    if (instatiatedObjects[guid] != null)
                    {
                        instatiatedObjects[guid](aSLObject.gameObject);
                    }
                    objectsToRemove.Add(guid);
                }
            }
            foreach (string guid in objectsToRemove)
            {
                instatiatedObjects.Remove(guid);
                instatiatedObjectOwners.Remove(guid);
            }
        }

        public void ReasignObjects(int playerID)
        {
            Debug.Log("PLAYER LEAVING: " + playerID);
            int physicsMasterID = ASL_PhysicsMasterSingleton.Instance.PhysicsMasterPeerID;
            foreach (ASLObject aSL_Object in autonomousObjects)
            {
                if (aSL_Object.GetComponent<ASL_AutonomousObject>().Owner == playerID)
                {
                    aSL_Object.GetComponent<ASL_AutonomousObject>().Owner = physicsMasterID;
                }
            }
        }

        /// <summary>
        /// Instatiates an ASL_AutonomousObject based on prefab. The caller of this function will be set to the owner of the instatiated ASL_AutonomousObject.
        /// The object is created using the prefab's transform.
        /// </summary>
        /// <param name="prefab">Prefab of GameObject to be instatiated. prefab MUST be located in Resources/MyPrefabs.</param>
        public void InstantiateAutonomousObject(GameObject prefab)
        {
            string guid = ASL.ASLHelper.InstantiateASLObjectReturnID(prefab.name, prefab.transform.position, prefab.transform.rotation);
            instatiatedObjects.Add(guid, null);
            instatiatedObjectOwners.Add(guid, ASL.GameLiftManager.GetInstance().m_PeerId);
        }

        /// <summary>
        /// Instatiates an ASL_AutonomousObject based on prefab. The caller of this function will be set to the owner of the instatiated ASL_AutonomousObject.
        /// The object is created using the prefab's transform. Calls callbackFunction once the object has been instantiated, with the instatiated GameObject 
        /// as the argument.
        /// </summary>
        /// <param name="prefab">Prefab of GameObject to be instatiated. prefab MUST be located in Resources/MyPrefabs.</param>
        /// <param name="callbackFunction">Callback function to be called once the object has been instatiated, with the GameObject as a parameter.</param>
        public void InstantiateAutonomousObject(GameObject prefab, ReturnInstantatedObjectCallback callbackFunction)
        {
            string guid = ASL.ASLHelper.InstantiateASLObjectReturnID(prefab.name, prefab.transform.position, prefab.transform.rotation);
            instatiatedObjects.Add(guid, callbackFunction);
            instatiatedObjectOwners.Add(guid, ASL.GameLiftManager.GetInstance().m_PeerId);
        }

        /// <summary>
        /// Instatiates an ASL_AutonomousObject based on prefab. The caller of this function will be set to the owner of the instatiated ASL_AutonomousObject.
        /// The object is created at position pos, and rotation rotation.
        /// </summary>
        /// <param name="prefab">Prefab of GameObject to be instatiated. prefab MUST be located in Resources/MyPrefabs.</param>
        /// <param name="pos">Desired initial position of the object.</param>
        /// <param name="rotation">Desired initial rotation of the object.</param>
        public void InstantiateAutonomousObject(GameObject prefab, Vector3 pos, Quaternion rotation)
        {
            string guid = ASL.ASLHelper.InstantiateASLObjectReturnID(prefab.name, pos, rotation);
            instatiatedObjects.Add(guid, null);
            instatiatedObjectOwners.Add(guid, ASL.GameLiftManager.GetInstance().m_PeerId);
        }

        /// <summary>
        /// Instatiates an ASL_AutonomousObject based on prefab. The caller of this function will be set to the owner of the instatiated ASL_AutonomousObject.
        /// The object is created at position pos, and rotation rotation. Calls callbackFunction once the object has been instantiated, with the instatiated 
        /// GameObject as the argument.
        /// </summary>
        /// <param name="prefab">Prefab of GameObject to be instatiated. prefab MUST be located in Resources/MyPrefabs.</param>
        /// <param name="pos">Desired initial position of the object.</param>
        /// <param name="rotation">Desired initial rotation of the object.</param>
        /// <param name="callbackFunction">Callback function to be called once the object has been instatiated, with the GameObject as a parameter.</param>
        public void InstantiateAutonomousObject(GameObject prefab, Vector3 pos, Quaternion rotation, ReturnInstantatedObjectCallback callbackFunction)
        {
            string guid = ASL.ASLHelper.InstantiateASLObjectReturnID(prefab.name, pos, rotation);
            instatiatedObjects.Add(guid, callbackFunction);
            instatiatedObjectOwners.Add(guid, ASL.GameLiftManager.GetInstance().m_PeerId);
        }

        /// <summary>
        /// Instatiates an ASL_AutonomousObject based on prefab. The caller of this function will be set to the owner of the instatiated ASL_AutonomousObject.
        /// The object is created at position pos, and rotation rotation. Calls callbackFunction once the object has been instantiated, with the instatiated 
        /// GameObject as the argument. The ASLGameObjectCreatedCallback is set to creationCallbackFunction, ClaimCancelledRecoveryCallback is set to 
        /// claimRecoveryFunction, and the FloatCallback is set to floatFunciton.
        /// </summary>
        /// <param name="prefab">Prefab of GameObject to be instatiated. prefab MUST be located in Resources/MyPrefabs.</param>
        /// <param name="pos">Desired initial position of the object.</param>
        /// <param name="rotation">Desired initial rotation of the object.</param>
        /// <param name="creationCallbackFunction">Callback function that will be called once the object is created.</param>
        /// <param name="claimRecoveryFunction">Callback function that will be called if a claim cannot be completed.</param>
        /// <param name="floatFunciton">The desired FloatCallback for the object.</param>
        public void InstantiateAutonomousObject(GameObject prefab, Vector3 pos, Quaternion rotation,
            ASLObject.ASLGameObjectCreatedCallback creationCallbackFunction,
            ASLObject.ClaimCancelledRecoveryCallback claimRecoveryFunction,
            ASLObject.FloatCallback floatFunciton)
        {
            string guid = ASL.ASLHelper.InstantiateASLObjectReturnID(prefab.name, pos, rotation, "", "", creationCallbackFunction, claimRecoveryFunction, floatFunciton);
            instatiatedObjects.Add(guid, null);
            instatiatedObjectOwners.Add(guid, ASL.GameLiftManager.GetInstance().m_PeerId);
        }

        /// <summary>
        /// Instatiates an ASL_AutonomousObject based on prefab. The caller of this function will be set to the owner of the instatiated ASL_AutonomousObject.
        /// The object is created at position pos, and rotation rotation. Calls callbackFunction once the object has been instantiated, with the instatiated 
        /// GameObject as the argument. The ASLGameObjectCreatedCallback is set to creationCallbackFunction, ClaimCancelledRecoveryCallback is set to 
        /// claimRecoveryFunction, and the FloatCallback is set to floatFunciton.
        /// </summary>
        /// <param name="prefab">Prefab of GameObject to be instatiated. prefab MUST be located in Resources/MyPrefabs.</param>
        /// <param name="pos">Desired initial position of the object.</param>
        /// <param name="rotation">Desired initial rotation of the object.</param>
        /// <param name="creationCallbackFunction">Callback function that will be called once the object is created.</param>
        /// <param name="claimRecoveryFunction">Callback function that will be called if a claim cannot be completed.</param>
        /// <param name="floatFunciton">The desired FloatCallback for the object.</param>
        /// <param name="callbackFunction">Callback function to be called once the object has been instatiated, with the GameObject as a parameter.</param>
        public void InstantiateAutonomousObject(GameObject prefab, Vector3 pos, Quaternion rotation,
            ASLObject.ASLGameObjectCreatedCallback creationCallbackFunction,
            ASLObject.ClaimCancelledRecoveryCallback claimRecoveryFunction,
            ASLObject.FloatCallback floatFunciton, ReturnInstantatedObjectCallback callbackFunction)
        {
            string guid = ASL.ASLHelper.InstantiateASLObjectReturnID(prefab.name, pos, rotation, "", "", creationCallbackFunction, claimRecoveryFunction, floatFunciton);
            instatiatedObjects.Add(guid, callbackFunction);
            instatiatedObjectOwners.Add(guid, ASL.GameLiftManager.GetInstance().m_PeerId);
        }


        /// <summary>
        /// Adds aSLObject to the list of objects to be handled by this object. NOTE: This list will be reset on SceneLoad.
        /// </summary>
        /// <param name="aSLObject">Object to be added</param>
        /// <returns>returns index of the object. Returns -1 if the object is already being handled or if not physicsMaster.</returns>
        public int AddAutonomousObject(ASLObject aSLObject)
        {
            if (!autonomousObjects.Contains(aSLObject))
            {
                autonomousObjects.Add(aSLObject);
                return autonomousObjects.IndexOf(aSLObject);
            }
            return -1;
        }

        /// <summary>
        /// Removes aSLObject from the list of objects handled by this object.
        /// </summary>
        /// <param name="aSLObject">Object to be removed.</param>
        public void RemoveAutonomousObject(ASLObject aSLObject)
        {
            if (!autonomousObjects.Contains(aSLObject))
            {
                autonomousObjects.Remove(aSLObject);
            }
        }

        /// <summary>
        /// If caller is the owner of the object indicated by index, rotates the object incrementally via World Rotation.
        /// </summary>
        /// <param name="index">Index of the ASL_AutonomousObject being transformed.</param>
        /// <param name="m_RotationAmount">The incremental amount that the object should be rotated.</param>
        public void IncrementWorldRotation(int index, Quaternion m_RotationAmount)
        {
            ASLObject aSLObject = checkIfOwnerOfObject(index);
            if (aSLObject != null)
            {
                aSLObject.SendAndSetClaim(() =>
                {
                    aSLObject.SendAndIncrementWorldRotation(m_RotationAmount);
                });
            }
        }
        /// <summary>
        /// If caller is the owner of the object indicated by index, rotates the object incrementally via World Rotation. Will call callback
        /// once the operation has been completed.
        /// </summary>
        /// <param name="index">Index of the ASL_AutonomousObject being transformed.</param>
        /// <param name="m_RotationAmount">The incremental amount that the object should be rotated.</param>
        /// <param name="callback">Callback function to be called once the operation has been completed.</param>
        public void IncrementWorldRotation(int index, Quaternion m_RotationAmount, ASL.GameLiftManager.OpFunctionCallback callback)
        {
            ASLObject aSLObject = checkIfOwnerOfObject(index);
            if (aSLObject != null)
            {
                aSLObject.SendAndSetClaim(() =>
                {
                    aSLObject.SendAndIncrementWorldRotation(m_RotationAmount, callback);
                });
            }
        }

        /// <summary>
        /// If caller is the owner of the object indicated by index, translates the object incrementally via World Position.
        /// </summary>
        /// <param name="index">Index of the ASL_AutonomousObject being transformed.</param>
        /// <param name="m_AdditiveMovementAmount">The change in world position.</param>
        public void IncrementWorldPosition(int index, Vector3 m_AdditiveMovementAmount)
        {
            ASLObject aSLObject = checkIfOwnerOfObject(index);
            if (aSLObject != null)
            {
                aSLObject.SendAndSetClaim(() =>
                {
                    aSLObject.SendAndIncrementWorldPosition(m_AdditiveMovementAmount);
                });
            }
        }

        /// <summary>
        /// If caller is the owner of the object indicated by index, translates the object incrementally via World Position. Will call callback
        /// once the operation has been completed.
        /// </summary>
        /// <param name="index">Index of the ASL_AutonomousObject being transformed.</param>
        /// <param name="m_AdditiveMovementAmount">The change in world position.</param>
        /// <param name="callback">Callback function to be called once the operation has been completed.</param>
        public void IncrementWorldPosition(int index, Vector3 m_AdditiveMovementAmount, ASL.GameLiftManager.OpFunctionCallback callback)
        {
            ASLObject aSLObject = checkIfOwnerOfObject(index);
            if (aSLObject != null)
            {
                aSLObject.SendAndSetClaim(() =>
                {
                    aSLObject.SendAndIncrementWorldPosition(m_AdditiveMovementAmount, callback);
                });
            }
        }

        /// <summary>
        /// If caller is the owner of the object indicated by index, scales the object incrementally via World Scale.
        /// </summary>
        /// <param name="index">Index of the ASL_AutonomousObject being transformed.</param>
        /// <param name="m_AdditiveScaleAmount">The amount that the object will be scaled by, incrementally.</param>
        public void IncrementWorldScale(int index, Vector3 m_AdditiveScaleAmount)
        {
            ASLObject aSLObject = checkIfOwnerOfObject(index);
            if (aSLObject != null)
            {
                aSLObject.SendAndSetClaim(() =>
                {
                    aSLObject.SendAndIncrementWorldScale(m_AdditiveScaleAmount);
                });
            }
        }
        /// <summary>
        /// If caller is the owner of the object indicated by index, scales the object incrementally via World Scale. Will call callback
        /// once the operation has been completed.
        /// </summary>
        /// <param name="index">Index of the ASL_AutonomousObject being transformed.</param>
        /// <param name="m_AdditiveScaleAmount">The amount that the object's scale will be increased by.</param>
        /// <param name="callback">Callback function to be called once the operation has been completed.</param>
        public void IncrementWorldScale(int index, Vector3 m_AdditiveScaleAmount, ASL.GameLiftManager.OpFunctionCallback callback)
        {
            ASLObject aSLObject = checkIfOwnerOfObject(index);
            if (aSLObject != null)
            {
                aSLObject.SendAndSetClaim(() =>
                {
                    aSLObject.SendAndIncrementWorldScale(m_AdditiveScaleAmount, callback);
                });
            }
        }

        /// <summary>
        /// If caller is the owner of the object indicated by index, the object's position will be set to WorldPosition via World Pososition.
        /// </summary>
        /// <param name="index">Index of the ASL_AutonomousObject being transformed.</param>
        /// <param name="WorldPosition">The position in World space that the object will be moved to.</param>
        public void SetWorldPosition(int index, Vector3 WorldPosition)
        {
            ASLObject aSLObject = checkIfOwnerOfObject(index);
            if (aSLObject != null)
            {
                aSLObject.SendAndSetClaim(() =>
                {
                    aSLObject.SendAndSetWorldPosition(WorldPosition);
                });
            }
        }

        /// <summary>
        /// If caller is the owner of the object indicated by index, the object's position will be set to WorldPosition via World Pososition.
        /// Calls callback once the opperation is complete.
        /// </summary>
        /// <param name="index">Index of the ASL_AutonomousObject being transformed.</param>
        /// <param name="WorldPosition">The position in World space that the object will be moved to.</param>
        /// <param name="callback">Callback function to be called once the operation has been completed.</param>
        public void SetWorldPosition(int index, Vector3 WorldPosition, ASL.GameLiftManager.OpFunctionCallback callback)
        {
            ASLObject aSLObject = checkIfOwnerOfObject(index);
            if (aSLObject != null)
            {
                aSLObject.SendAndSetClaim(() =>
                {
                    aSLObject.SendAndSetWorldPosition(WorldPosition, callback);
                });
            }
        }

        /// <summary>
        /// If the caller is the owner of the object indeicated by index, the object's roatation will be set to WorldRotation via World Rotation.
        /// </summary>
        /// <param name="index">Index of the ASL_AutonomousObject being transformed.</param>
        /// <param name="WorldRotation">The desired roataion of the object.</param>
        public void SetWorldRotation(int index, Quaternion WorldRotation)
        {
            ASLObject aSLObject = checkIfOwnerOfObject(index);
            if (aSLObject != null)
            {
                aSLObject.SendAndSetClaim(() =>
                {
                    aSLObject.SendAndSetWorldRotation(WorldRotation);
                });
            }
        }

        /// <summary>
        /// If the caller is the owner of the object indeicated by index, the object's roatation will be set to WorldRotation via World Rotation.
        /// Calls callback once the opperation is complete.
        /// </summary>
        /// <param name="index">Index of the ASL_AutonomousObject being transformed.</param>
        /// <param name="WorldRotation">The desired roataion of the object.</param>
        /// <param name="callback">Callback function to be called once the operation has been completed.</param>
        public void SetWorldRotation(int index, Quaternion WorldRotation, ASL.GameLiftManager.OpFunctionCallback callback)
        {
            ASLObject aSLObject = checkIfOwnerOfObject(index);
            if (aSLObject != null)
            {
                aSLObject.SendAndSetClaim(() =>
                {
                    aSLObject.SendAndSetWorldRotation(WorldRotation, callback);
                });
            }
        }

        /// <summary>
        /// If the caller is the owner of the object indeicated by index, the object's scale will be set to WorldScale via World Scale.
        /// </summary>
        /// <param name="index">Index of the ASL_AutonomousObject being transformed.</param>
        /// <param name="WorldScale">The desired scale of the object.</param>
        public void SetWorldScale(int index, Vector3 WorldScale)
        {
            if (physicsMaster.IsPhysicsMaster && index < autonomousObjects.Count)
            {
                ASLObject aSLObject = autonomousObjects[index];
                aSLObject.SendAndSetClaim(() =>
                {
                    aSLObject.SendAndSetWorldScale(WorldScale);
                });
            }
        }
        /// <summary>
        /// If the caller is the owner of the object indeicated by index, the object's scale will be set to WorldScale via World Scale.
        /// Calls callback once the opperation is complete.
        /// </summary>
        /// <param name="index">Index of the ASL_AutonomousObject being transformed.</param>
        /// <param name="WorldScale">The desired scale of the object.</param>
        /// <param name="callback">Callback function to be called once the operation has been completed.</param>
        public void SetWorldScale(int index, Vector3 WorldScale, ASL.GameLiftManager.OpFunctionCallback callback)
        {
            if (physicsMaster.IsPhysicsMaster && index < autonomousObjects.Count)
            {
                ASLObject aSLObject = autonomousObjects[index];
                aSLObject.SendAndSetClaim(() =>
                {
                    aSLObject.SendAndSetWorldScale(WorldScale, callback);
                });
            }
        }

        ASLObject checkIfOwnerOfObject(int index)
        {
            if (index < autonomousObjects.Count)
            {
                ASLObject aSLObject = autonomousObjects[index];
                if (aSLObject != null && ASL.GameLiftManager.GetInstance().m_PeerId == aSLObject.GetComponent<ASL_AutonomousObject>().Owner)
                {
                    return aSLObject;
                }
            }
            return null;
        }
    }

}