using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class Platformer_Collider : MonoBehaviour
{
    public enum CollisionSide { top, bottom, left, right, na }
    protected float x, y;

    protected ASL_ObjectCollider m_ASLObjectCollider;

    // Start is called before the first frame update
    void Start()
    {
        m_ASLObjectCollider = gameObject.GetComponent<ASL_ObjectCollider>();
        Debug.Assert(m_ASLObjectCollider != null);

        //Assigning the deligate function to the ASL_ObjectCollider
        m_ASLObjectCollider.ASL_OnTriggerEnter(CollideWithPlayerEnter);
        m_ASLObjectCollider.ASL_OnTriggerExit(CollideWithPlayerExit);

        Collider collider;
        if ((collider = GetComponent<BoxCollider>()) != null)
        {
            x = ((BoxCollider)collider).size.x * transform.localScale.x / 2 + ((BoxCollider)collider).center.x;
            y = ((BoxCollider)collider).size.y * transform.localScale.y / 2 + ((BoxCollider)collider).center.y;
        }
        else if ((collider = GetComponent<SphereCollider>()) != null)
        {
            x = ((CapsuleCollider)collider).radius * transform.localScale.x / 2;
            y = ((CapsuleCollider)collider).radius * transform.localScale.y / 2;
        }
        else if ((collider = GetComponent<CapsuleCollider>()) != null)
        {
            x = ((CapsuleCollider)collider).radius * transform.localScale.x / 2;
            y = ((CapsuleCollider)collider).height * transform.localScale.y / 2;
        }
        else
        {
            Debug.LogError("Platformer_Collider object must have a BoxCollider, CapsuleCollider, or SphereCOllider");
        }
    }

    private void CollideWithPlayerEnter(Collider other)
    {
        Platformer_Player player = other.GetComponent<Platformer_Player>();
        if (player != null)
        {
            CollisionSide side = determineColissionDirection(other);
            if (side == CollisionSide.na)
            {
                Debug.LogError("Unable to determine direction of collision between: " + gameObject.name + " and " + other.name);
            }
            else if (side == CollisionSide.top)
            {
                player.PlatformCollisionEnter(side, transform.position.y + y);
            }
            else if (side == CollisionSide.left)
            {
                player.PlatformCollisionEnter(side, transform.position.x - x);
            }
            else if (side == CollisionSide.right)
            {
                player.PlatformCollisionEnter(side, transform.position.x + x);
            }
            else
            {
                player.PlatformCollisionEnter(side, transform.position.y - y);
            }
        }
    }

    private void CollideWithPlayerExit(Collider other)
    {
        Platformer_Player player = other.GetComponent<Platformer_Player>();
        if (player != null)
        {
            CollisionSide side = determineColissionDirection(other);
            if (side == CollisionSide.na)
            {
                Debug.LogError("Unable to determine direction of collision between: " + gameObject.name + " and " + other.name);
            }
            else
            {
                player.PlatformCollisionExit(side);
            }
        }
    }


    protected CollisionSide determineColissionDirection(Collider other)
    {
        Vector3 otherPos = other.gameObject.transform.position;
        Vector3 thisPos = transform.position;
        CollisionSide side = CollisionSide.na;
        float otherX = other.transform.localScale.x;
        float otherY = other.transform.localScale.y;

        if (otherPos.y >= thisPos.y + y)
        {
            side = CollisionSide.top;
        }
        else if (otherPos.y <= thisPos.y - y)
        {
            side = CollisionSide.bottom;
        }

        if (otherPos.x >= thisPos.x + x)
        {
            //right
            if (side != CollisionSide.na)
            {
                if (otherX < otherY)
                {
                    side = CollisionSide.right;
                    other.GetComponent<Platformer_Player>().PlatformCollisionExit(CollisionSide.top);
                }
            }
            else
            {
                side = CollisionSide.right;
            }
        }
        else if (otherPos.x <= thisPos.x - x)
        {
            //left
            if (side != CollisionSide.na)
            {
                if (otherX < otherY)
                {
                    side = CollisionSide.left;
                    other.GetComponent<Platformer_Player>().PlatformCollisionExit(CollisionSide.top);
                }
            }
            else
            {
                side = CollisionSide.left;
            }
        }
        return side;
    }
}
