using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class Platformer_PatrolRoute : MonoBehaviour
{
    public GameObject _MovingObject;
    public float MovementSpeed = 3f;
    public Platformer_PatrolPoint[] PatrolPoints;

    int destination = 0;
    int direction = 1;

    ASLObject m_ASLObject;
    ASL_AutonomousObject m_AutonomousObject;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(_MovingObject != null);
        m_ASLObject = _MovingObject.GetComponent<ASLObject>();
        Debug.Assert(m_ASLObject != null);
        m_AutonomousObject = _MovingObject.GetComponent<ASL_AutonomousObject>();
        Debug.Assert(m_AutonomousObject != null);
    }

    private void Update()
    {
        if (_MovingObject.transform.position == PatrolPoints[destination].transform.position)
        {
            if (destination == PatrolPoints.Length - 1)
            {
                direction = -1;
            }
            else if (destination == 0)
            {
                direction = 1;
            }
            destination += direction;
        }
        Vector3 m_AdditiveMovementAmount = Vector3.MoveTowards(
            _MovingObject.transform.position,
            PatrolPoints[destination].transform.position,
            MovementSpeed * Time.deltaTime);
        m_AdditiveMovementAmount = m_AdditiveMovementAmount - _MovingObject.transform.position;
        m_AutonomousObject.AutonomousIncrementWorldPosition(m_AdditiveMovementAmount);
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < PatrolPoints.Length - 1; i++)
        {
            Gizmos.color = new Color(0, 0, 0, 0.75f);
            if (PatrolPoints[i] != null && PatrolPoints[i + 1] != null)
            {
                Gizmos.DrawLine(PatrolPoints[i].transform.position, PatrolPoints[i + 1].transform.position);
            }
        }
    }

    private void OnDestroy()
    {
        //autonomousObjectHandler.RemoveAutonomousObject(m_ASLObject);
    }
}
