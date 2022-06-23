using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ASL;

public class KickStudents : MonoBehaviour
{
    [SerializeField] Button KickButton;
    GameLiftManager manager;
    MenuManager menu;
    float isKicking = 0;

    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.Find("GameLiftManager").GetComponent<GameLiftManager>();
        menu = GameObject.Find("UI").GetComponent<MenuManager>();

        GetComponent<ASL.ASLObject>()._LocallySetFloatCallback(StartKickingStudents);
    }

    // Update is called once per frame
    void Update()
    {
        if(KickButton == null && manager.AmLowestPeer())
        {
            KickButton = menu.disbandClassButton.GetComponent<Button>();
            KickButton.onClick.AddListener(DisbandClass);
        }
        
        // Student
        if(isKicking != 0 && manager.AmLowestPeer() == false)
		{
            LeaveClass();
		}
        // Teacher leaves last
        else if(isKicking != 0 && manager.AmLowestPeer() == true &&
            manager.m_Players.Count == 1)
		{
            LeaveClass();
		}

        
    }

    // Kicks this user
    public void LeaveClass()
    {
        manager.DisconnectFromServer();
        Application.Quit();
    }

    // Kicks everyone from the class starting with the students and then finishes with the teacher
    void DisbandClass()
	{
        gameObject.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
        {
            float[] temp = new float[1];
            temp[0] = 1;
            GetComponent<ASLObject>().SendFloatArray(temp);
        });

    }

    public void StartKickingStudents(string _id, float[] _f)
	{
        isKicking = _f[0];
	}
}
