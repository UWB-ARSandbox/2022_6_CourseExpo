using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;
using UnityEngine.UI;

public class PrefabInstantiationTest : MonoBehaviour
{

    public Text textOutput;
    private static Text textOutput2;
    private bool instantiationChecked = false;
    private bool instantiated = false;
    // Start is called before the first frame update
    void Start()
    {
        textOutput2 = textOutput;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameLiftManager.GetInstance() == null)
        {
            return;
        }

        if (GameLiftManager.GetInstance().AmLowestPeer() && !instantiated)
        {
            instantiated = true;
            ASLHelper.InstantiateASLObject("TestPrefabs/TestPrefabNoRootASLNoChild", Vector3.zero, Quaternion.identity, "", "", TestPrefabNoRootASLNoChildCreated);
            ASLHelper.InstantiateASLObject("TestPrefabs/TestPrefabRootASLNoChild", Vector3.zero, Quaternion.identity, "", "", TestPrefabRootASLNoChildCreated);
            ASLHelper.InstantiateASLObject("TestPrefabs/TestPrefabNoRootASLChild", Vector3.zero, Quaternion.identity, "", "", TestPrefabNoRootASLChildCreated);
            ASLHelper.InstantiateASLObject("TestPrefabs/TestPrefabRootASLChild", Vector3.zero, Quaternion.identity, "", "", TestPrefabRootASLChildCreated);
            textOutput.text += "Awaiting prefab instantiation, checking if instantiation occured in 5 seconds:\n";
        }
        if (Time.timeSinceLevelLoad > 5 && !instantiationChecked)
        {
            instantiationChecked = true;
            if (GameObject.Find("TestPrefabNoRootASLNoChild(Clone)") == null)
            {
                textOutput.text += "FAIL: Prefab Creation: (No Root ASL Object, No Child ASL Object)\n";
            } else
            {
                textOutput.text += "PASS: Prefab Creation: (No Root ASL Object, No Child ASL Object)\n";
            }

            if (GameObject.Find("TestPrefabRootASLNoChild(Clone)") == null)
            {
                textOutput.text += "FAIL: Prefab Creation: (Root ASL Object, No Child ASL Object)\n";
            }
            else
            {
                textOutput.text += "PASS: Prefab Creation: (Root ASL Object, No Child ASL Object)\n";
            }

            if (GameObject.Find("TestPrefabNoRootASLChild(Clone)") == null || GameObject.Find("TestPrefabNoRootASLChild(Clone)").GetComponentInChildren<ASLObject>() == null)
            {
                textOutput.text += "FAIL: Prefab Creation: (No Root ASL Object, Child ASL Object)\n";
            }
            else
            {
                textOutput.text += "PASS: Prefab Creation: (No Root ASL Object, Child ASL Object)\n";
            }

            if (GameObject.Find("TestPrefabRootASLChild(Clone)") == null || GameObject.Find("TestPrefabRootASLChild(Clone)").GetComponentInChildren<ASLObject>() == null)
            {
                textOutput.text += "FAIL: Prefab Creation: (Root ASL Object, Child ASL Object)\n";
            }
            else
            {
                textOutput.text += "PASS: Prefab Creation: (Root ASL Object, Child ASL Object)\n";
            }
        }
    }

    public static void TestPrefabNoRootASLNoChildCreated(GameObject obj)
    {
        if (GameLiftManager.GetInstance().AmLowestPeer())
        {
            textOutput2.text += "PASS: Prefab Creation Callback (No Root ASL Object, No Child ASL Object)\n";
        } else
        {
            textOutput2.text += "FAIL: Unexpected Prefab Creation Callback (No Root ASL Object, No Child ASL Object)\n";
        }
    }

    public static void TestPrefabRootASLNoChildCreated(GameObject obj)
    {
        if (GameLiftManager.GetInstance().AmLowestPeer())
        {
            textOutput2.text += "PASS: Prefab Creation Callback (Root ASL Object, No Child ASL Object)\n";
        } else
        {
            textOutput2.text += "FAIL: Unexpected Prefab Creation Callback (Root ASL Object, No Child ASL Object)\n";
        }
    }

    public static void TestPrefabNoRootASLChildCreated(GameObject obj)
    {
        if (GameLiftManager.GetInstance().AmLowestPeer())
        {
            textOutput2.text += "PASS: Prefab Creation Callback (No Root ASL Object, Child ASL Object)\n";
        } else
        {
            textOutput2.text += "FAIL: Unexpected Prefab Creation Callback (No Root ASL Object, Child ASL Object)\n";
        }
    }
    public static void TestPrefabRootASLChildCreated(GameObject obj)
    {
        if (GameLiftManager.GetInstance().AmLowestPeer())
        {
            textOutput2.text += "PASS: Prefab Creation Callback (Root ASL Object, Child ASL Object)\n";
        } else
        {
            textOutput2.text += "FAIL: Unexpected Prefab Creation Callback (Root ASL Object, Child ASL Object)\n";
        }
    }
}
