using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseCaseStaticCollisionTest : MonoBehaviour
{
    public Text OnCollisionEnterText;
    public Text OnCollisionExitText;
    public Text OnTriggerEnterText;
    public Text OnTriggerExitText;
    public Text OnOnTriggerStayText;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(OnCollisionEnterText != null);
        Debug.Assert(OnCollisionExitText != null);
        Debug.Assert(OnTriggerEnterText != null);
        Debug.Assert(OnTriggerExitText != null);
        Debug.Assert(OnOnTriggerStayText != null);
    }

    private void OnCollisionEnter(Collision collision)
    {
        OnCollisionEnterText.text = "OnCollisionEnter called with " + gameObject.name + " at " + Time.time;
    }

    private void OnCollisionExit(Collision collision)
    {
        OnCollisionExitText.text = "OnCollisionExit called with " + gameObject.name + " at " + Time.time;
    }

    private void OnTriggerEnter(Collider other)
    {
        OnTriggerEnterText.text = "OnTriggerEnter called with " + gameObject.name + " at " + Time.time;
    }

    private void OnTriggerExit(Collider other)
    {
        OnTriggerExitText.text = "OnTriggerExit called with " + gameObject.name + " at " + Time.time;
    }

    private void OnTriggerStay(Collider other)
    {
        OnOnTriggerStayText.text = "OnTriggerStay called with " + gameObject.name + " at " + Time.time;
    }
}
