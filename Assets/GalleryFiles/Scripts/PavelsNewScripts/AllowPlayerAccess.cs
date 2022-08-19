using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ASL;

//Currently unsued script. Was intended as a way of requesting access to a canvas so that a person can join it
public class AllowPlayerAccess : MonoBehaviour, IClickable
{
    // Start is called before the first frame update
    public NewPaint canvas;

    [SerializeField] GameObject myText;

    [SerializeField] GameObject myButton;

    [SerializeField] int ownerID;

    int studentToEnable;

    
    
    void Start()
    {
        GetComponent<ASL.ASLObject>()._LocallySetFloatCallback(recieveInput);
    }

    // Update is called once per frame
    public void IClickableClicked()
    {
        float[] fArray = {ASL.GameLiftManager.GetInstance().m_PeerId};
        this.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
            {
				
                GetComponent<ASL.ASLObject>().SendFloatArray(fArray);
            });
    }

    public void recieveInput(string id, float[] i)
	{
        if(ASL.GameLiftManager.GetInstance().m_PeerId == ownerID)
        {
            myText.SetActive(true);
            myText.GetComponent<Text>().text = ASL.GameLiftManager.GetInstance().m_Players[(int)i[0]] + " has requested access";
            
            myButton.SetActive(true);
            myButton.GetComponent<Button>().onClick.RemoveAllListeners();
            myButton.GetComponent<Button>().onClick.AddListener(enableCanvasForStudent);
            studentToEnable = (int)i[0];
        }
    }
    void enableCanvasForStudent()
    {
        StartCoroutine(canvas.enableCanvasForPlayer(ASL.GameLiftManager.GetInstance().m_PeerId));
        StartCoroutine(canvas.enableViewingForPlayer(ASL.GameLiftManager.GetInstance().m_PeerId));
        myButton.GetComponent<Button>().onClick.RemoveAllListeners();

        myText.SetActive(false);
        myButton.SetActive(false);
    }
    
    


}
