using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class Spawn_ASL_Autonomus : MonoBehaviour
{
    public string mPrefab;
    public string mPrefab2;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.K))
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePos);
            worldPosition.z = 0;
            ASLHelper.InstantiateASLObject(mPrefab, worldPosition, Quaternion.identity);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 10;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePos);
            worldPosition.z = 0;
            ASLHelper.InstantiateASLObject(mPrefab2, worldPosition, Quaternion.identity);
        }
    }
}
