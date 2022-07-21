using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuScreen : MonoBehaviour
{
    public GameObject Stats;
    public GameObject Announcement;
    public GameObject Controls;
    public GameObject ChangeColor;
    public GameObject Groups;
    public GameObject Quit;
    public GameObject PlayerList;
    public GameObject AddPlayerScreen;
    public Button Refresh;
    public List<GameObject> Screens;
    public AudioClip flipAudio;

    public void flipScreen()
    {
        if (gameObject.activeSelf == false)
        {
            FindObjectOfType<GameManager>().GetComponent<AudioSource>().PlayOneShot(flipAudio);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            foreach(GameObject temp in Screens)
            {
                temp.SetActive(false);
            }

            Stats.SetActive(false);
            Announcement.SetActive(false);
            Controls.SetActive(false);
            ChangeColor.SetActive(false);
            Groups.SetActive(false);
            Quit.SetActive(false);
            AddPlayerScreen.SetActive(false);
            PlayerList.SetActive(true);
            if (PlayerList.GetComponent<PlayerListScreen>().GenerateType)
            {
                PlayerList.GetComponent<PlayerListScreen>().generateRangeList();
            }
            else
            {
                PlayerList.GetComponent<PlayerListScreen>().generateList();
            }
            gameObject.SetActive(true);
            Refresh.onClick.Invoke();
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;

            foreach (GameObject temp in Screens)
            {
                temp.SetActive(false);
            }

            Stats.SetActive(false);
            Announcement.SetActive(false);
            Controls.SetActive(false);
            ChangeColor.SetActive(false);
            Groups.SetActive(false);
            Quit.SetActive(false);
            PlayerList.SetActive(false);
            AddPlayerScreen.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}
