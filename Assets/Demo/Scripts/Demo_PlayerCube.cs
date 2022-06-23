using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class Demo_PlayerCube : MonoBehaviour
{
    [Tooltip("This determines the speed that the PlayerCube will move.")]
    public float MovementSpeed = 3.0f;

    ASLObject m_ASLObject;

    // Start is called before the first frame update
    void Start()
    {
        m_ASLObject = gameObject.GetComponent<ASLObject>();
        Debug.Assert(m_ASLObject != null);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow) ^ Input.GetKey(KeyCode.DownArrow))
        {
            if (Input.GetKey(KeyCode.UpArrow))
            {
                m_ASLObject.SendAndSetClaim(() =>
                {
                    Vector3 m_AdditiveMovementAmount = Vector3.forward * MovementSpeed * Time.deltaTime;
                    m_ASLObject.SendAndIncrementWorldPosition(m_AdditiveMovementAmount);
                });
            }
            else
            {
                m_ASLObject.SendAndSetClaim(() =>
                {
                    Vector3 m_AdditiveMovementAmount = Vector3.back * MovementSpeed * Time.deltaTime;
                    m_ASLObject.SendAndIncrementWorldPosition(m_AdditiveMovementAmount);
                });
            }
        }
        if (Input.GetKey(KeyCode.RightArrow) ^ Input.GetKey(KeyCode.LeftArrow))
        {
            if (Input.GetKey(KeyCode.RightArrow))
            {
                m_ASLObject.SendAndSetClaim(() =>
                {
                    Vector3 m_AdditiveMovementAmount = Vector3.right * MovementSpeed * Time.deltaTime;
                    m_ASLObject.SendAndIncrementWorldPosition(m_AdditiveMovementAmount);
                });
            }
            else
            {
                m_ASLObject.SendAndSetClaim(() =>
                {
                    Vector3 m_AdditiveMovementAmount = Vector3.left * MovementSpeed * Time.deltaTime;
                    m_ASLObject.SendAndIncrementWorldPosition(m_AdditiveMovementAmount);
                });
            }
        }
    }
}
