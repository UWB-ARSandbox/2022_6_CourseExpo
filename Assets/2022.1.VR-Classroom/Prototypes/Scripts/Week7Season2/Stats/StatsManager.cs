using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ASL;

//For message sending and receiving, as well as the storage of all players' stats for teacher viewing
public class StatsManager : MonoBehaviour
{
    enum BoothStatType {
        timeInBooth,
        percentageScore,
        timeTaken,
        completed,
        questionsTimedOut
    }

    public Dictionary<string, PersonalStats> studentStats = new Dictionary<string, PersonalStats>();
    PersonalStats myStats;
    ASLObject m_ASLObject;
    // Start is called before the first frame update
    void Start()
    {
        m_ASLObject = GetComponent<ASLObject>();
        m_ASLObject._LocallySetFloatCallback(FloatReceive);

        //If teacher, store stats of all students
        if (GameManager.AmTeacher) {
            foreach (var studentIdToName in GameLiftManager.GetInstance().m_Players) {
                if (studentIdToName.Key != 1) { //Don't add teacher/host to list
                    var newObject = new GameObject("PersonalStats_for_" + studentIdToName.Value);
                    studentStats.Add(studentIdToName.Value, newObject.AddComponent<PersonalStats>());
                    Debug.LogError($"ID: {studentIdToName.Key}, Name: {studentIdToName.Value}");
                }
            }
        }
        else {
            StartCoroutine(FindMyStats());
        }
    }

    //Get my PersonalStats object to read stats from
    IEnumerator FindMyStats() {
        while (myStats == null) {
            myStats = FindObjectOfType<PersonalStats>();
            yield return new WaitForSeconds(0.25f);
        }
        StartCoroutine(PeriodicallySendStats());
    }

    public void InitializeDicts() {
        foreach(var kvp in studentStats) {
            kvp.Value.StartInitializeDict();
        }
    }

    #region Sending
    //Message Headers
    //Chosen according to their value on a phone keypad in decimal
    private const float STATS = 78287;

    //The plan:
    //For students, this script will send their stats to the teacher appproximately every 30 seconds
    //and for certain stat changes or events (such as completing a quiz/test/assessment)

    //Message Composition:
    //In _f[0] the header designating the message as being STATS (78287 for STATS on the keypad)
    //In _f[1] a number to represent the type of stat to send:
        //0 = Booth Stat
    //In _f[2] a number to represent the id of the student the stat is coming from

    //(0) for Booth Stats
    //In _f[3] the type of booth stat:
        //0 = timeInBooth
        //1 = percentageScore
        //2 = timeTaken
        //3 = completed
        //4 = questionsTimedOut
    //In _f[4] number/value associated with the stat 
    //In _f[5+] the name of the booth

    IEnumerator PeriodicallySendStats() {
        //Sends approximately every 10 seconds
        while (myStats != null) {
            yield return new WaitForSeconds(10f);
            SendStats();
        }
    }

    void SendStats() {
        //Sends to the teacher all the stats a player has for each booth
        foreach(var boothStats in myStats.boothStats) {
            //Fill unchanging portions of the array
            float[] statInfoToSend = new float[5 + boothStats.Key.Length];
            statInfoToSend[0] = 78287; //header
            statInfoToSend[1] = 0; //stat type
            statInfoToSend[2] = GameManager.MyID; //player's id
            for (int i = 5; (i - 5) < boothStats.Key.Length; i++) { //booth name
                statInfoToSend[i] = (float)(int)boothStats.Key[i - 5];
            }
            Debug.LogError("SENT " + statInfoToSend.ToString());

            //Send information
            m_ASLObject.SendAndSetClaim(() => {
                for (int i = 0; i < Enum.GetNames(typeof(BoothStatType)).Length; i++) {
                    statInfoToSend[3] = i; //booth stat type
                    statInfoToSend[4] = GetSpecificBoothStatAsFloat(boothStats.Key, i); //stat data
                    
                    //only send the data if it exists for the particular booth
                    //-1 is returned on an invalid getter call
                    //and no need to send 0s since values default to a 0-like state
                    if (statInfoToSend[4] > 0) {
                        m_ASLObject.SendFloatArray(statInfoToSend);
                    }
                }
            }, -1);
        }
    }

    float GetSpecificBoothStatAsFloat(string boothName, int statType) {
        //for getting a specific kind of stat from a specific booth, in the form of a float to be used for sending
        switch (statType) {
            case (int)BoothStatType.timeInBooth:
                return (float)myStats.GetTimeInBooth(boothName);

            case (int)BoothStatType.percentageScore:
                return myStats.GetPercentageScore(boothName);

            case (int)BoothStatType.timeTaken:
                return myStats.GetTimeTaken(boothName);

            case (int)BoothStatType.completed:
                return (float)(myStats.GetCompletedState(boothName) ? 1 : 0);

            case (int)BoothStatType.questionsTimedOut:
                return (float)myStats.GetNumQuestionsTimedOut(boothName);

            default:
                return -1f;
        }
    }

    #endregion

    #region Receiving

    public void FloatReceive(string _id, float[] _f) {
        if (GameManager.AmTeacher) {
            switch (_f[0]) {
                case STATS:
                    Debug.LogError("RECIEVED " + _f.ToString());
                    StoreReceivedStats(_f);
                    break;
            }
        }
    }

    void StoreReceivedStats(float[] _f) {
        string boothName = "";
        for (int i = 5; i < _f.Length; i++) {
            boothName += (char)(int)_f[i];
        }
        string playerName = GameLiftManager.GetInstance().m_Players[(int)_f[2]];
        SetSpecificBoothStatForStudent(playerName, boothName, (int)_f[3], _f[4]);
    }    
    
    void SetSpecificBoothStatForStudent(string playerName, string boothName, int statType, float statValue) {
        //for getting a specific kind of stat from a specific booth, in the form of a float to be used for sending
        switch (statType) {
            case (int)BoothStatType.timeInBooth:
                studentStats[playerName].SetTimeInBooth(boothName, (int)statValue);
                break;
            case (int)BoothStatType.percentageScore:
                studentStats[playerName].SetPercentageScore(boothName, statValue);
                break;
            case (int)BoothStatType.timeTaken:
                studentStats[playerName].SetTimeTaken(boothName, statValue);
                break;
            case (int)BoothStatType.completed:
                studentStats[playerName].SetCompleted(boothName, statValue == 1 ? true : false);
                break;
            case (int)BoothStatType.questionsTimedOut:
                studentStats[playerName].SetNumQuestionsTimedOut(boothName, (int)statValue);
                break;
        }
    }

    #endregion
}