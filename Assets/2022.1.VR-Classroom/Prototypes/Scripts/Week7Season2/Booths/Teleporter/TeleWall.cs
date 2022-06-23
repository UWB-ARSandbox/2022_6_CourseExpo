using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TeleWall : MonoBehaviour
{
    public GameObject Exit;
    public GameObject Player;
    public GameObject Core;
    public TextMeshProUGUI text;
    public Text minimapText;
    public string textVal;
    public string minimapVal;

    public Camera camera;
    private Material newMat; //used for creating material and render on the fly
    private RenderTexture newText;

    private Transform CamTransform; //attempt to lock the camera back to normal once done

    public bool Looking; //determines if the user is in trigger box

    void Start()
    {
        CamTransform = camera.transform;
        if(camera.targetTexture != null)
        {
            camera.targetTexture.Release();
        }
        newMat = new Material(Shader.Find("Unlit/ScreenCutoutShader"));
        newText = new RenderTexture(Screen.width, Screen.height, 24);
        newMat.mainTexture = newText;
        camera.targetTexture = newText;

        if (textVal != null)
        {
            text.text = textVal;
        }
        if(minimapVal != null)
        {
            minimapText.text = minimapVal;
        }

        Player = GameObject.Find("FirstPersonPlayer(Clone)");

        Exit.GetComponentInChildren<TeleWall>().changeMat(newMat);
        StartCoroutine(DelayedInit());
    }

    IEnumerator DelayedInit() {
        while (Player == null) {
            Player = GameObject.Find("FirstPersonPlayer(Clone)");
            yield return new WaitForSeconds(0.5f);
        }
    }

    void Update()
    {
        if(Looking == true)
        {
            Vector3 posA = Player.transform.position;
            Vector3 posB = Core.transform.position;

            float dist = Vector3.Distance(posA, posB);
            Debug.Log("range = " + dist);
            Vector3 dir = (posB - posA);
            dir.y = -dir.y;

            float angularDifferenceBetweenPortals = Quaternion.Angle(gameObject.transform.rotation, Exit.transform.rotation);

            Quaternion portalDifference = Quaternion.AngleAxis(angularDifferenceBetweenPortals, Vector3.up);
            Vector3 newDirection = portalDifference * Player.GetComponentInChildren<Camera>().transform.forward;

            //Exit.GetComponentInChildren<TeleWall>().changeCamFOV(150 - (dist/8 * 90));
            Exit.GetComponentInChildren<TeleWall>().moveCam(dir);
            Exit.GetComponentInChildren<TeleWall>().changeCamRot(Quaternion.LookRotation(newDirection, Vector3.up));
        }
        else
        {
            //Exit.GetComponentInChildren<TeleWall>().changeCamFOV(60);
            camera.transform.rotation = CamTransform.rotation;
            camera.transform.position = CamTransform.position;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        other.gameObject.transform.GetComponent<CharacterController>().enabled = false;
        other.gameObject.transform.position = Exit.transform.GetChild(1).transform.position;
        other.gameObject.transform.eulerAngles = Exit.transform.eulerAngles +180f * Vector3.up;
        other.gameObject.transform.GetComponent<CharacterController>().enabled = true;
        Looking = false;
    }

    public void changeMat(Material mat)
    {
        gameObject.GetComponent<MeshRenderer>().material = mat;
    }

    public void SetLooking(bool var)
    {
        Looking = var;
    }

    public void moveCam(Vector3 dir)
    {
        camera.transform.position = Core.transform.position + dir;
    }

    public void changeCamFOV(float var)
    {
        camera.fieldOfView = var;
    }

    public void changeCamRot(Quaternion var)
    {
        camera.transform.rotation = var;
        camera.transform.rotation *= Quaternion.Euler(0, 90, 0);
        camera.transform.rotation =  Quaternion.Euler(camera.transform.eulerAngles.x, camera.transform.eulerAngles.y, 0);
    }
}
