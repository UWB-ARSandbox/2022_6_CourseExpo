using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuScreen : MonoBehaviour
{
    public GameObject Stats;
    public GameObject Announcement;
    public GameObject Controls;
    public GameObject Quit;
    public GameObject PlayerList;
    public Button Refresh;
    public List<GameObject> Screens;

    public void flipScreen()
    {
        if (gameObject.activeSelf == false)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            foreach(GameObject temp in Screens)
            {
                temp.SetActive(false);
            }

            Stats.SetActive(false);
            Announcement.SetActive(false);
            Controls.SetActive(false);
            Quit.SetActive(false);
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
            Quit.SetActive(false);
            PlayerList.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}
