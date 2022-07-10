using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using ASL;
using CourseXpo;
using Microsoft.MixedReality.Toolkit.Input;

[RequireComponent(typeof(ASLObject))]

public class GameManager : MonoBehaviour {
    #region Variables
    public Vector3 RespawnPoint;
    public Vector3 TeacherRespawnPoint;
    public GameObject FirstPersonPlayer;

    public static Dictionary<int, string> players;
    static int numGhostsInitialized;
    int playerIndex = 0;
    int playerIndex2 = 0;
    List<int> playerIDs = new List<int>();

    private static CourseXpo.Expo _currentExpo = null;
    public static CourseXpo.Expo CurrentExpo => _currentExpo;
    public static int MyID => GameLiftManager.GetInstance().m_PeerId;
    public static Dictionary<string, string> BoothNamesAndDescriptions => nameAndDesc;
    public static Dictionary<string, string> BoothNamesAndTypes => nameToType;

    private static Dictionary<string, string> nameAndDesc = new Dictionary<string, string>();
    private static Dictionary<string, string> nameToType = new Dictionary<string, string>();
    private int countVerify = -1;

    public static bool isTakingAssessment = false;

    public AudioManager _myAudioManager;

    public static bool PlayersVisible => arePlayersVisible;
    private static bool arePlayersVisible = true;

    //Message Headers
    //Chosen according to their value on a phone keypad in decimal
    private const float XML = 965;
    private const float ASMT = 2768;
    private const float CONT = 2960;
    private const float ANCMT = 26268;
    private const float CNNCT = 26628;
    private const float QUIT = 101;

    public void Quit()
    {
        StartCoroutine("QuitHelper");
    }

    IEnumerator QuitHelper()
    {
        float[] m_myFloatArray = new float[2] { QUIT, GameManager.MyID};
        _asl.SendAndSetClaim(() => { _asl.SendFloatArray(m_myFloatArray); });
        yield return new WaitForSeconds(1f);
        Application.Quit();
    }

    public static bool AmTeacher => (MyID == 1);
    private static ASLObject _asl;
    #endregion

    #region Setup
    private void Awake() {
        _asl = GetComponent<ASLObject>();
        if(_myAudioManager == null){
            _myAudioManager = gameObject.GetComponent<AudioManager>();
            if(_myAudioManager == null){
                _myAudioManager = gameObject.AddComponent<AudioManager>();
            } 
        }
        
    }

    // Start is called before the first frame update
    private void Start() {
        players = GameLiftManager.GetInstance().m_Players;
        ASL_PhysicsMasterSingleton.Instance.SetUpPhysicsMaster();
        
        _asl._LocallySetFloatCallback(FloatReceive);

        if (AmTeacher) {
            Instantiate(FirstPersonPlayer, new Vector3(TeacherRespawnPoint.x, TeacherRespawnPoint.y + 1.05f, TeacherRespawnPoint.z), Quaternion.identity);
        } else {
            Instantiate(FirstPersonPlayer, new Vector3(RespawnPoint.x, RespawnPoint.y + 1.05f, RespawnPoint.z + (2 * GameManager.MyID)), Quaternion.identity);
        }

        if (AmTeacher) {
            foreach (int playerID in players.Keys) {
                playerIDs.Add(playerID);

                // ASL.ASLHelper.InstantiateASLObject("FirstPersonPlayer",
                //     new Vector3(RespawnPoint.x, RespawnPoint.y + 1.05f, RespawnPoint.z),
                //     Quaternion.identity, "", "", playerSetUp);

                ASL.ASLHelper.InstantiateASLObject("GhostPlayer",
                    new Vector3(RespawnPoint.x, RespawnPoint.y + 1.05f, RespawnPoint.z),
                    Quaternion.identity, "", "", ghostSetUp);

                RespawnPoint.z += 2;
            }
            StartCoroutine(SendGhostIDs());
        }

        StartCoroutine(AlignBoothNames());
        
    }

    private IEnumerator SendGhostIDs() {
        while (numGhostsInitialized != players.Count) {
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(0.2f);

        bool oneGhostInvalid = true;
        while(oneGhostInvalid) {
            var ghostList = FindObjectsOfType<GhostPlayer>(true);
            foreach (int playerID in players.Keys) {
                float[] m_floatArray = new float[2] { 99, playerID };
                ghostList[playerID - 1].GetComponent<ASLObject>().SendAndSetClaim(() => {
                    ghostList[playerID - 1].GetComponent<ASLObject>().SendFloatArray(m_floatArray);
                }, -1);
            }

            oneGhostInvalid = false;
            foreach (var ghost in ghostList) {
                if (ghost.GetComponent<GhostPlayer>().ownerID == 0) {
                    oneGhostInvalid = true;
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    // private static void playerSetUp(GameObject _gameObject) {
    //     if (ASL_PhysicsMasterSingleton.Instance.IsPhysicsMaster) {
    //         GameManager _this = FindObjectOfType<GameManager>();
    //         int playerID = _this.playerIDs[_this.playerIndex];
    //         _this.playerIndex++;
    //         float[] m_floatArray = new float[2] { 1, playerID };
    //         _gameObject.GetComponent<ASLObject>().SendAndSetClaim(() => {
    //             _gameObject.GetComponent<ASLObject>().SendFloatArray(m_floatArray);
    //         }, -1);
    //     }
    // }

    private static void ghostSetUp(GameObject _gameObject) {
        numGhostsInitialized++;
        // if (ASL_PhysicsMasterSingleton.Instance.IsPhysicsMaster) {
        //     GameManager _this = FindObjectOfType<GameManager>();
        //     int playerID = _this.playerIDs[_this.playerIndex2];
        //     _this.playerIndex2++;
        //     float[] m_floatArray = new float[2] { 99, playerID };
        //     _gameObject.GetComponent<ASLObject>().SendAndSetClaim(() => {
        //         _gameObject.GetComponent<ASLObject>().SendFloatArray(m_floatArray);
        //     }, -1);
        // }
    }
    #endregion

    public static void TogglePlayerVisibility(bool visible) {
        if (!isTakingAssessment) {
            arePlayersVisible = visible;
            foreach (GhostPlayer gp in FindObjectsOfType<GhostPlayer>(true)) {
                if (gp.ownerID != 1 && gp.ownerID != MyID) {
                    gp.gameObject.SetActive(visible);
                }
            }
        }
    }

    public IEnumerator AlignBoothNames() {
        //to let the booth names load in
        //if we go back to loading in the names after loading xml, this will need to change
        yield return new WaitForSeconds(1f); 

        BoothManager[] boothManagers = FindObjectsOfType<BoothManager>();
        boothManagers = boothManagers.Where(booth => booth.boothName != "").ToArray();
        //for (int multiplier = -1; multiplier <= 1; multiplier += 2) {
            //go through twice, and move the boothname a different direction based on iteration, 
            //so that names don't drift higher and higher
            
            for (int i = 0; i < boothManagers.Length; i++) {
                //go through each booth's name
                Vector3 boothTransform1 = transform.TransformPoint(boothManagers[i].transform.Find("Booth/MinimapName").position);
                Rect boothRect1 = boothManagers[i].transform.Find("Booth/MinimapName").GetComponent<RectTransform>().rect;
                
                for (int j = i + 1; j < boothManagers.Length; j++) {
                    //compare the booth with each booth ahead of it in the list
                    Vector3 boothTransform2 = transform.TransformPoint(boothManagers[j].transform.Find("Booth/MinimapName").position);
                    Rect boothRect2 = boothManagers[j].transform.Find("Booth/MinimapName").GetComponent<RectTransform>().rect;
                    
                    //get distances/thresholds
                    float verticalDistance = Mathf.Abs(boothTransform1.x - boothTransform2.x);
                    float verticalThreshold = 0.75f * (boothRect1.height / 2 + boothRect2.height / 2);
                    float horizontalDistance = Mathf.Abs(boothTransform1.z - boothTransform2.z);
                    float horizontalThreshold = boothRect1.width / 2 + boothRect2.width / 2;
                    
                    if (horizontalDistance < horizontalThreshold && verticalDistance < verticalThreshold) 
                    {
                        Debug.Log($"{boothManagers[i].boothName}'s name intersects with {boothManagers[j].boothName}");
                        int boothIndex = j;
                        // if (multiplier > 0) {
                        //     boothIndex = i;
                        // }
                        if (boothTransform1.x > boothTransform2.x) { //other booth is below, so move it further below
                            boothManagers[boothIndex].transform.Find("Booth/MinimapName").position += (new Vector3(boothRect2.height, 0, 0) * -1 * 0.75f);
                        } else {
                            boothManagers[boothIndex].transform.Find("Booth/MinimapName").position += (new Vector3(boothRect2.height, 0, 0) * 0.75f);
                        }
                        
                    }
                }
            }
        //}
    }

    public static void ChooseNewFace() {
        FindObjectOfType<XpoPlayer>().SetFaceTexture();
    }

    public XpoPlayer GetXpoPlayer()
    {
        return FindObjectOfType<XpoPlayer>();
    }

    public static void LoadExpoFile()
    {
        nameAndDesc = new Dictionary<string, string>();
        nameToType = new Dictionary<string, string>();

        var extensions = new[] {
            new SFB.ExtensionFilter("XML Files", "xml"),
            new SFB.ExtensionFilter("Expo Files", "xpo" ),
        };

        static void SetFirstFound(string[] strs)
        {
            if (strs.Length > 0)
            {
                _currentExpo = CourseXpo.Expo.Load(strs[0]);
                if (_currentExpo == null)
                {
                    Debug.LogError("Failed to load Expo file, XML loading error!");
                    return;
                }
            }
            else
            {
                Debug.LogError("Failed to load Expo file, no file chosen!");
                return;
            }

            //do the XML extraction

            //contains all names found in the file, each must be unique for the unlocks system
            var allNames = new HashSet<string>();

            //array of modules: must be converted into unique set of modules for further processing
            var modulesArray = CurrentExpo.modules.Module;
            var modules = new HashSet<module>();

            //inner data
            var assignments = new HashSet<assignment>();
            var quizzes = new HashSet<quiz>();
            var tests = new HashSet<test>();
            var examples = new HashSet<example>();
            var contents = new HashSet<content>();
            var medias = new HashSet<media>();
            var discussions = new HashSet<discussion>();

            foreach (var module in modulesArray)
            {
                //do module uniqueness and name uniqueness checks
                if (!modules.Add(module))
                {
                    Debug.LogError("Duplicate Module!\n\t\"" + module.name + "\" will not be added\n\tDescription: " + module.description);
                }
                else if (!allNames.Add(module.name)) {
                    Debug.LogError("Duplicate Name!\n\t\"" + module.name + "\" will not be added");
                    modules.Remove(module);
                }
                else
                {
                    //add to dict of names and descriptions since we have verified the name is unique
                    nameAndDesc.Add(module.name, module.description);
                    
                    //if passed module checks, move on to inner data types
                    foreach (var assignment in module.assignments.Assignment)
                    {
                        if (!assignments.Add(assignment))
                        {
                            Debug.LogError("Duplicate Assignment!\n\t\"" + assignment.name + "\" will not be added\n\tDescription: " + assignment.description);
                        }
                        else if (!allNames.Add(assignment.name))
                        {
                            Debug.LogError("Duplicate Name!\n\t\"" + assignment.name + "\" will not be added");
                            assignments.Remove(assignment);
                        }
                        else
                        {
                            nameAndDesc.Add(assignment.name, assignment.description);
                        }
                    }

                    if (module.quizzes != null)
                    {
                        foreach (var quiz in module.quizzes.Quiz)
                        {
                            if (!quizzes.Add(quiz))
                            {
                                Debug.LogError("Duplicate Quiz!\n\t\"" + quiz.name +
                                               "\" will not be added\n\tDescription: " + quiz.description);
                            }
                            else if (!allNames.Add(quiz.name))
                            {
                                Debug.LogError("Duplicate Name!\n\t\"" + quiz.name + "\" will not be added");
                                quizzes.Remove(quiz);
                            }
                            else
                            {
                                nameAndDesc.Add(quiz.name, quiz.description);
                            }
                        }
                    }

                    foreach (var test in module.tests.Test)
                    {
                        if (!tests.Add(test))
                        {
                            Debug.LogError("Duplicate Test!\n\t\"" + test.name + "\" will not be added\n\tDescription: " + test.description);
                        }
                        else if (!allNames.Add(test.name))
                        {
                            Debug.LogError("Duplicate Name!\n\t\"" + test.name + "\" will not be added");
                            tests.Remove(test);
                        }
                        else
                        {
                            nameAndDesc.Add(test.name, test.description);
                        }
                    }

                    if (module.medias != null)
                    {
                        foreach (var media in module.medias.Media)
                        {
                            if (!medias.Add(media))
                            {
                                Debug.LogError("Duplicate Media!\n\t\"" + media.name +
                                               "\" will not be added\n\tDescription: " + media.description);
                            }
                            else if (!allNames.Add(media.name))
                            {
                                Debug.LogError("Duplicate Name!\n\t\"" + media.name + "\" will not be added");
                                medias.Remove(media);
                            }
                            else
                            {
                                nameAndDesc.Add(media.name, media.description);
                            }
                        }
                    }

                    if (module.examples != null)
                    {
                        foreach (var example in module.examples.Example)
                        {
                            if (!examples.Add(example))
                            {
                                Debug.LogError("Duplicate Example!\n\t\"" + example.name +
                                               "\" will not be added\n\tDescription: " + example.description);
                            }
                            else if (!allNames.Add(example.name))
                            {
                                Debug.LogError("Duplicate Name!\n\t\"" + example.name + "\" will not be added");
                                examples.Remove(example);
                            }
                            else
                            {
                                nameAndDesc.Add(example.name, example.description);
                            }
                        }
                    }

                    if (module.discussions != null)
                    {
                        foreach (var discussion in module.discussions.Discussion)
                        {
                            if (!discussions.Add(discussion))
                            {
                                Debug.LogError("Duplicate Discussion!\n\t\"" + discussion.name +
                                               "\" will not be added\n\tDescription: " + discussion.description);
                            }
                            else if (!allNames.Add(discussion.name))
                            {
                                Debug.LogError("Duplicate Name!\n\t\"" + discussion.name + "\" will not be added");
                                discussions.Remove(discussion);
                            }
                            else
                            {
                                nameAndDesc.Add(discussion.name, discussion.description);
                            }
                        }
                    }
                }
            }

            //fill the name-to-data-type structure

            foreach (var module in modules)
            {
                nameToType[module.name] = "Module";
            }

            foreach (var assignment in assignments)
            {
                nameToType[assignment.name] = "Assignment";
            }

            foreach (var quiz in quizzes)
            {
                nameToType[quiz.name] = "Quiz";
            }

            foreach (var test in tests)
            {
                nameToType[test.name] = "Test";
            }

            foreach (var example in examples)
            {
                nameToType[example.name] = "Example";
            }

            foreach (var media in medias)
            {
                nameToType[media.name] = "Media";
            }

            foreach (var discussion in discussions)
            {
                nameToType[discussion.name] = "Discussion";
            }

            GameObject.Find("GameManager").GetComponent<GameManager>().SendNamesAndDescriptions();
            GameObject.Find("GameManager").GetComponent<GameManager>().SendNamesAndTypes();

        }

        SFB.StandaloneFileBrowser.OpenFilePanelAsync("Open Expo File:", Application.dataPath, extensions, false,
            SetFirstFound);
        
        GameObject.FindObjectOfType<StatsManager>().InitializeDicts();
    }

    #region Requests
    public static void RequestAssessment(string boothName) {
        List<float> requestFloats = new List<float>();
        //Debug.LogWarning("GameManager MyID: " + MyID );
        var header = new List<float>() {
            2768, MyID, boothName.Length
        };
        requestFloats.AddRange(header);
        requestFloats.AddRange(stringToFloats(boothName));
        var requestFloatsArray = requestFloats.ToArray(); //store in temp var as float[]
        //Debug.LogError("RequestAssessment requestFloatsArray[0]: " + requestFloatsArray[0]);
        //Debug.LogError("RequestAssessment requestFloatsArray[1]: " + requestFloatsArray[1]);
        _asl.SendAndSetClaim(() => { _asl.SendFloatArray(requestFloatsArray); }, -1);
        requestFloats.Clear();
    }

    public static void RequestContent(string boothName)
    {
        List<float> requestFloats = new List<float>();
        var header = new List<float>() {
            2960, MyID, boothName.Length
        };
        requestFloats.AddRange(header);
        requestFloats.AddRange(stringToFloats(boothName));
        var requestFloatsArray = requestFloats.ToArray(); //store in temp var as float[]
        _asl.SendAndSetClaim(() => { _asl.SendFloatArray(requestFloatsArray); }, -1);
        requestFloats.Clear();
    }
    #endregion

    #region Sending
    public void SendNamesAndDescriptions() {
        if (nameAndDesc.Count > 0) {
            float[] numNameDescriptionPairs = new float[]
            {
                XML + 1, nameAndDesc.Count
            };

            _asl.SendAndSetClaim(() => { _asl.SendFloatArray(numNameDescriptionPairs); }, -1);

            List<float> curPair = new List<float>();
            foreach (var kvp in nameAndDesc) {
                var header = new List<float>()
                {
                    XML, kvp.Key.Length, kvp.Value.Length
                };
                curPair.AddRange(header);
                curPair.AddRange(stringToFloats(kvp.Key));
                curPair.AddRange(stringToFloats(kvp.Value));
                var curPairArray = curPair.ToArray(); //store in temp var as float[]
                _asl.SendAndSetClaim(() => { _asl.SendFloatArray(curPairArray); }, -1);
                curPair.Clear();
            }
        }
    }

    public void SendNamesAndTypes() {
        if (nameAndDesc.Count > 0) {
            List<float> curPair = new List<float>();
            foreach (var kvp in nameToType) {
                var header = new List<float>()
                {
                    XML + 2, kvp.Key.Length, kvp.Value.Length
                };
                curPair.AddRange(header);
                curPair.AddRange(stringToFloats(kvp.Key));
                curPair.AddRange(stringToFloats(kvp.Value));
                var curPairArray = curPair.ToArray(); //store in temp var as float[]
                _asl.SendAndSetClaim(() => { _asl.SendFloatArray(curPairArray); }, -1);
                curPair.Clear();
            }
        }
    }

    public void SendAssessment(int playerId, string boothName) {
        //Debug.LogError("SendAssessment playerId: " + playerId);
        float timeLimit = -1;
        question[] questions = null;
        string postUnlock = "None";
        int type = -1;
        foreach (var module in CurrentExpo.modules.Module) {
            //Type 0
            foreach (var assignment in module.assignments.Assignment) {
                if (assignment.name.Equals(boothName)) {
                    questions = assignment.questions.Question;
                    postUnlock = assignment.unlockAfterCompletion;
                    type = 0;
                    break;
                }
            }
            //Type 1
            foreach (var quiz in module.quizzes.Quiz) {
                if (quiz.name.Equals(boothName)) {
                    questions = quiz.questions.Question;
                    postUnlock = quiz.unlockAfterCompletion;
                    type = 1;
                    break;
                }
            }
            //Type 2
            foreach (var test in module.tests.Test) {
                if (test.name.Equals(boothName)) {
                    questions = test.questions.Question;
                    timeLimit = (float)test.timeLimit;
                    postUnlock = test.unlockAfterCompletion;
                    type = 2;
                    break;
                }
            }
        }

        //Check for Type and Questions existing
        if (type == -1) {
            Debug.LogWarning("Player [" + playerId + "] requested assignment [" + boothName + "], but it was not found.");
            return;
        }
        
        if (questions.Length < 1) {
            Debug.LogWarning("Player [" + playerId + "] requested questions from [" + boothName + "], but none were found.");
            return;
        }

        List<float> assessmentPreFloats = new List<float>();
        var header = new List<float>() {
            ASMT + 1,                               //_f[0]: ASMT + 1 = Assessment Header Response
            playerId,                               //_f[1]: playerId to send to [int]
            type,                                   //_f[2]: Assessment Type (0 = Assignment, 1  = Quiz, 2 = Test) [int]
            timeLimit,                              //_f[3]: timeLimit (-1 or 0 if doesn't exist)
            questions.Length,                       //_f[4]: questions.Length [int]
            postUnlock.Length,                      //_f[5]: postUnlock.Length (4 for None if none) [int]
            boothName.Length                        //_f[6]: boothName.Length [int]
        };
        assessmentPreFloats.AddRange(header);
        //  _f[7
        //      to postUnlock.Length + 6]:
        //          postUnlock [stringToFloats]
        assessmentPreFloats.AddRange(stringToFloats(postUnlock));
        //  _f[postUnlock.Length + 7
        //      to postUnlock.Length + boothName.Length + 6]:
        //          boothName [stringToFloats]
        assessmentPreFloats.AddRange(stringToFloats(boothName));
        var assessmentPreFloatsArray = assessmentPreFloats.ToArray(); //store in temp var as float[]
        //Debug.LogError("SendAssessment assessmentPreFloatsArray[0]: " + assessmentPreFloatsArray[0]);
        //Debug.LogError("SendAssessment assessmentPreFloatsArray[1]: " + assessmentPreFloatsArray[1]);
        _asl.SendAndSetClaim(() => { _asl.SendFloatArray(assessmentPreFloatsArray); }, -1);

        List<float> assessmentFloats = new List<float>();
        //Send each question and question data
        int tempQNumber = 1; //starts at 1
        foreach (question q in questions) {
            int questionType = -1;
            switch (q.questionType.ToString()) {
                case "MultipleChoice":
                    questionType = 0;
                    break;
                case "TrueFalse":
                    questionType = 1;
                    break;
                case "ShortAnswer":
                    questionType = 2;
                    break;
                default:
                    Debug.LogWarning("Question type \"" + q.questionType.ToString() + "\" not supported.");
                    break;
            }
            var ansArray = q.answers.Answer;
            int numWrongAnswers = ansArray.Length;

            List<float> lengthWrongAnswers = new List<float>();
            for (int i = 0; i < numWrongAnswers; i++) {
                lengthWrongAnswers.Add(ansArray[i].Length);
            }

            List<float> textWrongAnswers = new List<float>();
            for (int i = 0; i < numWrongAnswers; i++) {
                textWrongAnswers.AddRange(stringToFloats(ansArray[i]));
            }

            var qHeader = new List<float>() {
                ASMT + 2,                                   //_f[0]: ASMT + 2 = Assessment Question Data
                playerId, //_f[1]: playerId to send to [int]
                tempQNumber,                            //_f[2]: Question Number [int]
                questionType,                           //_f[3]: Question Type [int] (0 = MC, 1 = TF, 2 = SA)
                (float)q.timer,                         //_f[4]: Question Timer [double -> float in seconds] (0 if none)
                q.text.Length,                          //_f[5]: Question Text Length [int]
                numWrongAnswers,                        //_f[6]: Number of wrong answers [int]
                q.correct.Length,                       //_f[7]: Length of right answer [int]
                boothName.Length,                       //_f[8]: boothName.Length [int]
                q.value                                 //_f[9]: Question Value/Worth [float]
            };
            assessmentFloats.AddRange(qHeader);
            //  _f[10
            //      to numWrongAnswers + 9]:
            //          Length of wrong answers [int[]]
            assessmentFloats.AddRange(lengthWrongAnswers.ToArray());
            //  _f[numWrongAnswers + 10
            //      to q.text.Length + numWrongAnswers + 9]:
            //          Question Text [stringToFloats]
            assessmentFloats.AddRange(stringToFloats(q.text));
            //  _f[q.text.Length + numWrongAnswers + 10
            //      to textWrongAnswers.Length + q.text.Length + numWrongAnswers + 9]:
            //          Wrong answers Text [stringToFloats[]]
            assessmentFloats.AddRange(textWrongAnswers.ToArray());
            //  _f[textWrongAnswers.Length + q.text.Length + numWrongAnswers + 10
            //      to q.correct.Length + textWrongAnswers.Length + q.text.Length + numWrongAnswers + 9]:
            //          Right answer Text [stringToFloats]
            assessmentFloats.AddRange(stringToFloats(q.correct));
            //  _f[q.correct.Length + textWrongAnswers.Length + q.text.Length + numWrongAnswers + 10
            //      to boothName.Length + q.correct.Length + textWrongAnswers.Length + q.text.Length + numWrongAnswers + 9]:
            //          boothName Text [stringToFloats]
            assessmentFloats.AddRange(stringToFloats(boothName));
            var assessmentFloatsArray = assessmentFloats.ToArray(); //store in temp var as float[]
            //Debug.LogError("SendAssessment assessmentFloatsArray[0]: " + assessmentFloatsArray[0]);
            //Debug.LogError("SendAssessment assessmentFloatsArray[1]: " + assessmentFloatsArray[1]);
            _asl.SendAndSetClaim(() => { _asl.SendFloatArray(assessmentFloatsArray); }, -1);
            assessmentFloats.Clear();
            tempQNumber++; //Increment qNumber
        }
    }

    private void SendContent(int playerId, string boothName)
    {
        object[] contents = null;
        object[] extraContents = null; //for sending the Problem/Solution
        string postUnlock = "None";
        int type = -1;
        foreach (var module in CurrentExpo.modules.Module)
        {
            //Type 0: Example Problem
            if (module.examples != null)
            {
                foreach (var example in module.examples.Example)
                {
                    if (example.name.Equals(boothName) && example.Content == null)
                    {
                        contents = example.ProblemSolution.Problem.Slides.Items;
                        postUnlock = example.unlockAfterCompletion;
                        type = 0;
                        break;
                    }
                }
            }

            //Type 1: Media
            if (module.medias != null)
            {
                foreach (var media in module.medias.Media)
                {
                    if (media.name.Equals(boothName))
                    {
                        contents = media.links.link;
                        postUnlock = media.unlockAfterCompletion;
                        type = 1;
                        break;
                    }
                }
            }

            //Type 2: Discussions
            if (module.discussions != null)
            {
                foreach (var discussion in module.discussions.Discussion)
                {
                    if (discussion.name.Equals(boothName))
                    {
                        contents = discussion.Problem.Slides.Items;
                        postUnlock = discussion.unlockAfterCompletion;
                        type = 2;
                        break;
                    }
                }
            }

            //Type 3: Content
            if (module.examples != null)
            {
                foreach (var example in module.examples.Example)
                {
                    if (example.name.Equals(boothName) && example.ProblemSolution == null)
                    {
                        contents = example.Content.Slides.Items;
                        postUnlock = example.unlockAfterCompletion;
                        type = 3;
                        break;
                    }
                }
            }

            //Type 4: Example Solution
            if (module.examples != null)
            {
                foreach (var example in module.examples.Example)
                {
                    if (example.name.Equals(boothName) && example.Content == null)
                    {
                        extraContents = example.ProblemSolution.Solution.Slides.Items;
                        postUnlock = example.unlockAfterCompletion;
                        break;
                    }
                }
            }

        }

        //Check for Type and Questions existing
        if (type == -1)
        {
            Debug.LogWarning("Player [" + playerId + "] requested content [" + boothName + "], but it was not found.");
            return;
        }

        if (contents.Length < 1)
        {
            Debug.LogWarning("Player [" + playerId + "] requested contents from [" + boothName + "], but none were found.");
            return;
        }

        List<float> contentPreFloats = new List<float>();
        var header = new List<float>() {
            CONT + 1,                           //_f[0]: 2769 = CONT + 1 = Content Header Response
            playerId,                           //_f[1]: playerId to send to [int]
            type,                               //_f[2]: Content Type (0 = Example, 1  = Content, 2 = Media, 3 = Discussion) [int]
            contents.Length,                    //_f[3]: contents.Length [int]
            postUnlock.Length,                  //_f[4]: postUnlock.Length (4 for None if none) [int]
            boothName.Length                    //_f[5]: boothName.Length [int]
        };
        //if we have the example, we need to match the combined length of both of the contents on both ends
        if (extraContents != null) header[3] += extraContents.Length;

        contentPreFloats.AddRange(header);
        //  _f[6
        //      to postUnlock.Length + 5]:
        //          postUnlock [stringToFloats]
        contentPreFloats.AddRange(stringToFloats(postUnlock));
        //  _f[postUnlock.Length + 6
        //      to postUnlock.Length + boothName.Length + 5]:
        //          boothName [stringToFloats]
        contentPreFloats.AddRange(stringToFloats(boothName));
        var contentPreFloatsArray = contentPreFloats.ToArray(); //store in temp var as float[]
        _asl.SendAndSetClaim(() => { _asl.SendFloatArray(contentPreFloatsArray); }, -1);

        List<float> contentFloats = new List<float>();
        //Send each question and question data
        int contentNodeNum = 0; //starts at 1
        string curRawContent = "";
        foreach (var c in contents)
        {
            int contentType = -1;
            switch (c)
            {
                case string s:
                    contentType = 0;
                    curRawContent = s;
                    break;
                case image i:
                    contentType = 1;
                    curRawContent = i.link;
                    break;
                case links l:
                    contentType = 2;
                    curRawContent = string.Join("|", l.link);
                    break;
                /*No case for link because there is no way to discern what a link is unless we use a regex (TODO add regex for link)*/
                default:
                    Debug.LogWarning("Content \"" + c + "\" not supported.");
                    break;
            }

            var cnHeader = new List<float>() {
                CONT + 2,                               //_f[0]: CONT + 2 = Content In-order Data
                playerId,                               //_f[1]: playerId to send to [int]
                contentNodeNum,                         //_f[2]: Content Number [int]
                contentType,                            //_f[3]: Content Type [int] (0 = text, 1 = image)
                curRawContent.Length,                   //_f[4]: Content Node Raw Text Length [int]
                boothName.Length                        //_f[5]: boothName.Length [int]
            };

            contentFloats.AddRange(cnHeader);
            //  _f[6
            //      to curRawContent + 5]:
            //          Length of current raw content [int[]]
            contentFloats.AddRange(stringToFloats(curRawContent));
            //  _f[curRawContent + 6
            //      to curRawContent.length + boothName.Length + 5]:
            //          Name of booth [stringToFloats]
            contentFloats.AddRange(stringToFloats(boothName));
            var contentFloatsArray = contentFloats.ToArray(); //store in temp var as float[]
            _asl.SendAndSetClaim(() => { _asl.SendFloatArray(contentFloatsArray); }, -1);
            contentFloats.Clear();
            contentNodeNum++; //Increment cnNumber
        }

        if (extraContents != null)
        {
            //send an extra of the same header for the example solution content if it exists
            List<float> extraContentPreFloats = new List<float>();
            var extraHeader = new List<float>() {
                CONT + 1,                           //_f[0]: 2769 = CONT + 1 = Content Header Response
                playerId,                           //_f[1]: playerId to send to [int]
                4,                                  //_f[2]: Content Type (0 = Example, 1  = Content, 2 = Media, 3 = Discussion, 4 = Example Solution) [int]
                extraContents.Length + contents.Length,  //_f[3]: contents.Length [int]
                postUnlock.Length,                  //_f[4]: postUnlock.Length (4 for None if none) [int]
                boothName.Length                    //_f[5]: boothName.Length [int]
            };
            extraContentPreFloats.AddRange(extraHeader);
            //  _f[6
            //      to postUnlock.Length + 5]:
            //          postUnlock [stringToFloats]
            extraContentPreFloats.AddRange(stringToFloats(postUnlock));
            //  _f[postUnlock.Length + 6
            //      to postUnlock.Length + boothName.Length + 5]:
            //          boothName [stringToFloats]
            extraContentPreFloats.AddRange(stringToFloats(boothName));
            var extraContentPreFloatsArray = extraContentPreFloats.ToArray(); //store in temp var as float[]
            _asl.SendAndSetClaim(() => { _asl.SendFloatArray(extraContentPreFloatsArray); }, -1);

            List<float> extraContentFloats = new List<float>();
            //Send each question and question data
            int extraContentNodeNum = 0; //starts at 1
            string curRawExtraContent = "";
            foreach (var ec in extraContents)
            {
                int contentType = -1;
                switch (ec)
                {
                    case string s:
                        contentType = 0;
                        curRawExtraContent = s;
                        break;
                    case image i:
                        contentType = 1;
                        curRawExtraContent = i.link;
                        break;
                    case links l:
                        contentType = 2;
                        curRawExtraContent = string.Join("|", l);
                        break;
                    /*No case for link because there is no way to discern what a link is unless we use a regex (TODO add regex for link)*/
                    default:
                        Debug.LogWarning("Content \"" + ec + "\" not supported.");
                        break;
                }

                var cnHeader = new List<float>() {
                CONT + 2,                               //_f[0]: CONT + 2 = Content In-order Data
                playerId,                               //_f[1]: playerId to send to [int]
                extraContentNodeNum,                         //_f[2]: Content Number [int]
                contentType,                            //_f[3]: Content Type [int] (0 = text, 1 = image)
                curRawExtraContent.Length,                   //_f[4]: Content Node Raw Text Length [int]
                boothName.Length                        //_f[5]: boothName.Length [int]
            };

                extraContentFloats.AddRange(cnHeader);
                //  _f[6
                //      to curRawContent + 5]:
                //          Length of current raw content [int[]]
                extraContentFloats.AddRange(stringToFloats(curRawExtraContent));
                //  _f[curRawContent + 6
                //      to curRawContent.length + boothName.Length + 5]:
                //          Name of booth [stringToFloats]
                extraContentFloats.AddRange(stringToFloats(boothName));
                var extraContentFloatsArray = extraContentFloats.ToArray(); //store in temp var as float[]
                _asl.SendAndSetClaim(() => { _asl.SendFloatArray(extraContentFloatsArray); }, -1);
                extraContentFloats.Clear();
                extraContentNodeNum++; //Increment cnNumber
            }
        }
    }

    public static void SendAnnouncement(string txt_ancmt) {
        List<float> announcementFloats = new List<float>();
        var header = new List<float>() {
            ANCMT,                                  //_f[0]: ANCMT = Announcement Header Response
            txt_ancmt.Length,                       //_f[1]: length of string to send as announcement
        };
        announcementFloats.AddRange(header);
        //  _f[2
        //      to txt_ancmt.Length + 1]:
        //          txt_ancmt [stringToFloats]
        announcementFloats.AddRange(stringToFloats(txt_ancmt));
        var announcementFloatsArray = announcementFloats.ToArray(); //store in temp var as float[]
        _asl.SendAndSetClaim(() => { _asl.SendFloatArray(announcementFloatsArray); }, -1);

    }

    public static void SendEnableMessage(string HostName_Password){
        List<float> ConnectionFloats = new List<float>();
        var header = new List<float>(){
            CNNCT,                                  //_f[0]: CNNCT = Enable Voice Chat Header Response
            HostName_Password.Length,
        };
        ConnectionFloats.AddRange(header);
        ConnectionFloats.AddRange(stringToFloats(HostName_Password));
        var ConnectionFloatsArray = ConnectionFloats.ToArray();
        _asl.SendAndSetClaim(() => {_asl.SendFloatArray(ConnectionFloatsArray); }, -1);

    }
    #endregion

    public void FloatReceive(string _id, float[] _f) {
        switch(_f[0]) {
            case QUIT:
                Debug.Log(GameManager.players[(int)_f[1]] + " has left the game");
                GameObject.Find(GameManager.players[(int)_f[1]]).transform.parent.gameObject.SetActive(false);
                break;
            case XML + 1: // XML Response header
                //Receive the # of intended sent booth infos
                countVerify = (int)_f[1];
                break;
            case ASMT + 1: //2769 = ASMT + 1 = Assessment Header Response
                AssessmentManager.ReceiveAssessment(_f);
                break;
            case ASMT + 2: //2770 = ASMT + 2 = Assessment Question Data
                AssessmentManager.ReceiveAssessmentQuestions(_f);
                break;
            case CONT + 1: //2961 = CONT + 1 = Content Header Response
                ContentManager.ReceiveContent(_f);
                break;
            case CONT + 2: //2961 = CONT + 2 = Content In-order Data
                ContentManager.ReceiveInOrderData(_f);
                break;
            case ANCMT: //26268
                AnnouncementManager.ReceiveAnnouncement(_f);
                break;
            case CNNCT: //26628
                _myAudioManager.RecieveConnectionInfo_FromGamemanager(_f);
                break;
            case 404:
                string username = "";
                for (int i = 1; i < _f.Length; i++) {
                    username += (char)(int)_f[i];
                }
                GameObject.Find(username + "_GhostPlayer").gameObject.SetActive(false);
                break;
        }

        if (countVerify != -1 && !BoothManager.verified) {
            //See if all booths loaded
            CheckBooths();
        }

        if (!AmTeacher) {
            switch (_f[0]) {
                case XML: //965 = XML
                    //Read in Booth's Name+Description
                    string boothName = "";
                    string boothDesc = "";
                    int nameLength = (int)_f[1];
                    int descLength = (int)_f[2];
                    for (int i = 3; i < 3 + nameLength; i++) {
                        boothName += (char)(int)_f[i];
                    }
                    for (int i = 3 + nameLength; i < 3 + nameLength + descLength; i++) {
                        boothDesc += (char)(int)_f[i];
                    }
                    //Add to dictionary
                    nameAndDesc.Add(boothName, boothDesc);
                    break;
                case XML + 2: //967 = XML + 2
                    //Read in Booth's Name+Type
                    string boothName2 = "";
                    string boothType = "";
                    int nameLength2 = (int)_f[1];
                    int typeLength = (int)_f[2];
                    for (int i = 3; i < 3 + nameLength2; i++) {
                        boothName2 += (char)(int)_f[i];
                    }
                    for (int i = 3 + nameLength2; i < 3 + nameLength2 + typeLength; i++) {
                        boothType += (char)(int)_f[i];
                    }
                    //Add to dictionary
                    nameToType.Add(boothName2, boothType);
                    break;
            }
        }
        if (AmTeacher) {
            int playerId;
            int nameLength;
            string boothName = "";
            switch (_f[0]) {
                case ASMT: //2768 = ASMT = Assessment Request
                    playerId = (int)_f[1];
                    nameLength = (int)_f[2];
                    for (int i = 3; i < 3 + nameLength; i++) {
                        boothName += (char)(int)_f[i];
                    }
                    Debug.LogError("FloatReceive ASMT playerId: " + playerId);
                    SendAssessment(playerId, boothName);
                    break;
                case CONT: //2960 = CONT = Content Request
                    playerId = (int)_f[1];
                    nameLength = (int)_f[2];
                    for (int i = 3; i < 3 + nameLength; i++) {
                        boothName += (char)(int)_f[i];
                    }
                    Debug.LogError("FloatReceive CONT playerId: " + playerId);
                    SendContent(playerId, boothName);
                    break;
            }
        }
    }

    private void CheckBooths() {
        //Checks to see if all booths in Scene exist in XML.
        bool allBoothsFound = true;
        foreach (string boothName in BoothManager.boothNames.Keys) {
            if (!nameAndDesc.ContainsKey(boothName)) {
                allBoothsFound = false;
            }
        };
        if (allBoothsFound) {
            BoothManager.SetVerifiedStatus();
        }
    }

    public static List<float> stringToFloats(string toConvert) {
        var floats = new List<float>();
        foreach (var c in toConvert) {
            floats.Add((int)c);
        }
        return floats;
    }
}