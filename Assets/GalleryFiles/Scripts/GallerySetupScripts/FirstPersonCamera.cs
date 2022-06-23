using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class FirstPersonCamera : MonoBehaviour
{
    

    public float mouseSensitivity = 100f;
    public Transform playerBody;

    public float xRotation = 0f;

    public float yRotation = 0f;
    ASLObject m_ASLObject;
    
    public GameObject crosshair, settings;

    [SerializeField]GameObject LoadMenu, SaveConfirmMenu, Controls;

    
    [SerializeField]
    bool isLocked, isActive, menuActive;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        isLocked = false;
        isActive = false;
        
        crosshair = GameObject.Find("Crosshair");
        settings = GameObject.Find("Settings");

        menuActive = false;

    }

    // Update is called once per frame
    void Update()
    {
        if(isActive && !isLocked &&
            transform.parent.GetComponent<Pavel_Player>().GetZoomed() == false)
        {
            // Code for first person camera movement taken from Brackeys. https://www.youtube.com/watch?v=_QajrabyTJc
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            yRotation += mouseX;
            
            m_ASLObject.SendAndSetClaim(() =>
                    {
                        //transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
                        //playerBody.Rotate(Vector3.up * mouseX);
                        //m_ASLObject.SendAndIncrementLocalRotation(Quaternion.Euler(0f, mouseX, 0f));
                        m_ASLObject.SendAndSetLocalRotation(Quaternion.Euler(xRotation, yRotation, 0f));
                        //Debug.Log("Sent Local Rotation");
                    });

        }
        

        if(Input.GetKeyDown(KeyCode.LeftAlt))
        {
            settings.SetActive(!settings.activeSelf);
        }
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            // If SaveConfirmMenu is not active, disable LoadMenu
            if(!SaveConfirmMenu.activeSelf || !Controls.activeSelf)
            {
                Debug.Log("Toggling Menu");
                Controls.SetActive(false);
                SaveConfirmMenu.SetActive(false);
			    LoadMenu.SetActive(!LoadMenu.activeSelf);
                transform.parent.GetComponent<Pavel_Player>().SetMenuOpen(LoadMenu.activeSelf);
            }
            menuActive = LoadMenu.activeSelf;

            if(Cursor.lockState == CursorLockMode.Locked)
            {
                isLocked = true;
            }
            else
            {
                isLocked = false;
            }

            if(menuActive)
            {
                isLocked = true;
            }
            crosshair.SetActive(!isLocked);
            SetCursorLock(!isLocked);
        }

        if(Input.GetKeyDown(KeyCode.LeftControl) && !menuActive)
        {
            if(Cursor.lockState == CursorLockMode.Locked)
            {
                isLocked = true;
                transform.parent.GetComponent<Pavel_Player>().DoNotRenderPlayer();
            }
            else
            {
                isLocked = false;
                transform.parent.GetComponent<Pavel_Player>().DoRenderPlayer();
            }

            crosshair.SetActive(!isLocked);
            SetCursorLock(!isLocked);
        }
       
    }

    public void ReinitializeParent(GameObject parent)
    {
        m_ASLObject = parent.GetComponent<ASLObject>();
        this.gameObject.transform.position = parent.transform.position;
        this.gameObject.transform.rotation = parent.transform.rotation;
        playerBody = parent.transform;
        isActive = true;
    }

    public void SetCursorLock(bool isLocked)
    {
        if(isLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void SetIsLocked(bool locked)
	{
        isLocked = locked;
	}

    public void GoToCanvas()
    {
        if (isLocked == true)
		{
			GameObject canvas = transform.parent.parent.GetChild(1).gameObject;
            GameObject body = transform.parent.gameObject;
			Vector3 pos = canvas.transform.position;
			pos += (3f * canvas.transform.forward);
            transform.parent.GetComponent<Pavel_Player>().SetPosition(pos);
			transform.parent.GetComponent<Pavel_Player>().SetLockAtCanvas(true);
            xRotation = 0;
            yRotation = 0;
            m_ASLObject.SendAndSetClaim(() =>
                {
                    m_ASLObject.SendAndSetLocalRotation(Quaternion.Euler(0, 0, 0f));
                    //Debug.Log("Sent Local Rotation");
                });
		}
		else
		{
			transform.parent.GetComponent<Pavel_Player>().SetLockAtCanvas(false);
		}
    }

    public void SetLoadMenuReference(GameObject load, GameObject saveConfirm, GameObject controls)
    {
        LoadMenu = load;
        SaveConfirmMenu = saveConfirm;
        Controls = controls;
    }
}
