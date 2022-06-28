using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class HelpRequestedUI : MonoBehaviour
{

    public GameObject scrollPanel;
    public GameObject buttonPrefab;
    ASLObject m_ASLObject;
    float[] id = new float[1];
    GameObject requestHelpButton;

    void Start()
    {
        requestHelpButton = GameObject.Find("RequestHelpButton");
        m_ASLObject = GetComponent<ASLObject>();
        m_ASLObject._LocallySetFloatCallback(FloatReceive);

        if (GameManager.AmTeacher)
            requestHelpButton.SetActive(false);

        id[0] = GameManager.MyID;
    }

    public void HelpRequested() {
        m_ASLObject.SendAndSetClaim(() => {
            m_ASLObject.SendFloatArray(id);
        });
    }

    void FloatReceive(string _id, float[] _f)
    {
        if (GameManager.AmTeacher)
        {
            Debug.Log(GameManager.players[(int)_f[0]] + " has requested help");
            GameObject newButton = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            newButton.GetComponent<HelpRequestButton>().username = GameManager.players[(int)_f[0]];
            newButton.transform.parent = scrollPanel.transform;
            newButton.transform.localScale = new Vector3(1, 1, 1);
        }
    }
}
