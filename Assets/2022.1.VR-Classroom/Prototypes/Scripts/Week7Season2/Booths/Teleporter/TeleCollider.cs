using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleCollider : MonoBehaviour
{
    public GameObject InfoSender;
    public Collider TeleChecker;

    // Start is called before the first frame update
    void Start()
    {
        TeleChecker = transform.GetComponent<Collider>();
    }

    void OnTriggerStay(Collider other)
    {
        InfoSender.GetComponent<Teleporter>().EnterTrigger(true, other.gameObject);
    }
    void OnTriggerExit(Collider other)
    {
        InfoSender.GetComponent<Teleporter>().LeftTrigger(false);
    }
}
