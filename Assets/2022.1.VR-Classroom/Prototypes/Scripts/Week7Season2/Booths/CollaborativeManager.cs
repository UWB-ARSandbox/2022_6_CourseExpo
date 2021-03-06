using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ASL;
using System.Linq;

public class CollaborativeManager : MonoBehaviour
{
    public BoothManager _myBooth;
    public ASLObject m_ASLObject;
    public AssessmentManager _myAssessmentManager;
    public KeyboardEntry _myKeyBoardEntry;
    public GameObject TimerText;
    public BoothZoneManager BZM;
    public void SetBZM(BoothZoneManager BoothZM){BZM = BoothZM;}
    private GameObject Player;
    public TeleportTrigger TPChannelTrigger;

    public GroupManager m_GroupManager;

    //Definable number of students
    public int MaxStudents;
    //maybe float[] and track students ids
    public List<float> curStudents = new List<float>();
    public int RandomVal;

    public bool TimerStarted = false;
    public float StartTimer;
    public float currCountdownValue;
    public bool QuizActive = false;

    public InputField txtField;

    #region MessageHeaders
    //Answer inputs - dont necessarily need to be message headers could use the same header for all depends on how much information we want to send
    public const float GroupQuizStarted = 99;
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
    public const float SendTextField = 111;
    public const float BackSpace = 112;

    public const float FinalSubmit = 113;
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
        Player = GameObject.Find("FirstPersonPlayer(Clone)");
        TPChannelTrigger = gameObject.GetComponentInChildren<TeleportTrigger>();
        FinalSubmitButton.GetComponent<Button>().onClick.AddListener(() => SendInput(FinalSubmit));
        m_GroupManager = GameObject.Find("GroupsUI").GetComponent<GroupManager>();
    }

    public void SetMaxStudents(int maxStudents){
        MaxStudents = maxStudents;
    }

    public IEnumerator KickPlayerOut(){
        Player.transform.GetComponent<CharacterController>().enabled = false;
        Player.transform.position = gameObject.GetComponentInChildren<LockToggle>().transform.position + (Vector3.up);
        Player.transform.GetComponent<CharacterController>().enabled = true;
        yield return null;
    }

    public void DisableBooth(){
        if(!GameManager.AmTeacher){
            //_myBooth.lockToggle.Lock();
            if(!_myAssessmentManager.walls.gameObject.active)
                _myAssessmentManager.walls.gameObject.SetActive(true);
            _myAssessmentManager.pnl_Start.SetActive(false);
        }
        //kick users out that are in the booth but not in the curStudents list
        //IE compare the curStudentsList against the one in BoothZoneManager currentUsers
        //if the user is not in curStudents and they are in BoothZoneManager remove them
        if(!curStudents.Contains((float)GameManager.MyID) && BZM.currentUsers.Contains(GameManager.players[GameManager.MyID])){
            //teleport user away
            StartCoroutine(KickPlayerOut());
        }
    }

    public void EnableBooth(){
        //_myBooth.lockToggle.Unlock();
        QuizActive = false;
        //if(!_myAssessmentManager.pnl_Start.active)
        if(!curStudents.Contains(GameManager.MyID) && !_myAssessmentManager.AssessmentCompleted)
            _myAssessmentManager.pnl_Start.SetActive(true);
        //if(_myAssessmentManager.walls.gameObject.active)
            _myAssessmentManager.walls.gameObject.SetActive(false);
        if(TPChannelTrigger != null){
            TPChannelTrigger.Active = false;
        }
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
        currCountdownValue = countdownValue;
        while (currCountdownValue > 0)
        {
            Debug.Log("Countdown: " + currCountdownValue);
            TimerText.GetComponent<TextMesh>().text = "Starting in: "+currCountdownValue;
            yield return new WaitForSeconds(1.0f);
            currCountdownValue--;
        }
        TimerText.SetActive(false);
        QuizActive = true;
        TimerStarted = false;
        if(curStudents.Contains((float)GameManager.MyID)){
            _myAssessmentManager.StartAssessment();
            if(TPChannelTrigger != null){
                TPChannelTrigger.Active = true;
            }
        }
        else
            DisableBooth();
            //Lock room until assessment is finished 
        SetupVoteList();
        yield return null;
    }

    public void StartGroupQuiz(){
        if(m_GroupManager.MyGroup != null && m_GroupManager.MyGroup.members.Count <= _myAssessmentManager.NumberOfConcurrentUsers && curStudents.Count == 0){
            if(!GameManager.AmTeacher && !curStudents.Contains(GameManager.MyID) && !GameManager.isTakingAssessment){
                Debug.Log("Starting Group Quiz for: " + m_GroupManager.MyGroup.groupNumber);
                // AnnouncementDisplay Dspl = AnnouncementManager.pnl_PCAnnouncement;
                // Dspl.DisplayAnnouncement("Starting Group Quiz for: " + m_GroupManager.MyGroup.groupNumber);
                _myAssessmentManager.pnl_Start.SetActive(false);
                List<float> NewFloats = new List<float>();
                NewFloats.Add(-1);
                NewFloats.Add(GroupQuizStarted);
                NewFloats.Add((float)GameManager.MyID);
                var FloatsArray = NewFloats.ToArray();
                m_ASLObject.SendAndSetClaim(() => {
                    m_ASLObject.SendFloatArray(FloatsArray);
            });
            _myAssessmentManager.walls.gameObject.SetActive(true);
            }    
        }
        else
            SendStartMessage();
    }

    #region Sending Floats
    public void CurTestFinished(){
        if(curStudents[0] == GameManager.MyID){//make sure to only run once
            List<float> NewFloats = new List<float>();  
            NewFloats.Add(-1);
            NewFloats.Add(TestFinished);
            var FloatsArray = NewFloats.ToArray();
            m_ASLObject.SendAndSetClaim(() => {
                m_ASLObject.SendFloatArray(FloatsArray);
            });    
        }
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
        if(!GameManager.AmTeacher && !curStudents.Contains(GameManager.MyID) && _myAssessmentManager.pnl_Start.active 
            && !_myAssessmentManager.AssessmentCompleted && !GameManager.isTakingAssessment){
            GameManager.isTakingAssessment = true;
            _myAssessmentManager.pnl_Start.SetActive(false);
            List<float> NewFloats = new List<float>();
            NewFloats.Add(-1);
            NewFloats.Add(QuizStarted);
            NewFloats.Add((float)GameManager.MyID);
            var FloatsArray = NewFloats.ToArray();
            m_ASLObject.SendAndSetClaim(() => {
                m_ASLObject.SendFloatArray(FloatsArray);
            });
            _myAssessmentManager.walls.gameObject.SetActive(true);
        }    
    }
    //expected input should be a float between the values of 101 - 107
    //Expected output _f[curStudents[i]][101f-107f][GameManager.MyID]
    //Send own ID is for the voting system to keep track of what input
    //has been pressed locally
    public void SendInput(float _f){
        if(_f == SendTextField){
            SendText(txtField.text);
        }
        else{
            List<float> NewInput = new List<float>();
            NewInput.Add(0);
            NewInput.Add(_f);
            NewInput.Add((float)GameManager.MyID);
            for(int i = 0; i< curStudents.Count; i++){
                NewInput[0] = curStudents[i];
                var FloatsInput = NewInput.ToArray();
                m_ASLObject.SendAndSetClaim(() => {
                    m_ASLObject.SendFloatArray(FloatsInput);
                });
            }
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
    //should be used to set the txtField for all clients if the user was using a keyboard
    public void SendText(string ToSend){
        for(int i = 0; i< curStudents.Count; i++){        
            List<float> NewInput = new List<float>();
            NewInput.Add(curStudents[i]);//_f[0]
            NewInput.Add(SendTextField);//_f[1]
            NewInput.Add((float)GameManager.MyID);//_f[2]
            NewInput.Add(ToSend.Length);//_f[3]
            NewInput.AddRange(stringToFloats(ToSend));
            var FloatsInput = NewInput.ToArray();
            m_ASLObject.SendAndSetClaim(() => {
                m_ASLObject.SendFloatArray(FloatsInput);
            });
        }
    }
    public void SendBackSpace(){
        for(int i = 0; i< curStudents.Count; i++){        
            List<float> NewInput = new List<float>();
            NewInput.Add(curStudents[i]);
            NewInput.Add(BackSpace);
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
                    MaxStudents = _myAssessmentManager.NumberOfConcurrentUsers;
                    curStudents.Add(_f[2]);
                    Debug.Log("Student ID:" +_f[2] +"started test");
                    SyncedTimer();
                    break;
                }
                case GroupQuizStarted:{
                    MaxStudents = _myAssessmentManager.NumberOfConcurrentUsers;
                    if(GameManager.MyID != (int)_f[2] && !curStudents.Contains((float)GameManager.MyID) && m_GroupManager.MyGroup != null 
                        && m_GroupManager.MyGroup.members.Contains(GameManager.players[(int)_f[2]])&& !_myAssessmentManager.AssessmentCompleted && !GameManager.isTakingAssessment){
                        curStudents.Add(_f[2]);
                        GameManager.isTakingAssessment = true;
                        Debug.Log("Student ID:" +_f[2] +"started test");
                        SyncedTimer();
                        //teleport user infront of lectern
                        GameObject player = FindObjectOfType<XpoPlayer>().gameObject;
                        player.GetComponent<CharacterController>().enabled = false;
                        player.transform.SetParent(transform, true);
                        player.transform.localPosition = new Vector3(3.5f, 1.115f, 0);
                        player.transform.SetParent(null, true);
                        player.GetComponent<CharacterController>().enabled = true;
                        SendStartMessage();
                    }
                    break;
                }
                case ShortAnswerUpdate:{
                    //change to sendTextField
                    //txtField.text += (char)(int)_f[2];
                    break;   
                }
                case NewRandom:{
                    RandomVal = (int)_f[2];
                    break;
                }
                case TestFinished:{
                    EnableBooth();
                    curStudents.Clear();
                    curStudents.TrimExcess();
                    _myAssessmentManager.walls.gameObject.SetActive(false);
                    break;
                }
                case SendTextField:{
                    int length = (int)_f[3];
                    string NewText = "";
                    for (int i = 4; i < length + 4; i++) {
                        NewText += (char)(int)_f[i];
                    }
                    if(_f[2] == GameManager.MyID)
                        txtField.text = NewText;
                    CreateShortAnswerPrefab(NewText, _f[2]);
                    break;
                }
                case BackSpace:{
                    //txtField.text = txtField.text.Remove(txtField.text.Length - 1);
                    break;
                }
                case FinalSubmit:{
                    Debug.Log(GameManager.players[(int)_f[2]] + " Has hit the final submit button");
                    FinalSubmitBool[GameManager.players[(int)_f[2]]] = true;
                    CheckVotes();
                    break;
                }
                default:{
                    if(StudentVotes.ContainsKey(GameManager.players[(int)_f[2]]))
                        StudentVotes[GameManager.players[(int)_f[2]]] = _f[1];
                    else
                        StudentVotes.Add(GameManager.players[(int)_f[2]],_f[1]);            
                    CheckVotes();
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

    #region Voting System 
    public Dictionary<string, float> StudentVotes = new Dictionary<string,float>();
    public Dictionary<string, GameObject> VotePrefabs = new Dictionary<string, GameObject>();
    public GameObject VotePrefab;
    public GameObject voteAreaOptionA;
    public GameObject voteAreaOptionB;
    public GameObject voteAreaOptionC;
    public GameObject voteAreaOptionD;
    public GameObject voteAreaOptionTrue;
    public GameObject voteAreaOptionFalse;
    public GameObject voteAreaOptionSubmit;
    //important to check the shortanswers against each other
    public Dictionary<string, GameObject> ShortAnswer = new Dictionary<string, GameObject>();
    public GameObject TextAreaOptionSubmit;
    public GameObject ShortAnswerPrefab;

    //intent is to check to see that all students have hit that final submit button
    public Dictionary<string, bool> FinalSubmitBool = new Dictionary<string, bool>();
    public GameObject FinalSubmitButton;

    public void SubmitTextButtonClick(Button buttonObj){
        string ToSend = buttonObj.gameObject.GetComponentInChildren<TMPro.TextMeshProUGUI>().text;
        SendText(ToSend);

        StartCoroutine(SendInputDelayed());
    }
    public IEnumerator SendInputDelayed(){
        yield return new WaitForSeconds(1.0f);
        SendInput(buttonSubmit);
        yield return null;
    }

    public void SetupVoteList(){
        ClearVotes();
    }
    public void ShowVotes(){
        if(VotePrefabs.ContainsKey(GameManager.players[(int)GameManager.MyID])){
            if(VotePrefabs[GameManager.players[(int)GameManager.MyID]] != null){
                Debug.Log("I have casted my vote");
                foreach(KeyValuePair<string, GameObject> res in VotePrefabs){
                    if(res.Value != null)
                        res.Value.SetActive(true);
                }
            }
            if(ShortAnswer.ContainsKey(GameManager.players[(int)GameManager.MyID])){
                if(ShortAnswer[GameManager.players[(int)GameManager.MyID]] != null){
                    foreach(KeyValuePair<string, GameObject> res in ShortAnswer){
                    if(res.Value != null)
                        res.Value.SetActive(true);
                    }
                }
            }
        }
    }

    public void CreateShortAnswerPrefab(string text, float student_id){
        string studentName = GameManager.players[(int)student_id];
        if(ShortAnswer.ContainsKey(studentName)){
            if(ShortAnswer[studentName] != null)
                Destroy(ShortAnswer[studentName]);
            GameObject ShortAnswerP = (GameObject)Instantiate(ShortAnswerPrefab, TextAreaOptionSubmit.transform, false);
            ShortAnswerP.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = text;
            ShortAnswer[studentName] = ShortAnswerP;
            ShortAnswerP.SetActive(false);
            Button btn_Submit = ShortAnswerP.GetComponent<Button>();
            btn_Submit.onClick.AddListener(() => SubmitTextButtonClick(btn_Submit));
        }
        else{
            GameObject ShortAnswerP = (GameObject)Instantiate(ShortAnswerPrefab, TextAreaOptionSubmit.transform, false);
            ShortAnswerP.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = text;
            ShortAnswer.Add(studentName, ShortAnswerP);
            ShortAnswerP.SetActive(false);
            Button btn_Submit = ShortAnswerP.GetComponent<Button>();
            btn_Submit.onClick.AddListener(() => SubmitTextButtonClick(btn_Submit));
        }
    }

    public void CreateVotePrefab(string student){
        Debug.Log("Attempting to create vote for: " +student);
        bool studentExists = false;
        if(VotePrefabs.ContainsKey(student)){
            studentExists = true;
            if(VotePrefabs[student] != null)
                Destroy(VotePrefabs[student]);
        }
        if(!studentExists){
            switch(StudentVotes[student]){
                case buttonA:{
                    GameObject Vote = (GameObject)Instantiate(VotePrefab,voteAreaOptionA.transform,false);
                    if(Vote != null){
                        Vote.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = student;
                        VotePrefabs.Add(student, Vote);
                        Vote.SetActive(false); 
                    }
                    else
                    {
                        Debug.Log("Failed to Create VotePrefab");
                    }
                    break;   
                }
                case buttonB:{
                    GameObject Vote = (GameObject)Instantiate(VotePrefab,voteAreaOptionB.transform,false);
                    if(Vote != null){
                    Vote.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = student;
                    VotePrefabs.Add(student, Vote);
                    Vote.SetActive(false); 
                    }
                    else
                        Debug.Log("Failed to Create VotePrefab");
                    break;   
                }
                case buttonC:{
                    
                    GameObject Vote = (GameObject)Instantiate(VotePrefab,voteAreaOptionC.transform,false);
                    if(Vote != null){
                    Vote.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = student;
                    VotePrefabs.Add(student, Vote);
                    Vote.SetActive(false); 
                    }
                    else
                        Debug.Log("Failed to Create VotePrefab");
                    break;   
                }
                case buttonD:{
                    GameObject Vote = (GameObject)Instantiate(VotePrefab,voteAreaOptionD.transform,false);
                    if(Vote != null){
                    Vote.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = student;
                    VotePrefabs.Add(student, Vote);
                    Vote.SetActive(false); 
                    }
                    else
                        Debug.Log("Failed to Create VotePrefab");
                    break;   
                }
                case buttonTrue:{
                    GameObject Vote = (GameObject)Instantiate(VotePrefab,voteAreaOptionTrue.transform,false);
                    if(Vote != null){
                    Vote.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = student;
                    VotePrefabs.Add(student, Vote);
                    Vote.SetActive(false); 
                    }
                    else
                        Debug.Log("Failed to Create VotePrefab");
                    break;   
                }
                case buttonFalse:{
                    GameObject Vote = (GameObject)Instantiate(VotePrefab,voteAreaOptionFalse.transform,false);
                    if(Vote != null){
                    Vote.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = student;
                    VotePrefabs.Add(student, Vote);
                    Vote.SetActive(false); 
                    }
                    else
                        Debug.Log("Failed to Create VotePrefab");
                    break;   
                }
                case buttonSubmit:{
                    GameObject Vote = (GameObject)Instantiate(VotePrefab,voteAreaOptionSubmit.transform,false);
                    if(Vote != null){
                    Vote.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = student;
                    VotePrefabs.Add(student, Vote);
                    Vote.SetActive(false);                     
                    }
                    else
                        Debug.Log("Failed to Create VotePrefab");
                    break;   
                }
            }
        }
        else{
            switch(StudentVotes[student]){
                case buttonA:{
                    GameObject Vote = (GameObject)Instantiate(VotePrefab,voteAreaOptionA.transform,false);
                    if(Vote != null){
                    Vote.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = student;
                    VotePrefabs[student] = Vote; 
                    Vote.SetActive(false);                    
                    }
                    else
                        Debug.Log("Failed to Create VotePrefab");
                    break;   
                }
                case buttonB:{
                    GameObject Vote = (GameObject)Instantiate(VotePrefab,voteAreaOptionB.transform,false);
                    if(Vote != null){
                    Vote.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = student;
                    VotePrefabs[student] = Vote;
                    Vote.SetActive(false);                     
                    }
                    else
                        Debug.Log("Failed to Create VotePrefab");
                    break;   
                }
                case buttonC:{
                    GameObject Vote = (GameObject)Instantiate(VotePrefab,voteAreaOptionC.transform,false);
                    if(Vote != null){
                    Vote.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = student;
                    VotePrefabs[student] = Vote;
                    Vote.SetActive(false);                     
                    }
                    else
                        Debug.Log("Failed to Create VotePrefab");
                    break;   
                }
                case buttonD:{
                    GameObject Vote = (GameObject)Instantiate(VotePrefab,voteAreaOptionD.transform,false);
                    if(Vote != null){
                    Vote.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = student;
                    VotePrefabs[student] = Vote;
                    Vote.SetActive(false);                     
                    }
                    else
                        Debug.Log("Failed to Create VotePrefab");
                    break;   
                }
                case buttonTrue:{
                    GameObject Vote = (GameObject)Instantiate(VotePrefab,voteAreaOptionTrue.transform,false);
                    if(Vote != null){
                    Vote.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = student;
                    VotePrefabs[student] = Vote;
                    Vote.SetActive(false);                     
                    }
                    else
                        Debug.Log("Failed to Create VotePrefab");
                    break;   
                }
                case buttonFalse:{
                    GameObject Vote = (GameObject)Instantiate(VotePrefab,voteAreaOptionFalse.transform,false);
                    if(Vote != null){
                    Vote.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = student;
                    VotePrefabs[student] = Vote;
                    Vote.SetActive(false);                     
                    }
                    else
                        Debug.Log("Failed to Create VotePrefab");
                    break;   
                }
                case buttonSubmit:{
                    GameObject Vote = (GameObject)Instantiate(VotePrefab,voteAreaOptionSubmit.transform,false);
                    if(Vote != null){
                    Vote.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = student;
                    VotePrefabs[student] = Vote;
                    Vote.SetActive(false);                    
                    }
                    else
                        Debug.Log("Failed to Create VotePrefab");
                    break;   
                }
            }
        }
    }

    public void ClearVotes(){
        StudentVotes.Clear();
        foreach(KeyValuePair<string, GameObject> res in VotePrefabs){
            if(res.Value != null)
                Destroy(res.Value);
        }
        VotePrefabs.Clear();
        for(int i = 0;i < curStudents.Count;i++){
            StudentVotes.Add(GameManager.players[(int)curStudents[i]],(float)i);   
        }
        foreach(KeyValuePair<string, GameObject> res in ShortAnswer){
            if(res.Value != null)
                Destroy(res.Value);
        }
        ShortAnswer.Clear();
        FinalSubmitBool.Clear();
        FinalSubmitButton.SetActive(false);
        for(int i = 0; i < curStudents.Count; i++){
                FinalSubmitBool.Add(GameManager.players[(int)curStudents[i]],false);
        }
    }

    public void CheckVotes(){
        for(int i = 0;i < curStudents.Count;i++){
            CreateVotePrefab(GameManager.players[(int)curStudents[i]]);
        }
        ShowVotes();

        var distinctList = StudentVotes.Values.Distinct().ToList();
        Debug.Log("There are: "+distinctList.Count+"Distinct Votes");
        for(int i = 0; i< distinctList.Count; i++){ 
            Debug.Log("Current Votes are for: " +distinctList[i]);
        }
        if(distinctList.Count <= 1){
            if(distinctList[0] == buttonSubmit){//check that the actual submission texts between each student are equal
                 var distinctShortAnswer = ShortAnswer.Values.Distinct().ToList();
                 for(int i = 0; i < distinctShortAnswer.Count; i++){
                    if(distinctShortAnswer[0].GetComponentInChildren<TMPro.TextMeshProUGUI>().text != distinctShortAnswer[i].GetComponentInChildren<TMPro.TextMeshProUGUI>().text)
                        return;
                 }
            }
            Debug.Log("Votes are unanimous");
            
            FinalSubmitButton.SetActive(true);
            for(int i = 0; i < curStudents.Count; i++){
                if(FinalSubmitBool[GameManager.players[(int)curStudents[i]]] == false)
                    return;
            }

            FinalSubmitButton.SetActive(false);
            //Spawn final submit button and when that is pressed submit inputs
            //When submit button is pressed check votes again to make sure they are still distinct
            SubmitInputs(distinctList[0]);
            SetupVoteList();//clear old votes
        }
        else{
            Debug.Log("Votes are divided");
            FinalSubmitButton.SetActive(false);
            for(int i = 0; i < curStudents.Count; i++){
                if(FinalSubmitBool.ContainsKey(GameManager.players[(int)curStudents[i]])){
                    FinalSubmitBool[GameManager.players[(int)curStudents[i]]] = false;
                }
                else
                    FinalSubmitBool.Add(GameManager.players[(int)curStudents[i]],false);
            }
        }
    }

    public void SubmitInputs(float _f){
        switch(_f){
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
        }
    }

    #endregion
    // Update is called once per frame

    //function to remove the user from the CurStudents list upon disconnection or leaving the booth
    //if the user manages to leave the booth and is not disconnected, not sure how to reset the test
    public IEnumerator CheckForUserDisconnection(){
        foreach (float _f in curStudents){
            //Use a different system to check if the users are still in the room -- since BZM.currentUsers will not be updated if the user disconnects
            //try to look at the ghosts or tie into the disconnect function
            if(!BZM.currentUsers.Contains(GameManager.players[(int)_f])){
                if(_f == (float)GameManager.MyID){
                    //reset test since i managed to escape
                }
                curStudents.Remove(_f);
                yield return null;
            }
        }
        yield return new WaitForSeconds(5.0f);
    }
    void Update()
    {
        if(curStudents.Count >= MaxStudents && _myAssessmentManager.pnl_Start.active){
            //disable start button and lock booth
            _myAssessmentManager.pnl_Start.SetActive(false);
                //_myBooth.lockToggle.Lock();
        }
        //StartCoroutine(CheckForUserDisconnection());
    }
}
