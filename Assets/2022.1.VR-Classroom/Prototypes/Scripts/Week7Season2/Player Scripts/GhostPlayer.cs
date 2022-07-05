using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ASL;

public class GhostPlayer : MonoBehaviour
{
    public int ownerID;

    ASLObject m_ASLObject;

    public Text minimapUsername;
    public Text worldspaceUsername;

    public Transform[] movingBodyPartTransforms;
    public Renderer body;

    public Texture2D curFaceTexture; // initialized to the basic clipart face in the Unity editor

    // Start is called before the first frame update
    void Start()
    {
        m_ASLObject = GetComponent<ASLObject>();
        Debug.Assert(m_ASLObject != null);
        m_ASLObject._LocallySetFloatCallback(FloatReceive);
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
        m_ASLObject.SendAndSetClaim(() => {
            m_ASLObject.SendAndSetWorldPosition(worldPosition);
        });
    }

    public void SetWorldRotation(Quaternion worldRotation) {
        m_ASLObject.SendAndSetClaim(() => {
            m_ASLObject.SendAndSetWorldRotation(worldRotation);
        });
    }

    public void SetWorldScale(Vector3 worldScale) {
        if (ownerID == ASL.GameLiftManager.GetInstance().m_PeerId) {
            m_ASLObject.SendAndSetClaim(() => {
                m_ASLObject.SendAndSetWorldScale(worldScale);
            });
        }
    }

    public void SendHostColor() {
        float[] sendColor = new float[1] { 103 };
        m_ASLObject.SendAndSetClaim(() => {
            m_ASLObject.SendFloatArray(sendColor);
        });
    }

    public void SendUserColor(Color color)
    {
        float[] sendColor = new float[4] { 104, color.r, color.g, color.b };
        m_ASLObject.SendAndSetClaim(() => {
            m_ASLObject.SendFloatArray(sendColor);
        });
    }

    public void SetFaceTexture()
    {
        var extensions = new[] {
            new SFB.ExtensionFilter("Image Files", "png", "jpg"),
        };
        SFB.StandaloneFileBrowser.OpenFilePanelAsync("Open Image:", Application.dataPath, extensions, false, SendAndChangeTexture);
    }

    public void SendAndChangeTexture(string[] strs) {
        if (strs.Length < 1) {
            return;
        }
        Texture2D textureToSend = null;
        byte[] fileData;

        if (System.IO.File.Exists(strs[0]))
        {
            fileData = System.IO.File.ReadAllBytes(strs[0]);
            textureToSend = new Texture2D(1024, 1024);
            textureToSend.LoadImage(fileData); 
        }

        // Save these textures for future use (such as in avatar preview)
        curFaceTexture = textureToSend;

        m_ASLObject.SendAndSetTexture2D(textureToSend, ChangeSpriteTexture, true);
    }

    static public void ChangeSpriteTexture(GameObject _myGameObject, Texture2D _myTexture2D)
    {
        _myGameObject.transform.Find("Head/HeadRotationPoint/Face/Canvas/FaceImage").GetComponent<RawImage>().texture = _myTexture2D;
    }

    public void FloatReceive(string _id, float[] _f) {
        int code = (int)_f[0];
        switch(code) {
            case 99:
                SetOwner(_id, _f);
                break;
            case 100:
                string username = "";
                for (int i = 1; i < _f.Length; i++) {
                    username += (char)(int)_f[i];
                }
                Debug.Log("FloatReceive, Converted Username===============>" + username);
                minimapUsername.text = username;
                worldspaceUsername.text = username;
                worldspaceUsername.transform.parent.name = username;
                gameObject.name = username;
                break;
            case 101:
                Vector3[] newBodyPositions = new Vector3[movingBodyPartTransforms.Length];
                Vector3[] newBodyRotations = new Vector3[movingBodyPartTransforms.Length];
                for (int i = 0; i < movingBodyPartTransforms.Length; i++) {
                    newBodyPositions[i].x = _f[6 * i + 1];
                    newBodyPositions[i].y = _f[6 * i + 2];
                    newBodyPositions[i].z = _f[6 * i + 3];
                    newBodyRotations[i].x = _f[6 * i + 4];
                    newBodyRotations[i].y = _f[6 * i + 5];
                    newBodyRotations[i].z = _f[6 * i + 6];
                }
                for (int i = 0; i < movingBodyPartTransforms.Length; i++) {
                    movingBodyPartTransforms[i].position = newBodyPositions[i];
                    movingBodyPartTransforms[i].rotation = Quaternion.Euler(newBodyRotations[i]);
                }
                break;
            case 102:
                _f[0] = 0;
                GetComponent<SendAndPlayAudio>().AudioReceive(_f);
                break;
            case 103:
                //Set Host Color
                body.material.color = Color.yellow;
                break;
            case 104:
                // Set new user color
                body.material.color = new Color(_f[1], _f[2], _f[3]);
                break;
        }
    }

    public void SendPlayerName(string _playername) {
        Debug.Log("Player name: " + _playername);
        float[] usernameAsAFloatArray = new float[_playername.Length + 1];
        usernameAsAFloatArray[0] = 100;
        for (int i = 1; i < _playername.Length + 1; i++) {
            usernameAsAFloatArray[i] = (float)(int)_playername[i - 1];
        }

        m_ASLObject.SendAndSetClaim(() =>
        {
            m_ASLObject.SendFloatArray(usernameAsAFloatArray);
        });
    }

    public void SendBodyPositionsAndRotations(Transform[] playerBodyTransforms)
    {
        float[] bodyTransformValues = new float[1 + playerBodyTransforms.Length * 6];
        bodyTransformValues[0] = 101;
        for (int i = 0; i < playerBodyTransforms.Length; i++) {
            bodyTransformValues[6 * i + 1] = playerBodyTransforms[i].position.x;
            bodyTransformValues[6 * i + 2] = playerBodyTransforms[i].position.y;
            bodyTransformValues[6 * i + 3] = playerBodyTransforms[i].position.z;
            bodyTransformValues[6 * i + 4] = playerBodyTransforms[i].rotation.eulerAngles.x;
            bodyTransformValues[6 * i + 5] = playerBodyTransforms[i].rotation.eulerAngles.y;
            bodyTransformValues[6 * i + 6] = playerBodyTransforms[i].rotation.eulerAngles.z;
        }
        
        m_ASLObject.SendAndSetClaim(() =>
        {
            m_ASLObject.SendFloatArray(bodyTransformValues);
        });
    }

    public void SetOwner(string _id, float[] _f) {
        Debug.Log("ghostPlayer float function");
        ownerID = (int)_f[1];
        Debug.Log("-----");
        Debug.Log("GhostMyID: " + ASL.GameLiftManager.GetInstance().m_PeerId);
        Debug.Log("GhostOwnerID: " + ownerID);
    }
}
