using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASL
{
    /// <summary>
    /// ASL_UserObject: ASL_UserObject handles movement, rotation and scale for user controlled objects. 
    /// If a call is made to transform this object before the current transform is complete, the new call 
    /// will be ignored. Currently this is set up to work with Platformer_Player's floatFunction.
    /// This makes use of the float function provided by ASL to set the owner of the object, if the float
    /// function is overriden it will not work as intended.
    /// </summary>
    public class ASL_UserObject : MonoBehaviour
    {
        bool translateReady = true;
        bool rotateReady = true;
        bool scaleReady = true;
        int ownerID;
        ASLObject m_ASLObject;

        // Start is called before the first frame update
        void Start()
        {
            m_ASLObject = GetComponent<ASLObject>();
            Debug.Assert(m_ASLObject != null);
            m_ASLObject._LocallySetFloatCallback(SetOwner);
        }

        /// <summary>
        /// Returns true if the owner of this object is peerID.
        /// </summary>
        /// <param name="peerID">The owner of this object.</param>
        public bool IsOwner(int peerID)
        {
            return peerID == ownerID;
        }

        /// <summary>
        /// Increments the objects position via World Space.
        /// </summary>
        /// <param name="m_AdditiveMovementAmount">The change in position.</param>
        public void IncrementWorldPosition(Vector3 m_AdditiveMovementAmount)
        {
            if (translateReady && ownerID == ASL.GameLiftManager.GetInstance().m_PeerId)
            {
                m_ASLObject.SendAndSetClaim(() =>
                {
                    translateReady = false;
                    m_ASLObject.SendAndIncrementWorldPosition(m_AdditiveMovementAmount, translateComplete);
                });
            }
        }

        /// <summary>
        /// Increments the objects rotation via World Rotation.
        /// </summary>
        /// <param name="m_RotationAmount">The change in rotation.</param>
        public void IncrementWorldRotation(Quaternion m_RotationAmount)
        {
            if (rotateReady && ownerID == ASL.GameLiftManager.GetInstance().m_PeerId)
            {
                m_ASLObject.SendAndSetClaim(() =>
                {
                    rotateReady = false;
                    m_ASLObject.SendAndIncrementWorldRotation(m_RotationAmount, rotateComplete);
                });
            }
        }

        /// <summary>
        /// Increments the objects scale via World Scale.
        /// </summary>
        /// <param name="m_AdditiveScaleAmount">The change in scale.</param>
        public void IncrementWorldScale(Vector3 m_AdditiveScaleAmount)
        {
            if (scaleReady && ownerID == ASL.GameLiftManager.GetInstance().m_PeerId)
            {
                m_ASLObject.SendAndSetClaim(() =>
                {
                    scaleReady = false;
                    m_ASLObject.SendAndIncrementWorldScale(m_AdditiveScaleAmount, scaleComplete);
                });
            }
        }

        /// <summary>
        /// Sets position of the object via World Position.
        /// </summary>
        /// <param name="worldPosition">The position the object is to be moved to.</param>
        public void SetWorldPosition(Vector3 worldPosition)
        {
            if (translateReady && ownerID == ASL.GameLiftManager.GetInstance().m_PeerId)
            {
                m_ASLObject.SendAndSetClaim(() =>
                {
                    translateReady = false;
                    m_ASLObject.SendAndSetWorldPosition(worldPosition, translateComplete);
                });
            }
        }

        /// <summary>
        /// Sets the rotation of the object via World Rotation.
        /// </summary>
        /// <param name="worldRotation">The desired rotation of the object.</param>
        public void SetWorldRotation(Quaternion worldRotation)
        {
            if (rotateReady && ownerID == ASL.GameLiftManager.GetInstance().m_PeerId)
            {
                m_ASLObject.SendAndSetClaim(() =>
                {
                    rotateReady = false;
                    m_ASLObject.SendAndSetWorldRotation(worldRotation, rotateComplete);
                });
            }
        }

        /// <summary>
        /// Sets the scale of the object via World Scale.
        /// </summary>
        /// <param name="worldScale">The desired scale of the object.</param>
        public void SetWorldScale(Vector3 worldScale)
        {
            if (scaleReady && ownerID == ASL.GameLiftManager.GetInstance().m_PeerId)
            {
                m_ASLObject.SendAndSetClaim(() =>
                {
                    scaleReady = false;
                    m_ASLObject.SendAndSetWorldScale(worldScale, scaleComplete);
                });
            }
        }

        void translateComplete(GameObject obj)
        {
            translateReady = true;
        }

        void rotateComplete(GameObject obj)
        {
            rotateReady = true;
        }

        void scaleComplete(GameObject obj)
        {
            scaleReady = true;
        }

        /// <summary>
        /// Sets the owner of this object equal to peerID.
        /// </summary>
        /// <param name="_f">If _f[0] is equal to 1 then the owner is set to _f[1].
        /// If _f[0] is equal to 2, then the floatFunction for Platformer_Player is called instead.</param>
        public void SetOwner(string _id, float[] _f)
        {
            Debug.Log("userObject float function");
            if (_f[0] == 1)
            {
                ownerID = (int)_f[1];
            }
            else if (_f[0] == 2)
            {
                GetComponent<Platformer_Player>().floatFunction(_id, _f);
            }
        }
    }
}
