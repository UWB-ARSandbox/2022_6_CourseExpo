using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Demo_CreatedSphere : MonoBehaviour
{
    [Tooltip("How long this object will exist before destroying iteself.")]
    public float LifeSpan = 5.0f;

    float timer = 0.0f;

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= LifeSpan)
        {
            //After LifeSpan seconds, the object is deleted
            Destroy(gameObject);
        }
    }
}

