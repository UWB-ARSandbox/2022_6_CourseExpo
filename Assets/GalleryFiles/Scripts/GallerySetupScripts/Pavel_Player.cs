using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;
using UnityEngine.UI;

public class Pavel_Player : MonoBehaviour
{
    [Tooltip("This determines the speed that the PlayerCube will move.")]
    public float MovementSpeed = 3f;
    bool lockAtCanvas = false;
    bool menuOpen = false;

    ASLObject m_ASLObject;
    GameLiftManager manager;
    [SerializeField] Button LeaveButton;

    MenuManager menu;

    Transform lastTran;
    bool clicked = false;

    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.Find("GameLiftManager").GetComponent<GameLiftManager>();
        
        m_ASLObject = gameObject.GetComponent<ASLObject>();
        Debug.Assert(m_ASLObject != null);

        menu = GameObject.Find("UI").GetComponent<MenuManager>();
        if (manager.AmLowestPeer() == false)
		{
            //LeaveButton = GameObject.Find("LeaveTheClassButton").GetComponent<Button>();
            LeaveButton = menu.leaveClassButton.GetComponent<Button>();
            LeaveButton.onClick.AddListener(LeaveClass);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (lockAtCanvas == false && !clicked && menuOpen == false)
        {
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            Vector3 move = transform.right * x + transform.forward * z;

            m_ASLObject.SendAndSetClaim(() =>
            {
                Vector3 m_AdditiveMovementAmount = move * MovementSpeed * Time.deltaTime;
                m_AdditiveMovementAmount.y = 0;
                m_ASLObject.SendAndIncrementWorldPosition(m_AdditiveMovementAmount);
                //m_ASLObject.SendAndIncrementLocalPosition(m_AdditiveMovementAmount);
            });
        }

        // Call for moving/zooming into the canvas
        if (Input.GetMouseButtonDown(0) && clicked == false)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            int layerMask = LayMaskForRay();

            // If a raycast hits an object that is classified as a student canvas
            // move the camera over to the object
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask) && hit.transform.gameObject.
                name.Contains("StuCanvas"))
            {
                lastTran = transform;
                transform.GetChild(1).position = hit.transform.position;
                transform.GetChild(1).eulerAngles = new Vector3(0, hit.transform.eulerAngles.y + 180, 0);

                transform.GetChild(1).GetComponent<FirstPersonCamera>().SetCursorLock(true);
                transform.GetChild(1).GetComponent<FirstPersonCamera>().SetIsLocked(true);

                DoNotRenderPlayer();

                // Front wall
                if (hit.transform.eulerAngles.y == 180)
				{
                    transform.GetChild(1).position -= new Vector3(0, 0, 2);
                }
                // Right wall
                else if(hit.transform.eulerAngles.y == 270)
				{
                    transform.GetChild(1).position -= new Vector3(2, 0, 0);
                }
                // Back wall
                else if(hit.transform.eulerAngles.y == 0)
				{
                    transform.GetChild(1).position += new Vector3(0, 0, 2);
                }
                // Left wall
                else
				{
                    transform.GetChild(1).position += new Vector3(2, 0, 0);
                }
                clicked = true;
            }
        }

        // Resetting camera back to the player
        else if(clicked && Input.anyKeyDown && !Input.GetKeyDown(KeyCode.LeftControl) && !Input.GetMouseButtonDown(0))
		{
            clicked = false;
            transform.GetChild(1).GetComponent<FirstPersonCamera>().SetCursorLock(true);
            transform.GetChild(1).GetComponent<FirstPersonCamera>().SetIsLocked(false);
            
            DoRenderPlayer();

            transform.GetChild(1).position = lastTran.position;
            transform.GetChild(1).rotation = lastTran.rotation;
        }
    }

    // This function establishes a rendering mask for
    // the main camera so the player can raycast through gameobjects. 
    int LayMaskForRay()
	{
        PaintOnCanvas gone = transform.parent.GetChild(1).GetComponent<PaintOnCanvas>();
        // This is a layer that is not being used (a value must be assigned).
        int layerMask = 1 << 30;
        if (gone.GetClickStatus())
        {
            // Set students to be raycasted through
            layerMask |= (1 << 9);
            // Set canvas to be raycasted through
            layerMask |= (1 << 10);
            // Set teacher canvas to be raycasted through
            layerMask |= (1 << 11);
        }
        layerMask = ~layerMask;

        return layerMask;
    }

    // This will kick the student out of the class and quite the application
    public void LeaveClass()
    {
        // Delete player model in space
        gameObject.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
        {
            GetComponent<ASL.ASLObject>().DeleteObject();
            manager.DisconnectFromServer();
            Application.Quit();
        });
    }

    // Establishes a lock so that players can not move when zoomed in
    // on a canvas in gallery mode.
    public void SetLockAtCanvas(bool locked)
    {
        lockAtCanvas = locked;

        // Render player models
        if(lockAtCanvas)
		{
            DoNotRenderPlayer();
		}
        else
		{
            DoRenderPlayer();
		}
    }

    // Establishes a lock so that players can't move when menu open.
    public void SetMenuOpen(bool open)
    {
        menuOpen = open;
    }

    // This SPECIFICALLY does not render obects on the player layer
    public void DoNotRenderPlayer()
	{
        // Get this players camera
        Camera cam = transform.GetChild(1).GetComponent<Camera>();
        // Sets player to not render to camera
        cam.cullingMask &= ~(1 << 9);
    }

    // This SPECIFICALLY does render obects on the player layer
    public void DoRenderPlayer()
    {
        // Get this players camera
        Camera cam = transform.GetChild(1).GetComponent<Camera>();
        // Set player to render to camera
        cam.cullingMask |= (1 << 9);
    }

    // Sets player position in the world space for all users
    public void SetPosition(Vector3 pos)
    {
        m_ASLObject.SendAndSetClaim(() =>
            {
                m_ASLObject.SendAndSetWorldPosition(pos);
            });
    }

    // Gets the status if the player is looking in at a zoomed in
    // canvas
    public bool GetZoomed() { return clicked; }
}

