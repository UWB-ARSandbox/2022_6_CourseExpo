using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;
using ASL;

public class ChatManager : MonoBehaviour
{
    #region variables    
    enum ChatMode {
        All,
        Local,
        Direct
    }
    ChatMode chatMode = ChatMode.All;
    private int mostRecentlyMessaged = -1;
    TMP_Text textPlaceholder;
    public float localChatDistance = 20f;

    TMP_InputField inputField;
    TMP_Text localPlayersText;
    Text historyText;
    bool inputActive => PlayerController.IsTypingInput;
    bool selected => EventSystem.current.currentSelectedGameObject == inputField.gameObject;
    bool inputEmpty => inputField.text == "";

    GameObject historyObject;
    GameObject messageInput;
    GameObject localPlayersObject;
    Image historyBg;
    Color originalHistoryBg;
    Coroutine HideHistory;
    
    ASLObject m_ASLObject;
    #endregion

    #region Initialization
    // Start is called before the first frame update
    void Start()
    {
        messageInput = transform.Find("Background").gameObject;
        inputField = transform.Find("Background/Input").GetComponent<TMP_InputField>();
        AlignCaret();
        
        inputField.enabled = false;
        if (gameObject.name == "Chat")
            textPlaceholder = transform.Find("Background/Input Placeholder").GetComponent<TMP_Text>();
        
        historyObject = transform.Find("ChatHistory").gameObject;
        historyText = historyObject.transform.Find("Background/History").GetComponent<Text>();
        historyBg = historyObject.transform.Find("Background").GetComponent<Image>();
        
        originalHistoryBg = new Color(historyBg.color.r, historyBg.color.g, historyBg.color.b, historyBg.color.a);
        historyBg.color = new Color(originalHistoryBg.r, originalHistoryBg.g, originalHistoryBg.b, 0);
        historyText.GetComponent<CanvasRenderer>().SetAlpha(0f);

        localPlayersObject = transform.Find("Local Players").gameObject;
        localPlayersText = transform.Find("Local Players/Text").GetComponent<TMP_Text>();

        m_ASLObject = GetComponent<ASLObject>();
        m_ASLObject._LocallySetFloatCallback(FloatReceive);

        StartCoroutine(PlayerDistancesUpdate());
    }

    void AlignCaret() {
        messageInput.SetActive(true);
        inputField.ActivateInputField();
        transform.Find("Background/Caret").position += new Vector3(2.25f, 0, 0);
        inputField.DeactivateInputField();
        messageInput.SetActive(false);
    }
    #endregion

    // Update is called once per frame
    void Update()
    {
        if (EventSystem.current.currentSelectedGameObject != null) {
            PlayerController.IsTypingInput = EventSystem.current.currentSelectedGameObject.tag == "InputField";
        }
        if (selected) {
            if (Keyboard.current[Key.Enter].wasPressedThisFrame) {
                StartCoroutine(TrySendMessage());
            } else if (Keyboard.current[Key.Tab].wasPressedThisFrame) {
                SwitchChatMode();
            }
        }
    }

    IEnumerator PlayerDistancesUpdate() {
        Transform ownPlayer = null;
        while (ownPlayer == null) {
            yield return new WaitForSeconds(0.2f);
            ownPlayer = FindObjectOfType<PlayerController>().transform;
        }
        while (true) {
            if (chatMode == ChatMode.Local) {
                UpdateLocalPlayers(ownPlayer);
            }
            
            if (selected) {
                yield return new WaitForSeconds(0.2f); //keep it more consistent since the player currently has the window open
            } else {
                yield return new WaitForSeconds(1f);
            }
        }
    }

    void UpdateLocalPlayers(Transform ownPlayer = null) {
        if (ownPlayer == null) {
            ownPlayer = FindObjectOfType<PlayerController>().transform;
        }

        string nearbyPlayers = "";
        foreach (GhostPlayer gp in FindObjectsOfType<GhostPlayer>()) {
            if (gp.ownerID != GameManager.MyID && Vector3.Distance(ownPlayer.position, gp.transform.position) < localChatDistance) {
                nearbyPlayers += $"\n{GameManager.players[gp.ownerID]}";
            }
        }
        
        if (nearbyPlayers == "") {
            localPlayersText.text = "<b>Local players:</b>\nNone";
        } else {
            localPlayersText.text = $"<b>Local players:</b>{nearbyPlayers}";
        }
    }

    IEnumerator TrySendMessage() {
        if (selected) {
            if (!inputEmpty) {
                string message = inputField.text;
                int playerID = GameManager.MyID;
                
                int messageType = 100;
                int otherPlayerID = 0;
                switch (chatMode) {
                    case ChatMode.All:
                        messageType = 100;
                        break;
                    case ChatMode.Local:
                        messageType = 101;
                        break;
                    case ChatMode.Direct:
                        //@name indicates that you want to message that person
                        if (message[0] == '@') {
                            bool playerNameExists = false;
                            bool playerNameAtStart = false;
                            foreach (var kvp in GameManager.players) {
                                //See if their message contains any valid/existing playernames
                                if (message.Contains(kvp.Value)) {
                                    playerNameExists = true;
                                    //check to see if it's right after the @
                                    if (message[message.IndexOf(kvp.Value) - 1] == '@') {
                                        playerNameAtStart = true;
                                        otherPlayerID = kvp.Key;
                                        mostRecentlyMessaged = otherPlayerID;
                                        textPlaceholder.text = $"Message {GameManager.players[mostRecentlyMessaged]}";
                                        if (message.Contains(kvp.Value + " ")) {
                                            message = message.Substring(kvp.Value.Length + 2);
                                        } else {
                                            message = message.Substring(kvp.Value.Length + 1);
                                        }
                                        break;
                                    }
                                    continue; //a name was found so far, but it wasn't after the @, so keep checking
                                }
                            }
                            if (!playerNameExists) {
                                //They haven't messaged anyone before (so it doesn't default to anyone), and they didn't put in a proper name
                                OnSendMessageError("\n<color=red>The specified player could not be found</color>");
                                yield break;
                            }
                            if (!playerNameAtStart) {
                                //Name was put in somewhere, but not right after the @ at the start
                                OnSendMessageError("\n<color=red>Format direct messages like so:</color> @PlayerName message");
                                yield break;
                            }
                        } else {
                            //Message doesn't start with @, check if they've messaged someone before
                            if (mostRecentlyMessaged == -1) {
                                //If they haven't (and thus it wouldn't default to anyone), give an error
                                OnSendMessageError("\n<color=red>Format direct messages like so:</color> @PlayerName message");
                                yield break;
                            }
                            else {
                                //They've messaged someone already, so use the id of the most recently messaged person for convenience
                                otherPlayerID = mostRecentlyMessaged;
                            }
                        }
                        messageType = 102;
                        break;
                }

                int extraLength = (otherPlayerID == 0 ? 2 : 3);
                float[] messageToSend = new float[message.Length + extraLength];

                messageToSend[0] = messageType;
                messageToSend[1] = playerID;
                if (chatMode == ChatMode.Direct)
                    messageToSend[2] = otherPlayerID;
                
                for (int i = extraLength; (i - extraLength) < message.Length; i++) {
                    messageToSend[i] = (float)(int)message[i - extraLength];
                }
                
                foreach (ChatManager chatManager in FindObjectsOfType<ChatManager>(true)) {
                    chatManager.SendMessage(messageToSend);
                }
                HideChat();
                inputField.text = "";
            } 
            HideChat();
            OnDeselect();
            transform.Find("Background").GetComponent<Image>().enabled = false;
        }
        yield return null;
    }
    
    void OnSendMessageError(string errorMessage) {
        AddMessage(errorMessage);
        inputField.ActivateInputField();
    }

    private void SwitchChatMode() {
        switch (chatMode) {
            case ChatMode.All:
                chatMode = ChatMode.Local;
                textPlaceholder.text = "Message Locally";
                localPlayersObject.SetActive(true);
                UpdateLocalPlayers();
                break;
            case ChatMode.Local:
                chatMode = ChatMode.Direct;
                if (mostRecentlyMessaged == -1)
                    textPlaceholder.text = "Direct (@username message)";
                else
                    textPlaceholder.text = $"Message {GameManager.players[mostRecentlyMessaged]}";
                localPlayersObject.SetActive(false);
                break;
            case ChatMode.Direct:
                chatMode = ChatMode.All;
                textPlaceholder.text = "Message All";
                localPlayersObject.SetActive(false);
                break;
        }
    }

    public void SendMessage(float[] messageToSend) {
        m_ASLObject.SendAndSetClaim(() => {
            m_ASLObject.SendFloatArray(messageToSend);
        });
    }

    void ToggleTypingBubbleSync() {
        float[] toggleCode = new float[2];
        if (messageInput.activeSelf) {
            toggleCode[0] = 103f; //display
        } else {
            toggleCode[0] = 104f; //hide
        }
        toggleCode[1] = (float)GameManager.MyID;
        SendMessage(toggleCode);
    }

    void ToggleTypingBubble(bool enable, int playerID) {
        foreach (GhostPlayer gp in FindObjectsOfType<GhostPlayer>()) {
            if (gp.ownerID == playerID) {
                if (enable) {
                    gp.transform.Find("Head/TypingIndicator").gameObject.SetActive(true);
                } else {
                    gp.transform.Find("Head/TypingIndicator").gameObject.SetActive(false);
                }
                break;
            }
        }
    }

    #region UI
    public void OpenInput() {
        if (!inputActive && !GameManager.isTakingAssessment) {
            if (HideHistory != null) {StopCoroutine(HideHistory);}
            ShowHistory();

            messageInput.SetActive(true);
            if (chatMode == ChatMode.Local) {localPlayersObject.SetActive(true);}
            Cursor.lockState = CursorLockMode.None;
            inputField.enabled = true;
            PlayerController.IsTypingInput = true;
            inputField.ActivateInputField();
            transform.Find("Background").GetComponent<Image>().enabled = true;
            
            EventSystem.current.SetSelectedGameObject(inputField.gameObject);
            ToggleTypingBubbleSync();
        }
    }

    public void OnDeselect() {
        Cursor.lockState = CursorLockMode.Locked;
        PlayerController.IsTypingInput = false;
        inputField.DeactivateInputField();
        inputField.enabled = false;
        messageInput.SetActive(false);
        if (chatMode == ChatMode.Local) {localPlayersObject.SetActive(false);}
        EventSystem.current.SetSelectedGameObject(null);
        HideChat();
    }

    public void ShowHistory() {
        if (!GameManager.isTakingAssessment) {
            historyBg.color = originalHistoryBg;
            historyText.GetComponent<CanvasRenderer>().SetAlpha(1f);
            HideChat();
        }
    }

    public void HideChat() {
        if (inputEmpty) {
            if (HideHistory != null) {StopCoroutine(HideHistory);}
            HideHistory = StartCoroutine(HideAfterTime());
        }
        if (!selected) {
            messageInput.SetActive(false);
            if (chatMode == ChatMode.Local) {localPlayersObject.SetActive(false);}
        }
        ToggleTypingBubbleSync();
    }

    IEnumerator HideAfterTime() {
        yield return new WaitForSeconds(5f);
        if (!selected && inputEmpty) {  
            for (float i = 1; i >= 0; i -= Time.deltaTime) {
                SetHistoryTransparency(i);
                yield return null;
            }
            SetHistoryTransparency(0f);
        }
    }

    private void SetHistoryTransparency(float alpha) {
            float newAlpha = originalHistoryBg.a * alpha;
            Color newColor = new Color(originalHistoryBg.r, originalHistoryBg.g, originalHistoryBg.b, newAlpha);
            historyText.GetComponent<CanvasRenderer>().SetAlpha(alpha);
            historyBg.color = newColor;
    }

    public void AddMessage(string message) {
        historyText.text += message;
        if (!GameManager.isTakingAssessment)
            ShowHistory();
    }
    #endregion
    
    #region Receiving and Formatting
    void FloatReceive(string _id, float[] _f)
    {
        int opcode = (int)_f[0];
        int playerID = (int)_f[1];
        string message = "";
        switch(opcode) {
            case 100: //All
                for (int i = 2; i < _f.Length; i++) {
                    message += (char)(int)_f[i];
                }
                
                message = $"\n(All) {FormatPlayerMessage(playerID, message)}";
                break;
            case 101: //Local
                for (int i = 2; i < _f.Length; i++) {
                    message += (char)(int)_f[i];
                }
                
                if (WithinLocalDistanceRange(playerID)) {
                    message = $"\n(Local) {FormatPlayerMessage(playerID, message)}";
                } else {
                    return;
                }
                break;
            case 102: //Direct
                int otherPlayerID = (int)_f[2];
                for (int i = 3; i < _f.Length; i++) {
                    message += (char)(int)_f[i];
                }
                
                if (playerID == GameManager.MyID) {
                    message = $"\n(To <b>{FormatPlayerName(otherPlayerID)}</b>): {message}";
                } else {
                    //only display message to those that received it
                    if (otherPlayerID == GameManager.MyID) {
                        message = $"\n(From <b>{FormatPlayerName(playerID)}</b>): {message}";
                    } else {
                        return;
                    }
                }
                break;
            case 103: //show messaging bubble
                ToggleTypingBubble(true, playerID);
                return;
            case 104: //hide messaging bubble
                ToggleTypingBubble(false, playerID);
                return;
        }
        if (message != "") {
            AddMessage(message);
        }
    }

    bool WithinLocalDistanceRange(int otherPlayerID) {
        Transform otherPlayer = null;
        Transform ownPlayer = FindObjectOfType<PlayerController>().transform;
        if (otherPlayerID == GameManager.MyID) {
            return true;
        }
        foreach (GhostPlayer gp in FindObjectsOfType<GhostPlayer>()) {
            if (gp.ownerID == otherPlayerID) {
                otherPlayer = gp.transform;
                break;
            }
        }
        return Vector3.Distance(ownPlayer.position, otherPlayer.position) < localChatDistance;
    }

    string FormatPlayerMessage(int playerID, string message) {
        string playerName = FormatPlayerName(playerID);

        message = playerName + "<b>:</b> " + message;
        return message;
    }

    string FormatPlayerName(int playerID) {
        string playerName = GameManager.players[playerID];
        playerName = "<b>" + playerName + "</b>"; //bold name
        string color = playerID == 1 ? "<color=yellow>" : "<color=red>";
        playerName = color + playerName + "</color>"; //set color
        return playerName;
    }
    #endregion
}