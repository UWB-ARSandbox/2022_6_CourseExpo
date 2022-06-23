using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnnouncementManager : MonoBehaviour
{
    public AnnouncementDisplay pnl_PCAnnouncement;
    public AnnouncementDisplay pnl_VRAnnouncement;

    public Button btn_SendAnnouncement;
    public Button btn_CancelAnnouncement;
    public TMP_InputField inpt_Announcement;
    public GameObject pnl_PCUIBackground;
    public GameObject pnl_SendAnnouncement;

    public AnnouncementScreen announcementStoragePC;
    public AnnouncementScreen announcementStorageVR;

    public GameObject crosshair;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(pnl_PCAnnouncement != null);
        Debug.Assert(pnl_VRAnnouncement != null);
        Debug.Assert(btn_SendAnnouncement != null);
        Debug.Assert(btn_CancelAnnouncement != null);
        Debug.Assert(inpt_Announcement != null);
        Debug.Assert(pnl_PCUIBackground != null);
        Debug.Assert(pnl_SendAnnouncement != null);
        Debug.Assert(crosshair != null);
        pnl_PCAnnouncement.gameObject.SetActive(false);
        pnl_VRAnnouncement.gameObject.SetActive(false);
        pnl_PCUIBackground.SetActive(false);
        pnl_SendAnnouncement.SetActive(false);
        btn_SendAnnouncement.onClick.AddListener(SendAnnouncement);
        btn_CancelAnnouncement.onClick.AddListener(CancelAnnouncement);
    }

    void Update() {
        if (pnl_SendAnnouncement.gameObject.activeSelf) {
            PlayerController.IsTypingInput = inpt_Announcement.isFocused;
        }
    }

    public void OpenAnnouncementDialog() {
        inpt_Announcement.text = "";
        inpt_Announcement.textComponent.enableWordWrapping = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        pnl_PCUIBackground.SetActive(true);
        pnl_SendAnnouncement.SetActive(true);
        crosshair.SetActive(false);
    }

    public void CloseAnnouncementDialog()
    {
        Cursor.lockState = CursorLockMode.Locked;
        pnl_PCUIBackground.SetActive(false);
        pnl_SendAnnouncement.SetActive(false);
        crosshair.SetActive(true);
    }

    private void SendAnnouncement() {
        GameManager.SendAnnouncement(inpt_Announcement.text);
        Cursor.lockState = CursorLockMode.Locked;
        pnl_PCUIBackground.SetActive(false);
        pnl_SendAnnouncement.SetActive(false);
        crosshair.SetActive(true);
    }

    private void CancelAnnouncement() {
        Cursor.lockState = CursorLockMode.Locked;
        pnl_PCUIBackground.SetActive(false);
        pnl_SendAnnouncement.SetActive(false);
        crosshair.SetActive(true);
    }

    public static void ReceiveAnnouncement(float[] _f) {
        FindObjectOfType<AnnouncementManager>().LoadAnnouncement(_f);
    }

    private void LoadAnnouncement(float[] _f) {
        //Get length of string
        int length = (int)_f[1];
        string announcementText = "";
        for (int i = 2; i <= length + 1; i++) {
            announcementText += (char)(int)_f[i];
        }

        announcementStoragePC.addToList(announcementText);
        announcementStorageVR.addToList(announcementText);
        pnl_PCAnnouncement.DisplayAnnouncement(announcementText);
        pnl_VRAnnouncement.DisplayAnnouncement(announcementText);
    }
}