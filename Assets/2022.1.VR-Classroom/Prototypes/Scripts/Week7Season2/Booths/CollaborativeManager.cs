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

    //Definable number of students
    public int MaxStudents;
    //maybe float[] and track students ids
    public List<float> curStudents = new List<float>();
    public int RandomVal;

    #region MessageHeaders
    //Answer inputs - dont necessarily need to be message headers could use the same header for all depends on how much information we want to send
    const float QuizStarted = 100;

    const float buttonA = 101;
    const float buttonB = 102;
    const float buttonC = 103;
    const float buttonD = 104;
    const float buttonTrue = 105;
    const float buttonFalse = 106;
    const float buttonSubmit = 107;
    const float ShortAnswerUpdate = 108;

    const float NewRandom = 109;

    #endregion
    // Need to sync the randomize result IE need to take the result from the first student in curStudents
    //
    // Start is called before the first frame update
    void Start()
    {
        _myAssessmentManager = gameObject.GetComponent<AssessmentManager>();
        _myBooth = gameObject.GetComponent<BoothManager>();
        m_ASLObject = gameObject.GetComponent<ASLObject>();
    }

    public void SetMaxStudents(){

    }
    public void DisableBooth(){

    }

    #region Sending Floats
    //Send new random to each person taking the quiz
    //Intent is to provide a random float value that is the same for all members taking the collaborative quiz
    public void SendNewRandom(){
        List<float> NewRandomList = new List<float>();
        System.Random random = new System.Random();
        NewRandomList[1] = NewRandom;
        NewRandomList[2] = random.Next();
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
        NewFloats[0] = -1;
        NewFloats[1] = QuizStarted;
        NewFloats[2] = (float)GameManager.MyID;
        var FloatsArray = NewFloats.ToArray();
        m_ASLObject.SendAndSetClaim(() => {
            m_ASLObject.SendFloatArray(FloatsArray);
        });    
    }
    //expected input should be a float between the values of 101 - 107
    public void SendInput(float _f){
        List<float> NewInput = new List<float>();
        NewInput[1] = _f;
        for(int i = 0; i< curStudents.Count; i++){
            NewInput[0] = curStudents[i];
            var FloatsInput = NewInput.ToArray();
            m_ASLObject.SendAndSetClaim(() => {
                m_ASLObject.SendFloatArray(FloatsInput);
            });
        }
    }

    public void FloatReceive(string _id, float[] _f) {
        if((int)_f[0] == GameManager.MyID || (int)_f[0] == -1){
            switch(_f[1]){
                case QuizStarted:{
                    curStudents.Add(_f[2]);
                    break;
                }
                case buttonA:{

                    break;   
                }
                case buttonB:{

                    break;   
                }
                case buttonC:{

                    break;   
                }
                case buttonD:{

                    break;   
                }
                case buttonTrue:{

                    break;   
                }
                case buttonFalse:{

                    break;   
                }
                case buttonSubmit:{

                    break;   
                }
                case ShortAnswerUpdate:{

                    break;   
                }
                case NewRandom:{
                    RandomVal = (int)_f[2];
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
        if(MaxStudents <= curStudents.Count){
            //disable start button and lock booth
        }
    }
}