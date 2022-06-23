using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;
public class Change_Color : MonoBehaviour
{
    ASLObject m_object;
    // Start is called before the first frame update
    void Start()
    {
        m_object = GetComponent<ASLObject>();
    }

    // Update is called once per frame
    void Update()
    {
        Color color = GetComponent<MeshRenderer>().material.color;
        if (Input.GetKey(KeyCode.M) && color == new Color(1, 0, 0, 1))
        {
            m_object.SendAndSetClaim(() =>
            {
                //Once we have it claimed, delete the object
                m_object.DeleteObject();
            });
        }
    }
    private void OnMouseDown()
    {
        Color color = GetComponent<MeshRenderer>().material.color;
        if (color == new Color(1, 1, 1, 1))
        {
            m_object.SendAndSetClaim(() =>
            {
                m_object.SendAndSetObjectColor(new Color(1, 0, 0, 1), new Color(1, 0, 0, 1));
            });
        }
        else
        {
            m_object.SendAndSetClaim(() =>
            {
                m_object.SendAndSetObjectColor(new Color(1, 1, 1, 1), new Color(1, 1, 1, 1));
            });
        }

    }
}
