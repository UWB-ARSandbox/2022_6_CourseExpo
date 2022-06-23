using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASL
{
    /// <summary>
    /// ASL_AutonomousObject: ASL_AutonomousObject contains methods for transforming autonomous objects, objects
    /// not directly controlled by a user. Each object is given an owner, and will only listen to instructions
    /// from its owner. While the object is waiting for a tranformation to be completed, any calls to change the 
    /// transformation will be ignored. If the owner of an object leaves a session, their objects will no longer 
    /// respond to calls (this is temperary). This makes use of the float function provided by ASL to set the 
    /// owner of the object, if the float function is overriden it will not work as intended.
    /// </summary>
    public class ASL_AutonomousObject : MonoBehaviour
    {
        int owner = -1;
        /// <summary>
        /// Sets the owner and syncs it across all users.
        /// </summary>
        public int Owner
        {
            get { return owner; }
            set
            {
                m_ASLObject.SendAndSetClaim(() =>
                {
                    m_ASLObject.SendFloatArray(new float[1] { (float)value });
                });
                translateReady = true;
                rotateReady = true;
                scaleReady = true;
            }
        }

        bool translateReady = true;
        bool rotateReady = true;
        bool scaleReady = true;

        bool setToDestroy = false;

        int autonomousObjectIndex;
        ASLObject m_ASLObject;

        private void Start()
        {
            m_ASLObject = GetComponent<ASLObject>();
            Debug.Assert(m_ASLObject != null);
            m_ASLObject._LocallySetFloatCallback(floatFunction);
            autonomousObjectIndex = ASL_AutonomousObjectHandler.Instance.AddAutonomousObject(m_ASLObject);

            if (owner == -1)
            {
                owner = ASL_PhysicsMasterSingleton.Instance.PhysicsMasterPeerID;
            }
        }

        public void DestroyAutonousOject()
        {
            if (translateReady && rotateReady && scaleReady)
            {
                destroyAutonomousObject(null);
            }
            else setToDestroy = true;
        }

        /// <summary>
        /// Increments the objects position via World Space.
        /// </summary>
        /// <param name="m_AdditiveMovementAmount">The change in position.</param>
        public void AutonomousIncrementWorldPosition(Vector3 m_AdditiveMovementAmount)
        {
            if (Time.timeSinceLevelLoad > 0.1)
            {
                if (translateReady && owner == ASL.GameLiftManager.GetInstance().m_PeerId && !setToDestroy)
                {
                    translateReady = false;
                    ASL_AutonomousObjectHandler.Instance.IncrementWorldPosition(autonomousObjectIndex, m_AdditiveMovementAmount, translateComplete);
                }
                //else if (owner == ASL.GameLiftManager.GetInstance().m_PeerId)
                //{
                //    nextTranslate += m_AdditiveMovementAmount;
                //}
            }
        }

        /// <summary>
        /// Increments the objects rotation via World Rotation.
        /// </summary>
        /// <param name="m_RotationAmount">The change in rotation.</param>
        public void AutonomousIncrementWorldRotation(Quaternion m_RotationAmount)
        {
            if (Time.timeSinceLevelLoad > 0.1)
            {
                if (rotateReady && owner == ASL.GameLiftManager.GetInstance().m_PeerId && !setToDestroy)
                {
                    rotateReady = false;
                    ASL_AutonomousObjectHandler.Instance.IncrementWorldRotation(autonomousObjectIndex, m_RotationAmount, rotateComplete);
                }
                //else if (owner == ASL.GameLiftManager.GetInstance().m_PeerId)
                //{
                //    nextRotate.eulerAngles += m_RotationAmount.eulerAngles;
                //}
            }
        }

        /// <summary>
        /// Increments the objects scale via World Scale.
        /// </summary>
        /// <param name="m_AdditiveScaleAmount">The change in scale.</param>
        public void AutonomousIncrementWorldScale(Vector3 m_AdditiveScaleAmount)
        {
            if (Time.timeSinceLevelLoad > 0.1)
            {
                if (scaleReady && owner == ASL.GameLiftManager.GetInstance().m_PeerId && !setToDestroy)
                {
                    scaleReady = false;
                    ASL_AutonomousObjectHandler.Instance.IncrementWorldScale(autonomousObjectIndex, m_AdditiveScaleAmount, scaleComplete);
                }
                //else if (owner == ASL.GameLiftManager.GetInstance().m_PeerId)
                //{
                //    nextScale += m_AdditiveScaleAmount;
                //}
            }
        }

        /// <summary>
        /// Sets position of the object via World Position.
        /// </summary>
        /// <param name="worldPosition">The position the object is to be moved to.</param>
        public void AutonomousSetWorldPosition(Vector3 worldPosition)
        {
            if (owner == ASL.GameLiftManager.GetInstance().m_PeerId)
            {
                translateReady = false;
                ASL_AutonomousObjectHandler.Instance.SetWorldPosition(autonomousObjectIndex, worldPosition, translateComplete);
            }
        }

        /// <summary>
        /// Sets the rotation of the object via World Rotation.
        /// </summary>
        /// <param name="worldRotation">The desired rotation of the object.</param>
        public void AutonomousSetWorldRotation(Quaternion worldRotation)
        {
            if (owner == ASL.GameLiftManager.GetInstance().m_PeerId)
            {
                translateReady = false;
                ASL_AutonomousObjectHandler.Instance.SetWorldRotation(autonomousObjectIndex, worldRotation, translateComplete);
            }
        }

        /// <summary>
        /// Sets the scale of the object via World Scale.
        /// </summary>
        /// <param name="worldScale">The desired scale of the object.</param>
        public void AutonomousSetWorldScale(Vector3 worldScale)
        {
            if (owner == ASL.GameLiftManager.GetInstance().m_PeerId)
            {
                translateReady = false;
                ASL_AutonomousObjectHandler.Instance.SetWorldScale(autonomousObjectIndex, worldScale, translateComplete);
            }
        }

        void translateComplete(GameObject obj)
        {
            translateReady = true;
            if (setToDestroy && rotateReady && scaleReady)
            {
                destroyAutonomousObject(null);
            }
            //if (nextTranslate != Vector3.zero)
            //{
            //    ASL_AutonomousObjectHandler.Instance.IncrementWorldPosition(autonomousObjectIndex, nextTranslate, translateComplete);
            //    nextTranslate = Vector3.zero;
            //}
            //else
            //{
            //    translateReady = true;
            //}
        }

        void rotateComplete(GameObject obj)
        {
            rotateReady = true;
            if (setToDestroy && translateReady && scaleReady)
            {
                destroyAutonomousObject(null);
            }
            //if (nextRotate != Quaternion.identity)
            //{
            //    ASL_AutonomousObjectHandler.Instance.IncrementWorldRotation(autonomousObjectIndex, nextRotate, rotateComplete);
            //    nextRotate = Quaternion.identity;
            //}
            //else
            //{
            //    rotateReady = true;
            //}
        }

        void scaleComplete(GameObject obj)
        {
            scaleReady = true;
            if (setToDestroy && translateReady && rotateReady)
            {
                destroyAutonomousObject(null);
            }
            //if (nextScale != Vector3.one)
            //{
            //    ASL_AutonomousObjectHandler.Instance.IncrementWorldScale(autonomousObjectIndex, nextScale, scaleComplete);
            //    nextScale = Vector3.one;
            //}
            //else
            //{
            //    scaleReady = true;
            //}
        }

        void destroyAutonomousObject(GameObject ogj)
        {
            m_ASLObject.SendAndSetClaim(() =>
            {
                m_ASLObject.DeleteObject();
            });
        }

        /// <summary>
        /// Sets the autonomousObjectIndex equal to index.
        /// </summary>
        /// <param name="index">The index of this object in the ASL_AutonomousObjectHandler</param>
        public void SetAutonomousObjectIndex(int index)
        {
            autonomousObjectIndex = index;
        }

        void floatFunction(string _id, float[] _f)
        {
            owner = (int)_f[0];
        }
    }
}
