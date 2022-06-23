using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;
using UnityEngine.XR;
public class UserObject : MonoBehaviour {
    public int ownerID;

    ASLObject m_ASLObject;

    private Camera playerCamera;
    private AudioListener playerListener;

    public Renderer cube;

    // Start is called before the first frame update
    void Start() {
        m_ASLObject = GetComponent<ASLObject>();
        Debug.Assert(m_ASLObject != null);
        m_ASLObject._LocallySetFloatCallback(floatFunction);
    }

    public bool IsOwner(int peerID) {
        return peerID == ownerID;
    }

    public void IncrementWorldPosition(Vector3 m_AdditiveMovementAmount) {
        if (ownerID == ASL.GameLiftManager.GetInstance().m_PeerId) {
            m_ASLObject.SendAndSetClaim(() => {
                m_ASLObject.SendAndIncrementWorldPosition(m_AdditiveMovementAmount);
            });
        }
    }

    public void IncrementWorldRotation(Quaternion m_RotationAmount) {
        if (ownerID == ASL.GameLiftManager.GetInstance().m_PeerId) {
            m_ASLObject.SendAndSetClaim(() => {
                m_ASLObject.SendAndIncrementWorldRotation(m_RotationAmount);
            });
        }
    }

    public void IncrementWorldScale(Vector3 m_AdditiveScaleAmount) {
        if (ownerID == ASL.GameLiftManager.GetInstance().m_PeerId) {
            m_ASLObject.SendAndSetClaim(() => {
                m_ASLObject.SendAndIncrementWorldScale(m_AdditiveScaleAmount);
            });
        }
    }

    public void SetWorldPosition(Vector3 worldPosition) {
        if (ownerID == ASL.GameLiftManager.GetInstance().m_PeerId) {
            m_ASLObject.SendAndSetClaim(() => {
                m_ASLObject.SendAndSetWorldPosition(worldPosition);
            });
        }
    }

    public void SetWorldRotation(Quaternion worldRotation) {
        if (ownerID == ASL.GameLiftManager.GetInstance().m_PeerId) {
            m_ASLObject.SendAndSetClaim(() => {
                m_ASLObject.SendAndSetWorldRotation(worldRotation);
            });
        }
    }

    public void SetWorldScale(Vector3 worldScale) {
        if (ownerID == ASL.GameLiftManager.GetInstance().m_PeerId) {
            m_ASLObject.SendAndSetClaim(() => {
                m_ASLObject.SendAndSetWorldScale(worldScale);
            });
        }
    }

    public void floatFunction(string _id, float[] _f) {
        Debug.Log("userObject float function");
        if (_f[0] == 1) {
            ownerID = (int)_f[1];
            Debug.Log("-----");
            Debug.Log("myID: " + ASL.GameLiftManager.GetInstance().m_PeerId);
            Debug.Log("ownerID: " + ownerID);
            if (IsOwner(ASL.GameLiftManager.GetInstance().m_PeerId)) {
                playerCamera = m_ASLObject.gameObject.GetComponentInChildren<Camera>();
                playerListener = m_ASLObject.gameObject.GetComponentInChildren<AudioListener>();
                playerCamera.enabled = true;
                playerListener.enabled = true;
                m_ASLObject.gameObject.GetComponent<PlayerController>().enabled = true;
                m_ASLObject.gameObject.GetComponent<CharacterController>().enabled = true;
            } else {
                gameObject.SetActive(false);
            }
        }
        if (_f[0] == 130) {
            //Set Host Color
            cube.material.color = Color.yellow;
        }
    }
}
