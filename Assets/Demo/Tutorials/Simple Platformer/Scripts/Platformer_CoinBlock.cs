using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class Platformer_CoinBlock : Platformer_Collider
{
    public GameObject CoinPrefab;
    public Material ActivatedColor;

    bool coinSpawned = false;
    ASLObject m_ASLObject;
    //ASL_ObjectCollider m_ASLObjectCollider;

    // Start is called before the first frame update
    void Start()
    {
        m_ASLObject = GetComponent<ASLObject>();
        Debug.Assert(m_ASLObject != null);
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
            else
            {
                if (!coinSpawned && side == CollisionSide.bottom)
                {
                    spawnCoin();
                }
                player.PlatformCollisionEnter(side, transform.position.y + y);
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

    void spawnCoin()
    {
        ASL_AutonomousObjectHandler.Instance.InstantiateAutonomousObject(CoinPrefab, new Vector3(transform.position.x, transform.position.y + 1, transform.position.z), CoinPrefab.transform.rotation);

        coinSpawned = true;
        m_ASLObject.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
        {
            m_ASLObject.GetComponent<ASL.ASLObject>().SendAndSetObjectColor(ActivatedColor.color, ActivatedColor.color);
        });
    }
}
