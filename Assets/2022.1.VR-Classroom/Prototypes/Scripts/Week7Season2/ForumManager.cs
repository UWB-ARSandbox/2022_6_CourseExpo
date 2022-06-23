using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;
using ASL;

public class ForumManager : MonoBehaviour
{
    #region variables
    TMP_InputField inputField;
    Text historyText;
    bool inputActive => PlayerController.IsTypingInput;
    bool selected => EventSystem.current.currentSelectedGameObject == inputField.gameObject;
    bool inputEmpty => inputField.text == "";

    GameObject historyObject;
    Coroutine HideHistory;
    
    ASLObject m_ASLObject;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        inputField = transform.Find("Background/Input").GetComponent<TMP_InputField>();
        inputField.enabled = true;
        
        historyObject = transform.Find("ChatHistory").gameObject;
        historyText = historyObject.transform.Find("Background/History").GetComponent<Text>();

        m_ASLObject = GetComponent<ASLObject>();
        m_ASLObject._LocallySetFloatCallback(FloatReceive);
    }

    // Update is called once per frame
    void Update()
    {
        if (selected) {
            if (Keyboard.current[Key.Enter].wasPressedThisFrame) {
                StartCoroutine(TrySendMessage());
            }
        }
    }

    IEnumerator TrySendMessage() {
        if (selected) {
            if (!inputEmpty) {
                string message = inputField.text;
                int playerID = GameManager.MyID;
                float[] messageToSend = new float[message.Length + 2];

                messageToSend[0] = 100;
                messageToSend[1] = playerID;
                for (int i = 2; (i - 2) < message.Length; i++) {
                    messageToSend[i] = (float)(int)message[i - 2];
                }
                SendMessage(messageToSend);
                inputField.text = "";
            } 
            OnDeselect();
        }
        yield return null;
    }

    public void SendMessage(float[] messageToSend) {
        m_ASLObject.SendAndSetClaim(() => {
            m_ASLObject.SendFloatArray(messageToSend);
        });
    }

    public void OpenInput() {
        Cursor.lockState = CursorLockMode.Confined;
        PlayerController.IsTypingInput = true;
        inputField.ActivateInputField();
        EventSystem.current.SetSelectedGameObject(inputField.gameObject);
    }

    public void OnDeselect() {
        Cursor.lockState = CursorLockMode.Locked;
        PlayerController.IsTypingInput = false;
        inputField.DeactivateInputField();
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void AddMessage(string message) {
        historyText.text += message;
    }
    
    #region Receiving
    void FloatReceive(string _id, float[] _f)
    {
        int opcode = (int)_f[0];
        switch(opcode) {
            case 100:
                int playerID = (int)_f[1];
                string message = "";
                for (int i = 2; i < _f.Length; i++) {
                    message += (char)(int)_f[i];
                }
                
                message = FormatPlayerMessage(playerID, message);
                AddMessage(message);
                break;
        }
    }

    string FormatPlayerMessage(int playerID, string message) {
        string playerName = GameLiftManager.GetInstance().m_Players[playerID];
        playerName = "<b>" + playerName + "</b>"; //bold name
        string color = playerID == 1 ? "<color=yellow>" : "<color=red>";
        playerName = color + playerName + "</color>"; //set color

        message = "\n" + playerName + "<b>:</b> " + message;
        return message;
    }
    #endregion
}