using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ASL;

public class CollaborativeManager : MonoBehaviour
{

    public BoothManager myBooth;
    public ASLObject _myObject;

    //Definable number of students
    public int MaxStudents;

    #region MessageHeaders
    //Answer inputs
    const float buttonA = 101;
    const float buttonB = 102;
    const float buttonC = 103;
    const float buttonD = 104;
    const float buttonTrue = 105;
    const float buttonFalse = 106;
    const float buttonSubmit = 107;
    const float ShortAnswer = 108;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        
    }

    #region UI updates
    public void SendInput(){

    }
    public void FloatReceive(string _id, float[] _f) {

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
        
    }
}
