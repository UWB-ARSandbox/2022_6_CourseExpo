using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platformer_PatrolPoint : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 1, 0, 0.75f);
        Gizmos.DrawSphere(transform.position, 0.2f);
    }
}
