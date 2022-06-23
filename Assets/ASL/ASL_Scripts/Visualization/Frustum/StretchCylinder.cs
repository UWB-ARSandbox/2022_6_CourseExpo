using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A cylinder that stretches from one GameObject to another
/// </summary>
public class StretchCylinder : MonoBehaviour
{
    public GameObject StartPoint;
    public GameObject EndPoint;

    public float m_Width { get; set; }

    private Transform m_StartTransform;
    private Transform m_EndTransform;

    private Transform m_CylinderTransform;

    // Start is called before the first frame update
    void Start()
    {
        m_StartTransform = StartPoint.transform;
        m_EndTransform = EndPoint.transform;

        m_CylinderTransform = transform;
    }

    // Update is called once per frame
    void Update()
    {
        var diff = m_EndTransform.position - m_StartTransform.position;

        var position = m_StartTransform.position + (diff / 2);

        m_CylinderTransform.position = position;
        m_CylinderTransform.up = diff;
        m_CylinderTransform.localScale = new Vector3(m_Width, diff.magnitude / 2.0f, m_Width);
    }
}
