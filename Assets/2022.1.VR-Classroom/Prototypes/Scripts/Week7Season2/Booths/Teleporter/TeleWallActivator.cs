using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleWallActivator : MonoBehaviour
{
    public TeleWall Tele;
    private GameObject Player;

    void Start()
    {
        Player = GameObject.Find("FirstPersonPlayer(Clone)");
        StartCoroutine(DelayedInit());
    }

    IEnumerator DelayedInit()
    {
        while (Player == null)
        {
            Player = GameObject.Find("FirstPersonPlayer(Clone)");
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == Player)
        {
            Tele.SetLooking(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject == Player)
        {
            Tele.SetLooking(false);
        }
    }
}
