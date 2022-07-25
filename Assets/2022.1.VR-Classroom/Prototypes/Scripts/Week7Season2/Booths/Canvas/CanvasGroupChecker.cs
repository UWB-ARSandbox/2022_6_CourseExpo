using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

public class CanvasGroupChecker : MonoBehaviour
{
    public GroupManager m_GroupManager;
    public List<NewPaint> studentCanvases;

    private void Start()
    {
        Debug.Assert(m_GroupManager != null);
        Debug.Assert(studentCanvases.Capacity == 5); // For now, 5 groups = 5 canvases
    }

    private void OnTriggerEnter(Collider other)
    {
        if (GameManager.AmTeacher)
        {
            RefreshTables();
            Debug.Log("Teacher Entered Canvas Room");
        }
            
    }

    void RefreshTables()
    {
        // loop through all non-teacher players
        for (int i = 2; GameLiftManager.GetInstance().m_Players.ContainsKey(i); i++)
        {
            // loop through every group and enable/disable access to canvases based on player's group affiliation
            for (int j = 0; j < m_GroupManager.groups.Capacity; j++)
            {
                if (m_GroupManager.groups[j].members.Contains(GameLiftManager.GetInstance().m_Players[i]))
                {
                    // enable this group canvas for this user
                    
                    StartCoroutine(studentCanvases[j].enableCanvasForPlayer(i));
                    StartCoroutine(studentCanvases[j].enableViewingForPlayer(i));
                }
                else
                {
                    // disable this group canvas for this user
                    StartCoroutine(studentCanvases[j].disableViewingForPlayer(i));
                    StartCoroutine(studentCanvases[j].disableCanvasForPlayer(i));
                }
                    
            }
            
            
        }
    }
}
