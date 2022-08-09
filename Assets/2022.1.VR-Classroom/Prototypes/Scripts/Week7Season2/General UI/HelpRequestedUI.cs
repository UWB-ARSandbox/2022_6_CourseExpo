using UnityEngine;
using UnityEngine.UI;
using ASL;

public class HelpRequestedUI : MonoBehaviour
{
    public GameObject scrollPanel;
    public GameObject buttonPrefab;
    public ASLObject m_ASLObject;
    float[] id = new float[2];
    public GameObject requestHelpButton;
    public bool VR_UI_Script;
    public bool CurrentlyHelping = false;
    public float CurrentlyHelping_id = -1;
    //public GameObject HelpingFinishedPrefab;
    GameObject HelpingFinishedButton;

    const float STUDENT_REQUEST = 100;
    const float REENABLE_BUTTON = 101;
    const float FINISH_HELPING = 102;

    void Start()
    {
        // requestHelpButton = GameObject.Find("RequestHelpButton");
        //m_ASLObject = GetComponent<ASLObject>();
        if(VR_UI_Script && PlayerController.isXRActive)
            m_ASLObject._LocallySetFloatCallback(FloatReceive);
        else if(!VR_UI_Script && !PlayerController.isXRActive)
            m_ASLObject._LocallySetFloatCallback(FloatReceive);

        // disable students request help button if you are a teacher
        // and disable the teachers help request ui if you are a student
        if (GameManager.AmTeacher)
            requestHelpButton.SetActive(false);
        else
            GameObject.Find("RequestHelpUI").SetActive(false);
    }

    // called by student when pressing request help button
    public void HelpRequested() {
        requestHelpButton.GetComponent<Button>().enabled = false;
        float[] m_myFloatArray = new float[2] { STUDENT_REQUEST, GameManager.MyID };
        m_ASLObject.SendAndSetClaim(() => { m_ASLObject.SendFloatArray(m_myFloatArray); });
    }

    public void ReenableButton(float _id) {
        CurrentlyHelping = true;
        CurrentlyHelping_id = _id;
        float[] m_myFloatArray = new float[2] { REENABLE_BUTTON, _id };
        m_ASLObject.SendAndSetClaim(() => { m_ASLObject.SendFloatArray(m_myFloatArray); });
    }
    //call the HelpFinished function when the teacher clicks the done helping button-> should return the user being helped and the teacher to their previous channels
    public void HelpFinished(){
        GameObject.Find("GameManager").GetComponent<AudioManager>().IsBeingHelped = false;
        GameObject.Find("GameManager").GetComponent<AudioManager>().ReturnToPreviousChannel();
        if(HelpingFinishedButton != null){
            Destroy(HelpingFinishedButton);
        }
        CurrentlyHelping = false;
        CurrentlyHelping_id = -1;
        float[] m_myFloatArray = new float[2] { FINISH_HELPING, CurrentlyHelping_id };
        m_ASLObject.SendAndSetClaim(() => { m_ASLObject.SendFloatArray(m_myFloatArray); });
    }
    public void SpawnHelpFinishedButton(){
        if(HelpingFinishedButton != null){
            Destroy(HelpingFinishedButton);
        }
        HelpingFinishedButton = Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        HelpingFinishedButton.GetComponent<HelpRequestButton>().username = GameManager.players[(int)CurrentlyHelping_id];
        HelpingFinishedButton.GetComponent<HelpRequestButton>().id = GameManager.MyID;
        HelpingFinishedButton.transform.parent = scrollPanel.transform;
        HelpingFinishedButton.transform.localScale = new Vector3(1, 1, 1);
    }

    void FloatReceive(string _id, float[] _f)
    {
        switch(_f[0]) {
            case REENABLE_BUTTON:
                if (!GameManager.AmTeacher && (int)_f[1] == GameManager.MyID)
                {
                    requestHelpButton.GetComponent<Button>().enabled = true;
                    //move user to private voice channel when teacher teleports to them
                    GameObject.Find("GameManager").GetComponent<AudioManager>().moveChannel("Private");
                    GameObject.Find("GameManager").GetComponent<AudioManager>().IsBeingHelped = true;
                }
                break;
            case STUDENT_REQUEST:
                if (GameManager.AmTeacher)
                {
                    Debug.Log(GameManager.players[(int)_f[1]] + " has requested help");
                    GameObject newButton = (GameObject)Instantiate(buttonPrefab, Vector3.zero, Quaternion.identity, scrollPanel.transform);
                    newButton.transform.localScale = new Vector3(1, 1, 1);
                    newButton.GetComponent<HelpRequestButton>().username = GameManager.players[(int)_f[1]];
                    newButton.GetComponent<HelpRequestButton>().id = (int)_f[1];
                }
                break;
            case FINISH_HELPING:
                if (!GameManager.AmTeacher && (int)_f[1] == GameManager.MyID)
                {
                    GameObject.Find("GameManager").GetComponent<AudioManager>().IsBeingHelped = false;
                    GameObject.Find("GameManager").GetComponent<AudioManager>().ReturnToPreviousChannel();
                }
                break;
        }
    }
}
