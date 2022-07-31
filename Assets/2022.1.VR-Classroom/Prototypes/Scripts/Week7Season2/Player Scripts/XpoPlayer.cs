using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.InputSystem;
using ASL;

public class XpoPlayer : MonoBehaviour {

    private static readonly float UPDATES_PER_SECOND = 20.0f;
    private static readonly float CALIBRATE_INTERVAL = 60.0f;

    public UnityEngine.XR.Interaction.Toolkit.Inputs.InputActionManager iam;

    ASLObject m_ASLObject;
    //UserObject m_UserObject;
    public GhostPlayer m_GhostPlayer;
    //public SendAndPlayAudio audioManager;
    //public Text LocalUsername;
    public Renderer cube;
    public Transform[] movingBodyParts;
    public Transform face;
    //Allow/Disallow tele dropdown
    public static bool enteredExpo = false;

    // Start is called before the first frame update
    void Start() {
        //LocalUsername.text = ASL.GameLiftManager.GetInstance().m_Username;

        StartCoroutine(DelayedInit());
    }

    IEnumerator DelayedInit() {

        while (m_GhostPlayer == null) {
            Debug.Log("Finding ghost...");
            foreach (GhostPlayer gp in FindObjectsOfType<GhostPlayer>()) {
                if (gp.IsOwner(GameManager.MyID/*m_UserObject.ownerID*/) && gp.ownerID != 0) {
                    Debug.Log("GHOST FOUND - ID:" + gp.ownerID);
                    m_GhostPlayer = gp;
                    gp.gameObject.SetActive(false);
                    StartCoroutine(NetworkedUpdate());
                    StartCoroutine(InputActionsRecalibrate());
                    if (GameManager.AmTeacher) {
                        ColorHost();
                    }
                    m_GhostPlayer.SendPlayerName(ASL.GameLiftManager.GetInstance().m_Username);
                }
            }
            //Ask teacher to resend ghosts?
            //or create my own ghost
            yield return new WaitForSeconds(0.1f);
        }
        StartCoroutine(OrientMapNames());
    }

    private void ColorHost() {
        m_GhostPlayer.SendHostColor();
        cube.material.color = Color.yellow;
    }

    public void ColorUser(Color color)
    {
        m_GhostPlayer.SendUserColor(color);
        cube.material.color = color;
    }

    void Update() {

        if (Camera.main) {
            //set head rotation to be the same as the camera's
            Transform cameraTransform = Camera.main.transform;
            Vector3 adjustedNewRotation = new Vector3(cameraTransform.eulerAngles.x, cameraTransform.transform.eulerAngles.y, cameraTransform.eulerAngles.z);
            face.transform.rotation = Quaternion.Euler(adjustedNewRotation);
        }
        //audioManager.SendAudio(); //commented out for now, so that we can hear videos without having to hear each other in bad quality
    }

    IEnumerator OrientMapNames() {
        //Cache stuff in this method
        GhostPlayer[] ghostplayers = FindObjectsOfType<GhostPlayer>();
        Transform[] ghostPlayerMinimapNameCanvases = ghostplayers.Select(gp => gp.transform.Find("MinimapUsername")).ToArray();
        Transform[] ghostPlayerWorldNameCanvases = ghostplayers.Select(gp => gp.transform.Find("WorldUsername")).ToArray();

        BoothManager[] boothManagers = FindObjectsOfType<BoothManager>();
        Transform[] boothNameCanvasTransform = boothManagers.Select(bm => bm.transform.Find("Booth/MinimapName")).ToArray();

        MapToggle mapToggle = FindObjectOfType<MapToggle>();

        while (true) {
            for (int i = 0; i < ghostplayers.Length; i++) {
                Vector3 euler = (mapToggle.mapState == MapToggle.MapState.FullMap ? 
                                new Vector3(90, -90, 0) : new Vector3(90, transform.eulerAngles.y, transform.eulerAngles.z));
                ghostPlayerMinimapNameCanvases[i].rotation = Quaternion.Euler(euler);
                ghostPlayerWorldNameCanvases[i].rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
            }

            for (int i = 0; i < boothManagers.Length; i++) {
                Vector3 euler = (mapToggle.mapState == MapToggle.MapState.FullMap ? 
                                new Vector3(90, -90, 0) : new Vector3(90, transform.eulerAngles.y, transform.eulerAngles.z));
                boothNameCanvasTransform[i].rotation = Quaternion.Euler(euler);
            }
            yield return null;
        }
    }

    public void SetFaceTexture() {
        m_GhostPlayer.SetFaceTexture();
    }

    // Putting your update in a coroutine allows you to run it at a rate of your choice
    IEnumerator NetworkedUpdate() {
        while (true) {
            if (m_GhostPlayer == null) {
                yield return new WaitForSeconds(0.1f);
            }

            //Debug.Log("Sending Position");
            m_GhostPlayer.SetWorldPosition(transform.position);
            m_GhostPlayer.SetWorldRotation(transform.rotation);

            m_GhostPlayer.SendBodyPositionsAndRotations(movingBodyParts);

            yield return new WaitForSeconds(1 / UPDATES_PER_SECOND);
        }
    }

    IEnumerator InputActionsRecalibrate() {
        iam.enabled = false;
        iam.enabled = true;
        yield return new WaitForSeconds(CALIBRATE_INTERVAL);
    }

    void OnTriggerEnter(Collider collider) {
        if (collider.tag == "Chapter") {
            enteredExpo = true;
            GameObject overviewMapCamera = GameObject.Find("Overview Map Camera");
            float originalHeight = overviewMapCamera.transform.position.y;
            overviewMapCamera.transform.position = new Vector3(collider.transform.position.x, originalHeight, collider.transform.position.z);
            overviewMapCamera.GetComponent<MapNavigation>().ResetMapZoom(); 
        }
    }

    /*public void floatFunction(string _id, float[] _f) {
        Debug.LogError("player float function");
        if (_f[0] == 1) {
            m_UserObject.floatFunction(_id, new float[2] { _f[0], _f[1] });
        }
        if (_f[0] == 130) {
            m_UserObject.floatFunction(_id, new float[1] { _f[0] });
        }
    }*/
}
