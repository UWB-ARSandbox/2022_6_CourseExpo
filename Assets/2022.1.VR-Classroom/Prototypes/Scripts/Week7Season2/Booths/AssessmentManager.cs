using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CourseXpo;
using TMPro;
using UnityEngine.Events;
using System;
using ASL;
using System.Linq;

/**
 * Place this script on outermost layer each Booth Prefab
 * (Same layer as BoothManager.cs)
 */
public class AssessmentManager : MonoBehaviour {
    //Booth Manager
    private BoothManager boothManager;
    public CollaborativeManager _myCollabManager;
    public void Set_myCollabManager(CollaborativeManager collab){_myCollabManager = collab;}
    public int NumberOfConcurrentUsers = 1;

    //Assessment Walls
    public AssessmentWalls walls;

    //used for establishing the max amount of characters that can enter the booth
    public int UnitCounter = 0;

    //Assignment + Quiz = Show answers immediately
    //Test = Show result at end
    private Sprite BAR_BLUE;
    private Sprite BAR_RED;
    private Sprite BAR_GREEN;
    private Sprite BAR_ORANGE;

    public enum AssessmentType {
        none,
        assignment,
        quiz,
        test
    };
    public enum ResponseType {
        buttonA,
        buttonB,
        buttonC,
        buttonD,
        buttonTrue,
        buttonFalse,
        buttonSubmit
    };

    public AssessmentType assessmentType = AssessmentType.none;
    private question[] assessmentQuestions;
    private question currentQuestion;
    private string initalDescription;

    private string unlockAfterCompletion = "";

    //States
    private bool assessmentStarted = false;

    //Dashboard Stats
    public int num_CurrentQuestion = -1;
    private int num_TotalQuestions = -1;
    private int num_CorrectAnswers = -1;
    private float testTimeLimit = -1;
    private float testTimeRemaining = -1;
    private float questionTimeLimit = -1;
    private float questionTimeRemaining = -1;
    private bool lockedOut = false;
    private bool testLockedOut = false;
    private bool questionTimerActive = false;
    private bool lockoutTimerActive = false;

    public float num_CurrentPoints = 0;
    public float num_TotalPoints = 0;

    private float WAIT_TIME = 1.0f;
    private float REVEAL_TIME = 0.25f;
    
    //Stats
    private PersonalStats personalStats;
    private float timeStarted;

    //General Response Panel
    private GameObject pnl_Start;
    private GameObject pnl_Response;
    private Button btn_Start;

    //Welcome Screen
    private GameObject pnl_WelcomeScreen;
    private TextMeshProUGUI txt_boothName;
    private TextMeshProUGUI txt_boothDesc;

    //Multiple Choice
    private GameObject pnl_MultipleChoice;
    private TextMeshProUGUI txt_MultChoiceQuestion;
    private Image[] imgAry_multipleChoice = new Image[4];
    private Button[] btn_imgAry_multipleChoice = new Button[4];
    private TextMeshProUGUI[] txtAry_mcOptionLetters = new TextMeshProUGUI[4];
    private TextMeshProUGUI[] txtAry_mcOptionText = new TextMeshProUGUI[4];
    private Button[] btnAry_multipleChoice = new Button[4];
    private Color CLR_BLACK = Color.black;
    private Color CLR_ORANGE = new Color(1, 0.5f, 0, 1);
    private Color CLR_WHITE = Color.white;
    private Color CLR_RED = Color.red;
    private Color CLR_GREEN = Color.green;

    //True/False
    private GameObject pnl_TrueFalse;
    private TextMeshProUGUI txt_TrueFalseQuestion;
    private Image img_True;
    private Image img_False;
    private Button btn_img_True;
    private Button btn_img_False;
    private TextMeshProUGUI txt_True;
    private TextMeshProUGUI txt_False;
    private Button btn_True;
    private Button btn_False;

    //Short Answer
    private GameObject pnl_ShortAnswer;
    private TextMeshProUGUI txt_ShortAnswerQuestion;
    private TextMeshProUGUI txt_ShortAnswerResponse;
    private TextMeshProUGUI txt_ShortAnswerCorrect;
    private string resetResponseText;
    private InputField ipt_Answer;
    private Button btn_Submit;

    //End Screen
    private GameObject pnl_EndScreen;
    private TextMeshProUGUI txt_EndTitle;
    private TextMeshProUGUI txt_EndDesc;

    //Dashboard
    private GameObject pnl_Dashboard;
    private GameObject pnl_Progress;
    private TextMeshProUGUI lbl_Question;
    private TextMeshProUGUI txt_QuestionNumber;
    private GameObject pnl_CurrentScore;
    private TextMeshProUGUI lbl_CurrentScore;
    private TextMeshProUGUI txt_CurrentScore;
    private GameObject pnl_QuestionValue;
    private TextMeshProUGUI lbl_QuestionValue;
    private TextMeshProUGUI txt_QuestionValue;
    private GameObject pnl_CurrentPoints;
    private TextMeshProUGUI lbl_CurrentPoints;
    private TextMeshProUGUI txt_CurrentPoints;
    private GameObject pnl_TimeLeft;
    private TextMeshProUGUI lbl_TimeLeft;
    private TextMeshProUGUI txt_TimeLeft;
    private GameObject pnl_QuestionTimer;
    private Image img_QuestionTimer;
    private float QUESTION_TIMER_INTIAL_WIDTH;
    private TextMeshProUGUI lbl_QuestionTimer;
    private TextMeshProUGUI txt_QuestionTimer;

    private void Awake() {
        BAR_BLUE = Resources.Load<Sprite>("ans_Blue");
        BAR_RED = Resources.Load<Sprite>("ans_Red");
        BAR_GREEN = Resources.Load<Sprite>("ans_Green");
        BAR_ORANGE = Resources.Load<Sprite>("ans_Orange");
    }

    // Start is called before the first frame update
    void Start() {
        boothManager = GetComponent<BoothManager>();

        //Find and disable walls
        walls = GetComponentInChildren<AssessmentWalls>();
        walls.gameObject.SetActive(false);

        //Canvas objects
        LinkObjects();

        txt_boothName.text = "Booth Unavailable.";
        initalDescription = "There is no booth loaded at this location." +
                                "\nPlease contact the Expo host if this is a mistake.";
        txt_boothDesc.text = initalDescription;
        resetResponseText = "Type your response into the input field.\nPress \"Submit\" to submit response.";
        StartCoroutine(CheckLoadedAndVerify());
    }

    private void StartAssessment() {
        //Assessment already started
        if (assessmentStarted) {
            return;
        }

        //Coroutine? Check for when all questions are loaded before allow start.
        if (assessmentQuestions == null) {
            StartCoroutine(DisplayMessageOnBooth("Assessment has not been loaded. Please try again later."));
            return;
        }

        if (num_TotalQuestions < 1) {
            StartCoroutine(DisplayMessageOnBooth("Assessment has no questions. Please try again later.\nIf this continues, please contact the Expo host."));
            return;
        }

        if (!boothManager.boothVerified) {
            StartCoroutine(DisplayMessageOnBooth("This booth is still being verified. Please try again later.\nIf this continues, please contact the Expo host."));
            return;
        }

        if (GameManager.AmTeacher) {
            StartCoroutine(DisplayMessageOnBooth("The assessment is loaded and working properly."));
            return;
        }

        //Teleport player in front of lectern
        GameObject player = FindObjectOfType<XpoPlayer>().gameObject;
        player.GetComponent<CharacterController>().enabled = false;
        player.transform.SetParent(transform, true);
        player.transform.localPosition = new Vector3(3.5f, 1.115f, 0);
        player.transform.SetParent(null, true);
        player.GetComponent<CharacterController>().enabled = true;

        //Activate walls
        walls.gameObject.SetActive(true);

        //Hide Players
        // GameManager.TogglePlayerVisibility(false);
        GameManager.isTakingAssessment = true;

        timeStarted = Time.time;
        switch (assessmentType) {
            case AssessmentType.assignment:
                NextAssignmentQ();
                StartActivate();
                break;
            case AssessmentType.quiz:
                NextQuizQ();
                StartActivate();
                break;
            case AssessmentType.test:
                NextTestQ();
                StartActivate();
                //Start test timer
                lockoutTimerActive = true;
                //testTimeLimit + testTimeRemaining already set.
                pnl_TimeLeft.SetActive(true);
                break;
        }
    }

    private void StartActivate() {
        assessmentStarted = true;
        //Switch to response panel
        pnl_Start.SetActive(false);
        pnl_Response.SetActive(true);

        pnl_WelcomeScreen.SetActive(false);
        pnl_Dashboard.SetActive(true);

        /*  Variables already set
         *  num_TotalQuestions = # of questions in quiz
         *  assessmentQuestions = the questions in an array;
         *  num_CurrentQuestion = current question we're on (Start at 0);
         *  num_CorrectAnswers = number of answers correct so far (Start at 0);
         */
    }

    private void DisableQuestionPanels() {
        //Disable previous question panel
        pnl_MultipleChoice.SetActive(false);
        pnl_TrueFalse.SetActive(false);
        pnl_ShortAnswer.SetActive(false);
    }

    #region Start Questions
    IEnumerator DelayStartNextQ() {
        yield return new WaitForSecondsRealtime(3.0f);
        StartNextQuestion();
    }

    private void StartNextQuestion() {
        //Start next question OR End assessment
        if (num_CurrentQuestion < num_TotalQuestions) {
            switch (assessmentType) {
                case AssessmentType.assignment:
                    NextAssignmentQ();
                    break;
                case AssessmentType.quiz:
                    NextQuizQ();
                    break;
                case AssessmentType.test:
                    NextTestQ();
                    break;
            }
        } else {
            EndAssessment();
        }
    }

    private void StartQuestionType(AssessmentType aType) {
        switch (currentQuestion.questionType) {
            case QuestionType.MultipleChoice:
                StartMultChoiceQ(aType);
                break;
            case QuestionType.TrueFalse:
                StartTrueFalseQ(aType);
                break;
            case QuestionType.ShortAnswer:
                StartShortAnswerQ(aType);
                break;
        }
    }

    #region Assignment Types
    //Assignment = Score + !Timer + !Time Limit
    private void NextAssignmentQ() {
        //Reponse Panel
        pnl_Response.SetActive(true);

        //Score Panel
        pnl_CurrentScore.SetActive(true);
        txt_CurrentScore.text = num_CorrectAnswers.ToString() + " / " + num_TotalQuestions;

        //Points Panel
        pnl_CurrentPoints.SetActive(true);
        txt_CurrentPoints.text = num_CurrentPoints.ToString() + " / " + num_TotalPoints;

        //Advance Question Number
        num_CurrentQuestion++;
        txt_QuestionNumber.text = num_CurrentQuestion.ToString() + " / " + num_TotalQuestions;

        //Get current question
        currentQuestion = assessmentQuestions[num_CurrentQuestion - 1];

        //Show New Question Value
        pnl_QuestionValue.SetActive(true);
        txt_QuestionValue.text = (int)currentQuestion.value + " point";
        if (currentQuestion.value > 1) {
            txt_QuestionValue.text += "s";
        }

        //Disable previous question panel
        DisableQuestionPanels();

        //Start next question
        StartQuestionType(AssessmentType.assignment);
    }

    //Quiz = Score + Timer + !Time Limit
    private void NextQuizQ() {
        //Reponse Panel
        pnl_Response.SetActive(true);

        //Score Panel
        pnl_CurrentScore.SetActive(true);
        txt_CurrentScore.text = num_CorrectAnswers.ToString() + " / " + num_TotalQuestions;

        //Points Panel
        pnl_CurrentPoints.SetActive(true);
        txt_CurrentPoints.text = num_CurrentPoints.ToString() + " / " + num_TotalPoints;

        //Advance Question Number
        num_CurrentQuestion++;
        txt_QuestionNumber.text = num_CurrentQuestion.ToString() + " / " + num_TotalQuestions;

        //Get current question
        currentQuestion = assessmentQuestions[num_CurrentQuestion - 1];

        //Show New Question Value
        pnl_QuestionValue.SetActive(true);
        txt_QuestionValue.text = (int)currentQuestion.value + " point";
        if (currentQuestion.value > 1) {
            txt_QuestionValue.text += "s";
        }

        //Reset lockout if not perm-locked by test
        lockedOut = false;

        //Start timer if exists
        if (currentQuestion.timer > 0) {
            questionTimeLimit = currentQuestion.timer;
            questionTimeRemaining = questionTimeLimit;
            img_QuestionTimer.rectTransform.sizeDelta = new Vector2(QUESTION_TIMER_INTIAL_WIDTH, 0);
            txt_QuestionTimer.text = (int)(questionTimeLimit / 60) + ":" + (int)(questionTimeLimit % 60);
            pnl_QuestionTimer.SetActive(true);
            questionTimerActive = true;
        } else {
            pnl_QuestionTimer.SetActive(false);
        }

        //Disable previous question panel
        DisableQuestionPanels();

        //Start next question
        StartQuestionType(AssessmentType.quiz);
    }

    //Quiz = !Score + !Timer + Time Limit
    private void NextTestQ() {
        //Reponse Panel
        pnl_Response.SetActive(true);

        //Advance Question Number
        num_CurrentQuestion++;
        txt_QuestionNumber.text = num_CurrentQuestion.ToString() + " / " + num_TotalQuestions;

        //Get current question
        currentQuestion = assessmentQuestions[num_CurrentQuestion - 1];

        //Show New Question Value
        pnl_QuestionValue.SetActive(true);
        txt_QuestionValue.text = (int)currentQuestion.value + " point";
        if (currentQuestion.value > 1) {
            txt_QuestionValue.text += "s";
        }

        //Reset lockout if not perm-locked by test
        lockedOut = false;

        //Start timer if exists
        if (currentQuestion.timer > 0) {
            questionTimeLimit = currentQuestion.timer;
            questionTimeRemaining = questionTimeLimit;
            img_QuestionTimer.rectTransform.sizeDelta = new Vector2(QUESTION_TIMER_INTIAL_WIDTH, 0);
            txt_QuestionTimer.text = (int)(questionTimeLimit / 60) + ":" + (int)(questionTimeLimit % 60);
            pnl_QuestionTimer.SetActive(true);
            questionTimerActive = true;
        } else {
            pnl_QuestionTimer.SetActive(false);
        }

        //Restart Test timer if exists
        if (testTimeRemaining > 0) {
            lockoutTimerActive = true;
        }

        //Disable previous question panel
        DisableQuestionPanels();

        //Start next question
        StartQuestionType(AssessmentType.test);
    }
    #endregion
    #endregion

    #region Multiple-Choice
    private void StartMultChoiceQ(AssessmentType aType) {
        //Store Correct Answer
        string correctAnswer = currentQuestion.correct.ToString();

        //Randomize Answer Options
        string[] optionsToDisplay = RandomizeMCOptions();

        //Display Answer Options
        for (int i = 0; i < optionsToDisplay.Length; i++) {
            //Update Answer Text
            txtAry_mcOptionText[i].text = optionsToDisplay[i];

            //Reset Colors
            imgAry_multipleChoice[i].sprite = BAR_BLUE;
            txtAry_mcOptionLetters[i].color = CLR_ORANGE;
            txtAry_mcOptionText[i].color = CLR_WHITE;

            //Activate Option
            imgAry_multipleChoice[i].gameObject.SetActive(true);
        }

        //Hide Extra Answer Options
        for (int i = 3; i > optionsToDisplay.Length - 1; i--) {
            imgAry_multipleChoice[i].gameObject.SetActive(false);
        }

        //Set and Show Question
        txt_MultChoiceQuestion.text = currentQuestion.text;
        pnl_MultipleChoice.SetActive(true);
    }

    //old
    // private string[] RandomizeMCOptions() {
    //     //Declare
    //     int num_TotalAnswers = currentQuestion.answers.Answer.Length + 1;
    //     string[] result = new string[num_TotalAnswers];

    //     //Load
    //     for (int i = 0; i < num_TotalAnswers - 1; i++) {
    //         result[i] = currentQuestion.answers.Answer[i].ToString();
    //     }
    //     result[num_TotalAnswers - 1] = currentQuestion.correct;

    //     //Shuffle
    //     System.Random random = new System.Random();
    //     result = result.OrderBy(x => random.Next()).ToArray();

    //     //Return
    //     return result;
    // }

    private string[] RandomizeMCOptions() {
        //grab the random float from the Collaborative Manager RandomVal instead of creating a random value
        //IE result = result.OrderBy(x => _myCollabManager.RandomVal).ToArray();
        //This should ensure that all questions in are a synched random state as opposed to a random client side state

        //Declare
        int num_TotalAnswers = currentQuestion.answers.Answer.Length + 1;
        string[] result = new string[num_TotalAnswers];

        //Load
        for (int i = 0; i < num_TotalAnswers - 1; i++) {
            result[i] = currentQuestion.answers.Answer[i].ToString();
        }
        result[num_TotalAnswers - 1] = currentQuestion.correct;

        //Shuffle
        if(NumberOfConcurrentUsers == 1){
            System.Random random = new System.Random();
            result = result.OrderBy(x => random.Next()).ToArray();
        }
        else
            result = result.OrderBy(x => _myCollabManager.RandomVal).ToArray();

        //Return
        return result;
    }

    private void CheckMCResponse(int response) {
        //If response is out of range or if response already accepted, ignore.
        if (response > currentQuestion.answers.Answer.Length || !pnl_Response.activeSelf) {
            return;
        }

        //Stop the timer if active
        questionTimerActive = false;
        lockoutTimerActive = false;

        //Hide the response panel
        pnl_Response.SetActive(false);

        //Check for lockout
        if (response != -1) {
            //Lockin by Change Image + Text Color
            imgAry_multipleChoice[response].sprite = BAR_ORANGE;
            txtAry_mcOptionLetters[response].color = CLR_BLACK;
            txtAry_mcOptionText[response].color = CLR_BLACK;
        }
        
        //Get Correct Answer
        int correctAnswerIndex = -1;
        for (int i = 0; i < txtAry_mcOptionText.Length; i++) {
            if (txtAry_mcOptionText[i].text == currentQuestion.correct) {
                correctAnswerIndex = i;
                break;
            }
        }

        //Adjust stats
        if (correctAnswerIndex == response) {
            num_CorrectAnswers++;
            num_CurrentPoints += currentQuestion.value;
        }

        if (assessmentType != AssessmentType.test) {
            //Reveal Process
            StartCoroutine(RevealCorrectMC(response, correctAnswerIndex));
        } else {
            StartCoroutine(DelayStartNextQ());
        }
    }

    IEnumerator RevealCorrectMC(int response, int correctAnswerIndex) {
        Sprite img_InitialCorrectAnswer = imgAry_multipleChoice[correctAnswerIndex].sprite;
        Color clr_InitalCorrectLetterText = txtAry_mcOptionLetters[correctAnswerIndex].color;
        Color clr_InitalCorrectAnswerText = txtAry_mcOptionText[correctAnswerIndex].color;
        
        //Wait
        yield return new WaitForSecondsRealtime(WAIT_TIME);

        //Reveal Animation
        for (int i = 0; i < 2; i++) {
            imgAry_multipleChoice[correctAnswerIndex].sprite = BAR_GREEN;
            txtAry_mcOptionLetters[correctAnswerIndex].color = CLR_BLACK;
            txtAry_mcOptionText[correctAnswerIndex].color = CLR_BLACK;
            yield return new WaitForSecondsRealtime(REVEAL_TIME);
            imgAry_multipleChoice[correctAnswerIndex].sprite = img_InitialCorrectAnswer;
            txtAry_mcOptionLetters[correctAnswerIndex].color = clr_InitalCorrectLetterText;
            txtAry_mcOptionText[correctAnswerIndex].color = clr_InitalCorrectAnswerText;
            yield return new WaitForSecondsRealtime(REVEAL_TIME);
        }
        imgAry_multipleChoice[correctAnswerIndex].sprite = BAR_GREEN;
        txtAry_mcOptionLetters[correctAnswerIndex].color = CLR_BLACK;
        txtAry_mcOptionText[correctAnswerIndex].color = CLR_BLACK;

        //Advance score if showing (Assignment or Quiz)
        if (pnl_CurrentScore.activeSelf) {
            txt_CurrentScore.text = num_CorrectAnswers.ToString() + " / " + num_TotalQuestions;
            txt_CurrentPoints.text = num_CurrentPoints.ToString() + " / " + num_TotalPoints;
        }

        //Wait
        yield return new WaitForSecondsRealtime(WAIT_TIME);

        //Start next question OR End assessment
        StartNextQuestion();
    }
    #endregion

    #region True/False
    private void StartTrueFalseQ(AssessmentType aType) {
        //Store Correct Answer
        string correctAnswer = currentQuestion.correct.ToString();

        //Reset Colors
        img_True.sprite = BAR_BLUE;
        img_False.sprite = BAR_BLUE;
        txt_True.color = CLR_WHITE;
        txt_False.color = CLR_WHITE;

        //Set and Show Question
        txt_TrueFalseQuestion.text = currentQuestion.text;
        pnl_TrueFalse.SetActive(true);
    }

    private void CheckTFResponse(int response) {
        //If response is out of range or if response already accepted, ignore.
        if (response > 1 || !pnl_Response.activeSelf) {
            return;
        }

        //Stop the timer if active
        questionTimerActive = false;
        lockoutTimerActive = false;

        //Hide the response panel
        pnl_Response.SetActive(false);

        //Lockin by Change Image + Text Color (Also check for lockout at -1)
        if (response == 0) {
            img_False.sprite = BAR_ORANGE;
            txt_False.color = CLR_BLACK;
        } else if (response == 1){
            img_True.sprite = BAR_ORANGE;
            txt_True.color = CLR_BLACK;
        }

        //Get Correct Answer
        int correctAnswerInt = -1;
        if (currentQuestion.correct == "False") {
            correctAnswerInt = 0;
        } else {
            correctAnswerInt = 1;
        }

        //Adjust stats
        if (correctAnswerInt == response) {
            num_CorrectAnswers++;
            num_CurrentPoints += currentQuestion.value;
        }

        if (assessmentType != AssessmentType.test) {
            //Reveal Process
            StartCoroutine(RevealCorrectTF(response, correctAnswerInt));
        } else {
            StartCoroutine(DelayStartNextQ());
        }
    }

    IEnumerator RevealCorrectTF(int response, int correctAnswerInt) {
        Sprite img_InitialCorrectAnswer;
        Color clr_InitalCorrectAnswerText;

        //Wait
        yield return new WaitForSecondsRealtime(WAIT_TIME);

        if (correctAnswerInt == 0) {
            img_InitialCorrectAnswer = img_False.sprite;
            clr_InitalCorrectAnswerText = txt_False.color;
            //Reveal Animation
            for (int i = 0; i < 2; i++) {
                img_False.sprite = BAR_GREEN;
                txt_False.color = CLR_BLACK;
                yield return new WaitForSecondsRealtime(REVEAL_TIME);
                img_False.sprite = img_InitialCorrectAnswer;
                txt_False.color = clr_InitalCorrectAnswerText;
                yield return new WaitForSecondsRealtime(REVEAL_TIME);
            }
            img_False.sprite = BAR_GREEN;
            txt_False.color = CLR_BLACK;
        } else {
            img_InitialCorrectAnswer = img_True.sprite;
            clr_InitalCorrectAnswerText = txt_True.color;
            //Reveal Animation
            for (int i = 0; i < 2; i++) {
                img_True.sprite = BAR_GREEN;
                txt_True.color = CLR_BLACK;
                yield return new WaitForSecondsRealtime(REVEAL_TIME);
                img_True.sprite = img_InitialCorrectAnswer;
                txt_True.color = clr_InitalCorrectAnswerText;
                yield return new WaitForSecondsRealtime(REVEAL_TIME);
            }
            img_True.sprite = BAR_GREEN;
            txt_True.color = CLR_BLACK;
        }

        //Advance score if showing (Assignment or Quiz)
        if (pnl_CurrentScore.activeSelf) {
            txt_CurrentScore.text = num_CorrectAnswers.ToString() + " / " + num_TotalQuestions;
            txt_CurrentPoints.text = num_CurrentPoints.ToString() + " / " + num_TotalPoints;
        }

        //Wait
        yield return new WaitForSecondsRealtime(WAIT_TIME);

        //Start next question OR End assessment
        StartNextQuestion();
    }
    #endregion

    #region Short-Answer
    private void StartShortAnswerQ(AssessmentType aType) {
        //Store Correct Answer
        string correctAnswer = currentQuestion.correct;

        //Reset Colors
        txt_ShortAnswerResponse.color = CLR_WHITE;
        txt_ShortAnswerCorrect.color = CLR_WHITE;

        //Hide correct answer
        txt_ShortAnswerCorrect.gameObject.SetActive(false);

        //Set and Show Question
        txt_ShortAnswerQuestion.text = currentQuestion.text;
        txt_ShortAnswerResponse.text = resetResponseText;
        pnl_ShortAnswer.SetActive(true);
    }

    private void CheckSAResponse(string response) {
        //Stop the timer if active
        questionTimerActive = false;
        lockoutTimerActive = false;

        //Hide the response panel
        pnl_Response.SetActive(false);

        //null response = locked out
        if (response != null) {
            //Update display with their response
            txt_ShortAnswerResponse.text = response;
        } else {
            txt_ShortAnswerResponse.text = "Out of time!";
        }

        //Adjust stats
        if (response != null && currentQuestion.correct.ToLower().Replace(" ", "") == response.ToLower().Replace(" ", "")) {
            num_CorrectAnswers++;
            num_CurrentPoints += currentQuestion.value;
        }

        if (assessmentType != AssessmentType.test) {
            //Reveal Process
            StartCoroutine(RevealCorrectSA(response));
        } else {
            StartCoroutine(DelayStartNextQ());
        }
    }

    IEnumerator RevealCorrectSA(string response) {
        //Wait
        yield return new WaitForSecondsRealtime(WAIT_TIME);

        if (response != null && currentQuestion.correct.ToLower().Replace(" ", "") == response.ToLower().Replace(" ", "")) {
            txt_ShortAnswerResponse.color = CLR_GREEN;
        } else {
            txt_ShortAnswerResponse.color = CLR_RED;
        }

        txt_ShortAnswerCorrect.text = currentQuestion.correct;
        txt_ShortAnswerCorrect.gameObject.SetActive(true);

        //Advance score if showing (Assignment or Quiz)
        if (pnl_CurrentScore.activeSelf) {
            txt_CurrentScore.text = num_CorrectAnswers.ToString() + " / " + num_TotalQuestions;
            txt_CurrentPoints.text = num_CurrentPoints.ToString() + " / " + num_TotalPoints;
        }

        //Wait
        yield return new WaitForSecondsRealtime(WAIT_TIME);

        //Start next question OR End assessment
        StartNextQuestion();
    }
    #endregion

    /* ---Response Codes from User Response Panel---
     * MC (A = 0, B = 1, C = 2, D = 3)
     * TF (False = 0, True = 1)
     * SA (response = text in ipt_Answer)
     */
    public void ReceiveResponse(ResponseType rType) {
        if (!lockedOut) {
            switch (currentQuestion.questionType) {
                case QuestionType.MultipleChoice:
                    if (rType == ResponseType.buttonA) {
                        CheckMCResponse(0);
                    }
                    if (rType == ResponseType.buttonB) {
                        CheckMCResponse(1);
                    }
                    if (rType == ResponseType.buttonC) {
                        CheckMCResponse(2);
                    }
                    if (rType == ResponseType.buttonD) {
                        CheckMCResponse(3);
                    }
                    break;
                case QuestionType.TrueFalse:
                    if (rType == ResponseType.buttonFalse) {
                        CheckTFResponse(0);
                    }
                    if (rType == ResponseType.buttonTrue) {
                        CheckTFResponse(1);
                    }
                    break;
                case QuestionType.ShortAnswer:
                    if (rType == ResponseType.buttonSubmit) {
                        if (ipt_Answer.text != null) {
                            string response = ipt_Answer.text;
                            ipt_Answer.text = "";
                            CheckSAResponse(response);
                        }
                    }
                    break;
            }
        }
    }

    //What to do when locked out
    private void ResolveLockedOut(bool isTestLockout) {
        lockedOut = true;
        if (isTestLockout) {
            testLockedOut = true;
            EndAssessment();
            return;
        }

        if (!testLockedOut) {
            switch (currentQuestion.questionType) {
                case QuestionType.MultipleChoice:
                    CheckMCResponse(-1);
                    break;
                case QuestionType.TrueFalse:
                    CheckTFResponse(-1);
                    break;
                case QuestionType.ShortAnswer:
                    CheckSAResponse(null);
                    break;
            }
        }
    }

    private void EndAssessment() {
        questionTimerActive = false;
        lockoutTimerActive = false;

        //Hide Question and Dashboard
        DisableQuestionPanels();
        pnl_Dashboard.SetActive(false);

        //Hide the response panel
        pnl_Response.SetActive(false);

        string correctAnswersText = num_CorrectAnswers + " / " + num_TotalQuestions;
        string finalScoreText = num_CurrentPoints + " / " + num_TotalPoints;
        float finalScorePercentage = ((float)num_CurrentPoints / (float)num_TotalPoints) * 100;

        txt_EndTitle.text = "Assessment Complete";
        string endDescription = "";

        //Special Booth Check
        if (txt_boothName.text == "?????" && finalScorePercentage == 100) {
            endDescription += ("Congratulations! You've scored 100%!\n");
            endDescription += ("Be the first person to message Andy on discord @JustAnotherLurker#3922\n");
            endDescription += ("this code: [ckiga-i-sfkmb]\n");
        } else {
            switch (assessmentType) {
                case AssessmentType.assignment:
                    endDescription += "Assignment: ";
                    break;
                case AssessmentType.quiz:
                    endDescription += "Quiz: ";
                    break;
                case AssessmentType.test:
                    endDescription += "Test: ";
                    break;
            }
            endDescription += (txt_boothName.text + "\n");
            endDescription += ("Correct Answers: " + correctAnswersText + "\n");
            endDescription += ("Final Score: " + finalScoreText + " (" + String.Format("{0:0.00}", finalScorePercentage) + "%)\n");
        }

        txt_EndDesc.text = endDescription;

        pnl_EndScreen.SetActive(true);
        BoothManager.UnlockAfterCompletion(unlockAfterCompletion);

        //Release walls
        walls.gameObject.SetActive(false);

        //Show Players
        GameManager.isTakingAssessment = false;
        // GameManager.TogglePlayerVisibility(true);

        //Update Stats
        personalStats.SetPercentageScore(boothManager.boothName, finalScorePercentage);
        personalStats.SetTimeTaken(boothManager.boothName, Time.time - timeStarted);
        personalStats.SetCompleted(boothManager.boothName, true);
    }

    //Verify the content has been loaded
    IEnumerator CheckLoadedAndVerify() {
        while (!boothManager.boothVerified) {
            //Debug.LogError("boothName: " + txt_boothName.text + "\nboothManager.boothVerified: " + boothManager.boothVerified);
            bool isVerified = true;
            if (assessmentQuestions == null || num_TotalQuestions < 0) {
                isVerified = false;
            } else {
                for (int i = 0; i < num_TotalQuestions; i++) {
                    if (assessmentQuestions[i] == null) {
                        isVerified = false;
                    }
                }
            }
            boothManager.boothVerified = isVerified;

            if (isVerified) {
                personalStats = FindObjectOfType<PersonalStats>();
                //Set total points value
                for (int i = 0; i < num_TotalQuestions; i++) {
                    num_TotalPoints += assessmentQuestions[i].value;
                }
            }
            //Debug.LogWarning("boothVerified: " + txt_boothName.text);
            yield return new WaitForSeconds(3.0f);
        }
    }

    private void FixedUpdate() {
        int hours;
        int minutes;
        int seconds;
        if (questionTimerActive) {
            //Count down
            questionTimeRemaining -= Time.fixedDeltaTime;
            float displayQuestionTimeRemaining = Mathf.Ceil(questionTimeRemaining);
            //Adjust Question Bar
            float timerWidth = QUESTION_TIMER_INTIAL_WIDTH * (questionTimeRemaining / questionTimeLimit);
            img_QuestionTimer.rectTransform.sizeDelta = new Vector2(timerWidth, 0);
            //Adjust Question Timer text
            minutes = (int)(displayQuestionTimeRemaining / 60);
            seconds = (int)(displayQuestionTimeRemaining % 60);
            txt_QuestionTimer.text = minutes.ToString("00") + ":" + seconds.ToString("00");

            //Lock out if out of time
            if (questionTimeRemaining <= 0) {
                personalStats.IncrementNumQuestionsTimedOut(boothManager.boothName);
                questionTimeRemaining = 0;
                questionTimerActive = false;
                ResolveLockedOut(false);
                txt_QuestionTimer.text = "00:00";
            }
        }
        if (lockoutTimerActive) {
            //Count down
            testTimeRemaining -= Time.fixedDeltaTime;
            float displayTestTimeRemaining = Mathf.Ceil(testTimeRemaining);
            //Adjust Time Limit text
            hours = (int)(displayTestTimeRemaining / 3600);
            minutes = (int)((displayTestTimeRemaining - (3600 * hours)) / 60);
            seconds = (int)(displayTestTimeRemaining % 60);
            txt_TimeLeft.text = hours.ToString("00") + ":" + minutes.ToString("00") + ":" + seconds.ToString("00");

            //Lock out if out of time
            if (testTimeRemaining <= 0) {
                testTimeRemaining = 0;
                lockoutTimerActive = false;
                ResolveLockedOut(true);
                txt_TimeLeft.text = "00:00:00";
            }
        }
    }

    #region Loading Assessment
    IEnumerator DisplayMessageOnBooth(string errorMessage) {
        txt_boothDesc.text = errorMessage;
        yield return new WaitForSecondsRealtime(5.0f);
        txt_boothDesc.text = initalDescription;
    }

    public void GetAssessment(string boothName, string boothDesc) {
        txt_boothName.text = boothName;
        initalDescription = boothDesc;
        txt_boothDesc.text = initalDescription;
        GameManager.RequestAssessment(boothName);
    }

    public static void ReceiveAssessment(float[] _f) {
        //_f[0] = 2769 = ASMT + 1 = Assessment Header Response
        //Debug.LogError("ReceiveAssessment _f[1]:" + _f[1]);
        if (_f[1] != GameManager.MyID) {
            return;
        }

        //Get offset
        int postUnlockLength = (int)_f[5];
        int boothNameLength = (int)_f[6];

        //Get boothName
        string boothName = "";
        for (int i = 7 + postUnlockLength;
                i <= postUnlockLength + boothNameLength + 6; i++) {
            boothName += (char)(int)_f[i];
        }
        //Debug.LogError("ReceiveAssessment boothName:" + boothName);

        //Find correct booth and forward data
        foreach (AssessmentManager asmtMgr in FindObjectsOfType<AssessmentManager>()) {
            if (boothName == asmtMgr.gameObject.GetComponent<BoothManager>().boothName) {
                //Debug.LogError("ReceiveAssessment booth found:" + boothName);
                asmtMgr.gameObject.GetComponent<AssessmentManager>().LoadAssessment(_f);
                return;
            }
        }
        
    }
     
    public void LoadAssessment(float[] _f) {
        //_f[0]: ASMT + 1 = Assessment Header Response
        //_f[1]: playerId already checked
        //Debug.LogError("LoadAssessment _f[0]:" + _f[0]);
        //Debug.LogError("LoadAssessment _f[1]:" + _f[1]);
        //Debug.LogError("LoadAssessment _f[2]:" + _f[2]);
        //Debug.LogError("LoadAssessment _f[3]:" + _f[3]);
        switch (_f[2]) {
            case 0:
                assessmentType = AssessmentType.assignment;
                break;
            case 1:
                assessmentType = AssessmentType.quiz;
                break;
            case 2:
                assessmentType = AssessmentType.test;
                testTimeLimit = _f[3];
                testTimeRemaining = _f[3];
                break;
        }
        //Debug.LogError("LoadAssessment _f[4]:" + _f[4]);
        num_TotalQuestions = (int)_f[4];
        assessmentQuestions = new question[num_TotalQuestions];
        //Debug.LogError("LoadAssessment assessmentQuestions:" + assessmentQuestions);
        //Debug.LogError("LoadAssessment assessmentQuestions.Length:" + assessmentQuestions.Length);
        num_CurrentQuestion = 0;
        num_CorrectAnswers = 0;
        //_f[6]: boothName.Length and boothName already checked

        //Get name of unlockAfterCompletion
        var tempUnlock = "";
        int postUnlockLength = (int)_f[5];
        for (int i = 7;
                 i <= postUnlockLength + 6;
                 i++) {
            tempUnlock += (char)(int)_f[i];
        }

        if (unlockAfterCompletion.Length == 0) unlockAfterCompletion = tempUnlock;
    }

    public static void ReceiveAssessmentQuestions(float[] _f) {
        //_f[0] = ASMT + 2 = Assessment Question Data
        //Debug.LogError("ReceiveAssessmentQuestions _f[1]:" + _f[1]);
        if (_f[1] != GameManager.MyID) {
            return;
        }

        //Get offset
        int numWrongAnswers = (int)_f[6];
        int totalTextWrongAnswersLength = 0;
        for (int i = 10;
                i <= numWrongAnswers + 9; i++) {
            totalTextWrongAnswersLength += (int)_f[i];
        }
        int qCorrectLength = (int)_f[7];
        int qTextLength = (int)_f[5];

        int boothNameLength = (int)_f[8];

        //Get boothName
        string boothName = "";
        for (int i = 10 + qCorrectLength + totalTextWrongAnswersLength + qTextLength + numWrongAnswers;
                i <= boothNameLength + qCorrectLength + totalTextWrongAnswersLength + qTextLength + numWrongAnswers + 9; i++) {
            boothName += (char)(int)_f[i];
        }
        //Debug.LogError("ReceiveAssessmentQuestions boothName:" + boothName);

        //Find correct booth and forward data
        foreach (AssessmentManager asmtMgr in FindObjectsOfType<AssessmentManager>()) {
            if (boothName == asmtMgr.gameObject.GetComponent<BoothManager>().boothName) {
                //Debug.LogError("ReceiveAssessmentQuestions booth found:" + boothName);
                asmtMgr.gameObject.GetComponent<AssessmentManager>().LoadAssessmentQuestions(_f);
                return;
            }
        }
    }

    public void LoadAssessmentQuestions(float[] _f) {
        StartCoroutine(DelayLoadAssessmentQuestions(_f));
    }

    IEnumerator DelayLoadAssessmentQuestions(float[] _f) {
        //Debug.LogWarning("DelayLoadAssessmentQuestions num_TotalQuestions: " + num_TotalQuestions);
        while (num_TotalQuestions == -1) {
            yield return new WaitForSecondsRealtime(1.0f);
        }

        //_f[0]: 2770 = ASMT + 2 = Assessment Question Data
        //_f[1]: playerId already checked
        int qNumber = (int)_f[2]; //_f[2]: Question Number [int]

        question thisQ = new question(); //Question Object
        thisQ.timer = _f[4]; //_f[4]: Question Timer [double -> float in seconds] (0 if none)
        thisQ.value = _f[9];

        switch ((int)_f[3]) {
            case 0:
                thisQ.questionType = QuestionType.MultipleChoice;
                break;
            case 1:
                thisQ.questionType = QuestionType.TrueFalse;
                break;
            case 2:
                thisQ.questionType = QuestionType.ShortAnswer;
                break;
                //Default check performed in GameManager.SendAssessment()
        }
        int qTextLength = (int)_f[5];
        int qNumWrongAnswers = (int)_f[6];
        int qCorrectLength = (int)_f[7];
        //_f[8]: boothName.Length and boothName already checked

        //  _f[10
        //      to numWrongAnswers + 9]:
        //          Length of wrong answers [int[]]
        int[] qLengthWrongAnswers = new int[qNumWrongAnswers];
        int totalLengthWrongAnswers = 0;
        for (int i = 10;
                i <= qNumWrongAnswers + 9; i++) {
            qLengthWrongAnswers[i - 10] = (int)_f[i];
            totalLengthWrongAnswers += (int)_f[i];
        }

        //  _f[numWrongAnswers + 10
        //      to q.text.Length + numWrongAnswers + 9]:
        //          Question Text [stringToFloats]
        string qText = "";
        for (int i = qNumWrongAnswers + 10;
                i <= qTextLength + qNumWrongAnswers + 9; i++) {
            qText += (char)(int)_f[i]; //char cast
        }
        thisQ.text = qText;

        //  _f[q.text.Length + numWrongAnswers + 10
        //      to textWrongAnswers.Length + q.text.Length + numWrongAnswers + 9]:
        //          Wrong answers Text [stringToFloats[]]
        answers wrongAnswers = new answers();
        wrongAnswers.Answer = new string[qNumWrongAnswers];
        int wrongAnswerLength = 0;
        for (int i = 0; i < wrongAnswers.Answer.Length; i++) {
            string ansText = "";
            for (int j = qTextLength + qNumWrongAnswers + 10 + wrongAnswerLength;
                    j <= qLengthWrongAnswers[i] + qTextLength + qNumWrongAnswers + 9 + wrongAnswerLength; j++) {
                ansText += (char)(int)_f[j]; //char cast
            }
            wrongAnswers.Answer[i] = ansText;
            //offset for each answer
            wrongAnswerLength += qLengthWrongAnswers[i];
        }
        thisQ.answers = wrongAnswers;

        string correctText = "";
        for (int i = totalLengthWrongAnswers + qTextLength + qNumWrongAnswers + 10;
                i <= qCorrectLength + totalLengthWrongAnswers + qTextLength + qNumWrongAnswers + 9; i++) {
            correctText += (char)(int)_f[i]; //char cast
        }
        thisQ.correct = correctText;

        
        //Check for already existing question
        if (assessmentQuestions[qNumber - 1] == null) {
            //Set question in questionArray "assessmentQuestions"
            assessmentQuestions[qNumber - 1] = thisQ;
        }
    }

    private void LinkObjects() {
        foreach (CanvasRenderer obj in GetComponentsInChildren<CanvasRenderer>(true)) {
            switch (obj.gameObject.name) {
                case "pnl_Start":
                    pnl_Start = obj.gameObject;
                    pnl_Start.SetActive(true);
                    break;
                case "pnl_Response":
                    pnl_Response = obj.gameObject;
                    pnl_Response.SetActive(false);
                    break;
                case "btn_Start":
                    btn_Start = obj.GetComponent<Button>();
                    if(NumberOfConcurrentUsers == 1)
                        btn_Start.onClick.AddListener(StartAssessment);
                    else
                        btn_Start.onClick.AddListener(_myCollabManager.SendStartMessage);
                    btn_Start.gameObject.SetActive(true);
                    break;
                case "pnl_WelcomeScreen":
                    pnl_WelcomeScreen = obj.gameObject;
                    pnl_WelcomeScreen.SetActive(true);
                    break;
                case "txt_BoothName":
                    txt_boothName = obj.GetComponent<TextMeshProUGUI>();
                    txt_boothName.gameObject.SetActive(true);
                    break;
                case "txt_BoothDesc":
                    txt_boothDesc = obj.GetComponent<TextMeshProUGUI>();
                    txt_boothDesc.gameObject.SetActive(true);
                    break;
                case "pnl_MultipleChoice":
                    pnl_MultipleChoice = obj.gameObject;
                    pnl_MultipleChoice.SetActive(false);
                    break;
                case "txt_MultChoiceQuestion":
                    txt_MultChoiceQuestion = obj.GetComponent<TextMeshProUGUI>();
                    txt_MultChoiceQuestion.gameObject.SetActive(true);
                    break;
                case "img_OptionA":
                    imgAry_multipleChoice[0] = obj.GetComponent<Image>();
                    btn_imgAry_multipleChoice[0] = obj.GetComponent<Button>();
                    if(NumberOfConcurrentUsers == 1)
                        btn_imgAry_multipleChoice[0].onClick.AddListener(() => ReceiveResponse(ResponseType.buttonA));
                    else
                        btn_imgAry_multipleChoice[0].onClick.AddListener(() => _myCollabManager.SendInput(CollaborativeManager.buttonA));
                    imgAry_multipleChoice[0].gameObject.SetActive(false);
                    break;
                case "img_OptionB":
                    imgAry_multipleChoice[1] = obj.GetComponent<Image>();
                    btn_imgAry_multipleChoice[1] = obj.GetComponent<Button>();
                    if(NumberOfConcurrentUsers == 1)
                        btn_imgAry_multipleChoice[1].onClick.AddListener(() => ReceiveResponse(ResponseType.buttonB));
                    else
                        btn_imgAry_multipleChoice[1].onClick.AddListener(() => _myCollabManager.SendInput(CollaborativeManager.buttonB));
                    imgAry_multipleChoice[1].gameObject.SetActive(false);
                    break;
                case "img_OptionC":
                    imgAry_multipleChoice[2] = obj.GetComponent<Image>();
                    btn_imgAry_multipleChoice[2] = obj.GetComponent<Button>();
                    if(NumberOfConcurrentUsers == 1)
                        btn_imgAry_multipleChoice[2].onClick.AddListener(() => ReceiveResponse(ResponseType.buttonC));
                    else
                        btn_imgAry_multipleChoice[2].onClick.AddListener(() => _myCollabManager.SendInput(CollaborativeManager.buttonC));
                    imgAry_multipleChoice[2].gameObject.SetActive(false);
                    break;
                case "img_OptionD":
                    imgAry_multipleChoice[3] = obj.GetComponent<Image>();
                    btn_imgAry_multipleChoice[3] = obj.GetComponent<Button>();
                    if(NumberOfConcurrentUsers == 1)
                        btn_imgAry_multipleChoice[3].onClick.AddListener(() => ReceiveResponse(ResponseType.buttonD));
                    else
                        btn_imgAry_multipleChoice[3].onClick.AddListener(() => _myCollabManager.SendInput(CollaborativeManager.buttonD));
                    imgAry_multipleChoice[3].gameObject.SetActive(false);
                    break;
                case "txt_LetterA":
                    txtAry_mcOptionLetters[0] = obj.GetComponent<TextMeshProUGUI>();
                    txtAry_mcOptionLetters[0].gameObject.SetActive(true);
                    break;
                case "txt_LetterB":
                    txtAry_mcOptionLetters[1] = obj.GetComponent<TextMeshProUGUI>();
                    txtAry_mcOptionLetters[1].gameObject.SetActive(true);
                    break;
                case "txt_LetterC":
                    txtAry_mcOptionLetters[2] = obj.GetComponent<TextMeshProUGUI>();
                    txtAry_mcOptionLetters[2].gameObject.SetActive(true);
                    break;
                case "txt_LetterD":
                    txtAry_mcOptionLetters[3] = obj.GetComponent<TextMeshProUGUI>();
                    txtAry_mcOptionLetters[3].gameObject.SetActive(true);
                    break;
                case "txt_AnswerA":
                    txtAry_mcOptionText[0] = obj.GetComponent<TextMeshProUGUI>();
                    txtAry_mcOptionText[0].gameObject.SetActive(true);
                    break;
                case "txt_AnswerB":
                    txtAry_mcOptionText[1] = obj.GetComponent<TextMeshProUGUI>();
                    txtAry_mcOptionText[1].gameObject.SetActive(true);
                    break;
                case "txt_AnswerC":
                    txtAry_mcOptionText[2] = obj.GetComponent<TextMeshProUGUI>();
                    txtAry_mcOptionText[2].gameObject.SetActive(true);
                    break;
                case "txt_AnswerD":
                    txtAry_mcOptionText[3] = obj.GetComponent<TextMeshProUGUI>();
                    txtAry_mcOptionText[3].gameObject.SetActive(true);
                    break;
                case "ButtonA":
                    btnAry_multipleChoice[0] = obj.GetComponent<Button>();
                    if(NumberOfConcurrentUsers == 1)
                        btnAry_multipleChoice[0].onClick.AddListener(() => ReceiveResponse(ResponseType.buttonA));
                    else
                        btnAry_multipleChoice[0].onClick.AddListener(() => _myCollabManager.SendInput(CollaborativeManager.buttonA));
                    btnAry_multipleChoice[0].gameObject.SetActive(true);
                    break;
                case "ButtonB":
                    btnAry_multipleChoice[1] = obj.GetComponent<Button>();
                    if(NumberOfConcurrentUsers == 1)
                        btnAry_multipleChoice[1].onClick.AddListener(() => ReceiveResponse(ResponseType.buttonB));
                    else
                        btnAry_multipleChoice[1].onClick.AddListener(() => _myCollabManager.SendInput(CollaborativeManager.buttonB));
                    btnAry_multipleChoice[1].gameObject.SetActive(true);
                    break;
                case "ButtonC":
                    btnAry_multipleChoice[2] = obj.GetComponent<Button>();
                    if(NumberOfConcurrentUsers == 1)
                        btnAry_multipleChoice[2].onClick.AddListener(() => ReceiveResponse(ResponseType.buttonC));
                    else
                        btnAry_multipleChoice[2].onClick.AddListener(() => _myCollabManager.SendInput(CollaborativeManager.buttonC));
                    btnAry_multipleChoice[2].gameObject.SetActive(true);
                    break;
                case "ButtonD":
                    btnAry_multipleChoice[3] = obj.GetComponent<Button>();
                    if(NumberOfConcurrentUsers == 1)
                        btnAry_multipleChoice[3].onClick.AddListener(() => ReceiveResponse(ResponseType.buttonD));
                    else
                        btnAry_multipleChoice[3].onClick.AddListener(() => _myCollabManager.SendInput(CollaborativeManager.buttonD));
                    btnAry_multipleChoice[3].gameObject.SetActive(true);
                    break;
                case "pnl_TrueFalse":
                    pnl_TrueFalse = obj.gameObject;
                    pnl_TrueFalse.SetActive(false);
                    break;
                case "txt_TrueFalseQuestion":
                    txt_TrueFalseQuestion = obj.GetComponent<TextMeshProUGUI>();
                    txt_TrueFalseQuestion.gameObject.SetActive(true);
                    break;
                case "img_True":
                    img_True = obj.GetComponent<Image>();
                    btn_img_True = obj.GetComponent<Button>();
                    if(NumberOfConcurrentUsers == 1)
                        btn_img_True.onClick.AddListener(() => ReceiveResponse(ResponseType.buttonTrue));
                    else
                        btn_img_True.onClick.AddListener(() => _myCollabManager.SendInput(CollaborativeManager.buttonTrue));
                    img_True.gameObject.SetActive(true);
                    break;
                case "img_False":
                    img_False = obj.GetComponent<Image>();
                    btn_img_False = obj.GetComponent<Button>();
                    if(NumberOfConcurrentUsers == 1)
                        btn_img_False.onClick.AddListener(() => ReceiveResponse(ResponseType.buttonFalse));
                    else
                        btn_img_False.onClick.AddListener(() => _myCollabManager.SendInput(CollaborativeManager.buttonFalse));
                    img_False.gameObject.SetActive(true);
                    break;
                case "txt_True":
                    txt_True = obj.GetComponent<TextMeshProUGUI>();
                    txt_True.gameObject.SetActive(true);
                    break;
                case "txt_False":
                    txt_False = obj.GetComponent<TextMeshProUGUI>();
                    txt_False.gameObject.SetActive(true);
                    break;
                case "ButtonTrue":
                    btn_True = obj.GetComponent<Button>();
                    if(NumberOfConcurrentUsers == 1)
                        btn_True.onClick.AddListener(() => ReceiveResponse(ResponseType.buttonTrue));
                    else
                        btn_True.onClick.AddListener(() => _myCollabManager.SendInput(CollaborativeManager.buttonTrue));
                    btn_True.gameObject.SetActive(true);
                    break;
                case "ButtonFalse":
                    btn_False = obj.GetComponent<Button>();
                    if(NumberOfConcurrentUsers == 1)
                        btn_False.onClick.AddListener(() => ReceiveResponse(ResponseType.buttonFalse));
                    else
                        btn_False.onClick.AddListener(() => _myCollabManager.SendInput(CollaborativeManager.buttonFalse));
                    btn_False.gameObject.SetActive(true);
                    break;
                case "pnl_ShortAnswer":
                    pnl_ShortAnswer = obj.gameObject;
                    pnl_ShortAnswer.SetActive(false);
                    break;
                case "txt_ShortAnswerQuestion":
                    txt_ShortAnswerQuestion = obj.GetComponent<TextMeshProUGUI>();
                    txt_ShortAnswerQuestion.gameObject.SetActive(true);
                    break;
                case "txt_ShortAnswerResponse":
                    txt_ShortAnswerResponse = obj.GetComponent<TextMeshProUGUI>();
                    txt_ShortAnswerResponse.gameObject.SetActive(true);
                    break;
                case "txt_ShortAnswerCorrect":
                    txt_ShortAnswerCorrect = obj.GetComponent<TextMeshProUGUI>();
                    txt_ShortAnswerCorrect.gameObject.SetActive(false);
                    break;
                case "InputField":
                    ipt_Answer = obj.GetComponent<InputField>();
                    ipt_Answer.gameObject.SetActive(true);
                    break;
                case "Submit":
                    btn_Submit = obj.GetComponent<Button>();
                    if(NumberOfConcurrentUsers == 1)
                        btn_Submit.onClick.AddListener(() => ReceiveResponse(ResponseType.buttonSubmit));
                    else
                        btn_Submit.onClick.AddListener(() => _myCollabManager.SendInput(CollaborativeManager.buttonSubmit));
                    btn_Submit.gameObject.SetActive(true);
                    break;
                case "pnl_Dashboard":
                    pnl_Dashboard = obj.gameObject;
                    pnl_Dashboard.SetActive(false);
                    break;
                case "pnl_Progress":
                    pnl_Progress = obj.gameObject;
                    pnl_Progress.SetActive(true);
                    break;
                case "lbl_Question":
                    lbl_Question = obj.GetComponent<TextMeshProUGUI>();
                    lbl_Question.gameObject.SetActive(true);
                    break;
                case "txt_QuestionNumber":
                    txt_QuestionNumber = obj.GetComponent<TextMeshProUGUI>();
                    txt_QuestionNumber.gameObject.SetActive(true);
                    break;
                case "pnl_QuestionValue":
                    pnl_QuestionValue = obj.gameObject;
                    pnl_QuestionValue.SetActive(false);
                    break;
                case "lbl_QuestionValue":
                    lbl_QuestionValue = obj.GetComponent<TextMeshProUGUI>();
                    lbl_QuestionValue.gameObject.SetActive(true);
                    break;
                case "txt_QuestionValue":
                    txt_QuestionValue = obj.GetComponent<TextMeshProUGUI>();
                    txt_QuestionValue.gameObject.SetActive(true);
                    break;
                case "pnl_CurrentPoints":
                    pnl_CurrentPoints = obj.gameObject;
                    pnl_CurrentPoints.SetActive(false);
                    break;
                case "lbl_CurrentPoints":
                    lbl_CurrentPoints = obj.GetComponent<TextMeshProUGUI>();
                    lbl_CurrentPoints.gameObject.SetActive(true);
                    break;
                case "txt_CurrentPoints":
                    txt_CurrentPoints = obj.GetComponent<TextMeshProUGUI>();
                    txt_CurrentPoints.gameObject.SetActive(true);
                    break;
                case "pnl_CurrentScore":
                    pnl_CurrentScore = obj.gameObject;
                    pnl_CurrentScore.SetActive(false);
                    break;
                case "lbl_CurrentScore":
                    lbl_CurrentScore = obj.GetComponent<TextMeshProUGUI>();
                    lbl_CurrentScore.gameObject.SetActive(true);
                    break;
                case "txt_CurrentScore":
                    txt_CurrentScore = obj.GetComponent<TextMeshProUGUI>();
                    txt_CurrentScore.gameObject.SetActive(true);
                    break;
                case "pnl_TimeLeft":
                    pnl_TimeLeft = obj.gameObject;
                    pnl_TimeLeft.SetActive(false);
                    break;
                case "lbl_TimeLeft":
                    lbl_TimeLeft = obj.GetComponent<TextMeshProUGUI>();
                    lbl_TimeLeft.gameObject.SetActive(true);
                    break;
                case "txt_TimeLeft":
                    txt_TimeLeft = obj.GetComponent<TextMeshProUGUI>();
                    txt_TimeLeft.gameObject.SetActive(true);
                    break;
                case "pnl_QuestionTimer":
                    pnl_QuestionTimer = obj.gameObject;
                    pnl_QuestionTimer.SetActive(false);
                    break;
                case "img_QuestionTimer":
                    img_QuestionTimer = obj.GetComponent<Image>();
                    QUESTION_TIMER_INTIAL_WIDTH = img_QuestionTimer.rectTransform.rect.width;
                    img_QuestionTimer.gameObject.SetActive(true);
                    break;
                case "lbl_QuestionTimer":
                    lbl_QuestionTimer = obj.GetComponent<TextMeshProUGUI>();
                    lbl_QuestionTimer.gameObject.SetActive(true);
                    break;
                case "txt_QuestionTimer":
                    txt_QuestionTimer = obj.GetComponent<TextMeshProUGUI>();
                    txt_QuestionTimer.gameObject.SetActive(true);
                    break;
                case "pnl_EndScreen":
                    pnl_EndScreen = obj.gameObject;
                    pnl_EndScreen.SetActive(false);
                    break;
                case "txt_EndTitle":
                    txt_EndTitle = obj.GetComponent<TextMeshProUGUI>();
                    txt_EndTitle.gameObject.SetActive(true);
                    break;
                case "txt_EndDesc":
                    txt_EndDesc = obj.GetComponent<TextMeshProUGUI>();
                    txt_EndDesc.gameObject.SetActive(true);
                    break;
                default:
                    break;
            }
        }
    }
    #endregion
}
