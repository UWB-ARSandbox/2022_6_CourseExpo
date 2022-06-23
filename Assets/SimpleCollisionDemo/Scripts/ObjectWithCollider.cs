using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectWithCollider : MonoBehaviour
{
    [Tooltip("The default color of the object. NOTE: The selected color is yellow, the default " +
       "color should be something else.")]
    public Color DefaultColor;
    [Tooltip("This determines how much force is applied to the object when it Jumps.")]
    public Vector3 JumpForce = new Vector3(0f, 200f, 0f);

    Color selectedColor = Color.yellow;
    public Color SelectedColor
    {
        get { return selectedColor; }
    }
    const float MOVEMENT_SPEED = 1.0f;
    bool isSelected = false;
    public bool IsSelected
    {
        get { return isSelected; }
    }
    public Toggle KinimaticToggle;

    bool isStatic = false;
    bool isTrigger = false;
    bool isKinimatic = false;
    Vector3 origin;


    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(gameObject.GetComponent<Rigidbody>() != null);
        Debug.Assert(GetComponent<MeshRenderer>() != null);
        Debug.Assert(KinimaticToggle != null);
        GetComponent<MeshRenderer>().material.color = DefaultColor;
        origin = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        if (isSelected)
        {
            if (Input.GetKey(KeyCode.UpArrow) ^ Input.GetKey(KeyCode.DownArrow))
            {
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    Vector3 movement = Vector3.up * MOVEMENT_SPEED * Time.deltaTime;
                    transform.localPosition += movement;
                }
                else
                {
                    Vector3 movement = Vector3.down * MOVEMENT_SPEED * Time.deltaTime;
                    transform.localPosition += movement;
                }
            }
            if (Input.GetKey(KeyCode.RightArrow) ^ Input.GetKey(KeyCode.LeftArrow))
            {
                if (Input.GetKey(KeyCode.RightArrow))
                {
                    Vector3 movement = Vector3.right * MOVEMENT_SPEED * Time.deltaTime;
                    transform.localPosition += movement;
                    if (!isStatic)
                    {
                        Vector3 v = gameObject.GetComponent<Rigidbody>().velocity;
                        gameObject.GetComponent<Rigidbody>().velocity = new Vector3(0f, v.y, 0f);
                    }
                }
                else
                {
                    Vector3 movement = Vector3.left * MOVEMENT_SPEED * Time.deltaTime;
                    transform.localPosition += movement;
                    if (!isStatic)
                    {
                        Vector3 v = gameObject.GetComponent<Rigidbody>().velocity;
                        gameObject.GetComponent<Rigidbody>().velocity = new Vector3(0f, v.y, 0f);
                    }
                }
            }
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (!isStatic)
                {
                    gameObject.GetComponent<Rigidbody>().AddForce(JumpForce);
                }
            }
        }

        if (transform.localPosition.y < -3.5f)
        {
            transform.localPosition = origin;
            if (GetComponent<Rigidbody>() != null)
            {
                GetComponent<Rigidbody>().velocity = Vector3.zero;
            }
        }
    }

    public void SelectObject(bool _isSelected)
    {
        isSelected = _isSelected;
    }

    public void ToggleStatic()
    {
        isStatic = !isStatic;
        if (isStatic)
        {
            if (gameObject.GetComponent<Rigidbody>() != null)
            {
                Destroy(GetComponent<Rigidbody>());
            }
            KinimaticToggle.isOn = false;
            KinimaticToggle.interactable = false;
            isKinimatic = false;
        }
        else
        {
            gameObject.AddComponent<Rigidbody>();
            KinimaticToggle.interactable = true;
        }
    }
    public void ToggleTrigger()
    {
        isTrigger = !isTrigger;
        GetComponent<Collider>().isTrigger = isTrigger;
    }
    public void ToggleKinimatic()
    {
        isKinimatic = !isKinimatic;
        GetComponent<Rigidbody>().isKinematic = isKinimatic;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(name + " collided with: " + collision.gameObject.name);
    }

    public void OnTriggerEnter(Collider other)
    {
        Debug.Log(name + " triggered: " + other.gameObject.name);
    }
}
