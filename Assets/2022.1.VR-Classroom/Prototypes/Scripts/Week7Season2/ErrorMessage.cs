using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ErrorMessage : MonoBehaviour
{
    public void trigger()
    {
        StartCoroutine(ClosePopup());
    }

    public IEnumerator ClosePopup()
    {
        yield return new WaitForSeconds(2);
        gameObject.SetActive(false);
    }
}
