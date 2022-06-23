using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class Sample_AutonomousObject : MonoBehaviour
{
    public float MovementSpeed = 2;

    ASL_ObjectCollider m_ObjectCollider;
    ASL_AutonomousObject m_AutonomousObject;
    Vector3 startPos;
    float direction = 1;

    // Start is called before the first frame update
    void Start()
    {
        m_ObjectCollider = GetComponent<ASL_ObjectCollider>();
        m_AutonomousObject = GetComponent<ASL_AutonomousObject>();
        startPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.x - startPos.x >= 1)
        {
            direction = -1;
        }
        else if (transform.position.x - startPos.x <= -1)
        {
            direction = 1;
        }
        Vector3 m_AdditiveMovementAmount = new Vector3(Time.deltaTime * MovementSpeed * direction, 0, 0);
        m_AutonomousObject.AutonomousIncrementWorldPosition(m_AdditiveMovementAmount);
    }
}
