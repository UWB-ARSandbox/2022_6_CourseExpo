using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AnnouncementDisplay : MonoBehaviour
{
    public RectTransform canvasTransform;

    public TextMeshProUGUI txt_Announcement;
    public float SCROLL_SPEED = 0.075f;

    public TextMeshProUGUI txt_CloneAnnouncement;

    public RectTransform rect_AnnouncementText;

    public float width;
    public Vector3 startPosition;

    private const float FONT_SCALE = 0.62f;

    private string initialAnnouncement = "";
    private int scrollCount = 0;

    public Button btn_Dismiss;

    private void Awake() {
        txt_Announcement = GetComponentInChildren<TextMeshProUGUI>();
        canvasTransform = gameObject.GetComponentInParent<RectTransform>();

        rect_AnnouncementText = txt_Announcement.GetComponent<RectTransform>();
        width = txt_Announcement.preferredWidth;
        startPosition = rect_AnnouncementText.anchoredPosition;
        txt_Announcement.fontSize = canvasTransform.rect.height * FONT_SCALE;
        txt_Announcement.text += " \t ";

        txt_CloneAnnouncement = Instantiate(txt_Announcement);
        txt_CloneAnnouncement.fontSize = txt_Announcement.fontSize;
        RectTransform rect_CloneAnnouncementText = txt_CloneAnnouncement.GetComponent<RectTransform>();
        rect_CloneAnnouncementText.SetParent(rect_AnnouncementText);
        rect_CloneAnnouncementText.anchorMin = new Vector2(1, 0.5f);
        rect_CloneAnnouncementText.localScale = new Vector3(1, 1, 1);
        rect_CloneAnnouncementText.localPosition = new Vector3(0, 0, 0);
        rect_CloneAnnouncementText.anchoredPosition = new Vector2(rect_CloneAnnouncementText.anchoredPosition.x, rect_AnnouncementText.anchoredPosition.y);
    }

    // Start is called before the first frame update
    private void Start()
    {
        Debug.Log(btn_Dismiss != null);
        btn_Dismiss.onClick.AddListener(DismissAnnouncement);
        btn_Dismiss.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    private void Update() {
        txt_Announcement.fontSize = canvasTransform.rect.height * FONT_SCALE;
        txt_CloneAnnouncement.fontSize = txt_Announcement.fontSize;

        //Update if text changes
        if (txt_Announcement.havePropertiesChanged) {
            txt_Announcement.text = initialAnnouncement + " \t\t ";
            width = txt_Announcement.preferredWidth;
            txt_CloneAnnouncement.text = txt_Announcement.text;
        }

        //Add whitespace if needed for seamless loop
        while (width < canvasTransform.rect.width) {
            txt_Announcement.text += " \t ";
            width = txt_Announcement.preferredWidth;
        }

        //Scroll
        rect_AnnouncementText.anchoredPosition = Vector2.MoveTowards(
            rect_AnnouncementText.anchoredPosition, new Vector2(
                -width + startPosition.x, rect_AnnouncementText.anchoredPosition.y
            ), SCROLL_SPEED * canvasTransform.rect.width * Time.deltaTime
        );

        /*Debug.LogError("rect_AnnouncementText.anchoredPosition.x: " + rect_AnnouncementText.anchoredPosition.x);
        Debug.LogError("startPosition.x: " + startPosition.x);
        Debug.LogError("-width + startPosition.x: " + -width + startPosition.x);*/

        //Reset Scroll to loop
        if (rect_AnnouncementText.anchoredPosition.x <= -width + startPosition.x) {
            rect_AnnouncementText.anchoredPosition = new Vector2(startPosition.x, rect_AnnouncementText.anchoredPosition.y);
            scrollCount++;
        }

        if (scrollCount > 1) {
            gameObject.SetActive(false);
            btn_Dismiss.gameObject.SetActive(false);
            scrollCount = 0;
        }
    }
    
    public void DisplayAnnouncement(string txt_ancmt) {
        //Set announcement text
        initialAnnouncement = txt_ancmt;
        txt_Announcement.text = txt_ancmt + " \t\t ";
        txt_CloneAnnouncement.text = txt_Announcement.text;
        width = txt_Announcement.preferredWidth;

        //Reset scroll count
        scrollCount = 0;

        //Show announcement
        gameObject.SetActive(true);
        btn_Dismiss.gameObject.SetActive(true);
    }

    public void DismissAnnouncement() {
        gameObject.SetActive(false);
        btn_Dismiss.gameObject.SetActive(false);
        scrollCount = 0;
    }
}
