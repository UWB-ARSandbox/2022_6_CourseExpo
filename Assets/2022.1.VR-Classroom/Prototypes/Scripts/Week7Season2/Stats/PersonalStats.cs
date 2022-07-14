using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonalStats : MonoBehaviour
{ 
    #region Data storing classes
    public class BoothStats {
        public int timeInBooth;
        public virtual string OutputStats() {
            return $"{timeInBooth}";
        }
    }
    public class AssessmentStats : BoothStats {
        public float percentageScore { get; set; }
        public float timeTaken { get; set; }
        public bool completed { get; set; }


        //writer.AppendLine($"{item.Key},Time in booth,Time taken to complete,Questions timed out,Score,Completed");

        public override string OutputStats()
        {
            return $"{timeInBooth},{timeTaken},,{percentageScore},{completed}";
        }

    }
    public class QuizAndTestStats : AssessmentStats {
        public int questionsTimedOut { get; set; }
        public override string OutputStats()
        {
            return $"{timeInBooth},{timeTaken},{questionsTimedOut},{percentageScore},{completed}";
        }
    }
    #endregion

    #region Variables
    public Dictionary<string, BoothStats> boothStats = new Dictionary<string, BoothStats>();
    int timeOutsideBooths;
    private bool boothsLoaded = false;
    private bool inBooth = false;
    private bool atLeastBoothVerified = false;
    private string boothInsideOf;

    private Coroutine incrementBoothTime;
    private Coroutine incrementOutsideBoothTime;
    #endregion

    #region Initialization
    // Start is called before the first frame update
    void Start()
    {
        //Disable script is user is the teacher
        if (!GameManager.AmTeacher) {
            StartInitializeDict();
            incrementOutsideBoothTime = StartCoroutine(IncrementOutsideBoothTime());
        }
    }

    public void StartInitializeDict() {
        StartCoroutine(DelayedInit());
    }

    IEnumerator DelayedInit()
    //Stats for booths will only be counted once they have actually loaded
    {
        //initialize dictionary when booths get verified and loaded in
        while (!atLeastBoothVerified) {
            atLeastBoothVerified = true;
            foreach (GameObject obj in BoothManager.boothNames.Values) {
                if (!obj.GetComponent<BoothManager>().boothVerified) { 
                    atLeastBoothVerified = false;
                    break;
                }
            }
            yield return new WaitForSeconds(1f);
        }

        while(!BoothManager.verified) {
            yield return new WaitForSeconds(1f);
        }

        //add an entry in the dict for each booth
        InitializeDict();

        // foreach(var kvp in boothStats) {
        //     Debug.LogError($"Booth name: {kvp.Key}, Booth time: {kvp.Value.timeInBooth}");
        // }

        boothsLoaded = true;
    }

    public void InitializeDict() {
        foreach(BoothManager booth in FindObjectsOfType<BoothManager>()) {
            //add an instance to the value of the dict depending on booth type
            try {
                AssessmentManager assessmentManager = booth.GetComponent<AssessmentManager>();
                if (assessmentManager != null) {
                    /*int assessmentType = (int)assessmentManager.assessmentType;
                    if (GameManager.AmTeacher) {
                        assessmentType = BoothTypeToInt(booth.boothName);
                    }*/
                    switch (assessmentManager.assessmentType) {
                        case AssessmentManager.AssessmentType.assignment:
                            Debug.Log("PS: " + booth.boothName + " added as AssessmentStats()");
                            boothStats.Add(booth.boothName, new AssessmentStats());
                            break;
                        case AssessmentManager.AssessmentType.quiz:
                        case AssessmentManager.AssessmentType.test:
                            Debug.Log("PS: " + booth.boothName + " added as QuizAndTestStats()");
                            boothStats.Add(booth.boothName, new QuizAndTestStats());
                            break;
                        default:
                            Debug.Log("PS: " + booth.boothName + " added as BoothStats()1");
                            boothStats.Add(booth.boothName, new BoothStats());
                            break;
                    }
                } 
                else {
                    Debug.Log("PS: " + booth.boothName + " added as BoothStats()2");
                    boothStats.Add(booth.boothName, new BoothStats());
                }
            }
            catch (Exception e) {
                Debug.Log($"The name {booth.boothName} already exists");
            }
        }
    }

    int BoothTypeToInt(string boothName) {
        string boothType = GameManager.BoothNamesAndTypes[boothName];
        switch(boothType) {
            case "Assignment":
                return(int)AssessmentManager.AssessmentType.assignment;
            case "Quiz":
                return(int)AssessmentManager.AssessmentType.quiz;
            case "Test":
                return(int)AssessmentManager.AssessmentType.test;
            default:
                return(int)AssessmentManager.AssessmentType.none;
        }
    }
    #endregion

    #region Setters
    public void SetTimeInBooth(string boothName, int time) {
        Debug.LogWarning(boothName);
        boothStats[boothName].timeInBooth = time;
    }
    public void SetPercentageScore(string boothName, float score) {
        ((AssessmentStats)boothStats[boothName]).percentageScore = score;
    }
    public void SetCompleted(string boothName, bool completed) {
        try {
            ((AssessmentStats)boothStats[boothName]).completed = completed;
        }
        catch (Exception e) {}
    }
    public void SetTimeTaken(string boothName, float time) {
        ((AssessmentStats)boothStats[boothName]).timeTaken = time;
    }
    public void SetNumQuestionsTimedOut(string boothName, int numQuestions) {
        ((QuizAndTestStats)boothStats[boothName]).questionsTimedOut = numQuestions;
    }
    public void IncrementNumQuestionsTimedOut(string boothName) {
        ((QuizAndTestStats)boothStats[boothName]).questionsTimedOut++;
    }
    #endregion

    #region Getters
    public int GetTimeInBooth(string boothName) {
        return boothStats[boothName].timeInBooth;
    }
    public float GetPercentageScore(string boothName) {
        try {
            return ((AssessmentStats)boothStats[boothName]).percentageScore;
        }
        catch (Exception e) {
            return -1f;
        }
    }
    public bool GetCompletedState(string boothName) {
        try {
            return ((AssessmentStats)boothStats[boothName]).completed;
        }
        catch (Exception e) {
            return false;
        }
    }
    public float GetTimeTaken(string boothName) {
        try {
            return ((AssessmentStats)boothStats[boothName]).timeTaken;
        }
        catch (Exception e) {
            return -1f;
        }
    }
    public int GetNumQuestionsTimedOut(string boothName) {
        try {
            return ((QuizAndTestStats)boothStats[boothName]).questionsTimedOut;
        }
        catch (Exception e) {
            return -1;
        }
    }
    #endregion

    #region Time incrementation
    IEnumerator IncrementBoothTime() 
    //Increments every second
    {
        while (true) {
            yield return new WaitForSeconds(1f);
            boothStats[boothInsideOf].timeInBooth++;
            //Debug.LogError($"Total time inside of {boothInsideOf}: {boothStats[boothInsideOf].timeInBooth}");
        }
    }

    IEnumerator IncrementOutsideBoothTime() 
    //Increments every second
    {
        while (true) {
            yield return new WaitForSeconds(1f);
            timeOutsideBooths++;
            //Debug.LogError($"Total time outside of booths: {timeOutsideBooths}");
        }
    }

    void OnTriggerStay(Collider collider) 
    //for when the teacher loads the xml, and the student is in a booth that begins unlocked
    {
        if (collider.tag == "BoothZone" && boothsLoaded) {
            if (!inBooth) {
                inBooth = true;
                boothInsideOf = collider.transform.parent.parent.GetComponent<BoothManager>().boothName;
                incrementBoothTime = StartCoroutine(IncrementBoothTime());
                StopCoroutine(incrementOutsideBoothTime);
            }
        }
    }

    void OnTriggerEnter(Collider collider) 
    //starts incrementing time within the booth that the collider is the grandchildchild of every second, 
    //and stops incrementing the counter for being outside of booths
    {
        if (collider.tag == "BoothZone" && boothsLoaded) {
            inBooth = true;
            boothInsideOf = collider.transform.parent.parent.GetComponent<BoothManager>().boothName;
            incrementBoothTime = StartCoroutine(IncrementBoothTime());
            StopCoroutine(incrementOutsideBoothTime);
        }
    }

    void OnTriggerExit(Collider collider)
    //Stops incrementing the timer for being in a booth
    {
        if (inBooth && boothsLoaded) {
            inBooth = false;
            incrementOutsideBoothTime = StartCoroutine(IncrementOutsideBoothTime());
            StopCoroutine(incrementBoothTime);
        }
    }
    #endregion
}