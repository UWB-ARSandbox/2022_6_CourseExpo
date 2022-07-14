using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ASL;

public class CollaborativeManager : MonoBehaviour
{

    public BoothManager _myBooth;
    public ASLObject m_ASLObject;
    public AssessmentManager _myAssessmentManager;
    public KeyboardEntry _myKeyBoardEntry;
    public GameObject TimerText;
    public BoothZoneManager BZM;
    public void SetBZM(BoothZoneManager BoothZM){BZM = BoothZM;}

    //Definable number of students
    public int MaxStudents;
    //maybe float[] and track students ids
    public List<float> curStudents = new List<float>();
    public int RandomVal;

    public bool TimerStarted = false;
    public float StartTimer;
    public float currCountdownValue;
    public bool QuizActive = false;

    #region MessageHeaders
    //Answer inputs - dont necessarily need to be message headers could use the same header for all depends on how much information we want to send
    public const float QuizStarted = 100;

    public const float buttonA = 101;
    public const float buttonB = 102;
    public const float buttonC = 103;
    public const float buttonD = 104;
    public const float buttonTrue = 105;
    public const float buttonFalse = 106;
    public const float buttonSubmit = 107;
    public const float ShortAnswerUpdate = 108;

    public const float NewRandom = 109;

    public const float TestFinished = 110;

    #endregion
    // Need to sync the randomize result IE need to take the result from the first student in curStudents
    //
    // Start is called before the first frame update
    void Start()
    {
        _myAssessmentManager = gameObject.GetComponent<AssessmentManager>();
        _myAssessmentManager.Set_myCollabManager(this);
        MaxStudents = _myAssessmentManager.NumberOfConcurrentUsers;
        _myBooth = gameObject.GetComponent<BoothManager>();
        m_ASLObject = gameObject.GetComponent<ASLObject>();
        m_ASLObject._LocallySetFloatCallback(FloatReceive);
        if(StartTimer == 0f){
            StartTimer = 15f;
        }
        Debug.Assert(TimerText != null);
    }

    public void SetMaxStudents(int maxStudents){
        MaxStudents = maxStudents;
    }

    public void DisableBooth(){
        _myBooth.lockToggle.Lock();
        //kick users out that are in the booth but not in the curStudents list
        //IE compare the curStudentsList against the one in BoothZoneManager currentUsers
        //if the user is not in curStudents and they are in BoothZoneManager remove them
        if(!curStudents.Contains((float)GameManager.MyID) && BZM.currentUsers.Contains(GameManager.players[GameManager.MyID])){
            //teleport user away
        }
    }

    public void EnableBooth(){
        _myBooth.lockToggle.Unlock();
        _myAssessmentManager.pnl_Start.SetActive(true);
    }

    public void SyncedTimer(){
        if(!TimerStarted && !QuizActive){
            if(curStudents[0] == (float)GameManager.MyID)
                SendNewRandom();
            StartCoroutine(StartCountdown(StartTimer));
        }
    }

    public IEnumerator StartCountdown(float countdownValue)
    {
        TimerStarted = true;
        TimerText.SetActive(true);
        TimerText.GetComponent<TextMesh>().text = "Starting in: "+countdownValue;
        currCountdownValue = countdownValue;
        while (currCountdownValue > 0)
        {
            Debug.Log("Countdown: " + currCountdownValue);
            TimerText.GetComponent<TextMesh>().text = "Starting in: "+countdownValue;
            yield return new WaitForSeconds(1.0f);
            currCountdownValue--;
        }
        TimerText.SetActive(false);
        QuizActive = true;
        TimerStarted = false;
        if(curStudents.Contains((float)GameManager.MyID))
            _myAssessmentManager.StartAssessment();
        else
            DisableBooth();
            //Lock room until assessment is finished 
        yield return null;
    }

    #region Sending Floats
    public void CurTestFinished(){
        List<float> NewFloats = new List<float>();  
        NewFloats.Add(-1);
        NewFloats.Add(TestFinished);
        var FloatsArray = NewFloats.ToArray();
        m_ASLObject.SendAndSetClaim(() => {
            m_ASLObject.SendFloatArray(FloatsArray);
        });    
    }
    //Send new random to each person taking the quiz
    //Intent is to provide a random float value that is the same for all members taking the collaborative quiz
    public void SendNewRandom(){
        List<float> NewRandomList = new List<float>();
        System.Random random = new System.Random();
        NewRandomList.Add(0);
        NewRandomList.Add(NewRandom);
        NewRandomList.Add(random.Next());
        for(int i = 0; i< curStudents.Count; i++){
            NewRandomList[0] = curStudents[i];
            var RandomFloatsArray = NewRandomList.ToArray();
            m_ASLObject.SendAndSetClaim(() => {
                m_ASLObject.SendFloatArray(RandomFloatsArray);
            });
        }
    }
    //Send ID of player that has started quiz IE hit the button
    public void SendStartMessage(){
        List<float> NewFloats = new List<float>();  
        NewFloats.Add(-1);
        NewFloats.Add(QuizStarted);
        NewFloats.Add((float)GameManager.MyID);
        var FloatsArray = NewFloats.ToArray();
        m_ASLObject.SendAndSetClaim(() => {
            m_ASLObject.SendFloatArray(FloatsArray);
        });    
    }
    //expected input should be a float between the values of 101 - 107
    public void SendInput(float _f){
        List<float> NewInput = new List<float>();
        NewInput.Add(0);
        NewInput.Add(_f);
        for(int i = 0; i< curStudents.Count; i++){
            NewInput[0] = curStudents[i];
            var FloatsInput = NewInput.ToArray();
            m_ASLObject.SendAndSetClaim(() => {
                m_ASLObject.SendFloatArray(FloatsInput);
            });
        }
    }
    
    //do I need to send each input or do I need to only send strings?
    public void SendTextUpdates(string Character){
        //use KeyboardEntry.cs to send text updates
        for(int i = 0; i< curStudents.Count; i++){        
            List<float> NewInput = new List<float>();
            NewInput.Add(curStudents[i]);
            NewInput.Add(ShortAnswerUpdate);
            NewInput.AddRange(stringToFloats(Character));
            var FloatsInput = NewInput.ToArray();
            m_ASLObject.SendAndSetClaim(() => {
                m_ASLObject.SendFloatArray(FloatsInput);
            });
        }
    }

    public void FloatReceive(string _id, float[] _f) {
        if((int)_f[0] == GameManager.MyID || _f[0] == -1){
            switch(_f[1]){
                case QuizStarted:{
                    curStudents.Add(_f[2]);
                    Debug.Log("Student ID:" +_f[2] +"started test");
                    SyncedTimer();
                    break;
                }
                case buttonA:{
                    _myAssessmentManager.ReceiveResponse(AssessmentManager.ResponseType.buttonA);
                    break;   
                }
                case buttonB:{
                    _myAssessmentManager.ReceiveResponse(AssessmentManager.ResponseType.buttonB);
                    break;   
                }
                case buttonC:{
                    _myAssessmentManager.ReceiveResponse(AssessmentManager.ResponseType.buttonC);
                    break;   
                }
                case buttonD:{
                    _myAssessmentManager.ReceiveResponse(AssessmentManager.ResponseType.buttonD);
                    break;   
                }
                case buttonTrue:{
                    _myAssessmentManager.ReceiveResponse(AssessmentManager.ResponseType.buttonTrue);
                    break;   
                }
                case buttonFalse:{
                    _myAssessmentManager.ReceiveResponse(AssessmentManager.ResponseType.buttonFalse);
                    break;   
                }
                case buttonSubmit:{
                    _myAssessmentManager.ReceiveResponse(AssessmentManager.ResponseType.buttonSubmit);
                    break;   
                }
                case ShortAnswerUpdate:{
                    //
                    break;   
                }
                case NewRandom:{
                    RandomVal = (int)_f[2];
                    break;
                }
                case TestFinished:{
                    curStudents.Clear();
                    curStudents.TrimExcess();
                    EnableBooth();
                    break;
                }
            }
        }
    }

    //not sure if necessary yet to convert strings to floats
    public static List<float> stringToFloats(string toConvert) {
        var floats = new List<float>();
        foreach (var c in toConvert) {
            floats.Add((int)c);
        }
        return floats;
    }
    #endregion

    // Update is called once per frame
    void Update()
    {
        if(curStudents.Count >= MaxStudents){
            //disable start button and lock booth
            if(!curStudents.Contains(GameManager.MyID)){
                _myAssessmentManager.pnl_Start.SetActive(false);
                _myBooth.lockToggle.Lock();
            }
        }
    }
}
