using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class Platformer_WinZone : MonoBehaviour
{
    ASL_ObjectCollider m_ASLObjectCollider;

    void Start()
    {
        m_ASLObjectCollider = gameObject.GetComponent<ASL_ObjectCollider>();
        Debug.Assert(m_ASLObjectCollider != null);

        //Assigning the deligate function to the ASL_ObjectCollider
        m_ASLObjectCollider.ASL_OnTriggerEnter(CollideWithPlayerEnter);
    }

    public void CollideWithPlayerEnter(Collider other)
    {
        Platformer_Player player = other.GetComponent<Platformer_Player>();
        if (player != null)
        {
            player.EnterWinZone();
        }
    }
}
