using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ASL;

public class HelpRequestedUI : MonoBehaviour
{

    public GameObject scrollPanel;
    public GameObject buttonPrefab;
    ASLObject m_ASLObject;
    float[] id = new float[2];
    GameObject requestHelpButton;

    void Start()
    {
        requestHelpButton = GameObject.Find("RequestHelpButton");
        m_ASLObject = GetComponent<ASLObject>();
        m_ASLObject._LocallySetFloatCallback(FloatReceive);

        if (GameManager.AmTeacher)
            requestHelpButton.SetActive(false);
    }

    public void HelpRequested() {
        id[0] = GameManager.MyID;
        id[1] = 100;
        requestHelpButton.GetComponent<Button>().enabled = false;
        m_ASLObject.SendAndSetClaim(() => {
            m_ASLObject.SendFloatArray(id);
        });
    }

    public void ReenableButton(float _id) {
        id[0] = _id;
        id[1] = 101;
        m_ASLObject.SendAndSetClaim(() => {
            m_ASLObject.SendFloatArray(id);
        });
    }

    void FloatReceive(string _id, float[] _f)
    {
        if ((int)_f[1] == 101 && !GameManager.AmTeacher)
        {
            if ((int)_f[0] == GameManager.MyID)
                requestHelpButton.GetComponent<Button>().enabled = true;
        }
        else if ((int)_f[1] == 100 && GameManager.AmTeacher)
        {
            Debug.Log(GameManager.players[(int)_f[0]] + " has requested help");
            GameObject newButton = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity) as GameObject;
            newButton.GetComponent<HelpRequestButton>().username = GameManager.players[(int)_f[0]];
            newButton.GetComponent<HelpRequestButton>().id = (int)_f[0];
            newButton.transform.parent = scrollPanel.transform;
            newButton.transform.localScale = new Vector3(1, 1, 1);
        }
    }
}
