using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unity_PlayerCube : MonoBehaviour
{
    public float MovementSpeed = 3.0f;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow) ^ Input.GetKey(KeyCode.DownArrow))
        {
            if (Input.GetKey(KeyCode.UpArrow))
            {
                Vector3 m_AdditiveMovementAmount = Vector3.forward * MovementSpeed * Time.deltaTime;
                transform.position += m_AdditiveMovementAmount;
            }
            else
            {
                Vector3 m_AdditiveMovementAmount = Vector3.back * MovementSpeed * Time.deltaTime;
                transform.position += m_AdditiveMovementAmount;
            }
        }
        if (Input.GetKey(KeyCode.RightArrow) ^ Input.GetKey(KeyCode.LeftArrow))
        {
            if (Input.GetKey(KeyCode.RightArrow))
            {
                Vector3 m_AdditiveMovementAmount = Vector3.right * MovementSpeed * Time.deltaTime;
                transform.position += m_AdditiveMovementAmount;
            }
            else
            {
                Vector3 m_AdditiveMovementAmount = Vector3.left * MovementSpeed * Time.deltaTime;
                transform.position += m_AdditiveMovementAmount;
            }
        }
    }
}
