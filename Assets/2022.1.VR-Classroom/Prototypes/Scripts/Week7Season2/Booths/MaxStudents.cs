using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using ASL;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class MaxStudents : MonoBehaviour
{
    ASLObject m_ASLObject;
    public Text maxStudentsText;
    AssessmentManager assessmentManager;
    
    void Start()
    {
        assessmentManager = transform.parent.parent.GetComponent<AssessmentManager>();
        maxStudentsText.text = assessmentManager.NumberOfConcurrentUsers.ToString();
        m_ASLObject = GetComponent<ASLObject>();
    }

    public void Incremenent()
    {
        assessmentManager.NumberOfConcurrentUsers++;
        maxStudentsText.text = assessmentManager.NumberOfConcurrentUsers.ToString();
    }

    public void Decrement()
    {
        assessmentManager.NumberOfConcurrentUsers--;
        maxStudentsText.text = assessmentManager.NumberOfConcurrentUsers.ToString();
    }


    #region sending
    public void IncrementMaxStudents() {
        ChangeMaxStudents(102f);
    }
    public void DecrementMaxStudents() {
        ChangeMaxStudents(103f);
    }
    public void ChangeMaxStudents(float code) {
        if (GameManager.AmTeacher)
        {
            float[] boothStatus = new float[1] { code };
            m_ASLObject.SendAndSetClaim(() =>
            {
                m_ASLObject.SendFloatArray(boothStatus);
            });
        }
    }
    #endregion
}
